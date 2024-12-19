
(function () {
    var menuId, pageName, menuParam;
    var pageId, pageIdWithHash;
    var isBlended = false;
    var toolbarId, _oRow, _index, _modalFrom, _oRowCollar, _indexCollar, _oRowCuff, _indexCuff;
    var $divTblEl, $divDetailsEl, $pageEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblCreateCompositionId, $tblCreateCompositionEl, tblChildId, tblChildCollarId, $tblChildCollarIdEl
        , tblChildCuffId, $tblChildCuffIdEl, tblItemSummary, $tblItemSummaryEl,
        tblFabricChildId, tblCollarChildId, tblCuffChildId, tblPlanningId, $tblPlanningEl, $modalPlanningEl
        , tblCriteriaId, $tblCriteriaIdEl, $modalCriteriaEl, _indexc, _oRowc, ids;
    var $modalUtilizationPropoasalEl, $tblStockInfoEl, tblStockInfoId, $tblStockSummaryEl, tblStockSummaryId;
    var tblChildIdFP, $tblChildElFP, tblColorChildIdFP, $tblColorChildElFP;

    var tblFFUtilizationId, $tblFFUtilizationEL, $modalFFUtilizationInfoEL;
    var tblGFUtilizationId, $tblGFUtilizationEL, $modalGFUtilizationInfoEL;
    var tblGeryYarnUtilizationId, $tblGeryYarnUtilizationEL, $modalGeryYarnUtilizationInfoEL;
    var tblDyedYarnUtilizationId, $tblDyedYarnUtilizationEL, $modalDyedYarnUtilizationInfoEL;
    var tblFBAckChildReplacementInfoId, $tblFBAckChildReplacementInfoEL, $modalFBAckChildReplacementInfoEL;
    var tblFBAckYarnNetYarnReqQtyInfoId, $tblFBAckYarnNetYarnReqQtyInfoEL, $modalFBAckYarnNetYarnReqQtyInfoEL;
    var selectedCompositionId = 0;
    var selectedGSMId = 0;
    var GFUtilizationSummary = [];
    var FinishFabricUtilizationDataList = [];
    var GreyYarnUtilizationDataList = [];
    var DyedYarnUtilizationDataList = [];

    var GreyYarnUtilizationSummary = [];
    var DyedYarnUtilizationSummary = [];

    var menuType = 0;
    var idsList = [];
    var idsListCopyFabric = [],
        idsListCopyCollarOrCuff = [];
    var status = statusConstants.ACTIVE;
    var CriteriaName;
    var _isBDS = 1;
    var masterData;
    var bmtArray = [];
    var itemTNAInfo = null, itemTNAInfoCollar = null; //itemTNAInfoCuff = null;
    var maxCol = 999;
    var _saveType = "";
    var _fpBookingChildID = 0;
    var _fpBookingChildColorID = 0;
    var _fbChildID = 999;
    var _fbChildItemID = 999;
    var _fpRow = {};
    var _isRevise = false;
    var _isFirstTime = true;
    var _isPendingList = false;
    var _statusText = "";

    var fabYarnItem = {
        ParentInfo: null,
        ChildItems: []
    };
    var collarCuffYarnItem = {
        ParentInfo: null,
        ChildItems: []
    };
    var _yarnSegments = [];
    var _yarnSegmentsMapping = [];

    var _isLabDipAck = false;
    var _isLabDipAck_RnD = false;
    var _isRemarksShow = false;
    var _isRemarksEditable = false;
    var _isInternalRevise = false;
    var _allYarnList = [];

    var stockData = [];
    var stockSummary = [];
    var selectedChildIndex;
    var selectedBookingChildID = 0;
    var selectedYBChildItemID = 0;

    var _isYP = false,
        _isYPConfirmed = false;


    var __GSMId = '', __GSMNumber = '', __CompositionId = '', __ConstructionId = '', __SubGroupID = '', __ItemMasterID = '';
    var _IsYarnRevision = false;
    var _isFirstLoad = true;
    var AdditionalReplacementDataList = [];
    var AdditionalNetReqDataList = [];

    var _paramType = {
        BDSAcknowledge: 0,

        BulkBookingAck: 1,

        Projection: 2,

        BulkBookingCheck: 3,
        BulkBookingApprove: 4,
        BulkBookingFinalApprove: 5,
        BulkBookingYarnAllowance: 6,

        LabdipBookingAcknowledge: 7,
        LabdipBookingAcknowledgeRnD: 8,

        AdditionalYarnBooking: 9,
        AYBQtyFinalizationPMC: 10,
        AYBProdHeadApproval: 11,
        AYBTextileHeadApproval: 12,
        AYBKnittingUtilization: 13,
        AYBKnittingHeadApproval: 14,
        AYBOperationHeadApproval: 15,

        BulkBookingUtilizationProposal: 16,
        BulkBookingUtilizationConfirmation: 17,

        YarnBookingAcknowledge: 18
    }

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName"); 
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");
        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;

        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblChildCollarId = "#tblChildCollarId" + pageId;
        tblChildCuffId = "#tblChildCuffId" + pageId;
        tblItemSummary = "#tblItemSummary" + pageId;
        tblPlanningId = "#tblPlanning" + pageId;
        $modalPlanningEl = $("#modalPlanning" + pageId);
        tblCriteriaId = "#tblCriteria" + pageId;
        $modalCriteriaEl = $("#modalCriteria" + pageId);
        pageIdWithHash = "#" + pageId;
        tblChildIdFP = "tblChildFP" + pageId;
        tblColorChildIdFP = "tblColorChildIdFP" + pageId;
        tblCreateCompositionId = `#tblCreateComposition-${pageId}`;

        tblFFUtilizationId = `#tblFFUtilizationInfo${pageId}`;
        $modalFFUtilizationInfoEL = $("#modalFFUtilizationInfo" + pageId);

        tblGFUtilizationId = `#tblGFUtilizationInfo${pageId}`;
        $modalGFUtilizationInfoEL = $("#modalGFUtilizationInfo" + pageId);

        tblGeryYarnUtilizationId = `#tblGreyYarnUtilizationInfo${pageId}`;
        $modalGeryYarnUtilizationInfoEL = $("#modalGeryYarnUtilizationInfo" + pageId);

        tblDyedYarnUtilizationId = `#tblDyedYarnUtilizationInfo${pageId}`;
        $modalDyedYarnUtilizationInfoEL = $("#modalDyedYarnUtilizationInfo" + pageId);

        $modalUtilizationPropoasalEl = $("#modalUtilizationPropoasal" + pageId);

        tblFBAckChildReplacementInfoId = `#tblFBAckChildReplacementInfo${pageId}`;
        $modalFBAckChildReplacementInfoEL = $("#modalFBAckChildReplacementInfo" + pageId);

        tblFBAckYarnNetYarnReqQtyInfoId = `#tblFBAckYarnNetYarnReqQtyInfo${pageId}`;
        $modalFBAckYarnNetYarnReqQtyInfoEL = $("#modalFBAckYarnNetYarnReqQtyInfo" + pageId);

        //menuType = localStorage.getItem("bulkBookingAckPage");
        //menuType = parseInt(menuType);
        
        menuType = setBulkBookingAckPage(menuParam);

        $formEl.find(".divForBBFA").hide();
        $formEl.find(".divForWeight").hide();
        $formEl.find(".SizeWithConsumption").prop("disabled", true);

        if (menuType == _paramType.BulkBookingUtilizationProposal) _isYP = true;
        else if (menuType == _paramType.BulkBookingUtilizationConfirmation) _isYPConfirmed = true;

        if (menuType == _paramType.LabdipBookingAcknowledge) _isLabDipAck = true;
        if (menuType == _paramType.LabdipBookingAcknowledgeRnD) _isLabDipAck_RnD = true;

        if (menuType == _paramType.BulkBookingAck
            || menuType == _paramType.BulkBookingYarnAllowance
            || menuType == _paramType.BulkBookingCheck
            || menuType == _paramType.BulkBookingApprove
            || menuType == _paramType.BulkBookingFinalApprove
            || menuType == _paramType.BulkBookingUtilizationProposal
            || menuType == _paramType.BulkBookingUtilizationConfirmation) {
            _isRemarksShow = true;
            _isRemarksEditable = true;
        }
        if (menuType == _paramType.YarnBookingAcknowledge) {
            $formEl.find("#divCommonGeneralData").hide();
            $formEl.find("#divYBAckGeneralData").show();

            $formEl.find("#divtblChild").hide();
            $formEl.find("#btntblChild").click(function (e) {
                $formEl.find("#divtblChild").toggle();
                initChild(masterData.FBookingChild);
            });
            $formEl.find("#divtblChildCollar").hide();
            $formEl.find("#btntblChildCollar").click(function (e) {
                $formEl.find("#divtblChildCollar").toggle();
                initChildCollar(masterData.FBookingChildCollor);
            });
            $formEl.find("#divtblChildCuff").hide();
            $formEl.find("#btntblChildCuff").click(function (e) {
                $formEl.find("#divtblChildCuff").toggle();
                initChildCuff(masterData.FBookingChildCuff);
            });
        }
        else {
            $formEl.find("#divCommonGeneralData").show();
            $formEl.find("#divYBAckGeneralData").hide();

            $formEl.find("#divtblChild").show();
            $formEl.find("#btntblChild").click(function (e) {
                $formEl.find("#divtblChild").toggle();
                initChild(masterData.FBookingChild);
            });
            $formEl.find("#divtblChildCollar").show();
            $formEl.find("#btntblChildCollar").click(function (e) {
                $formEl.find("#divtblChildCollar").toggle();
                initChildCollar(masterData.FBookingChildCollor);
            });
            $formEl.find("#divtblChildCuff").show();
            $formEl.find("#btntblChildCuff").click(function (e) {
                $formEl.find("#divtblChildCuff").toggle();
                initChildCuff(masterData.FBookingChildCuff);
            });
        }


        $toolbarEl.find("#btnDraftList").hide();
        $formEl.find("#btnSaveAsDraft").hide();
        $formEl.find(".divYBookingNo,.divForBBKI").hide();
        $formEl.find("#addYarnComposition").hide();
        $formEl.find("#divAddYarnBookingDate").hide();

        $formEl.find("#addYarnComposition").on("click", function (e) {
            showAddComposition();
        });

        $formEl.find("#btnLoadFinishingFabricUtilization").on("click", function (e) {
            LoadFinishingFabricUtilizationPopUp();
        });
        $formEl.find("#btnLoadGreyFabricUtilization").on("click", function (e) {
            LoadGreyFabricUtilization();
        });
        $formEl.find("#btnLoadGreyYarnUtilization").on("click", function (e) {
            LoadGreyYarnUtilization();
        });
        $formEl.find("#btnLoadDyedYarnUtilization").on("click", function (e) {
            LoadDyedYarnUtilization();
        });

        if (isBulkBookingKnittingInfoMenu()) {
            $formEl.find("#lblTableTitle").text("Booking Consumption");
            tootBarButtonHideShow();

            _isBDS = 2;

            $toolbarEl.find("#btnReceive,#btnReceived").hide();
            $formEl.find("#btnSaveAsDraft").show();

            if (status != statusConstants.PENDING) $formEl.find(".divYBookingNo").show();
            $formEl.find(".divForBBKI").show();

            $formEl.find("#btnCollarApplyKG").click(function (e) {

                var size = $(pageIdWithHash).find("#CollarSizeID").val();
                if (size == null || size.length == 0) {
                    toastr.error("Select size");
                    return false;
                }
                var weight = $(pageIdWithHash).find("#CollarWeightInGm").val();
                if (weight == null || weight == 0) {
                    toastr.error("Give consumption(gm)");
                    return false;
                }
                if (menuType == _paramType.BulkBookingAck && status == statusConstants.PENDING) {
                    SetCollarBookingWeightKG();
                }
                else {
                    SetCollarBookingWeightKGAfterSave();
                }

                if (menuType == _paramType.BulkBookingAck && status == statusConstants.PENDING) {
                    GetCalculatedFBookingChildCollor(masterData.Collars);
                } else {
                    GetCalculatedFBookingChildCollor(masterData.FBookingChildCollor);
                }
                $tblChildCollarIdEl.refresh();
            });
            $formEl.find("#btnCuffApplyKG").click(function (e) {

                var size = $(pageIdWithHash).find("#CuffSizeID").val();
                if (size == null || size.length == 0) {
                    toastr.error("Select size");
                    return false;
                }
                var weight = $(pageIdWithHash).find("#CuffWeightInGm").val();
                if (weight == null || weight == 0) {
                    toastr.error("Give consumption(gm)");
                    return false;
                }
                if (menuType == _paramType.BulkBookingAck && status == statusConstants.PENDING) {
                    SetCuffBookingWeightKG();
                }
                else {
                    SetCuffBookingWeightKGAfterSave();
                }
                if (menuType == _paramType.BulkBookingAck && status == statusConstants.PENDING) {
                    GetCalculatedFBookingChildCuff(masterData.Cuffs);
                } else {
                    GetCalculatedFBookingChildCuff(masterData.FBookingChildCuff);
                }
                $tblChildCuffIdEl.refresh();
            });

            $toolbarEl.find("#btnPendingList").click(function (e) {
                isEnableSizeConsumption(true);
                _isPendingList = false;
                e.preventDefault();
                $formEl.find(".btnAction").hide();

                toggleActiveToolbarBtn(this, $toolbarEl);
                $formEl.find("#btnSave,#btnUnAcknowledge").hide();
                status = statusConstants.PENDING;
                $formEl.find("#btnSaveAsDraft").show();
                $formEl.find("#btnRejectByKnittingInputPopup").show();
                initBulkAckList(0);
            });
            $toolbarEl.find("#btnDraftList").click(function (e) {
                isEnableSizeConsumption(true);
                _isPendingList = false;

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                $formEl.find(".btnAction").hide();

                $formEl.find("#btnSaveAsDraft,#btnSave,#btnUnAcknowledge").show();
                $formEl.find('#btnSave').html("Save & Send For Approval");
                $formEl.find('#btnUnAcknowledge').html("Reject");

                status = statusConstants.DRAFT;
                initBulkAckList(0);
            });
            $toolbarEl.find("#btnBookingList").click(function (e) {
                isEnableSizeConsumption(false);

                _isPendingList = false;

                e.preventDefault();
                $formEl.find(".btnAction").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.ACTIVE;
                $formEl.find("#btnSaveAsDraft").hide();
                initBulkAckList(0);
            });
            $toolbarEl.find("#btnUnAcknowledgedList").on("click", function (e) {
                isEnableSizeConsumption(false);
                _isPendingList = false;

                e.preventDefault();
                $formEl.find(".btnAction").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.UN_ACKNOWLEDGE;
                $formEl.find("#btnSaveAsDraft").hide();
                initBulkAckList(0);
            });

            $formEl.find("#btnCancel").on("click", backToListBulk2);

            $formEl.find("#btnUnAcknowledge").click(function (e) {
                bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                    if (!result) {
                        return toastr.error("UnAcknowledge reason is required.");
                    }
                    _saveType = "UnAcknowledge";
                    save(result, false, false, false, false, false, "btnUnAcknowledge");
                });
            });

            $toolbarEl.find("#btnFinalApprovaledList").click(function (e) {

                isEnableSizeConsumption(false);
                _isPendingList = false;
                e.preventDefault();
                $formEl.find(".btnAction").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.APPROVED_PMC;
                $formEl.find("#btnSaveAsDraft").hide();

                $formEl.find("#btnApproveByPMC").hide();
                $formEl.find("#btnRejectByPMCPopup,#btnInternalReviseByPMCPopup").hide();
                initBulkAckList(0);
            });
            $toolbarEl.find("#btnInternalRejectionList").click(function (e) {
                isEnableSizeConsumption(true);
                _isPendingList = false;

                e.preventDefault();
                $formEl.find(".btnAction").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.INTERNAL_REJECTION;
                $formEl.find("#btnSave").show();
                $formEl.find("#btnUnAcknowledge").show();
                initBulkAckList(0);
            });

            if (menuType == _paramType.BulkBookingCheck) //Bulk Booking Knitting check
            {
                $toolbarEl.find("#btnPendingCheckList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = true;
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.PROPOSED_FOR_APPROVAL;
                    $formEl.find("#btnSaveAsDraft").hide();
                    $formEl.find("#btnCheckByKnittingHead").show();
                    $formEl.find("#btnRejectByKnittingHeadPopup").show();
                    initBulkAckList(_paramType.BulkBookingCheck);
                });
                $toolbarEl.find("#btnCheckedList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.CHECK;
                    $formEl.find("#btnSaveAsDraft").hide();
                    $formEl.find("#btnCheckByKnittingHead").hide();
                    $formEl.find("#btnRejectByKnittingHeadPopup").hide();
                    initBulkAckList(0);
                });
                $toolbarEl.find("#btnRejectedCheckList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.CHECK_REJECT;
                    $formEl.find("#btnSaveAsDraft").hide();
                    $formEl.find("#btnCheckByKnittingHead").hide();
                    $formEl.find("#btnRejectByKnittingHeadPopup").hide();
                    initBulkAckList(0);
                });
                $formEl.find("#btnCheckByKnittingHead").click(function (e) {
                    _saveType = "SaveAsDraft";
                    save("", true, false, false, false, false, $(this).attr('id'));
                });
                $formEl.find("#btnRejectByKnittingHeadPopup").click(function (e) {
                    $(pageIdWithHash).find("#txtRejectReason").val("");
                    $(pageIdWithHash).find("#modalRejectReason").modal('show');
                });

                $(pageIdWithHash).find("#btnRejectKI").click(function (e) {
                    $(pageIdWithHash).find("#modalRejectReason").modal('hide');
                    _saveType = "SaveAsDraft";
                    save("", false, true, false, false, false, $(this).attr('id'));
                });

                $toolbarEl.find("#btnPendingCheckList").click();
            }
            else if (menuType == _paramType.BulkBookingApprove) //Bulk Booking Knitting approve
            {
                $toolbarEl.find("#btnPendingApprovalList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = true;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.CHECK;
                    $formEl.find("#btnSaveAsDraft").hide();
                    $formEl.find("#btnApproveByProductionHead").show();
                    $formEl.find("#btnRejectByProductionHeadPopup").show();
                    initBulkAckList(_paramType.BulkBookingApprove);
                });
                $toolbarEl.find("#btnApprovedList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED_DONE;
                    $formEl.find("#btnSaveAsDraft").hide();
                    $formEl.find("#btnApproveByProductionHead").hide();
                    $formEl.find("#btnRejectByProductionHeadPopup").hide();
                    initBulkAckList(0);
                });
                $toolbarEl.find("#btnRejectedApproveList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT_REVIEW;
                    $formEl.find("#btnSaveAsDraft").hide();
                    $formEl.find("#btnApproveByProductionHead").hide();
                    $formEl.find("#btnRejectByProductionHeadPopup").hide();
                    initBulkAckList(0);
                });
                $formEl.find("#btnApproveByProductionHead").click(function (e) {
                    approveRejectOperationBBKI(false, false);
                });
                $formEl.find("#btnRejectByProductionHeadPopup").click(function (e) {
                    $(pageIdWithHash).find("#txtRejectReason").val("");
                    $(pageIdWithHash).find("#modalRejectReason").modal('show');
                });

                $(pageIdWithHash).find("#btnRejectKI").click(function (e) {
                    $(pageIdWithHash).find("#modalRejectReason").modal('hide');
                    approveRejectOperationBBKI(true, false);
                });

                $toolbarEl.find("#btnPendingApprovalList").click();
            }
            else if (menuType == _paramType.BulkBookingFinalApprove) //Bulk Booking Final Approve (PMC)
            {
                //PMC
                $toolbarEl.find("#btnPendingFinalApprovalList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = true;
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED_DONE;
                    $formEl.find("#btnSaveAsDraft").hide();
                    $formEl.find("#btnApproveByPMC").show();
                    $formEl.find("#btnRejectByPMCPopup,#btnInternalReviseByPMCPopup").show();
                    initBulkAckList(_paramType.BulkBookingFinalApprove);
                });

                $toolbarEl.find("#btnRejectedFinalApprovalList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT_PMC;
                    $formEl.find("#btnSaveAsDraft").hide();

                    $formEl.find("#btnApproveByPMC").hide();
                    $formEl.find("#btnRejectByPMCPopup,#btnInternalReviseByPMCPopup").hide();
                    initBulkAckList(0);
                });
                $formEl.find(".divForBBFA").show();

                $formEl.find("#btnApproveByPMC").click(function (e) {
                    approveRejectOperationBBKI(false, false);
                });
                $formEl.find("#btnInternalReviseByPMCPopup").click(function (e) {
                    _isInternalRevise = true;
                    $(pageIdWithHash).find("#txtRejectReason").val("");
                    $(pageIdWithHash).find("#modalRejectReason").modal('show');
                });
                $formEl.find("#btnRejectByPMCPopup").click(function (e) {
                    _isInternalRevise = false;
                    $(pageIdWithHash).find("#txtRejectReason").val("");
                    $(pageIdWithHash).find("#modalRejectReason").modal('show');
                });

                $(pageIdWithHash).find("#btnRejectKI").click(function (e) {
                    $(pageIdWithHash).find("#modalRejectReason").modal('hide');
                    var isReject = _isInternalRevise ? false : true;
                    var isInternalRevise = _isInternalRevise ? true : false;

                    approveRejectOperationBBKI(isReject, isInternalRevise);
                });

                $toolbarEl.find("#btnPendingFinalApprovalList").click();
            }
            else if (menuType == _paramType.BulkBookingYarnAllowance) //Bulk Booking Yarn Allowance
            {
                //Allowance
                $toolbarEl.find("#btnPendingAllowanceList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = true;
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.ACTIVE;
                    $formEl.find("#btnSaveAsDraft").hide();
                    $formEl.find("#btnApproveByAllowance").show();
                    //$formEl.find("#btnRejectByAllowancePopup").show();
                    $formEl.find("#btnRejectByAllowancePopup").hide();
                    initBulkAckList(_paramType.BulkBookingYarnAllowance);
                });
                $toolbarEl.find("#btnAllowanceList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED_Allowance;
                    $formEl.find("#btnSaveAsDraft").hide();
                    $formEl.find("#btnApproveByAllowance").hide();
                    $formEl.find("#btnRejectByAllowancePopup").hide();
                    initBulkAckList(0);
                });
                $toolbarEl.find("#btnRejectedAllowanceList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT_Allowance;
                    $formEl.find("#btnSaveAsDraft").hide();
                    $formEl.find("#btnApproveByAllowance").hide();
                    $formEl.find("#btnRejectByAllowancePopup").hide();
                    initBulkAckList(0);
                });
                $formEl.find("#btnApproveByAllowance").click(function (e) {
                    approveRejectOperationBBKI(false, false);
                });
                $formEl.find("#btnRejectByAllowancePopup").click(function (e) {
                    $(pageIdWithHash).find("#txtRejectReason").val("");
                    $(pageIdWithHash).find("#modalRejectReason").modal('show');
                });

                $(pageIdWithHash).find("#btnRejectKI").click(function (e) {
                    $(pageIdWithHash).find("#modalRejectReason").modal('hide');
                    approveRejectOperationBBKI(true, false);
                });

                $toolbarEl.find("#btnPendingAllowanceList").click();
            }
            else if (menuType == _paramType.BulkBookingUtilizationProposal) //Bulk Booking Utilization Proposal
            {
                //Utilization Proposal
                $toolbarEl.find("#btnPendingUtilizationProposalList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = true;
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED_Allowance;
                    $formEl.find("#btnSaveUtilizationProposal").show();
                    initBulkAckList(_paramType.BulkBookingUtilizationProposal);
                });
                $formEl.find("#btnSaveUtilizationProposal").click(function (e) {
                    approveRejectOperationBBKI(false, false);
                });
                $toolbarEl.find("#btnPendingUtilizationProposalList").click();
            }
            else if (menuType == _paramType.BulkBookingUtilizationConfirmation) //Bulk Booking Utilization Confirmation
            {
                //Utilization Confirmation
                $toolbarEl.find("#btnPendingUtilizationConfirmationList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = true;
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.PENDING_CONFIRMATION;
                    $formEl.find("#btnSendForUtilizationApproval").show();
                    initBulkAckList(_paramType.BulkBookingUtilizationConfirmation);
                });
                $toolbarEl.find("#btnUtilizationConfirmedList").click(function (e) {
                    isEnableSizeConsumption(false);
                    _isPendingList = false;
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.CONFIRM;
                    $formEl.find("#btnSendForUtilizationApproval").hide();
                    initBulkAckList(_paramType.BulkBookingUtilizationConfirmation);
                });

                $formEl.find("#btnSendForUtilizationApproval").click(function (e) {
                    approveRejectOperationBBKI(false, false);
                });
                $toolbarEl.find("#btnPendingUtilizationConfirmationList").click();
            }
            else if (menuType == _paramType.YarnBookingAcknowledge) {
                $toolbarEl.find("#btnPendingYBAList").click(function (e) {
                    isEnableSizeConsumption(false);
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.PENDING;
                    initBulkAckList(_paramType.YarnBookingAcknowledge);
                });
                $toolbarEl.find("#btnRevisionYarnYBAList").click(function (e) {
                    isEnableSizeConsumption(false);
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REVISE;
                    initBulkAckList(_paramType.YarnBookingAcknowledge);
                });
                //$toolbarEl.find("#btnRevisionBookingYBAList").click(function (e) {
                //    isEnableSizeConsumption(false);
                //    e.preventDefault();
                //    $formEl.find(".btnAction").hide();
                //    toggleActiveToolbarBtn(this, $toolbarEl);
                //    status = statusConstants.REVISE2;
                //    initBulkAckList(_paramType.YarnBookingAcknowledge);
                //});
                $toolbarEl.find("#btnAckYBAList").click(function (e) {
                    isEnableSizeConsumption(false);
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.ACKNOWLEDGE;
                    initBulkAckList(_paramType.YarnBookingAcknowledge);
                });
                $toolbarEl.find("#btnUnAckYBAList").click(function (e) {
                    isEnableSizeConsumption(false);
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.UN_ACKNOWLEDGE;
                    initBulkAckList(_paramType.YarnBookingAcknowledge);
                });
                $formEl.find("#divItemSummary").show();
                $formEl.find("#btnItemSummary").click(function () {
                    loadItemSummary();
                });
                $formEl.find("#btnYarnBookingSummaryReport").click(function () {
                    displayYarnBookingSummaryReport();
                });
                $formEl.find("#btnUnAckYBA").click(function (e) {
                    bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                        if (!result) {
                            return toastr.error("UnAcknowledge reason is required.");
                        }
                        acknowledgeYBA(true, result);
                    });
                });
                $formEl.find("#btnAckYBA").click(function (e) {
                    acknowledgeYBA(false);
                });
                $toolbarEl.find("#btnPendingYBAList").click();
            }
            else {
                if (menuType == _paramType.BulkBookingAck) {

                    $formEl.find("#addYarnComposition").show();
                    $formEl.find("#btnReviseBBKI").click(function (e) {
                        bootbox.prompt("Enter your Revise reason:", function (result) {
                            if (!result) {
                                return toastr.error("Revise reason is required.");
                            }
                            e.preventDefault();
                            _saveType = "Save";

                            save(result, false, false, false, false, true, $(this).attr('id'));
                        });
                    });
                    $formEl.find("#btnReviseBBKIYarn").click(function (e) {

                        var finder = new commonFinder({
                            title: "Select Revision Reason",
                            pageId: pageId,
                            apiEndPoint: `/api/bds-acknowledge/get-yarn-revision-reason`,
                            fields: "ReasonName",
                            headerTexts: "Reason Name",
                            isMultiselect: true,
                            allowPaging: false,
                            primaryKeyColumn: "ReasonID",
                            onMultiselect: function (selectedRecords) {
                                if (selectedRecords.length > 0) {
                                    selectedRecords.forEach(function (value) {

                                        value.YBookingNo = masterData.YBookingNo;

                                    });
                                    masterData.RevisionReasonList = selectedRecords;
                                }
                                else {
                                    data.RevisionReasonList = [];
                                }
                                approveRejectOperationBBKI(false, false, true);
                            }
                        });
                        finder.showModal();


                    });
                }

                $toolbarEl.find(".spnBookingListText").text("Sending For Approval List");
                $toolbarEl.find(".spnUnAcknowledgedListText").html("Reject List");
                $toolbarEl.find("#btnPendingList").click();
            }

            if (menuType == _paramType.BulkBookingAck) {
                $formEl.find("#btnRejectByKnittingInputPopup").click(function (e) {
                    $(pageIdWithHash).find("#txtRejectReason").val("");
                    $(pageIdWithHash).find("#modalRejectReason").modal('show');
                });

                $(pageIdWithHash).find("#btnRejectKI").click(function (e) {
                    $(pageIdWithHash).find("#modalRejectReason").modal('hide');
                    _saveType = "SaveAsDraft";

                    save("", false, false, true, false, false, $(this).attr('id'));
                });
                $pageEl.find('input[type=radio][name=BlendedBBKI]').change(function (e) {
                    e.preventDefault();
                    isBlended = convertToBoolean(this.value);
                    initTblCreateComposition();
                    return false;
                });
            }

            //-----------------------Load Notification Count-----------------------------------
            /*
            var countTagProps = "";
            switch (menuType) {
                case _paramType.BulkBookingAck:
                    countTagProps = "Pending,Draft,SendingForApproval,Reject,Approved,Reject2,AllCount";
                    break;
                case _paramType.BulkBookingYarnAllowance:
                    countTagProps = "Pending,SendingForApproval,Reject,AllCount";
                    break;
                case _paramType.BulkBookingUtilizationProposal:
                    countTagProps = "UtilizationProposalPending,AllCount";
                    break;
                case _paramType.BulkBookingUtilizationConfirmation:
                    countTagProps = "UtilizationConfirmationPending,UtilizationConfirmed,AllCount";
                    break;
                case _paramType.BulkBookingCheck:
                    countTagProps = "Pending,CheckCount,Reject,AllCount";
                    break;
                case _paramType.BulkBookingApprove:
                    countTagProps = "Pending,Approved,Reject,AllCount";
                    break;
                case _paramType.BulkBookingFinalApprove:
                    countTagProps = "Pending,Approved,Reject,AllCount";
                    break;
                default:
                // code block
            }
            if (countTagProps.length > 0) {
                loadListCountMethod({
                    ToolbarId: toolbarId,
                    URL: `/api/bds-acknowledge/bulk/bulk-booking-knitting-info/get-list-count/${menuType}`,
                    CountTagProps: countTagProps,
                    IsDefaultAllCount: false
                });
            }
            */
            //-----------------------Load Notification Count-----------------------------------
        }
        else if (menuType == _paramType.BDSAcknowledge) {
            $formEl.find("#lblTableTitle").text("Sample Booking Consumption");
            _isBDS = 1;
            $toolbarEl.find("#btnPendingList,#btnBookingList").hide();
            $toolbarEl.find("#btnWaitingForRevisionList").show();

            $toolbarEl.find("#btnList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.PENDING;
                initMasterTable();
            });
            $toolbarEl.find("#btnCancelList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.REJECT;
                initMasterTable();
            });
            $toolbarEl.find("#btnRcvList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.ACTIVE;
                initMasterTable();
            });
            $toolbarEl.find("#btnAcknowledgedList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.COMPLETED;
                initMasterTable();
            });
            $toolbarEl.find("#btnUnAcknowledgedList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.UN_ACKNOWLEDGE;
                initMasterTable();
            });
            $toolbarEl.find("#btnWaitingForRevisionList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.OTHERS;
                initMasterTable();
            });

            $toolbarEl.find("#btnDeliveredList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.APPROVED;
                initMasterTable();
            });

            $formEl.find("#btnCancel").on("click", backToList);

            $formEl.find("#btnReceive").click(function (e) {
                e.preventDefault();
                Receive(this);
            });
            $formEl.find("#btnReceived").click(function (e) {
                e.preventDefault();
                Received(this);
            });
            $formEl.find("#btnUnAcknowledge").click(function (e) {
                bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                    if (!result) {
                        return toastr.error("UnAcknowledge reason is required.");
                    }
                    _saveType = "UnAcknowledge";

                    save(result, false, false, false, false, false, "btnUnAcknowledge");
                });
            });

            $formEl.find("#btnCancelAcknowledge").click(function (e) {
                e.preventDefault();
                _saveType = "CancelAcknowledge";
                cancelSave();
            });

            $formEl.find("#btnCancelUnAcknowledge").click(function (e) {
                bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                    if (!result) {
                        return toastr.error("UnAcknowledge reason is required.");
                    }
                    _saveType = "CancelUnAcknowledge";
                    cancelSave(result);
                });
            });

            initMasterTable();

        }
        else if (menuType == _paramType.Projection) {
            $toolbarEl.find("#btnRcvList").hide();
            status = statusConstants.PENDING;

            $formEl.find("#lblTableTitle").text("Sample Booking Consumption");
            _isBDS = 3;
            $toolbarEl.find("#btnPendingList,#btnBookingList").hide();

            $toolbarEl.find("#btnList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.PENDING;
                initMasterTable();
            });
            $toolbarEl.find("#btnCancelList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.REJECT;
                initMasterTable();
            });
            $toolbarEl.find("#btnAcknowledgedList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.COMPLETED;
                initMasterTable();
            });
            $toolbarEl.find("#btnUnAcknowledgedList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.UN_ACKNOWLEDGE;
                initMasterTable();
            });

            $toolbarEl.find("#btnDeliveredList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.APPROVED;
                initMasterTable();
            });

            $formEl.find("#btnCancel").on("click", backToList);

            $formEl.find("#btnReceive").click(function (e) {
                e.preventDefault();
                Receive(this);
            });
            $formEl.find("#btnReceived").click(function (e) {
                e.preventDefault();
                Received(this);
            });
            $formEl.find("#btnUnAcknowledge").click(function (e) {
                bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                    if (!result) {
                        return toastr.error("UnAcknowledge reason is required.");
                    }
                    _saveType = "UnAcknowledge";

                    save(result, false, false, false, false, false, "btnUnAcknowledge");
                });
            });

            $formEl.find("#btnCancelAcknowledge").click(function (e) {
                e.preventDefault();
                _saveType = "CancelAcknowledge";
                cancelSave();
            });

            $formEl.find("#btnCancelUnAcknowledge").click(function (e) {
                bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                    if (!result) {
                        return toastr.error("UnAcknowledge reason is required.");
                    }
                    _saveType = "CancelUnAcknowledge";
                    cancelSave(result);
                });
            });

            $toolbarEl.find("#btnList").click();
        }
        else if (isLabdipMenu()) {
            $toolbarEl.find(".btnToolbar").hide();
            $toolbarEl.find("#btnPendingLabDipList,#btnLabDipAcknowledgeList,#btnLabDipUnAcknowledgeList").show();
            $toolbarEl.find("#btnLabDipRnDAckList").hide();

            if (_isLabDipAck) {
                $toolbarEl.find("#btnLabDipRnDAckList").show();
                $toolbarEl.find("#btnFLLabDipUnAcknowledgeList").show();
            }
            if (_isLabDipAck_RnD) {
                $toolbarEl.find("#btnLabDipRevisionList").show();
            }

            $toolbarEl.find("#btnPendingLabDipList").click(function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);

                if (_isLabDipAck_RnD) {
                    status = statusConstants.ACKNOWLEDGE;
                }
                else {
                    status = statusConstants.PENDING;
                }
                initLabDipAckTable();
            });
            $toolbarEl.find("#btnLabDipAcknowledgeList").click(function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);

                if (_isLabDipAck_RnD) {
                    status = statusConstants.REVISE;
                }
                else {
                    status = statusConstants.ACKNOWLEDGE;
                }
                initLabDipAckTable();
            });
            $toolbarEl.find("#btnFLLabDipUnAcknowledgeList").click(function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);

                status = statusConstants.REJECT;

                initLabDipAckTable();
            });
            $toolbarEl.find("#btnLabDipUnAcknowledgeList").click(function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.UN_ACKNOWLEDGE;
                initLabDipAckTable();
            });
            $toolbarEl.find("#btnLabDipRnDAckList").click(function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.REVISE;
                initLabDipAckTable();
            });
            $toolbarEl.find("#btnLabDipRevisionList").click(function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.REVISE_FOR_ACKNOWLEDGE;
                initLabDipAckTable();
            });

            $toolbarEl.find("#btnPendingLabDipList").click();

            $formEl.find("#btnCancel").on("click", backToList);
            $formEl.find("#btnSaveLabDipAck").click(function (e) {
                e.preventDefault();

                acknowledgeLabDip(false, true, false, false);
            });
            $formEl.find("#btnSaveLabDipUnAck").click(function (e) {
                e.preventDefault();
                bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                    if (!result) {
                        return toastr.error("UnAcknowledge reason is required.");
                    }
                    acknowledgeLabDip(result, false, false, false);
                });
            });
            $formEl.find("#btnReviseLabDip").click(function (e) {
                e.preventDefault();

                var isRevise = status != statusConstants.UN_ACKNOWLEDGE ? true : false;
                var isReviseFromUnAckList = status == statusConstants.UN_ACKNOWLEDGE ? true : false;

                acknowledgeLabDip(false, true, isRevise, isReviseFromUnAckList);
            });
            $formEl.find("#btnReviseLabDipRnD").click(function (e) {
                e.preventDefault();
                _saveType = "Save";

                save("", false, false, false, true, false, $(this).attr('id'));
            });
            $formEl.find("#btnUnAcknowledge").click(function (e) {
                bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                    if (!result) {
                        return toastr.error("UnAcknowledge reason is required.");
                    }
                    _saveType = "UnAcknowledge";

                    save(result, false, false, false, false, false, $(this).attr('id'));
                });
            });
        }
        else if (isAdditionBulkBooking()) {
            $formEl.find("#lblTableTitle").text("Fabric Information");
            tootBarButtonHideShow();
            _isBDS = 2;

            isEnableSizeConsumption(true);

            $toolbarEl.find(".btnToolbar").hide();
            $formEl.find(".divYBookingNo").show();
            $formEl.find("#btnCancel").on("click", backToListBulk2);
            $formEl.find(".btnHideShowPlus").hide();
            $formEl.find("#divAddYarnBookingDate").show();

            $(pageIdWithHash).find("#CollarSizeID").prop("disabled", true);
            $(pageIdWithHash).find("#CollarWeightInGm").prop("disabled", true);
            $(pageIdWithHash).find("#CuffSizeID").prop("disabled", true);
            $(pageIdWithHash).find("#CuffWeightInGm").prop("disabled", true);

            if (menuType == _paramType.AdditionalYarnBooking) {
                $formEl.find(".forAYB").show();
                $toolbarEl.find("#btnPendingList,#btnAdditionBBKIList,#btnFinalApprovaledList,#btnInternalRejectionList").show();

                $toolbarEl.find("#btnPendingList").click(function (e) {
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();

                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED2;
                    initBulkAckList(0);
                });
                $toolbarEl.find("#btnAdditionBBKIList").click(function (e) {
                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.ADDITIONAL;
                    initBulkAckList(0);
                });
                $toolbarEl.find("#btnInternalRejectionList").click(function (e) {
                    isEnableSizeConsumption(true);
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.ADDITIONAL_INTERNAL_REJECTION;

                    //$formEl.find("#btnSaveAsDraft,#btnSave,#btnUnAcknowledge").show();
                    $formEl.find('#btnSave').html("Save & Send For Approval");
                    //$formEl.find('#btnUnAcknowledge').html("Reject");

                    $formEl.find("#btnSave").show();
                    $formEl.find("#btnUnAcknowledge").hide();
                    initBulkAckList(0);
                });
                $formEl.find("#btnAddFabricItem").click(function () {
                    _isFirstLoad = true;
                    loadFCCItem(1);
                });
                $formEl.find("#btnAddCollarItem").click(function () {
                    _isFirstLoad = true;
                    loadFCCItem(11);
                });
                $formEl.find("#btnAddCuffItem").click(function () {
                    _isFirstLoad = true;
                    loadFCCItem(12);
                });

                $toolbarEl.find("#btnPendingList").click();

            }
            else if (menuType == _paramType.AYBQtyFinalizationPMC) {
                $toolbarEl.find("#btnPendingList, #btnApproveBBKIList, #btnRejectBBKIList").show();

                $toolbarEl.find("#btnPendingList").click(function (e) {
                    _isPendingList = true;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.ADDITIONAL;
                    initBulkAckList(0);
                });
                $toolbarEl.find("#btnApproveBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initBulkAckList(_paramType.AYBQtyFinalizationPMC);
                });
                $toolbarEl.find("#btnRejectBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initBulkAckList(_paramType.AYBQtyFinalizationPMC);
                });

                $toolbarEl.find("#btnPendingList").click();
            }
            else if (menuType == _paramType.AYBProdHeadApproval) {

                $toolbarEl.find("#btnPendingList, #btnApproveBBKIList, #btnRejectBBKIList").show();

                $toolbarEl.find("#btnPendingList").click(function (e) {
                    _isPendingList = true;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initBulkAckList(_paramType.AYBQtyFinalizationPMC);
                });
                $toolbarEl.find("#btnApproveBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initBulkAckList(_paramType.AYBProdHeadApproval);
                });
                $toolbarEl.find("#btnRejectBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initBulkAckList(_paramType.AYBProdHeadApproval);
                });

                $toolbarEl.find("#btnPendingList").click();
            }
            else if (menuType == _paramType.AYBTextileHeadApproval) {

                $toolbarEl.find("#btnPendingList, #btnApproveBBKIList, #btnRejectBBKIList").show();

                $toolbarEl.find("#btnPendingList").click(function (e) {
                    _isPendingList = true;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initBulkAckList(_paramType.AYBProdHeadApproval);
                });
                $toolbarEl.find("#btnApproveBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initBulkAckList(_paramType.AYBTextileHeadApproval);
                });
                $toolbarEl.find("#btnRejectBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initBulkAckList(_paramType.AYBTextileHeadApproval);
                });

                $toolbarEl.find("#btnPendingList").click();
            }
            else if (menuType == _paramType.AYBKnittingUtilization) {

                $toolbarEl.find("#btnPendingList, #btnApproveBBKIList, #btnRejectBBKIList").show();

                $toolbarEl.find("#btnPendingList").click(function (e) {
                    _isPendingList = true;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initBulkAckList(_paramType.AYBTextileHeadApproval);
                });
                $toolbarEl.find("#btnApproveBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initBulkAckList(_paramType.AYBKnittingUtilization);
                });
                $toolbarEl.find("#btnRejectBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initBulkAckList(_paramType.AYBKnittingUtilization);
                });

                $toolbarEl.find("#btnPendingList").click();
            }
            else if (menuType == _paramType.AYBKnittingHeadApproval) {

                $toolbarEl.find("#btnPendingList, #btnApproveBBKIList, #btnRejectBBKIList").show();

                $toolbarEl.find("#btnPendingList").click(function (e) {
                    _isPendingList = true;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initBulkAckList(_paramType.AYBKnittingUtilization);
                });
                $toolbarEl.find("#btnApproveBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initBulkAckList(_paramType.AYBKnittingHeadApproval);
                });
                $toolbarEl.find("#btnRejectBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initBulkAckList(_paramType.AYBKnittingHeadApproval);
                });

                $toolbarEl.find("#btnPendingList").click();
            }
            else if (menuType == _paramType.AYBOperationHeadApproval) {

                $toolbarEl.find("#btnPendingList, #btnApproveBBKIList, #btnRejectBBKIList").show();

                $toolbarEl.find("#btnPendingList").click(function (e) {
                    _isPendingList = true;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initBulkAckList(_paramType.AYBKnittingHeadApproval);
                });
                $toolbarEl.find("#btnApproveBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.APPROVED;
                    initBulkAckList(_paramType.AYBOperationHeadApproval);
                });
                $toolbarEl.find("#btnRejectBBKIList").click(function (e) {
                    _isPendingList = false;

                    e.preventDefault();
                    $formEl.find(".btnAction").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    status = statusConstants.REJECT;
                    initBulkAckList(_paramType.AYBOperationHeadApproval);
                });

                $toolbarEl.find("#btnPendingList").click();
            }
            $toolbarEl.find("#btnAllBulkBookingList").show();
            $toolbarEl.find("#btnAllBulkBookingList").click(function (e) {

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                $formEl.find(".btnAction").hide();
                status = statusConstants.ALL_STATUS;
                initBulkAckList(menuType);
            });

            $formEl.find("#btnApproveAdditionBBKI").click(function () {
                approveAddition(false, '', menuType, false);
            });
            $toolbarEl.find("#btnFinalApprovaledList").click(function (e) {

                isEnableSizeConsumption(false);
                _isPendingList = false;
                e.preventDefault();
                $formEl.find(".btnAction").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                status = statusConstants.ADDITIONAL_APPROVED_OPERATION_HEAD;
                $formEl.find("#btnSaveAsDraft").hide();

                $formEl.find("#btnApproveByPMC").hide();
                $formEl.find("#btnRejectByPMCPopup,#btnInternalReviseByPMCPopup").hide();
                initBulkAckList(0);
            });
            $formEl.find("#btnReviseBBKIAdYarn").click(function (e) {

                var finder = new commonFinder({
                    title: "Select Revision Reason",
                    pageId: pageId,
                    apiEndPoint: `/api/bds-acknowledge/get-yarn-revision-reason`,
                    fields: "ReasonName",
                    headerTexts: "Reason Name",
                    isMultiselect: true,
                    allowPaging: false,
                    primaryKeyColumn: "ReasonID",
                    onMultiselect: function (selectedRecords) {
                        if (selectedRecords.length > 0) {
                            selectedRecords.forEach(function (value) {

                                value.YBookingNo = masterData.YBookingNo;

                            });
                            masterData.RevisionReasonList = selectedRecords;
                        }
                        else {
                            data.RevisionReasonList = [];
                        }
                        approveAddition(false, '', menuType, true);
                    }
                });
                finder.showModal();


            });
            $formEl.find("#btnRejectAdditionBBKI").click(function (e) {
                bootbox.prompt("Enter your Reject reason:", function (result) {
                    if (!result) {
                        return toastr.error("Reject reason is required.");
                    }
                    e.preventDefault();

                    approveAddition(true, result, menuType, false);
                });
            });

            $formEl.find("#btnCollarApplyKG").click(function (e) {

                var size = $(pageIdWithHash).find("#CollarSizeID").val();
                if (size == null || size.length == 0) {
                    toastr.error("Select size");
                    return false;
                }
                var weight = $(pageIdWithHash).find("#CollarWeightInGm").val();
                if (weight == null || weight == 0) {
                    toastr.error("Give consumption(gm)");
                    return false;
                }
                if ((menuType == _paramType.BulkBookingAck && status == statusConstants.PENDING)) {
                    SetCollarBookingWeightKG();
                }
                else {
                    SetCollarBookingWeightKGAfterSave();
                }

                if (menuType == _paramType.BulkBookingAck && status == statusConstants.PENDING) {
                    GetCalculatedFBookingChildCollor(masterData.Collars);
                } else {
                    GetCalculatedFBookingChildCollor(masterData.FBookingChildCollor);
                }
                $tblChildCollarIdEl.refresh();
                //initChildCollar(masterData.FBookingChildCollor);
            });
            $formEl.find("#btnCuffApplyKG").click(function (e) {

                var size = $(pageIdWithHash).find("#CuffSizeID").val();
                if (size == null || size.length == 0) {
                    toastr.error("Select size");
                    return false;
                }
                var weight = $(pageIdWithHash).find("#CuffWeightInGm").val();
                if (weight == null || weight == 0) {
                    toastr.error("Give consumption(gm)");
                    return false;
                }
                if (menuType == _paramType.BulkBookingAck && status == statusConstants.PENDING) {
                    SetCuffBookingWeightKG();
                }
                else {
                    SetCuffBookingWeightKGAfterSave();
                }
                if (menuType == _paramType.BulkBookingAck && status == statusConstants.PENDING) {
                    GetCalculatedFBookingChildCuff(masterData.Cuffs);
                } else {
                    GetCalculatedFBookingChildCuff(masterData.FBookingChildCuff);
                }
                $tblChildCuffIdEl.refresh();
            });

        }

        if (isBulkBookingKnittingInfoMenu()) {
            $toolbarEl.find("#btnAllBulkBookingList").click(function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                $formEl.find(".btnAction").hide();
                status = statusConstants.ALL;
                initBulkAckList(0);
            });

            $toolbarEl.find("#btnPendingExport").click(function (e) {
                isEnableSizeConsumption(false);
                _isPendingList = false;

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                $formEl.find(".btnAction").hide();
                status = statusConstants.PENDING_EXPORT_DATA;
                $formEl.find("#btnSaveAsDraft").hide();
                $formEl.find("#btnApproveByPMC").hide();
                $formEl.find("#btnRejectByPMCPopup,#btnInternalReviseByPMCPopup").hide();
                initBulkAckList(0);

            });

            $toolbarEl.find("#btnExport").click(function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                $formEl.find(".btnAction").hide();
                status = statusConstants.EXPORT_DATA;
                initBulkAckList(0);
            });
        }

        if (isYarnBookingAcknowledgeMenu()) {
            $formEl.find(".lblBookingNo").text("Fabric Booing No");
            $formEl.find(".lblSupplierName").text("FB Supplier Name");
            $formEl.find(".lblOrderQty").text("FB Order Qty");
            $formEl.find(".lblTeamLeader").text("YB Created By");
        }

        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        $formEl.find("#btnSaveAsDraft").click(function (e) {
            e.preventDefault();
            _saveType = "SaveAsDraft";
            save("", false, false, false, false, false, $(this).attr('id'));
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            _saveType = "Save";

            if (masterData.IsIncreaseRevisionNo == true) {
                var finder = new commonFinder({
                    title: "Select Revision Reason",
                    pageId: pageId,
                    apiEndPoint: `/api/bds-acknowledge/get-yarn-revision-reason`,
                    fields: "ReasonName",
                    headerTexts: "Reason Name",
                    isMultiselect: true,
                    allowPaging: false,
                    primaryKeyColumn: "ReasonID",
                    onMultiselect: function (selectedRecords) {
                        if (selectedRecords.length > 0) {
                            selectedRecords.forEach(function (value) {

                                value.YBookingNo = masterData.YBookingNo;

                            });
                            masterData.RevisionReasonList = selectedRecords;
                        }
                        else {
                            data.RevisionReasonList = [];
                        }
                        approveRejectOperationBBKI(false, false, true);
                        initMasterTable();
                    }
                });
                finder.showModal();
            }
            else {
                save("", false, false, false, false, false, $(this).attr('id'));
                initMasterTable();
            }
        });

        $formEl.find("#btnOk").click(function (e) {
            e.preventDefault();

            if (_modalFrom == subGroupNames.FABRIC) {
                setPlanningData(_oRowc, $tblCriteriaIdEl, _indexc);
            } else if (_modalFrom == subGroupNames.COLLAR) {
                setPlanningData(_oRowc, $tblCriteriaIdEl, _indexc);
            } else if (_modalFrom == subGroupNames.CUFF) {
                setPlanningData(_oRowc, $tblCriteriaIdEl, _indexc);
            }
        });
        //$formEl.find("#btnUPOk").click(function (e) {

        //    //masterData.Childs[selectedChildIndex].ChildItems = stockSummary;
        //    //var AdvStockAllocationQty = 0;
        //    //for (var i = 0; i < stockSummary.length; i++) {
        //    //    AdvStockAllocationQty = stockSummary[i].AdvanceAllocationQty + stockSummary[i].SampleAllocationQty + stockSummary[i].LiabilitiesAllocationQty + stockSummary[i].LeftoverAllocationQty;
        //    //}
        //    //masterData.Childs[selectedChildIndex].AdvanceStockAllocationQty = AdvStockAllocationQty;
        //    //masterData.Childs[selectedChildIndex].TotalAllocationQty = TotalAllocationQty(masterData.Childs[selectedChildIndex].AdvanceStockAllocationQty, masterData.Childs[selectedChildIndex].PipelineStockAllocationQty, masterData.Childs[selectedChildIndex].QtyForPO);
        //    //initChild(masterData.Childs);
        //    //stockSummary = [];

        //    $modalFFUtilizationInfoEl.modal('hide');

        //});
        $formEl.find("#btnGFUPOk").click(function (e) {

            _isFirstLoad = false;
            GFUtilizationSummary = $tblGFUtilizationEL.getCurrentViewRecords();

            var GreFabricUtilizationQty = 0;
            for (var i = 0; i < GFUtilizationSummary.length; i++) {
                GreFabricUtilizationQty += GFUtilizationSummary[i].GreyFabricUtilizationQTYinkg;
            }
            //var YBChildID = 0;
            //Imrez
            if (__SubGroupID == 1) {
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).GreyFabricUtilizationPopUpList = GFUtilizationSummary;
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).GreyLeftOverQty = GreFabricUtilizationQty;

                var list = getSelectedItems(__SubGroupID);
                initChild(list);
            }
            else if (__SubGroupID == 11) {
                masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).GreyFabricUtilizationPopUpList = GFUtilizationSummary;
                masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).GreyLeftOverQty = GreFabricUtilizationQty;

                var list = getSelectedItems(__SubGroupID);
                initChildCollar(list);
            }
            else if (__SubGroupID == 12) {
                masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).GreyFabricUtilizationPopUpList = GFUtilizationSummary;
                masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).GreyLeftOverQty = GreFabricUtilizationQty;

                var list = getSelectedItems(__SubGroupID);
                initChildCuff(list);
            }

            GFUtilizationSummary = [];
            $modalGFUtilizationInfoEL.modal('hide');

        });
        $formEl.find("#btnFFUtilizationOK").click(function (e) {
            _isFirstLoad = false;
            FinishFabricUtilizationDataList = $tblFFUtilizationEL.getCurrentViewRecords();

            var FinishFabricUtilizationQty = 0;
            for (var i = 0; i < FinishFabricUtilizationDataList.length; i++) {
                FinishFabricUtilizationQty += FinishFabricUtilizationDataList[i].FinishFabricUtilizationQTYinkg;
            }

            if (__SubGroupID == 1) {
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).FinishFabricUtilizationPopUpList = FinishFabricUtilizationDataList;
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).FinishFabricUtilizationQty = FinishFabricUtilizationQty;

                var list = getSelectedItems(__SubGroupID);
                initChild(list);
            }
            else if (__SubGroupID == 11) {
                masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).FinishFabricUtilizationPopUpList = FinishFabricUtilizationDataList;
                masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).FinishFabricUtilizationQty = FinishFabricUtilizationQty;

                var list = getSelectedItems(__SubGroupID);
                initChildCollar(list);
            }
            else if (__SubGroupID == 12) {
                masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).FinishFabricUtilizationPopUpList = FinishFabricUtilizationDataList;
                masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).FinishFabricUtilizationQty = FinishFabricUtilizationQty;

                var list = getSelectedItems(__SubGroupID);
                initChildCuff(list);
            }
            FinishFabricUtilizationDataList = [];

            $modalFFUtilizationInfoEL.modal('hide');

        });
        $formEl.find("#btnGreyYarnPOPUPOk").click(function (e) {

            _isFirstLoad = false;
            GreyYarnUtilizationDataList = $tblGeryYarnUtilizationEL.getCurrentViewRecords();

            var GreyYarnUtilizationQty = 0; var validGreyYarn = "N";
            for (var i = 0; i < GreyYarnUtilizationDataList.length; i++) {
                //var TotalUtilization = getDefaultValueWhenInvalidN_FloatWithFourDigit(GreyYarnUtilizationDataList[i].TotalUtilization);
                var TotalUtilization = getDefaultValueWhenInvalidN_FloatWithFourDigit(GreyYarnUtilizationDataList[i].UtilizationLeftoverStock) + getDefaultValueWhenInvalidN_FloatWithFourDigit(GreyYarnUtilizationDataList[i].UtilizationLiabilitiesStock) + getDefaultValueWhenInvalidN_FloatWithFourDigit(GreyYarnUtilizationDataList[i].UtilizationSampleStock) + getDefaultValueWhenInvalidN_FloatWithFourDigit(GreyYarnUtilizationDataList[i].UtilizationUnusableStock);
                GreyYarnUtilizationDataList[i].TotalUtilization = TotalUtilization;
                if (TotalUtilization <= 0) {

                    //GreyYarnUtilizationDataList.splice(i, 1);
                    validGreyYarn = "Y";

                }
                else
                    GreyYarnUtilizationQty += GreyYarnUtilizationDataList[i].TotalUtilization;
            }

            if (validGreyYarn == "Y") {
                //initGreyYarnUtilization(GreyYarnUtilizationDataList);
                //return toastr.error("Invalid: please enter atleast one Stock QTY");
            }
            var YBChildID = 0; var YBChildItemID = 0;

            if (__SubGroupID == 1) {
                var ChildItemList = masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID);

                YBChildID = ChildItemList.YBChildID;
                YBChildItemID = ChildItemList.YBChildItemID;
                for (var i = 0; i < GreyYarnUtilizationDataList.length; i++) {
                    GreyYarnUtilizationDataList[i].YBChildID = YBChildID;
                    GreyYarnUtilizationDataList[i].YBChildItemID = YBChildItemID;

                }
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).GreyYarnUtilizationPopUpList = GreyYarnUtilizationDataList;
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).GreyYarnUtilizationQty = GreyYarnUtilizationQty;
                if (isAdditionBulkBooking()) {
                    var list = getSelectedItems(__SubGroupID);
                    initChild(list);
                } else {
                    initChild(masterData.FBookingChild);
                }
            }
            else if (__SubGroupID == 11) {
                var ChildItemList = masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID);

                YBChildID = ChildItemList.YBChildID;
                YBChildItemID = ChildItemList.YBChildItemID;
                for (var i = 0; i < GreyYarnUtilizationDataList.length; i++) {
                    GreyYarnUtilizationDataList[i].YBChildID = YBChildID;
                    GreyYarnUtilizationDataList[i].YBChildItemID = YBChildItemID;
                }
                masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).GreyYarnUtilizationPopUpList = GreyYarnUtilizationDataList;
                masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).GreyYarnUtilizationQty = GreyYarnUtilizationQty;

                if (isAdditionBulkBooking()) {
                    var list = getSelectedItems(__SubGroupID);
                    initChildCollar(list);
                } else {
                    initChildCollar(masterData.FBookingChildCollor);
                }
            }
            else if (__SubGroupID == 12) {
                var ChildItemList = masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID);

                YBChildID = ChildItemList.YBChildID;
                YBChildItemID = ChildItemList.YBChildItemID;
                for (var i = 0; i < GreyYarnUtilizationDataList.length; i++) {
                    GreyYarnUtilizationDataList[i].YBChildID = YBChildID;
                    GreyYarnUtilizationDataList[i].YBChildItemID = YBChildItemID;

                }
                masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).GreyYarnUtilizationPopUpList = GreyYarnUtilizationDataList;
                masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).GreyYarnUtilizationQty = GreyYarnUtilizationQty;
                if (isAdditionBulkBooking()) {
                    var list = getSelectedItems(__SubGroupID);
                    initChildCuff(list);
                } else {
                    initChildCuff(masterData.FBookingChildCuff);
                }
            }
            GreyYarnUtilizationDataList = [];
            $modalGeryYarnUtilizationInfoEL.modal('hide');
        });
        $formEl.find("#btnGreyYarnPOPUPClose").click(function (e) {
            GreyYarnUtilizationDataList = $tblGeryYarnUtilizationEL.getCurrentViewRecords();
            var validGreyYarn = "N";
            for (var i = 0; i < GreyYarnUtilizationDataList.length; i++) {
                var TotalUtilization = getDefaultValueWhenInvalidN_FloatWithFourDigit(GreyYarnUtilizationDataList[i].TotalUtilization);
                if (TotalUtilization <= 0) {
                    GreyYarnUtilizationDataList.splice(i, 1);
                    validGreyYarn = "Y";

                }

            }

            if (validGreyYarn == "Y") {
                initGreyYarnUtilization(DeepClone(GreyYarnUtilizationDataList));
                //return toastr.error("Invalid: please enter atleast one Stock QTY");
            }

            GreyYarnUtilizationDataList = [];

            $modalGeryYarnUtilizationInfoEL.modal('hide');

        });
        $formEl.find("#btnDyedYarnPOPUPOk").click(function (e) {

            _isFirstLoad = false;
            DyedYarnUtilizationDataList = $tblDyedYarnUtilizationEL.getCurrentViewRecords();

            var DyedYarnUtilizationQty = 0; var validDyedYarn = "N";
            for (var i = 0; i < DyedYarnUtilizationDataList.length; i++) {
                var TotalUtilization = getDefaultValueWhenInvalidN_FloatWithFourDigit(DyedYarnUtilizationDataList[i].DyedYarnUtilizationQty);
                if (TotalUtilization <= 0) {

                    validDyedYarn = "Y";

                }
                else
                    DyedYarnUtilizationQty += TotalUtilization;
            }

            if (validDyedYarn == "Y") {
                return toastr.error("Invalid: please enter atleast one QTY");
            }
            var YBChildID = 0; var YBChildItemID = 0;

            if (__SubGroupID == 1) {
                var ChildItemList = masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID);

                YBChildID = ChildItemList.YBChildID;
                YBChildItemID = ChildItemList.YBChildItemID;
                for (var i = 0; i < DyedYarnUtilizationDataList.length; i++) {
                    DyedYarnUtilizationDataList[i].YBChildID = YBChildID;
                    DyedYarnUtilizationDataList[i].YBChildItemID = YBChildItemID;
                }
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).DyedYarnUtilizationPopUpList = DyedYarnUtilizationDataList;
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).DyedYarnUtilizationQty = DyedYarnUtilizationQty;

                if (isAdditionBulkBooking()) {
                    var list = getSelectedItems(__SubGroupID);
                    initChild(list);
                } else {
                    initChild(masterData.FBookingChild);
                }
            }
            else if (__SubGroupID == 11) {
                var ChildItemList = masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID);

                YBChildID = ChildItemList.YBChildID;
                YBChildItemID = ChildItemList.YBChildItemID;
                for (var i = 0; i < DyedYarnUtilizationDataList.length; i++) {
                    DyedYarnUtilizationDataList[i].YBChildID = YBChildID;
                    DyedYarnUtilizationDataList[i].YBChildItemID = YBChildItemID;
                }
                masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).DyedYarnUtilizationPopUpList = DyedYarnUtilizationDataList;
                masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).DyedYarnUtilizationQty = DyedYarnUtilizationQty;

                if (isAdditionBulkBooking()) {
                    var list = getSelectedItems(__SubGroupID);
                    initChildCollar(list);
                } else {
                    initChildCollar(masterData.FBookingChildCollor);
                }
            }
            else if (__SubGroupID == 12) {
                var ChildItemList = masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID);
                YBChildID = ChildItemList.YBChildID;
                YBChildItemID = ChildItemList.YBChildItemID;
                for (var i = 0; i < DyedYarnUtilizationDataList.length; i++) {
                    DyedYarnUtilizationDataList[i].YBChildID = YBChildID;
                    DyedYarnUtilizationDataList[i].YBChildItemID = YBChildItemID;
                }
                masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).DyedYarnUtilizationPopUpList = DyedYarnUtilizationDataList;
                masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).DyedYarnUtilizationQty = DyedYarnUtilizationQty;

                if (isAdditionBulkBooking()) {
                    var list = getSelectedItems(__SubGroupID);
                    initChildCuff(list);
                } else {
                    initChildCuff(masterData.FBookingChildCuff);
                }

            }
            DyedYarnUtilizationDataList = [];
            $modalDyedYarnUtilizationInfoEL.modal('hide');
        });
        $formEl.find("#btnDyedYarnPOPUPClose").click(function (e) {
            DyedYarnUtilizationDataList = $tblDyedYarnUtilizationEL.getCurrentViewRecords();
            var validDyedYarn = "N";
            for (var i = 0; i < DyedYarnUtilizationDataList.length; i++) {
                var TotalUtilization = getDefaultValueWhenInvalidN_FloatWithFourDigit(DyedYarnUtilizationDataList[i].DyedYarnUtilizationQty);
                if (TotalUtilization <= 0) {
                    DyedYarnUtilizationDataList.splice(i, 1);
                    validDyedYarn = "Y";
                }
            }

            if (validDyedYarn == "Y") {
                initDyedYarnUtilization(DeepClone(DyedYarnUtilizationDataList));
            }
            DyedYarnUtilizationDataList = [];
            $modalDyedYarnUtilizationInfoEL.modal('hide');
        });

        $formEl.find("#btnFBAckChildReplacementInfoPOPUPOk").click(function (e) {
            _isFirstLoad = false;

            AdditionalReplacementDataList = $tblFBAckChildReplacementInfoEL.getCurrentViewRecords();
            var ReplacementQty = 0; var IsValid = "N";
            for (var i = 0; i < AdditionalReplacementDataList.length; i++) {
                var TotalQTY = getDefaultValueWhenInvalidN_FloatWithFourDigit(AdditionalReplacementDataList[i].ReplacementQTY);
                if (TotalQTY <= 0) {
                    IsValid = "Y";
                }
                else {
                    ReplacementQty += TotalQTY;
                }
            }

            if (IsValid == "Y") {
                return toastr.error("Invalid: please enter atleast one QTY");
            }
            var BookingChildID = 0;

            if (__SubGroupID == 1) {
                var ChildList = masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID);

                BookingChildID = ChildList.BookingChildID;
                for (var i = 0; i < AdditionalReplacementDataList.length; i++) {
                    AdditionalReplacementDataList[i].BookingChildID = BookingChildID;

                }
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).AdditionalReplacementPOPUPList = AdditionalReplacementDataList;
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).BookingQty = ReplacementQty;
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).IsForFabric = true;

                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.forEach(z => {
                    z.BookingQty = (ReplacementQty / 100) * z.Distribution;
                    z.BookingQty = getDefaultValueWhenInvalidN_Float(z.BookingQty);
                    var bookingQty = masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).GreyProdQty;
                    z.RequiredQty = GetQtyFromPer(bookingQty, z.Distribution, z.Allowance)
                    z.RequiredQty = getDefaultValueWhenInvalidN_Float(z.RequiredQty);
                });
                var list = getSelectedItems(__SubGroupID);
                initChild(list);

            }
            else if (__SubGroupID == 11) {
                var ChildList = masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID);

                BookingChildID = ChildList.BookingChildID;
                for (var i = 0; i < AdditionalReplacementDataList.length; i++) {
                    AdditionalReplacementDataList[i].BookingChildID = BookingChildID;

                }
                var child = masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID);
                child.AdditionalReplacementPOPUPList = AdditionalReplacementDataList;
                child.BookingQty = ReplacementQty;
                child.IsForFabric = true;

                ///
                var Sizelist = masterData.AllCollarSizeList.filter(y => y.Construction == child.Construction && y.Composition == child.Composition && y.Color == child.Color);
                var BookingWeightGM = 0;
                var TotalBookingQty = 0;
                Sizelist.forEach(z => {
                    TotalBookingQty += z.BookingQty;
                });
                Sizelist.forEach(z => {
                    var percent = (z.BookingQty / TotalBookingQty) * 100;
                    z.BookingQty = (ReplacementQty / 100) * percent;
                    z.BookingQty = getDefaultValueWhenInvalidN_Float(z.BookingQty);
                });
                Sizelist.forEach(z => {
                    BookingWeightGM += getBookingQtyKG(z.Length, z.Width, z.BookingQty, 11);
                });
                child.BookingQtyKG = getDefaultValueWhenInvalidN_Float(BookingWeightGM);
                child = setBookingQtyKGRelatedFieldsValue(child, 11);
                ///

                var list = getSelectedItems(__SubGroupID);
                initChildCollar(list);

            }
            else if (__SubGroupID == 12) {
                var ChildList = masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID);

                BookingChildID = ChildList.BookingChildID;
                for (var i = 0; i < AdditionalReplacementDataList.length; i++) {
                    AdditionalReplacementDataList[i].BookingChildID = BookingChildID;

                }
                var child = masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID);
                child.AdditionalReplacementPOPUPList = AdditionalReplacementDataList;
                child.BookingQty = ReplacementQty;
                child.IsForFabric = true;

                ///
                var Sizelist = masterData.AllCuffSizeList.filter(y => y.Construction == child.Construction && y.Composition == child.Composition && y.Color == child.Color);
                var BookingWeightGM = 0;
                var TotalBookingQty = 0;
                Sizelist.forEach(z => {
                    TotalBookingQty += z.BookingQty;
                });
                Sizelist.forEach(z => {
                    var percent = (z.BookingQty / TotalBookingQty) * 100;
                    z.BookingQty = (ReplacementQty / 100) * percent;
                    z.BookingQty = getDefaultValueWhenInvalidN_Float(z.BookingQty);
                });
                Sizelist.forEach(z => {
                    BookingWeightGM += getBookingQtyKG(z.Length, z.Width, z.BookingQty, 12);
                });
                child.BookingQtyKG = getDefaultValueWhenInvalidN_Float(BookingWeightGM);
                child = setBookingQtyKGRelatedFieldsValue(child, 12);
                ///

                var list = getSelectedItems(__SubGroupID);
                initChildCuff(list);

            }
            AdditionalReplacementDataList = [];
            $modalFBAckChildReplacementInfoEL.modal('hide');
        });

        $formEl.find("#btnLoadFBAckChildReplacementInfo").click(function (e) {
            AdditionalReplacementDataList = $tblFBAckChildReplacementInfoEL.getCurrentViewRecords();

            var ReplacementPOPUPList =
            {
                Reason: "",
                Department: "",
                ReplacementQTY: "0",
                Remarks: ""

            };
            if (AdditionalReplacementDataList == null) {
                AdditionalReplacementDataList = [];
            }
            AdditionalReplacementDataList.push(ReplacementPOPUPList);

            initAdditioalReplacementQTY(AdditionalReplacementDataList);


        });
        $formEl.find("#btnLoadFBAckYarnNetYarnReqQtyInfo").click(function (e) {
            AdditionalNetReqDataList = $tblFBAckYarnNetYarnReqQtyInfoEL.getCurrentViewRecords();
            var ReplacementPOPUPList =
            {
                Reason: "",
                Department: "",
                ReplacementQTY: "0",
                Remarks: ""

            };
            if (AdditionalNetReqDataList == null) {
                AdditionalNetReqDataList = [];
            }
            AdditionalNetReqDataList.push(ReplacementPOPUPList);
            initAdditioalNetReqQTY(AdditionalNetReqDataList);


        });
        $formEl.find("#btnFBAckYarnNetYarnReqQtyInfoPOPUPOk").click(function (e) {
            _isFirstLoad = false;

            //Imrez
            AdditionalNetReqDataList = $tblFBAckYarnNetYarnReqQtyInfoEL.getCurrentViewRecords();
            var ReplacementQty = 0; var IsValid = "N";
            for (var i = 0; i < AdditionalNetReqDataList.length; i++) {
                var TotalQTY = getDefaultValueWhenInvalidN_FloatWithFourDigit(AdditionalNetReqDataList[i].ReplacementQTY);
                if (TotalQTY <= 0) {
                    IsValid = "Y";
                }
                else {
                    ReplacementQty += TotalQTY;
                }
            }

            if (IsValid == "Y") {
                return toastr.error("Invalid: please enter atleast one QTY");
            }
            var YBChildID = 0;
            var YBChildItemID = 0;

            if (__SubGroupID == 1) {
                var ChildItemList = masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID);

                YBChildID = ChildItemList.YBChildID;
                YBChildItemID = ChildItemList.YBChildItemID;
                for (var i = 0; i < AdditionalNetReqDataList.length; i++) {
                    AdditionalNetReqDataList[i].YBChildID = YBChildID;
                    AdditionalNetReqDataList[i].YBChildItemID = YBChildItemID;

                }
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).AdditionalNetReqPOPUPList = AdditionalNetReqDataList;
                masterData.FBookingChild.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).NetYarnReqQty = ReplacementQty;

                var list = getSelectedItems(__SubGroupID);
                initChild(list, true);

            }
            else if (__SubGroupID == 11) {

                var ChildItemList = masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID);

                YBChildID = ChildItemList.YBChildID;
                YBChildItemID = ChildItemList.YBChildItemID;
                for (var i = 0; i < AdditionalNetReqDataList.length; i++) {
                    AdditionalNetReqDataList[i].YBChildID = YBChildID;
                    AdditionalNetReqDataList[i].YBChildItemID = YBChildItemID;
                }
                masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).AdditionalNetReqPOPUPList = AdditionalNetReqDataList;
                masterData.FBookingChildCollor.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).NetYarnReqQty = ReplacementQty;

                var list = getSelectedItems(__SubGroupID);
                initChildCollar(list, true);

            }
            else if (__SubGroupID == 12) {
                var ChildItemList = masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID);

                YBChildID = ChildItemList.YBChildID;
                YBChildItemID = ChildItemList.YBChildItemID;
                for (var i = 0; i < AdditionalNetReqDataList.length; i++) {
                    AdditionalNetReqDataList[i].YBChildID = YBChildID;
                    AdditionalNetReqDataList[i].YBChildItemID = YBChildItemID;
                }
                masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).AdditionalNetReqPOPUPList = AdditionalNetReqDataList;
                masterData.FBookingChildCuff.find(x => x.BookingChildID == selectedBookingChildID).ChildItems.find(x => x.YBChildItemID == selectedYBChildItemID).NetYarnReqQty = ReplacementQty;

                var list = getSelectedItems(__SubGroupID);
                initChildCuff(list, true);
            }
            AdditionalNetReqDataList = [];
            $modalFBAckYarnNetYarnReqQtyInfoEL.modal('hide');
        });
        $formEl.find("#btnOkk").click(function (e) {
            e.preventDefault();
            var selectedRows = $tblCriteriaIdEl.getCurrentViewRecords();
            for (var i = 0; i < selectedRows.length; i++) {
                if ((selectedRows[i].CriteriaName == "Material" || selectedRows[i].CriteriaName == "Dyeing" || selectedRows[i].CriteriaName == "Finishing" || selectedRows[i].CriteriaName == "Testing") && selectedRows[i].TotalTime == 0) {
                    toastr.error("Please enter criteria for Material, Dyeing, Finishing, Testing!");
                    return;
                }
            }

            if (_modalFrom == subGroupNames.FABRIC) {
                setPlanningCriteriaData(_oRow, $tblChildEl, _index);
            } else if (_modalFrom == subGroupNames.COLLAR) {
                setPlanningCriteriaData(_oRowCollar, $tblChildCollarIdEl, _indexCollar);
            } else if (_modalFrom == subGroupNames.CUFF) {
                setPlanningCriteriaData(_oRowCuff, $tblChildCuffIdEl, _indexCuff);
            }
        });

        $pageEl.find("#btnAddComposition").click(saveComposition);

        initCommonFinderFP();

        changeButtonName();

        //GreyQCDefectHKs
        axios.get(`/api/fabric-con-sub-class-tech-name/list`)
            .then(function (response) {
                bmtArray = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });

        if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
            getYarnSegments();
            getYarnSegmentsMapping();
        }
    });
    function setBulkBookingAckPage(menuParam) {
        menuType = 0;

        if (menuParam) {
            switch (menuParam) {
                case "BulkBookingAck":
                    menuType = 1;
                    break;
                case "Projection":
                    menuType = 2;
                    break;
                case "BulkBookingCheck":
                    menuType = 3;
                    break;
                case "BulkBookingApprove":
                    menuType = 4;
                    break;
                case "BulkBookingFinalApprove":
                    menuType = 5;
                    break;
                case "BulkBookingYarnAllowance":
                    menuType = 6;
                    break;
                case "LabdipBookingAcknowledge":
                    menuType = 7;
                    break;
                case "LabdipBookingAcknowledgeRnD":
                    menuType = 8;
                    break;
                case "AdditionalYarnBooking":
                    menuType = 9;
                    break;
                case "AYBQtyFinalizationPMC":
                    menuType = 10;
                    break;
                case "AYBProdHeadApproval":
                    menuType = 11;
                    break;
                case "AYBTextileHeadApproval":
                    menuType = 12;
                    break;
                case "AYBKnittingUtilization":
                    menuType = 13;
                    break;
                case "AYBKnittingHeadApproval":
                    menuType = 14;
                    break;
                case "AYBOperationHeadApproval":
                    menuType = 15;
                    break;
                case "BulkBookingUtilizationProposal":
                    menuType = 16;
                    break;
                case "BulkBookingUtilizationConfirmation":
                    menuType = 17;
                    break;
                case "YarnBookingAcknowledge":
                    menuType = 18;
                    break;
                default:
                    menuType = 0;
                    break;
            }
        }

        return menuType;
    }
    async function getYarnSegments() {
        var response = await axios.get(getYarnItemsApiUrl([]));
        _yarnSegments = response.data;
    }
    async function getYarnSegmentsMapping() {
        var response = await axios.get("/api/items/yarn/item-segments-mapping");
        _yarnSegmentsMapping = response.data;
    }
    function showAddComposition() {
        initTblCreateComposition();
        $pageEl.find(`#modal-new-composition-${pageId}`).modal("show");
    }
    var subProgramElem, certificationElem, fiberElem;
    var subProgramObj, certificationObj, fiberObj;
    function initTblCreateComposition() {
        var YarnSubProgramNewsFilteredList = [];//masterData.YarnSubProgramNews;
        var CertificationsFilteredList = [];//masterData.Certifications;
        compositionComponents = [];
        var columns = [
            {
                field: 'Id', isPrimaryKey: true, visible: false
            },
            {
                headerText: '', width: 70, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            {
                field: 'Percent', headerText: 'Percent(%)', width: 120, editType: "numericedit", params: { decimals: 0, format: "N", min: 1, validateDecimalOnType: true }, allowEditing: isBlended
            },
            //{
            //    field: 'Fiber', headerText: 'Component', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.FabricComponents, field: "Fiber" })
            //}
            {
                field: 'Fiber', headerText: 'Fiber', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        fiberElem = document.createElement('input');
                        return fiberElem;
                    },
                    read: function () {
                        return fiberObj.text;
                    },
                    destroy: function () {
                        fiberObj.destroy();
                    },
                    write: function (e) {
                        fiberObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.FabricComponentsNew,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Component',
                            floatLabelType: 'Never',
                            change: function (f) {

                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.Fiber = f.itemData.id;
                                e.rowData.Fiber = f.itemData.text;

                                YarnSubProgramNewsFilteredList = masterData.YarnSubProgramNews.filter(y => y.additionalValue == f.itemData.id);
                                subProgramObj.dataSource = YarnSubProgramNewsFilteredList;
                                subProgramObj.dataBind();

                                certificationObj.dataSource = [];
                                certificationObj.dataBind();

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        fiberObj.appendTo(fiberElem);

                    }
                }
            },
            //{
            //    field: 'YarnSubProgramNew', headerText: 'Yarn Sub Program New', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.YarnSubProgramNews, field: "YarnSubProgramNew" })
            //},
            {
                field: 'YarnSubProgramNew', headerText: 'Yarn Sub Program New', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        subProgramElem = document.createElement('input');
                        return subProgramElem;
                    },
                    read: function () {
                        return subProgramObj.text;
                    },
                    destroy: function () {
                        subProgramObj.destroy();
                    },
                    write: function (e) {
                        subProgramObj = new ej.dropdowns.DropDownList({
                            dataSource: YarnSubProgramNewsFilteredList,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Yarn Sub Program',
                            floatLabelType: 'Never',
                            change: function (f) {

                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.YarnSubProgramNew = f.itemData.id;
                                e.rowData.YarnSubProgramNew = f.itemData.text;

                                //CertificationsFilteredList = masterData.Certifications.filter(y => y.additionalValue == f.itemData.id);
                                CertificationsFilteredList = masterData.Certifications.filter(y => y.additionalValue == f.itemData.id && y.additionalValue2 == f.itemData.additionalValue);
                                certificationObj.dataSource = CertificationsFilteredList;
                                certificationObj.dataBind();

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        subProgramObj.appendTo(subProgramElem);
                    }
                }
            },
            //{
            //    field: 'Certification', headerText: 'Certification', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.Certifications, field: "Certification" })
            //},
            {
                field: 'Certification', headerText: 'Certification', valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        certificationElem = document.createElement('input');
                        return certificationElem;
                    },
                    read: function () {
                        return certificationObj.text;
                    },
                    destroy: function () {
                        certificationObj.destroy();
                    },
                    write: function (e) {
                        certificationObj = new ej.dropdowns.DropDownList({
                            dataSource: CertificationsFilteredList,
                            fields: { value: 'id', text: 'text' },
                            //enabled: false,
                            placeholder: 'Select Certification',
                            floatLabelType: 'Never',
                            change: function (f) {

                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.Certification = f.itemData.id;
                                e.rowData.Certification = f.itemData.text;

                                $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                            }
                        });
                        certificationObj.appendTo(certificationElem);
                    }
                }
            }

        ];

        var gridOptions = {
            tableId: tblCreateCompositionId,
            data: compositionComponents,
            columns: columns,
            actionBegin: function (args) {

                if (args.requestType === "add") {
                    if (isBlended) {
                        if (compositionComponents.length === 5) {
                            toastr.info("You can only add 5 components.");
                            args.cancel = true;
                            return;
                        }
                    }
                    else {
                        if (compositionComponents.length === 1) {
                            toastr.info("You can only add 1 component.");
                            args.cancel = true;
                            return;
                        }
                        else args.data.Percent = 100;
                    }

                    args.data.Id = getMaxIdForArray(compositionComponents, "Id");
                }
                else if (args.requestType === "save") {

                    var fiberID = 0;
                    var subProgramID = 0;
                    var certificationsID = 0;
                    if (typeof args.rowData.Fiber != 'undefined') {
                        fiberID = masterData.FabricComponentsNew.find(y => y.text == args.rowData.Fiber).id;
                    }
                    if (typeof args.rowData.YarnSubProgramNew != 'undefined') {
                        subProgramID = masterData.YarnSubProgramNews.find(y => y.text == args.rowData.YarnSubProgramNew).id;
                    }
                    if (typeof args.rowData.Certification != 'undefined') {
                        certificationsID = masterData.Certifications.find(y => y.text == args.rowData.Certification).id;
                    }

                    var cnt = masterData.FabricComponentMappingSetupList.filter(y => y.FiberID == fiberID && y.SubProgramID == subProgramID && y.CertificationsID == certificationsID);
                    if (cnt == 0) {
                        if (fiberID == 0) {
                            toastr.warning("Fiber is required.");
                            args.cancel = true;
                            return;
                        }
                        if (subProgramID == 0) {
                            toastr.warning("Sub Program is required.");
                            args.cancel = true;
                            return;
                        }
                        if (certificationsID == 0) {
                            toastr.warning("certifications is required.");
                            args.cancel = true;
                            return;
                        }
                    }

                    if (args.action === "edit") {

                        if (!args.data.Fiber) {
                            toastr.warning("Fabric component is required.");
                            args.cancel = true;
                            return;
                        }
                        else if (!args.data.Percent || args.data.Percent <= 0 || args.data.Percent > 100) {
                            toastr.warning("Composition percent must be greater than 0 and less than or equal 100.");
                            args.cancel = true;
                            return;
                        }
                    }
                }
            },
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
        };

        if ($tblCreateCompositionEl) $tblCreateCompositionEl.destroy();
        $tblCreateCompositionEl = new initEJ2Grid(gridOptions);
    }
    function saveComposition() {

        var totalPercent = sumOfArrayItem(compositionComponents, "Percent");
        if (totalPercent != 100) return toastr.error("Sum of compostion percent must be 100");
        compositionComponents.reverse();

        var composition = "";
        compositionComponents = _.sortBy(compositionComponents, "Percent").reverse();
        compositionComponents.forEach(function (component) {

            composition += composition ? ` ${component.Percent}%` : `${component.Percent}%`;
            if (component.YarnSubProgramNew) {
                if (component.YarnSubProgramNew != 'N/A') {
                    composition += ` ${component.YarnSubProgramNew}`;
                }
            }
            //if (component.Certification) composition += ` ${component.Certification}`;
            if (component.Certification) {
                if (component.Certification != 'N/A') {
                    composition += ` ${component.Certification}`;
                }
            }
            composition += ` ${component.Fiber}`;
        });

        var data = {
            SegmentValue: composition
        };
        axios.post("/api/rnd-free-concept-mr/save-yarn-composition", data)
            .then(function () {
                $pageEl.find(`#modal-new-composition-${pageId}`).modal("hide");
                toastr.success("Composition added successfully.");
                //masterData.CompositionList.unshift({ id: response.data.Id, text: response.data.SegmentValue });
                // initChildTable(masterData.Childs);
            })
            .catch(showResponseError)
    }
    function initCommonFinderFP() {
        if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
            $(pageIdWithHash).find("#btnAddPreProcessFP").click(function () {
                var processType = 'Pre Set';
                var finder = new commonFinder({
                    title: "Select Process",
                    pageId: pageId,
                    apiEndPoint: `/api/finishing-process/get-finishing-process/${processType}`,
                    fields: "ProcessName,ProcessType,MachineName",
                    headerTexts: "Process Name,Process Type,Machine Name",
                    isMultiselect: true,
                    allowPaging: false,
                    primaryKeyColumn: "ProcessID",
                    onMultiselect: function (selectedRecords) {

                        var preDyingProcesses = $tblChildElFP.getCurrentViewRecords();
                        var list = [];
                        for (var i = 0; i < selectedRecords.length; i++) {
                            var oPreProcess = {
                                FPChildID: getMaxIdForArray(preDyingProcesses, "FPChildID"),
                                FPMasterID: 0,
                                ProcessID: selectedRecords[i].ProcessID,
                                ProcessTypeID: selectedRecords[i].ProcessTypeID,
                                ProcessName: selectedRecords[i].ProcessName,
                                ProcessType: selectedRecords[i].ProcessType,
                                MachineName: selectedRecords[i].MachineName,
                                FMCMasterID: selectedRecords[i].FMCMasterID,
                                ColorID: _fpBookingChildColorID,
                                IsPreProcess: true,
                                MachineNo: "",
                                UnitName: "",
                                BrandName: "",
                                Remarks: "",
                                PreFinishingProcessChildItems: []
                            }

                            var indexF = -1;
                            if (preDyingProcesses.length > 0) {
                                indexF = preDyingProcesses.findIndex(y => y.ProcessName == oPreProcess.ProcessName && y.ProcessType == oPreProcess.ProcessType && y.MachineName == oPreProcess.MachineName);
                            }
                            if (indexF == -1) {
                                preDyingProcesses.push(oPreProcess);
                            }
                        }
                        initChildTableFP(preDyingProcesses, true);
                    }
                });
                finder.showModal();
            });
            $(pageIdWithHash).find("#btnModalAddFP").click(function () {

                $(pageIdWithHash).find('#divModalFP').modal('hide');
                var indexF = -1;
                if (_fpRow.SubGroupId == 1 || _fpRow.SubGroupID == 1) {
                    indexF = masterData.Childs.findIndex(x => x.BookingChildID == _fpBookingChildID);
                }
                else {
                    indexF = masterData.Childs.findIndex(x => x.Construction == _fpRow.Construction && x.Composition == _fpRow.Composition && x.Color == _fpRow.Color);
                }
                if (indexF > -1) {
                    //_fbChildItemID => Missing
                    masterData.Childs[indexF].PreFinishingProcessChilds = $tblChildElFP.getCurrentViewRecords();
                    masterData.Childs[indexF].PreFinishingProcessChilds.map(x => {
                        x.FPChildID = _fbChildID++;
                        x.BookingChildID = _fpBookingChildID;
                        x.FinishingProcessChildItems = [];
                    });
                    masterData.Childs[indexF].PostFinishingProcessChilds = $tblColorChildElFP.getCurrentViewRecords();
                    masterData.Childs[indexF].PostFinishingProcessChilds.map(x => {
                        if (x.FMSID == null) x.FMSID = 0;
                        x.FPChildID = _fbChildID++;
                        x.BookingChildID = _fpBookingChildID;
                        x.FinishingProcessChildItems = [];
                    });
                }
            });
            $(pageIdWithHash).find("#btnModalCloseFP").click(function () {
                _fpBookingChildID = 0;
                _fpBookingChildColorID = 0;
                $(pageIdWithHash).find('#divModalFP').modal('hide');
            });
            initChildTableFP([], false);
            initChildTableColorFP([], false);
        }
    }
    function isYarnBookingAckMenu(currentStatus) {
        if (menuType == _paramType.YarnBookingAcknowledge) {
            if (currentStatus == null || typeof currentStatus === "undefined") return true;
            if (status == statusConstants[currentStatus]) return true;
        }
        return false;
    }
    function isBulkBookingKnittingInfoMenu() {
        if (menuType == _paramType.BulkBookingAck ||
            menuType == _paramType.BulkBookingUtilizationProposal ||
            menuType == _paramType.BulkBookingUtilizationConfirmation ||
            menuType == _paramType.BulkBookingCheck ||
            menuType == _paramType.BulkBookingApprove ||
            menuType == _paramType.BulkBookingFinalApprove ||
            menuType == _paramType.BulkBookingYarnAllowance ||
            menuType == _paramType.YarnBookingAcknowledge) return true;
        return false;
    }
    function isOnlyBulkBookingKnittingInfoMenu() {
        if (menuType == _paramType.BulkBookingAck ||
            menuType == _paramType.BulkBookingUtilizationProposal ||
            menuType == _paramType.BulkBookingUtilizationConfirmation ||
            menuType == _paramType.BulkBookingCheck ||
            menuType == _paramType.BulkBookingApprove ||
            menuType == _paramType.BulkBookingFinalApprove ||
            menuType == _paramType.BulkBookingYarnAllowance) return true;
        return false;
    }
    function isBulkBookingKnittingInfoRevisionMenu() {

        if ((menuType == _paramType.BulkBookingAck && status == statusConstants.INTERNAL_REJECTION) ||
            menuType == _paramType.BulkBookingUtilizationProposal ||
            menuType == _paramType.BulkBookingUtilizationConfirmation ||
            menuType == _paramType.BulkBookingCheck ||
            menuType == _paramType.BulkBookingApprove ||
            menuType == _paramType.BulkBookingFinalApprove ||
            menuType == _paramType.BulkBookingYarnAllowance) return true;
        return false;
    }
    function isYarnBookingAcknowledgeMenu() {
        if (menuType == _paramType.YarnBookingAcknowledge) return true;
        return false;
    }
    function isLabdipMenu() {
        if (menuType == _paramType.LabdipBookingAcknowledge ||
            menuType == _paramType.LabdipBookingAcknowledgeRnD) return true;
        return false;
    }

    function setPlanningData(oRowc, $tblElq, ind) {
        var selectedRows = $tblPlanningEl.getSelectedRecords();

        if (selectedRows.length == 0 && (oRowc.CriteriaName == "Material" || oRowc.CriteriaName == "Dyeing" || oRowc.CriteriaName == "Finishing" || oRowc.CriteriaName == "Testing")) {
            toastr.warning("Please select item(s)!");
            return;
        }
        ids = selectedRows.map(function (el) { return el.CriteriaID }).toString();

        var indexF = idsList.findIndex(x => x.RowIndex == _index
            && x.CriteriaIndex == ind
            && x.SubGroupName == _modalFrom
            && x.CriteriaName == oRowc.CriteriaName);

        if (indexF > -1) {
            idsList[indexF].CriteriaIDs = ids;
        }
        else {

            var subGroupWiseIndex = 0;
            if (_modalFrom == "Fabric") subGroupWiseIndex = _index;
            else if (_modalFrom == "Collar") subGroupWiseIndex = _indexCollar;
            else if (_modalFrom == "Cuff") subGroupWiseIndex = _indexCuff;

            idsList.push({
                RowIndex: _index,
                CriteriaIndex: ind,
                SubGroupWiseIndex: subGroupWiseIndex,
                SubGroupName: _modalFrom,
                CriteriaName: oRowc.CriteriaName,
                CriteriaIDs: ids
            });
        }

        //idsList.push(oRowc.CriteriaIDs);
        //CriteriaIDs


        var cids = idsList.filter(x => x.RowIndex == _index
            && x.SubGroupName == _modalFrom)
            .filter(x => x.CriteriaIDs.length > 0)
            .map(x => x.CriteriaIDs).join(",");

        oRowc.CriteriaIDs = cids;

        var TotalTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        var FinishingTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        var DyeingTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        var KnittingTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        var MaterialTime = 0;
        var TestReportDaysTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        var PreprocessTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        var batchPreparationTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        var TestingTime = oRowc.TechnicalTime + (oRowc.IsSubContact ? 14 : 0);
        selectedRows.forEach(function (row) {
            //console.log(row);
            if (row.CriteriaName == "Dyeing") {
                DyeingTime += row.ProcessTime;
            }
            else if (row.CriteriaName == "Finishing") {
                FinishingTime += row.ProcessTime;
            }
            else if (row.CriteriaName == "Material") {
                MaterialTime += row.ProcessTime;
            }
            else if (row.CriteriaName == "Preprocess") {
                PreprocessTime += row.ProcessTime;
            }
            else if (row.CriteriaName == "Testing") {
                TestingTime += row.ProcessTime;
            }
            TotalTime += row.ProcessTime;
        });
        //selectedRows.CriteriaName(function (row) {
        //    console.log(row);
        //    TotalTime += row.ProcessTime;
        //});
        oRowc.TotalTime = TotalTime;
        oRowc.FinishingTime = parseInt(FinishingTime + MaterialTime + oRowc.StructureDays + PreprocessTime + DyeingTime);
        oRowc.DyeingTime = parseInt(DyeingTime + MaterialTime + oRowc.StructureDays + PreprocessTime);
        oRowc.batchPreparationTime = parseInt(MaterialTime + oRowc.StructureDays + PreprocessTime);
        //oRowc.KnittingDays = TotalTime;
        oRowc.MaterialTime = MaterialTime;
        oRowc.PreprocessTime = PreprocessTime;
        oRowc.TestingTime = TestingTime;
        oRowc.KnittingTime = parseInt(MaterialTime + oRowc.StructureDays);
        //alert(oRowc.FinishingTime);
        //alert(oRowc.StructureDays);
        oRowc.TestReportTime = parseInt(oRowc.StructureDays + MaterialTime + PreprocessTime + DyeingTime + FinishingTime + TestingTime);
        //alert(oRowc.TestReportTime);
        $tblElq.updateRow(ind, oRowc);
        $modalPlanningEl.modal('toggle');
    }

    function setPlanningCriteriaData(oRow, $tblEl, ind) {

        //ids = idsList.filter(x => x.CriteriaIndex == ind &&
        //    x.RowIndex == _index &&
        //    x.SubGroupName == _modalFrom).map(x => x.CriteriaIDs).join(",");

        ids = idsList.filter(x => x.RowIndex == _index
            && x.SubGroupName == _modalFrom)
            .filter(x => x.CriteriaIDs.length > 0)
            .map(x => x.CriteriaIDs).join(",");

        /*
        ids = idsList.join(",");
        var selectedRows = $tblCriteriaIdEl.getCurrentViewRecords();
        */
        oRow.CriteriaIDs = ids;
        oRow.TechnicalTime = setTechnicalTime(oRow);
        oRow = setArgDataValues(oRow);

        $tblEl.updateRow(ind, oRow);
        $modalCriteriaEl.modal('toggle');
    }

    function initBulkAckList(paramTypeId) {

        var commandList = [],
            widthValue = 20;
        if ((status == statusConstants.PENDING || status == statusConstants.REJECT) && !isAdditionBulkBooking() && !isYarnBookingAckMenu("PENDING")) {
            widthValue = 20;
            commandList = [
                { type: 'AddBulk', title: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } },
                { type: 'Bulk Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Tech Pack Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } },
                { type: 'Booking Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
            ];
        }
        else if (status == statusConstants.DRAFT || status == statusConstants.ACTIVE || status == statusConstants.CHECK || status == statusConstants.APPROVED_DONE || status == statusConstants.CHECK_REJECT || status == statusConstants.REJECT_REVIEW || status == statusConstants.APPROVED_Allowance || status == statusConstants.REJECT_Allowance || status == statusConstants.APPROVED_PMC || status == statusConstants.REJECT_PMC || status == statusConstants.UN_ACKNOWLEDGE || status == statusConstants.APPROVED2 || status == statusConstants.ADDITIONAL || status == statusConstants.PENDING_CONFIRMATION || status == statusConstants.CONFIRM || status == statusConstants.PROPOSED_FOR_APPROVAL || status == statusConstants.INTERNAL_REJECTION) {
            widthValue = 50;
            commandList = [
                { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                { type: 'Bulk Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Bulk Booking Yarn Info', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Tech Pack Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } },
                { type: 'Booking Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
            ];
        }
        else if (menuType == _paramType.YarnBookingAcknowledge) {
            widthValue = 50;
            commandList = [
                { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                { type: 'Bulk Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Bulk Booking Yarn Info', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Yarn Booking Summary Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ];
        }
        else if (status == statusConstants.ALL) {
            widthValue = 50;
            commandList = [
                { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                { type: 'Bulk Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Bulk Booking Yarn Info', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Tech Pack Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } },
                { type: 'Booking Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
            ];
        }
        else if (status == statusConstants.PENDING_EXPORT_DATA || status == statusConstants.EXPORT_DATA) {
            widthValue = 50;
            commandList = [
                { type: 'Export', title: 'Export', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-arrow-circle-down' } },
            ];

        }
        else if (isAdditionBulkBooking()) {
            widthValue = 50;
            commandList = [
                { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                { type: 'Bulk Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Bulk Booking Yarn Info', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Tech Pack Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } },
                { type: 'Booking Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
            ];
        }
        var columns = [
            {
                headerText: 'Command', width: widthValue, textAlign: 'center', commands: commandList
            },
            //{
            //    field: 'BBStatus', headerText: 'Status', width: 60, visible: menuType == _paramType.BulkBookingFinalApprove && status == statusConstants.APPROVED_DONE
            //},
            {
                field: 'BookingType', headerText: 'Booking Type', width: 40, visible: isYarnBookingAckMenu("Pending") || ((menuType == _paramType.BulkBookingFinalApprove && (status == statusConstants.APPROVED_DONE || status == statusConstants.APPROVED_PMC || status == statusConstants.REJECT_PMC || status == statusConstants.ALL || status == statusConstants.PENDING_EXPORT_DATA || status == statusConstants.EXPORT_DATA)))
            },
            {
                field: 'StatusText', headerText: 'Status', width: 80, visible: (isBulkBookingKnittingInfoMenu() && status == statusConstants.ALL) || status == statusConstants.ALL_STATUS
            },
            {
                field: 'Ageing', headerText: 'Ageing (hh:mm:ss)', width: 40, textAlign: 'center', visible: status == statusConstants.ALL
            },
            //{
            //    field: 'RejectReason', headerText: 'Reason', width: 60, visible: menuType == _paramType.BulkBookingFinalApprove && status == statusConstants.APPROVED_DONE
            //},
            //{
            //    field: 'StatusText', headerText: 'Booking Status', width: 30, visible: (menuType == _paramType.BulkBookingAck && status == statusConstants.DRAFT) || _isPendingList
            //},
            {
                field: 'BookingStatus', headerText: 'Fabric Booking Status', width: 40, visible: (menuType == _paramType.BulkBookingAck && (status == statusConstants.PENDING || status == statusConstants.DRAFT || status == statusConstants.ACTIVE || status == statusConstants.UN_ACKNOWLEDGE || status == statusConstants.INTERNAL_REJECTION || status == statusConstants.APPROVED_PMC || status == statusConstants.PENDING_EXPORT_DATA || status == statusConstants.EXPORT_DATA)) || (menuType == _paramType.BulkBookingYarnAllowance && (status == statusConstants.ACTIVE)) || (menuType == _paramType.BulkBookingUtilizationProposal && (status == statusConstants.APPROVED_Allowance)) || (menuType == _paramType.BulkBookingUtilizationConfirmation && (status == statusConstants.PENDING_CONFIRMATION || status == statusConstants.CONFIRM))
            },
            //{
            //    field: 'YarnBookingStatus', headerText: 'Yarn Booking Status', width: 30, visible: (menuType == _paramType.BulkBookingAck && (status == statusConstants.ACTIVE))
            //},
            {
                field: 'BookingNo', headerText: 'Fabric Booking No', width: 60, visible: menuType != _paramType.YarnBookingAcknowledge
            },
            {
                field: 'YBookingNo', headerText: 'Yarn Booking No', width: 130, visible: menuType == _paramType.YarnBookingAcknowledge
            },
            {
                field: 'SLNo', headerText: 'SL No', visible: false
            },
            {
                field: 'BookingDate', headerText: 'Booking Date', type: 'date', format: _ch_date_format_1, width: 40, visible: menuType != _paramType.YarnBookingAcknowledge
            },
            {
                field: 'AddYarnBookingDate', headerText: 'Add. Yarn Booking Date', type: 'date', format: _ch_date_format_1, width: 40, visible: isAdditionBulkBooking() && status != statusConstants.APPROVED2
            },
            {
                field: 'YarnBookingDate', headerText: 'Yarn Booking Date', type: 'date', format: _ch_date_format_1, width: 130, visible: menuType == _paramType.YarnBookingAcknowledge
            },
            {
                field: 'RevisionDate', headerText: 'Booking Last Revise Date', type: 'date', format: _ch_date_format_4, visible: (menuType == _paramType.BulkBookingAck && (status == statusConstants.PENDING || status == statusConstants.DRAFT || status == statusConstants.ACTIVE || status == statusConstants.UN_ACKNOWLEDGE || status == statusConstants.APPROVED_PMC || status == statusConstants.PENDING_EXPORT_DATA || status == statusConstants.EXPORT_DATA)) || (menuType == _paramType.BulkBookingYarnAllowance && (status == statusConstants.ACTIVE)) || (menuType == _paramType.BulkBookingFinalApprove && (status == statusConstants.APPROVED_DONE || status == statusConstants.ALL)) || (menuType == _paramType.BulkBookingUtilizationProposal && (status == statusConstants.APPROVED_Allowance)) || (menuType == _paramType.BulkBookingUtilizationConfirmation && (status == statusConstants.PENDING_CONFIRMATION || status == statusConstants.CONFIRM)), width: 40
            },
            {
                field: 'FBAcknowledgeDate', headerText: 'Acknowledge Date', type: 'date', format: _ch_date_format_4, visible: (menuType == _paramType.BulkBookingAck && status == statusConstants.PENDING) || (menuType == _paramType.BulkBookingFinalApprove && (status == statusConstants.APPROVED_DONE || status == statusConstants.APPROVED_PMC || status == statusConstants.REJECT_PMC || status == statusConstants.ALL || status == statusConstants.PENDING_EXPORT_DATA || status == statusConstants.EXPORT_DATA)), width: 40
            },
            {
                field: 'CalendarDays', headerText: 'Event Day', visible: (menuType == _paramType.BulkBookingFinalApprove && (status == statusConstants.APPROVED_DONE || status == statusConstants.APPROVED_PMC || status == statusConstants.REJECT_PMC || status == statusConstants.ALL || status == statusConstants.PENDING_EXPORT_DATA || status == statusConstants.EXPORT_DATA)), width: 40
            },
            {
                field: 'FirstShipmentDate', headerText: '1st Shipment Date', type: 'date', format: _ch_date_format_1, visible: (menuType == _paramType.BulkBookingFinalApprove && (status == statusConstants.APPROVED_DONE || status == statusConstants.APPROVED_PMC || status == statusConstants.REJECT_PMC || status == statusConstants.ALL || status == statusConstants.PENDING_EXPORT_DATA || status == statusConstants.EXPORT_DATA)), width: 40
            },
            {
                field: 'YarnBookingDate', headerText: 'Yarn Booking Date', type: 'date', format: _ch_date_format_1, visible: (menuType == _paramType.BulkBookingFinalApprove && (status == statusConstants.APPROVED_DONE || status == statusConstants.ALL)), width: 130
            },
            {
                field: 'BuyerName', headerText: 'Buyer', width: 40
            },
            {
                field: 'BuyerTeamName', headerText: 'Buyer Team', width: 40
            },
            {
                field: 'YarnBookingRevisionDate', headerText: 'Yarn Booking Revision Date', width: 40, visible: (menuType == _paramType.BulkBookingAck && (status == statusConstants.ACTIVE || status == statusConstants.UN_ACKNOWLEDGE || status == statusConstants.APPROVED_PMC || status == statusConstants.PENDING_EXPORT_DATA || status == statusConstants.EXPORT_DATA)) || (menuType == _paramType.BulkBookingYarnAllowance && (status == statusConstants.ACTIVE)) || (menuType == _paramType.BulkBookingFinalApprove && (status == statusConstants.APPROVED_DONE || status == statusConstants.ALL)) || (menuType == _paramType.BulkBookingUtilizationProposal && (status == statusConstants.APPROVED_Allowance)) || (menuType == _paramType.BulkBookingUtilizationConfirmation && (status == statusConstants.PENDING_CONFIRMATION || status == statusConstants.CONFIRM))
            },
            {
                field: 'YBookingNo', headerText: 'Yarn Booking No', width: 130, visible: status != statusConstants.PENDING && !isYarnBookingAckMenu()
            },
            {
                field: 'BookingNo', headerText: 'Fabric Booking No', width: 130, visible: menuType == _paramType.YarnBookingAcknowledge
            },
            {
                field: 'FabricBookingType', headerText: 'Fabric Booking Type', width: 100, visible: menuType == _paramType.YarnBookingAcknowledge
            },

            {
                field: 'YarnRevisionNo', headerText: 'Yarn Revision No', width: 130, visible: menuType == _paramType.YarnBookingAcknowledge && status == statusConstants.REVISE
            },
            {
                field: 'YarnRevisedDate', headerText: 'Yarn Revision Date', type: 'date', format: _ch_date_format_1, visible: menuType == _paramType.YarnBookingAcknowledge && status == statusConstants.REVISE, width: 130
            },

            {
                field: 'YarnAcknowledgeDate', headerText: 'Yarn Acknowledged Date', type: 'date', format: _ch_date_format_1, visible: menuType == _paramType.YarnBookingAcknowledge && status == statusConstants.ACKNOWLEDGE, width: 130
            },
            {
                field: 'YarnAcknowledgeBy', headerText: 'Yarn Acknowledged By', width: 130, visible: menuType == _paramType.YarnBookingAcknowledge && status == statusConstants.ACKNOWLEDGE
            },

            {
                field: 'YarnUnAcknowledgeDate', headerText: 'Yarn UnAcknowledged Date', type: 'date', format: _ch_date_format_1, visible: menuType == _paramType.YarnBookingAcknowledge && status == statusConstants.UN_ACKNOWLEDGE, width: 130
            },
            {
                field: 'YarnUnAcknowledgeBy', headerText: 'Yarn UnAcknowledged By', width: 130, visible: menuType == _paramType.YarnBookingAcknowledge && status == statusConstants.UN_ACKNOWLEDGE
            },

            {
                field: 'RevisionReason', headerText: 'Revision Reason', width: 40, visible: (menuType == _paramType.BulkBookingAck && (status == statusConstants.PENDING || status == statusConstants.DRAFT || status == statusConstants.UN_ACKNOWLEDGE || status == statusConstants.INTERNAL_REJECTION || status == statusConstants.APPROVED_PMC || status == statusConstants.PENDING_EXPORT_DATA || status == statusConstants.EXPORT_DATA)) || (menuType == _paramType.BulkBookingYarnAllowance && (status == statusConstants.ACTIVE)) || (menuType == _paramType.BulkBookingFinalApprove && status == statusConstants.APPROVED_DONE)
            },
            //{
            //    field: 'UnAcknowledgeReason', headerText: 'Reject Reason', width: 40, visible: menuType == _paramType.BulkBookingFinalApprove
            //},
            {
                field: 'UnAcknowledgeReason', headerText: 'UnAck Reason', width: 240, visible: isYarnBookingAckMenu("UN_ACKNOWLEDGE")
            },
            {
                field: 'InternalRivisionReason', headerText: 'Internal Revision Reason', width: 40, visible: (menuType == _paramType.BulkBookingYarnAllowance && (status == statusConstants.ACTIVE))
            },
            {
                field: 'CreatedByName', headerText: 'Created By', width: 80, visible: isBulkBookingKnittingInfoMenu() && status == statusConstants.ALL
            },
            {
                field: 'ApprovedDatePMC', headerText: 'PMC Final Approve Date', type: 'date', format: _ch_date_format_5, visible: menuType == (_paramType.BulkBookingFinalApprove && status == statusConstants.APPROVED_PMC) || status == statusConstants.PENDING_EXPORT_DATA || status == statusConstants.EXPORT_DATA, width: 40
            },
            {
                field: 'PMCApprovedBy', headerText: 'PMC Approved By', width: 50, visible: menuType == (_paramType.BulkBookingFinalApprove && status == statusConstants.APPROVED_PMC) || status == statusConstants.PENDING_EXPORT_DATA || status == statusConstants.EXPORT_DATA
            },
            {
                field: 'RejectDatePMC', headerText: 'PMC Final Reject Date', type: 'date', format: _ch_date_format_1, visible: menuType == _paramType.BulkBookingFinalApprove && status == statusConstants.REJECT_PMC, width: 40
            },
            {
                field: 'PMCRejectedBy', headerText: 'PMC Reject By', width: 50, visible: (menuType == _paramType.BulkBookingFinalApprove && (status == statusConstants.REJECT_PMC))
            },
            {
                field: 'ApproveRejectDatePMC', headerText: 'PMC Final Approve/Reject Date', type: 'date', format: _ch_date_format_5, visible: menuType == _paramType.BulkBookingFinalApprove && status == statusConstants.ALL, width: 40
            },
            {
                field: 'PMCApprovedRejectedBy', headerText: 'PMC Approve/Reject By', width: 50, visible: (menuType == _paramType.BulkBookingFinalApprove && (status == statusConstants.ALL))
            },
            {
                field: 'RejectReason', headerText: 'Reject Reason', width: 30, visible: menuType == _paramType.BulkBookingAck && status == statusConstants.INTERNAL_REJECTION
            },
            {
                field: 'Reason', headerText: 'Reject Reason', width: 80, visible: isBulkBookingKnittingInfoMenu() && menuType != _paramType.YarnBookingAcknowledge && (status == statusConstants.ALL || status == statusConstants.ACTIVE || status == statusConstants.UN_ACKNOWLEDGE || status == statusConstants.INTERNAL_REJECTION || status == statusConstants.REJECT_Allowance || status == statusConstants.CHECK_REJECT || status == statusConstants.REJECT_REVIEW || status == statusConstants.REJECT_PMC) || (menuType == _paramType.BulkBookingUtilizationProposal && (status == statusConstants.APPROVED_Allowance)) || (menuType == _paramType.BulkBookingUtilizationConfirmation && (status == statusConstants.PENDING_CONFIRMATION || status == statusConstants.CONFIRM))
            },
            {
                field: 'CompanyName', headerText: 'Company', visible: menuType != 1 && _isBDS != 2, width: 40
            },
            {
                field: 'StyleNo', headerText: 'Style No', visible: (status == statusConstants.DRAFT || status == statusConstants.ACTIVE) && _isBDS != 2, width: 40
            },
            {
                field: 'SupplierName', headerText: 'Supplier Name', visible: (status == statusConstants.DRAFT || status == statusConstants.ACTIVE) && _isBDS != 2, width: 40
            },
            {
                field: 'SeasonName', headerText: 'Season Name', visible: (status == statusConstants.DRAFT || status == statusConstants.ACTIVE) && _isBDS != 2, width: 40
            },
            {
                field: 'Remarks', headerText: 'Remarks', width: 40, visible: _isBDS != 2
            }
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/bds-acknowledge/bulk/list?status=${status}&paramTypeId=${paramTypeId}`,
            columns: columns,
            autofitColumns: true,
            isFilterTypeExcel: menuType == _paramType.YarnBookingAcknowledge,
            //allowSorting: true,
            commandClick: handleCommands,
            queryCellInfo: cellModifyForBDSAck
        });
    }
    function initMasterTable() {
        var columns = [
            {
                headerText: 'Command', width: 100, textAlign: 'Left', visible: status == statusConstants.ACTIVE, commands: [
                    { type: 'Add', title: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
                ]
            },
            {
                headerText: 'Command', width: 100, textAlign: 'Left', visible: (status == statusConstants.PENDING || status == statusConstants.REJECT), commands: [
                    { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'View Attachment', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-image-o' } }
                ]
            },
            {
                headerText: 'Command', width: 100, textAlign: 'Left', visible: (status == statusConstants.COMPLETED || status == statusConstants.UN_ACKNOWLEDGE || status == statusConstants.OTHERS), commands: [
                    { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'View Attachment', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-image-o' } }
                ]
            },
            {
                headerText: 'Command', width: 100, textAlign: 'Left', visible: (status == statusConstants.APPROVED), commands: [
                    { type: 'ViewRecive', title: 'ViewRecive', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'View Attachment', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-image-o' } }
                ]
            },
            {
                field: 'PendingRevision', headerText: 'Status', visible: status == 2
            },
            {
                field: 'BookingNo', headerText: 'Booking No'
            },
            {
                field: 'SLNo', headerText: 'SL No', visible: false
            },
            {
                field: 'BookingDate', headerText: 'Booking Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'ApproveDate', headerText: 'Approved Date', type: 'date', format: _ch_date_format_5
            },
            {
                field: 'AckByName', headerText: 'Acknowledge By', visible: status == statusConstants.COMPLETED
            },
            {
                field: 'DateAdded', headerText: 'Acknowledge Date', visible: status == statusConstants.COMPLETED, type: 'date', format: _ch_date_format_5
            },
            {
                field: 'UnAckByName', headerText: 'UnAcknowledge By', visible: status == statusConstants.UN_ACKNOWLEDGE
            },
            {
                field: 'UnAcknowledgeDate', headerText: 'UnAcknowledge Date', visible: status == statusConstants.UN_ACKNOWLEDGE, type: 'date', format: _ch_date_format_5
            },
            {
                field: 'BuyerName', headerText: 'Buyer'
            },
            {
                field: 'BuyerTeamName', headerText: 'Buyer Team'
            },
            {
                field: 'CompanyName', headerText: 'Company'
            },
            {
                field: 'StyleNo', headerText: 'Style No'
            },
            {
                field: 'SupplierName', headerText: 'Supplier Name'
            },
            {
                field: 'SeasonName', headerText: 'Season Name'
            },
            {
                field: 'Remarks', headerText: 'Remarks'
            }
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            allowGrouping: true,
            apiEndPoint: `/api/bds-acknowledge/list?status=${status}&isBDS=${_isBDS}`,
            columns: columns,
            //allowSorting: true,
            commandClick: handleCommands,
            queryCellInfo: cellModifyForBDSAck
        });
    }
    function initLabDipAckTable() {
        var columns = [
            {
                headerText: 'Command', width: 100, textAlign: 'center', visible: status == statusConstants.PENDING && _isLabDipAck, commands: [
                    { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
                ]
            },
            {
                headerText: 'Command', width: 100, textAlign: 'center', visible: (status == statusConstants.ACKNOWLEDGE || status == statusConstants.REVISE || status == statusConstants.REVISE_FOR_ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE || status == statusConstants.REJECT) && _isLabDipAck, commands: [
                    { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
                ]
            },
            {
                headerText: 'Command', width: 100, textAlign: 'center', visible: (status == statusConstants.ACKNOWLEDGE || status == statusConstants.REVISE || status == statusConstants.REVISE_FOR_ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE || status == statusConstants.REJECT) && _isLabDipAck_RnD, commands: [
                    { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                    { type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
                ]
            },
            {
                field: 'StatusText', headerText: 'Status', visible: status == statusConstants.ACKNOWLEDGE && _isLabDipAck_RnD
            },
            {
                field: 'PendingRevision', headerText: 'Status', visible: status == 2
            },
            {
                field: 'BookingNo', headerText: 'Booking No'
            },
            {
                field: 'SLNo', headerText: 'SL No', visible: false
            },
            {
                field: 'BookingDate', headerText: 'Booking Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'AckByName', headerText: 'Acknowledge By', visible: status == statusConstants.COMPLETED
            },
            {
                field: 'DateAdded', headerText: 'Acknowledge Date', visible: status == statusConstants.COMPLETED, type: 'date', format: _ch_date_format_5
            },
            {
                field: 'UnAckByName', headerText: 'UnAcknowledge By', visible: status == statusConstants.UN_ACKNOWLEDGE
            },
            {
                field: 'UnAcknowledgeDate', headerText: 'UnAcknowledge Date', visible: status == statusConstants.UN_ACKNOWLEDGE, type: 'date', format: _ch_date_format_5
            },
            {
                field: 'LabdipUnAcknowledgeReason', headerText: 'UnAcknowledge Reason', visible: _isLabDipAck && status == statusConstants.REJECT
            },
            {
                field: 'BuyerName', headerText: 'Buyer'
            },
            {
                field: 'BuyerTeamName', headerText: 'Buyer Team'
            },
            {
                field: 'CompanyName', headerText: 'Company'
            },
            {
                field: 'StyleNo', headerText: 'Style No'
            },
            {
                field: 'SupplierName', headerText: 'Supplier Name'
            },
            {
                field: 'SeasonName', headerText: 'Season Name'
            },
            {
                field: 'Remarks', headerText: 'Remarks'
            },
            {
                field: 'UnAcknowledgeReason', headerText: 'Reason', visible: status == statusConstants.UN_ACKNOWLEDGE
            }
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            allowGrouping: true,
            apiEndPoint: `/api/bds-acknowledge/labdip/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands,
            queryCellInfo: cellModifyForBDSAck
        });
    }

    function handleCommands(args) {

        //var isValidate =  FBookingAckRevisionPendingValidation(args.rowData.BookingNo, args.rowData.ExportOrderID);
        if (isOnlyBulkBookingKnittingInfoMenu() && args.commandColumn.type != 'View Attachment' && args.commandColumn.type != 'Tech Pack Attachment' && args.commandColumn.type != 'Booking Attachment' && args.commandColumn.type != 'Export' && args.commandColumn.type != 'Booking Report' && args.commandColumn.type != 'Bulk Booking Report' && args.commandColumn.type != 'Bulk Booking Yarn Info') {
            FBookingAckRevisionPendingValidation(args.rowData.BookingNo, args.rowData.ExportOrderID)
                .then(function () {
                    // Handle validation not apply  or API call get success
                    handleCommandsSuccess(args);
                })
                .catch(function (error) {
                    // Handle validation failure or API call errors
                    toastr.error(error);

                });
        }
        else if (args.commandColumn.type == 'Booking Report') {
            if (_isLabDipAck_RnD) {
                window.open(`/reports/InlinePdfView?ReportName=SampleFabricLBAck.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            }
            else if (pageName == "Projection") {
                window.open(`/reports/InlinePdfView?ReportName=GmtProjectionBooking.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            } else {
                window.open(`/reports/InlinePdfView?ReportName=SampleFabric.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');

            }
        }
        else if (args.commandColumn.type == 'Bulk Booking Report') {
            if (args.rowData.IsSample == 1) {
                //window.open(`/reports/InlinePdfView?ReportName=SampleFabric.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
                window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainAck.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            } else {
                //window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainForPMCN.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
                window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainAck.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            }
        }
        else if (args.commandColumn.type == 'Tech Pack Attachment') {
            if (args.rowData.ImagePath == '' || args.rowData.ImagePath == null) {
                toastr.error("No attachment found!!");
                return false;
            }

            var url = window.location.href;
            var path = "";
            if (url.includes("8060") || url.includes("local")) {
                path = constants.GMT_ERP_LOCAL_PATH;
            } else {
                path = constants.GMT_ERP_BASE_PATH;
            }
            var imagePath = path + args.rowData.ImagePath;
            window.open(imagePath, "_blank");
        }
        else if (args.commandColumn.type == 'Booking Attachment') {
            if (args.rowData.ImagePath1 == '' || args.rowData.ImagePath1 == null) {
                toastr.error("No attachment found!!");
                return false;
            }
            var url = window.location.href;
            var path = "";
            if (url.includes("8060") || url.includes("local")) {
                path = constants.GMT_ERP_LOCAL_PATH;
            } else {
                path = constants.GMT_ERP_BASE_PATH;
            }
            var imagePath1 = path + args.rowData.ImagePath1;
            window.open(imagePath1, "_blank");
        }
        else if (args.commandColumn.type == 'View Attachment') {
            if (args.rowData.ImagePath == '' || args.rowData.ImagePath == null) {
                toastr.error("No attachment found!!");
                return false;
            }
            var url = window.location.href;
            var path = "";
            if (url.includes("8060") || url.includes("local")) {
                path = constants.GMT_ERP_LOCAL_PATH;
            } else {
                path = constants.GMT_ERP_BASE_PATH;
            }
            var imagePath = path + args.rowData.ImagePath;
            window.open(imagePath, "_blank");
        }
        else if (args.commandColumn.type == 'Export') {
            getExportData(args.rowData.BookingNo, args.rowData.IsSample);

        }
        else if (args.commandColumn.type == 'Bulk Booking Yarn Info') {
            window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainForFPYMSNew.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
        }
        else {
            handleCommandsSuccess(args);
        }
        //$formEl.find("#btnSave").show();
    }

    function handleCommandsSuccess(args) {

        //args.rowData.IsMktRevisionPending = typeof args.rowData.IsMktRevisionPending === "undefined" ? false : args.rowData.IsMktRevisionPending;
        //if (args.rowData.IsMktRevisionPending) {
        //    toastr.error("Marketing Ack Pending");
        //    return false;
        //}

        if (args.commandColumn.type == 'Add') {
            if (args.rowData.PendingRevision != null && typeof args.rowData.PendingRevision !== "undefined" && $.trim(args.rowData.PendingRevision.length) > 0) {
                getNew(args.rowData.BookingID, true);
            }
            else {
                getNew(args.rowData.BookingID, false);
            }
            $formEl.find("#btnReceive,#btnOkk").show();
            $formEl.find("#btnSave,#btnUnAcknowledge,#btnReceived,#btnCancelAcknowledge,#btnCancelUnAcknowledge,.divUnAcknowledgeReason").hide();
        }
        else if (args.commandColumn.type == 'Edit') {
            if (args.rowData.PendingRevision != null && typeof args.rowData.PendingRevision !== "undefined" && $.trim(args.rowData.PendingRevision && args.rowData.PendingRevision.length) > 0) {
                getNew(args.rowData.BookingID, true);
            }
            else {
                getNew(args.rowData.BookingID, false);
            }
            $formEl.find("#btnReceive,#btnReceived,#btnCancelAcknowledge,#btnCancelUnAcknowledge,#btnReviseLabDipRnD,.divUnAcknowledgeReason").hide();
            $formEl.find("#btnSave,#btnUnAcknowledge,#btnOkk").show();
            if (status == statusConstants.REJECT) {
                $formEl.find("#btnCancelAcknowledge,#btnCancelUnAcknowledge").show();
                $formEl.find("#btnSave,#btnUnAcknowledge,#btnOkk").hide();
            }

            if (_isLabDipAck && status == statusConstants.PENDING) {
                $formEl.find("#btnSaveLabDipAck,#btnSaveLabDipUnAck").show();
                $formEl.find("#btnReviseLabDip,#btnReviseLabDipRnD").hide();
            }
        }
        else if (args.commandColumn.type == 'View') {

            //if (args.rowData.IsInvalidBooking && isBulkBookingKnittingInfoMenu()) {
            //    toastr.error("Fabric booking revision acknowledge pending.");
            //    return false;
            //}
            if (!args.rowData.Acknowledge && menuType == _paramType.AdditionalYarnBooking && status == statusConstants.ADDITIONAL_APPROVED_OPERATION_HEAD) {
                toastr.error("Pending yarn booking acknowledge.");
                return false;
            }
            _statusText = "";
            if (status === statusConstants.UN_ACKNOWLEDGE) {
                $formEl.find(".divUnAcknowledgeReason").show();
            }
            else {
                $formEl.find(".divUnAcknowledgeReason").hide();
            }

            _allYarnList = [];
            if (args.rowData.IsRevised && menuType == _paramType.BulkBookingAck) //BBKnitting Info
            {
                getRevise(args.rowData.BookingNo, args.rowData.IsSample);
            }
            else if (isLabdipMenu()) {
                _statusText = args.rowData.StatusText;
                getViewLabDip(args.rowData.BookingID);
            }
            else {
                getView(args.rowData.FBAckID, args.rowData.BookingNo, args.rowData.IsSample, args.rowData.YBookingNo, args.rowData.IsRevisionValid);
            }
            if (isLabdipMenu()) {

                if (status == statusConstants.UN_ACKNOWLEDGE) {
                    $formEl.find(".divUnAcknowledgeReason").show();
                }

                $formEl.find("#btnReceive").hide();
                $formEl.find("#btnSave,#btnUnAcknowledge,#btnReceived,#btnOkk,#btnCancelAcknowledge,#btnCancelUnAcknowledge").hide();
                $formEl.find("#btnSaveAsDraft,#btnSave,#btnUnAcknowledge").hide();
                $formEl.find("#btnApproveByPMC").hide();
                $formEl.find("#btnRejectByKnittingInputPopup").hide();
                $formEl.find("#btnSaveLabDipAck, #btnSaveLabDipUnAck, #btnReviseLabDipRnD,#btnReviseLabDip").hide();

                if (_isLabDipAck_RnD && (status == statusConstants.REVISE || status == statusConstants.REVISE_FOR_ACKNOWLEDGE)) {
                    $formEl.find("#btnReviseLabDipRnD,#btnOkk").show();
                }
                else if (_isLabDipAck_RnD && status == statusConstants.ACKNOWLEDGE) {
                    $formEl.find("#btnSave,#btnUnAcknowledge,#btnOkk").show();
                }
                if (_isLabDipAck && (status == statusConstants.ACKNOWLEDGE || status == statusConstants.REVISE || status == statusConstants.UN_ACKNOWLEDGE)) {
                    $formEl.find("#btnReviseLabDip").show();
                }
            }
            else if (isAdditionBulkBooking()) {
                if (menuType == _paramType.AdditionalYarnBooking) {
                    if (status == statusConstants.APPROVED2) {
                        $formEl.find("#btnSaveAsDraft").show();
                    }
                }

                $formEl.find("#btnApproveAdditionBBKI,#btnRejectAdditionBBKI").hide();

                if (menuType >= _paramType.AYBQtyFinalizationPMC && menuType <= _paramType.AYBOperationHeadApproval && _isPendingList) {
                    $formEl.find("#btnApproveAdditionBBKI,#btnRejectAdditionBBKI").show();
                }
            }
            else {
                $formEl.find("#btnReceive").hide();
                $formEl.find("#btnSave,#btnUnAcknowledge,#btnReceived,#btnOkk,#btnCancelAcknowledge,#btnCancelUnAcknowledge").hide();
                if (status == statusConstants.DRAFT) {
                    $formEl.find("#btnSaveAsDraft,#btnSave,#btnUnAcknowledge").show();
                }
                if (args.rowData.IsReject && status == statusConstants.APPROVED_DONE) {
                    $formEl.find("#btnApproveByPMC").hide();
                } else if (menuType == _paramType.AYBQtyFinalizationPMC) {
                    $formEl.find("#btnApproveByPMC").show();
                }
                if (status == statusConstants.UN_ACKNOWLEDGE) {
                    $formEl.find("#btnRejectByKnittingInputPopup").hide();
                }
            }
        }
        else if (args.commandColumn.type == 'ViewRecive') {
            getView(args.rowData.FBAckID, args.rowData.BookingNo);
            $formEl.find("#btnReceive").hide();
            $formEl.find("#btnSave,#btnUnAcknowledge,#btnOkk,#btnCancelAcknowledge,#btnCancelUnAcknowledge").hide();
            $formEl.find("#btnReceived").show();

            $formEl.find("#btnSave").show();
            $formEl.find("#btnReceived").hide();
        }

        else if (args.commandColumn.type == 'Booking Report') {
            if (_isLabDipAck_RnD) {
                window.open(`/reports/InlinePdfView?ReportName=SampleFabricLBAck.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            }
            else if (pageName == "Projection") {
                window.open(`/reports/InlinePdfView?ReportName=GmtProjectionBooking.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            } else {
                window.open(`/reports/InlinePdfView?ReportName=SampleFabric.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            }
        }
        else if (args.commandColumn.type == 'Bulk Booking Report') {
            if (menuType == _paramType.YarnBookingAcknowledge) {
                window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainAck.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            }
            else if (args.rowData.IsSample == 1) {
                window.open(`/reports/InlinePdfView?ReportName=SampleFabric.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            } else {
                window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainForPMCN.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            }
        }
        else if (args.commandColumn.type == 'Bulk Booking Yarn Info') {
            window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainForFPYMSNew.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'Yarn Booking Summary Report') {
            window.open(`/reports/InlinePdfView?ReportName=YarnBookingSummary_New.rdl&YBookingNo=${args.rowData.YBookingNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'View Attachment') {

            if (args.rowData.ImagePath == '' || args.rowData.ImagePath == null) {
                toastr.error("No attachment found!!");
            } else {
                var url = window.location.href;
                var path = "";
                if (url.includes("8060") || url.includes("local")) {
                    path = constants.GMT_ERP_LOCAL_PATH;
                } else {
                    path = constants.GMT_ERP_BASE_PATH;
                }
                //var imagePath = path + "/" + args.rowData.ImagePath;

                var imagePath = path + args.rowData.ImagePath;

                window.open(imagePath, "_blank");
            }

            /*if (args.rowData.ImagePath == '') {
                toastr.error("No attachment found!!");
            } else {
                //var a = document.createElement('a');
                //a.href = args.rowData.ImagePath;
                //a.setAttribute('target', '_blank');
                //a.click();

                var imagePath = constants.GMT_ERP_BASE_PATH + args.rowData.ImagePath;
                window.open(imagePath, "_blank");
            }*/
        }

        else if (args.commandColumn.type == 'AddBulk') {
            //if (isBulkBookingKnittingInfoMenu()) {
            //    if (typeof args.rowData.IsSample === "undefined") args.rowData.IsSample = true;
            //    if (args.rowData.IsSample == true) {
            //        toastr.error("Bulk Booking Knitting Info for SMS Booking is under construction.");
            //        return false;
            //    }
            //}
            getNewBulk(args.rowData.BookingNo, args.rowData.IsSample);
            //if (status == statusConstants.DRAFT) {
            //    $formEl.find("#btnSave,#btnUnAcknowledge").show();
            //} else {
            //    $formEl.find("#btnSave,#btnUnAcknowledge").hide();
            //}
            $formEl.find("#btnReceive,#btnReceived,#btnCancelAcknowledge,#btnCancelUnAcknowledge").hide();

        }

    }

    function cellModifyForBDSAck(args) {
        if (args.data.ImagePath == '') {
            //if (args.cell.classList.contains("e-unboundcell")) {
            //args.cell.querySelector(".booking_attImage").style.display = "none";
            if (args.cell.childNodes.length > 0) {
                for (var i = 0; i < args.cell.childNodes[0].childNodes.length; i++) {
                    if (args.cell.childNodes[0].childNodes[i].title === 'View Attachment') {
                        args.cell.childNodes[0].childNodes[i].style.display = "none";
                    }
                }
            }
            //}
        }
    }

    var YarnSourceNameElem;
    var YarnSourceNameobj;
    var machineTypeElem;
    var machineTypeObj;
    var technicalNameElem;
    var technicalNameObj;
    var brandElem;
    var brandObj;

    var gaugeElem;
    var gaugeObj;
    var diaElem;
    var diaObj;
    function setChildData(x, subGroupId) {
        if (subGroupId == 1) {
            x.ReqFinishFabricQty = x.BookingQty - x.FinishFabricUtilizationQty;
        }
        else {
            x.ReqFinishFabricQty = x.BookingQtyKG - x.FinishFabricUtilizationQty;
        }
        x = setBookingQtyKGRelatedFieldsValue(x, subGroupId);

        x.ChildItems.forEach(y => {

            if (typeof x.GreyProdQty != 'undefined' && x.GreyProdQty != null && typeof y.Distribution != 'undefined' && y.Distribution != null && typeof x.YarnAllowance != 'undefined' && x.YarnAllowance != null) {
                y.YarnReqQty = (x.GreyProdQty * (y.Distribution / 100)) / (1 + (x.YarnAllowance / 100) - (0.5 / 100));
                if (isAdditionBulkBooking() && x.IsForFabric == false) {
                    y.YarnReqQty = y.NetYarnReqQty;
                }
                y.YarnReqQty = parseFloat(y.YarnReqQty).toFixed(2);
                //y.GreyYarnUtilizationQty = 0;
                //y.DyedYarnUtilizationQty = 0;
                if (typeof y.GreyAllowance == 'undefined' || y.GreyAllowance == null) {
                    y.GreyAllowance = 0;
                }
                if (typeof y.YDAllowance == 'undefined' || y.YDAllowance == null) {
                    y.YDAllowance = 0;
                }

                if (isAdditionBulkBooking() && x.IsForFabric == false) {
                    y = getYarnRelatedPropsAdditionalYarn(y, x, false, true);
                }
                else {
                    y = getYarnRelatedProps(y, x, false, true);
                }
            }

        });
    }
    async function initChild(data, isDoCalculateFields = true) {

        data.forEach(x => {
            if (menuType == _paramType.AdditionalYarnBooking && status == statusConstants.APPROVED2) {
                //if (_isFirstLoad) {
                //    x.YarnAllowance = 0;
                //    x.BookingQty = 0;
                //    x.BookingQtyKG = 0;
                //    x = setAdditionalAllowance(x);
                //}
                if (typeof x.FinishFabricUtilizationQty == 'undefined' || x.FinishFabricUtilizationQty == null) {
                    x.FinishFabricUtilizationQty = 0;
                }
                if (typeof x.GreyLeftOverQty == 'undefined' || x.GreyLeftOverQty == null) {
                    x.GreyLeftOverQty = 0;
                }
            }

            x.ReqFinishFabricQty = x.BookingQty - x.FinishFabricUtilizationQty;
            x = setBookingQtyKGRelatedFieldsValue(x, 1);
            x.ChildItems.forEach(y => {
                if (typeof x.GreyProdQty != 'undefined' && x.GreyProdQty != null && typeof y.Distribution != 'undefined' && y.Distribution != null && typeof x.YarnAllowance != 'undefined' && x.YarnAllowance != null) {
                    y.YarnReqQty = (x.GreyProdQty * (y.Distribution / 100)) / (1 + (x.YarnAllowance / 100) - (0.5 / 100));
                    if (isAdditionBulkBooking() && x.IsForFabric == false) {
                        y.YarnReqQty = y.NetYarnReqQty;
                    }
                    y.YarnReqQty = parseFloat(y.YarnReqQty).toFixed(2);
                    if (typeof y.GreyAllowance == 'undefined' || y.GreyAllowance == null) {
                        y.GreyAllowance = 0;
                    }
                    if (typeof y.YDAllowance == 'undefined' || y.YDAllowance == null) {
                        y.YDAllowance = 0;
                    }
                    if (isAdditionBulkBooking() && x.IsForFabric == false) {
                        y = getYarnRelatedPropsAdditionalYarn(y, x, false, isDoCalculateFields);
                    }
                    else {
                        y = getYarnRelatedProps(y, x, false, isDoCalculateFields);
                    }
                }
            });
        });

        data = setCalculatedValues(data);
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        var additionalColumns = [];
        var isAllowEditingCell = true;
        var isAllowEditingAllowanceCell = false;
        if (menuType == _paramType.BulkBookingYarnAllowance) {

            isAllowEditingCell = false;
            isAllowEditingAllowanceCell = true;
        }
        if (menuType == _paramType.BulkBookingCheck || menuType == _paramType.BulkBookingApprove) {

            //isAllowEditingCell = false;
            isAllowEditingAllowanceCell = true;
        }

        if (_isLabDipAck) {
            columns = [
                { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
                { field: 'ConceptTypeID', visible: false },

                {
                    field: 'FinishingDays', headerText: 'Finishing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'DyeingDays', headerText: 'Dyeing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'BatchPreparationDays', headerText: 'Batch Preparation Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'KnittingDays', headerText: 'Knitting Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'TestReportDays', headerText: 'Test Report Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'MaterialDays', headerText: 'Material Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'QualityDays', headerText: 'Quality Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'Construction', headerText: 'Construction', allowEditing: false
                },
                {
                    field: 'Composition', headerText: 'Composition', allowEditing: false
                },
                {
                    field: 'Color', headerText: 'Color', width: 85, allowEditing: false
                },
                {
                    field: 'GSM', headerText: 'GSM', width: 85, allowEditing: false
                },
                {
                    field: 'FabricWidth', headerText: 'Fabric Width', width: 85, allowEditing: false
                },
                {
                    field: 'KnittingType', headerText: 'Knitting Type', width: 85, allowEditing: false
                },
                {
                    field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false
                },
                {
                    field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false
                },
                {
                    field: 'ReferenceSourceName', headerText: 'Reference Source', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
                },
                {
                    field: 'ReferenceNo', headerText: 'Ref No', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
                },
                {
                    field: 'ColorReferenceNo', headerText: 'Color Ref No', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
                },
                {
                    field: 'ValueName', headerText: 'Yarn Source', visible: false/* status != statusConstants.ACTIVE*/, edit: {
                        create: function () {
                            YarnSourceNameElem = document.createElement('input');
                            return YarnSourceNameElem;
                        },
                        read: function () {
                            return YarnSourceNameobj.text;
                        },
                        destroy: function () {
                            YarnSourceNameobj.destroy();
                        },
                        write: function (e) {
                            YarnSourceNameobj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.YarnSourceNameList,
                                fields: { value: 'id', text: 'text' },
                                change: function (f) {
                                    //
                                    technicalNameObj.enabled = true;
                                    //var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                    //technicalNameObj.query = tempQuery;
                                    technicalNameObj.text = null;
                                    technicalNameObj.dataBind();

                                    e.rowData.YarnSourceID = f.itemData.id;
                                    e.rowData.ValueName = f.itemData.text;
                                },
                                placeholder: 'Select one',
                                floatLabelType: 'Never'
                            });
                            YarnSourceNameobj.appendTo(YarnSourceNameElem);
                        }
                    }
                },
                {
                    field: 'DyeingType', headerText: 'Dyeing Type', width: 85, allowEditing: false
                },
                {
                    field: 'Instruction', headerText: 'Instruction', allowEditing: false
                },
                {
                    field: 'LabDipNo', headerText: 'Lab Dip No', allowEditing: false
                },
                {
                    field: 'IsFabricReq', headerText: 'Fabric Req?', allowEditing: _isLabDipAck, visible: isLabdipMenu(), displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center'
                },
                {
                    field: 'BookingQty', headerText: 'Booking Qty', width: 85, allowEditing: _isLabDipAck
                },
                {
                    field: 'TotalQty', headerText: 'Total Qty', width: 85, allowEditing: false, visible: status == false //statusConstants.COMPLETED
                }
            ];
        }
        else if (isAdditionBulkBooking()) {
            columns = [
                { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
                { field: 'ConceptTypeID', visible: false },
                { field: 'SubGroupID', visible: false },
                {
                    field: 'MachineType', headerText: 'Machine Type', width: 80, allowEditing: false
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', width: 80, allowEditing: false
                },
                {
                    field: 'ExistingYarnAllowance', headerText: 'Existing Yarn Allowance', allowEditing: false
                },
                {
                    field: 'YarnAllowance', headerText: 'Add. Yarn Allowance', allowEditing: true
                },
                {
                    field: 'TotalYarnAllowance', headerText: 'Total Yarn Allowance', allowEditing: false
                },
                {
                    field: 'MachineGauge', headerText: 'Gauge', width: 80, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } }
                },
                {
                    field: 'MachineDia', headerText: 'Dia', width: 80, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } }
                },
                {
                    field: 'Brand', headerText: 'Brand', allowEditing: false
                },
                {
                    field: 'Construction', headerText: 'Construction', allowEditing: false
                },
                {
                    field: 'Composition', headerText: 'Composition', allowEditing: false
                },
                {
                    field: 'Color', headerText: 'Color', width: 85, allowEditing: false
                },
                {
                    field: 'GSM', headerText: 'GSM', width: 85, allowEditing: false
                },
                {
                    field: 'FabricWidth', headerText: 'Fabric Width', width: 85, allowEditing: false
                },
                {
                    field: 'KnittingType', headerText: 'Knitting Type', width: 85, allowEditing: false
                },
                {
                    field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false
                },
                {
                    field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false
                },
                {
                    field: 'DyeingType', headerText: 'Dyeing Type', width: 85, allowEditing: false
                },
                {
                    field: 'Instruction', headerText: 'Instruction', allowEditing: false
                },
                {
                    field: 'LabDipNo', headerText: 'Lab Dip No', allowEditing: false
                },
                {
                    field: 'RefSourceNo', headerText: 'Ref No', width: 85, allowEditing: false
                },
                {
                    field: 'ActualBookingQty', headerText: 'Booking Qty(KG)', width: 85, allowEditing: false
                },
                {
                    field: 'BookingQty', headerText: 'Replacement Qty(KG)', width: 120, allowEditing: false
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: false, width: 40, commands: [
                        { buttonOption: { type: 'findAdditionalReplacementQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for Replacement qty" } }
                    ]
                },
                {
                    field: 'IsForFabric', headerText: 'For Fabric?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', allowEditing: true
                },
                {
                    field: 'FinishFabricUtilizationQty', headerText: 'Finish Fabric Utilization Qty', width: 120, propField: 'FinishFabricUtilizationQty', allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 40, propField: 'FinishFabricUtilizationQty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findFinishFabricUtilizationQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for finish fabric utilization qty" } }
                    ]
                },
                {
                    field: 'ReqFinishFabricQty', headerText: 'Req. Finish Fabric Qty', width: 120, allowEditing: false
                },
                {
                    field: 'TotalQty', headerText: 'Total Qty', width: 85, allowEditing: false, visible: status == false //statusConstants.COMPLETED
                },
                {
                    field: 'GreyReqQty', headerText: 'Grey Req Qty', width: 85, allowEditing: false
                },
                {
                    field: 'GreyLeftOverQty', headerText: 'Grey Utilization Qty', width: 85, propField: 'GreyLeftOverQty', allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 40, propField: 'GreyLeftOverQty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findGreyLeftOverQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for grey left over qty" } }
                    ]
                },
                {
                    field: 'GreyProdQty', headerText: 'Grey Prod Qty', width: 95, allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                }
            ];
        }
        else {
            columns = [
                { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
                { field: 'ConceptTypeID', visible: false },
                //MachineType
                //TechnicalName
                {
                    field: 'MachineType', headerText: 'Machine Type', visible: (status != statusConstants.ACTIVE || (_isBDS == 2)) && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation && isAllowEditingCell == true, allowEditing: isAllowEditingCell,
                    valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            machineTypeElem = document.createElement('input');
                            return machineTypeElem;
                        },
                        read: function () {
                            return machineTypeObj.value;
                        },
                        destroy: function () {
                            machineTypeObj.destroy();
                        },
                        write: function (e) {

                            machineTypeObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.MCTypeForFabricList,
                                fields: { value: 'id', text: 'text' },

                                placeholder: 'Select Machine Type',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',

                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                change: function (f) {

                                    technicalNameObj.enabled = true;
                                    var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                    technicalNameObj.query = tempQuery;
                                    technicalNameObj.text = null;
                                    technicalNameObj.dataBind();

                                    e.rowData.MachineTypeId = f.itemData.id;
                                    e.rowData.MachineType = f.itemData.text;
                                    e.rowData.KTypeId = f.itemData.desc;
                                    e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);
                                },
                                placeholder: 'Select M/C Type',
                                floatLabelType: 'Never'
                            });
                            machineTypeObj.appendTo(machineTypeElem);
                        }
                    }
                },
                {
                    field: 'MachineType', headerText: 'Machine Type', visible: (status != statusConstants.ACTIVE || (_isBDS == 2)) && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation && isAllowEditingCell == false, allowEditing: isAllowEditingCell,
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', displayField: "TechnicalName", visible: isAllowEditingCell == true, allowEditing: isAllowEditingCell, valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            technicalNameElem = document.createElement('input');
                            return technicalNameElem;
                        },
                        read: function () {
                            return technicalNameObj.value;
                        },
                        destroy: function () {
                            technicalNameObj.destroy();
                        },
                        write: function (e) {
                            technicalNameObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.TechnicalNameList,
                                fields: { value: 'id', text: 'text' },
                                //enabled: false,
                                placeholder: 'Select Technical Name',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                change: function (f) {
                                    if (!f.isInteracted || !f.itemData) return false;

                                    e.rowData.TechnicalTime = parseInt(f.itemData.desc);
                                    e.rowData.TechnicalNameId = f.itemData.id;
                                    e.rowData.TechnicalName = f.itemData.text;
                                    e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);

                                    //$tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                                }
                            });
                            technicalNameObj.appendTo(technicalNameElem);
                        }
                    }
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', visible: isAllowEditingCell == false, allowEditing: isAllowEditingCell,
                },
                {
                    field: 'YarnAllowance', headerText: 'Yarn Allowance', visible: isBulkBookingKnittingInfoMenu() && menuType != _paramType.BulkBookingAck && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation, allowEditing: isAllowEditingAllowanceCell,
                },
                {
                    field: 'IsSubContact', headerText: 'Sub-Contact?', visible: (status != statusConstants.ACTIVE && _isBDS == 1) || _isLabDipAck_RnD, displayAsCheckBox: true, editType: "booleanedit", width: 85, textAlign: 'Center'
                },
                {
                    field: 'TotalDays', headerText: 'Total Days', visible: (status != statusConstants.ACTIVE && _isBDS == 1) || _isLabDipAck_RnD, allowEditing: false, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'StructureDays', headerText: 'Structure Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'FinishingDays', headerText: 'Finishing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'DyeingDays', headerText: 'Dyeing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'BatchPreparationDays', headerText: 'Batch Preparation Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'KnittingDays', headerText: 'Knitting Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'TestReportDays', headerText: 'Test Report Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'MaterialDays', headerText: 'Material Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'QualityDays', headerText: 'Quality Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'DeliveryDate', headerText: 'Delivery Date', visible: status != statusConstants.ACTIVE && _isBDS == 1 && !isLabdipMenu(), textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
                },
                //{
                //    field: 'MachineGauge', headerText: 'Gauge', visible: _isBDS == 2, width: 80, allowEditing: true, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } }
                //},
                //{
                //    field: 'MachineDia', headerText: 'Dia', visible: _isBDS == 2, width: 80, allowEditing: true, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } }
                //},

                {
                    field: 'MachineGauge', headerText: 'Gauge', visible: (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) && isAllowEditingCell == true, allowEditing: isAllowEditingCell,
                    valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            gaugeElem = document.createElement('input');
                            return gaugeElem;
                        },
                        read: function () {
                            return gaugeObj.value;
                        },
                        destroy: function () {
                            gaugeObj.destroy();
                        },
                        write: function (e) {
                            gaugeObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.GaugeList,
                                fields: { value: 'id', text: 'text' },

                                placeholder: 'Select Gauge',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                change: function (f) {
                                    diaObj.enabled = true;
                                    var tempQuery = new ej.data.Query().where('additionalValue', 'equal', gaugeObj.value);
                                    diaObj.query = tempQuery;
                                    diaObj.text = null;
                                    diaObj.dataBind();

                                    e.rowData.MachineGauge = f.itemData.id;
                                    e.rowData.MachineGauge = f.itemData.text;
                                    e.rowData.MachineDia = f.itemData.desc;

                                    if (masterData.MachineBrandList.length > 0) {
                                        if (typeof machineBrandObj != 'undefined') {
                                            machineBrandObj.dataSource = masterData.MachineBrandList.filter(function (el) {
                                                if (typeof e.rowData.MachineGauge != 'undefined' && typeof e.rowData.MachineDia != 'undefined')
                                                    return el.GG == e.rowData.MachineGauge && el.Dia == e.rowData.MachineDia;
                                                else
                                                    return null;
                                            });
                                            machineBrandObj.dataBind();
                                        }
                                    }

                                    //$tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                                },
                                placeholder: 'Select Gauge',
                                floatLabelType: 'Never'
                            });
                            gaugeObj.appendTo(gaugeElem);
                        }
                    }
                },
                {
                    field: 'MachineGauge', headerText: 'Gauge', visible: (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) && isAllowEditingCell == false, allowEditing: isAllowEditingCell,
                },
                {
                    field: 'MachineDia', headerText: 'Dia', displayField: "Dia", allowEditing: isAllowEditingCell, visible: (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) && isAllowEditingCell == true, valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            diaElem = document.createElement('input');
                            return diaElem;
                        },
                        read: function () {
                            return diaObj.value;
                        },
                        destroy: function () {
                            diaObj.destroy();
                        },
                        write: function (e) {
                            diaObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.DiaList,
                                fields: { value: 'id', text: 'text' },
                                //enabled: false,
                                placeholder: 'Select Dia',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },


                                change: function (f) {

                                    if (!f.isInteracted || !f.itemData) return false;
                                    e.rowData.MachineDia = f.itemData.id;
                                    e.rowData.MachineDia = f.itemData.text;

                                    if (masterData.MachineBrandList.length > 0) {
                                        if (typeof machineBrandObj != 'undefined') {
                                            machineBrandObj.dataSource = getMachineBrandList(masterData.MachineBrandList, e.rowData.MachineGauge, e.rowData.MachineDia, 1);
                                            machineBrandObj.dataBind();
                                        }
                                    }
                                }
                            });
                            diaObj.appendTo(diaElem);
                        }
                    }
                },
                {
                    field: 'MachineDia', headerText: 'Dia', visible: (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) && isAllowEditingCell == false, allowEditing: isAllowEditingCell,
                },
                {
                    field: 'BrandID', headerText: 'Brand', displayField: "Brand", visible: (status != statusConstants.ACTIVE || _isBDS == 2) && isAllowEditingCell == true, allowEditing: isAllowEditingCell,
                    valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            machineBrandElem = document.createElement('input');
                            return machineBrandElem;
                        },
                        read: function () {
                            return machineBrandObj.value;
                        },
                        destroy: function () {
                            machineBrandObj.destroy();
                        },
                        write: function (e) {
                            machineBrandObj = new ej.dropdowns.DropDownList({
                                dataSource: [],
                                fields: { value: 'BrandID', text: 'Brand' },
                                //enabled: false,
                                placeholder: 'Select Machine Brand',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                change: function (f) {
                                    if (!f.isInteracted || !f.itemData) return false;
                                    e.rowData.BrandID = f.itemData.BrandID;
                                    e.rowData.Brand = f.itemData.Brand;

                                    $tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                                }
                            });
                            machineBrandObj.appendTo(machineBrandElem);
                        }
                    }
                },
                {
                    field: 'Brand', headerText: 'Brand', visible: (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) && isAllowEditingCell == false, allowEditing: isAllowEditingCell,
                },
                {
                    field: 'Construction', headerText: 'Construction', allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'Composition', headerText: 'Composition', allowEditing: false
                },
                {
                    field: 'Color', headerText: 'Color', width: 85, allowEditing: false
                },
                {
                    field: 'GSM', headerText: 'GSM', width: 85, allowEditing: false
                },
                {
                    field: 'FabricWidth', headerText: 'Fabric Width', width: 85, allowEditing: false
                },
                {
                    field: 'KnittingType', headerText: 'Knitting Type', width: 85, allowEditing: false
                },
                {
                    field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'ReferenceSourceName', headerText: 'Reference Source', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
                },
                {
                    field: 'ReferenceNo', headerText: 'Ref No', visible: _isBDS == 1, width: 85, allowEditing: false
                },
                {
                    field: 'ColorReferenceNo', headerText: 'Color Ref No', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
                },
                {
                    field: 'ValueName', headerText: 'Yarn Source', visible: false, allowEditing: isAllowEditingCell/* status != statusConstants.ACTIVE*/, edit: {
                        create: function () {
                            YarnSourceNameElem = document.createElement('input');
                            return YarnSourceNameElem;
                        },
                        read: function () {
                            return YarnSourceNameobj.text;
                        },
                        destroy: function () {
                            YarnSourceNameobj.destroy();
                        },
                        write: function (e) {
                            YarnSourceNameobj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.YarnSourceNameList,
                                fields: { value: 'id', text: 'text' },
                                change: function (f) {
                                    //
                                    technicalNameObj.enabled = true;
                                    //var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                    //technicalNameObj.query = tempQuery;
                                    technicalNameObj.text = null;
                                    technicalNameObj.dataBind();

                                    e.rowData.YarnSourceID = f.itemData.id;
                                    e.rowData.ValueName = f.itemData.text;
                                },
                                placeholder: 'Select one',
                                floatLabelType: 'Never'
                            });
                            YarnSourceNameobj.appendTo(YarnSourceNameElem);
                        }
                    }
                },
                //{
                //    field: 'LengthYds', headerText: 'Length (Yds)', width: 85, allowEditing: false
                //},
                //{
                //    field: 'LengthInch', headerText: 'Length (Inch)', width: 85, allowEditing: false
                //},
                {
                    field: 'DyeingType', headerText: 'Dyeing Type', width: 85, allowEditing: false
                },
                {
                    field: 'DayValidDurationName', headerText: 'Yarn Sourcing Mode', width: 120, allowEditing: false, visible: menuType == _paramType.Projection
                },
                {
                    field: 'Instruction', headerText: 'Instruction', allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'LabDipNo', headerText: 'Lab Dip No', allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'IsFabricReq', headerText: 'Fabric Req?', allowEditing: false, visible: isLabdipMenu(), displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center'
                },
                {
                    field: 'RefSourceNo', headerText: 'Ref No', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), width: 85, allowEditing: false
                },
                {
                    headerText: '', textAlign: 'Center', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), allowEditing: isAllowEditingCell, width: 40, commands: [
                        { buttonOption: { type: 'findRefSourceNo', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Ref Detail" } }
                    ]
                },
                //{
                //    field: isAdditionBulkBooking() ? 'ActualBookingQty' : 'BookingQty', headerText: 'Booking Qty (KG)', width: 85, allowEditing: false
                //},
                {
                    field: isAdditionBulkBooking() ? 'ActualBookingQty' : 'BookingQty', headerText: 'Booking Qty (KG)', editType: "numericedit",
                    edit: { params: { showSpinButton: false, decimals: 2, min: 0 } }, width: 85, allowEditing: false
                },
                {
                    field: 'FinishFabricUtilizationQty', headerText: 'Finish Fabric Utilization Qty', width: 120, allowEditing: false, propField: 'FinishFabricUtilizationQty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 40, propField: 'FinishFabricUtilizationQty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findFinishFabricUtilizationQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for finish fabric utilization qty" } }
                    ]
                },
                {
                    field: 'ReqFinishFabricQty', headerText: 'Req. Finish Fabric Qty', width: 120, allowEditing: false, visible: isBulkBookingKnittingInfoMenu()
                },
                {
                    field: 'BookingQty', headerText: 'Replacement Qty', width: 120, allowEditing: false, visible: isAdditionBulkBooking()
                },
                {
                    field: 'IsForFabric', headerText: 'For Fabric?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', allowEditing: true, visible: isAdditionBulkBooking()
                },
                {
                    field: 'TotalQty', headerText: 'Total Qty', width: 85, allowEditing: false, visible: status == false //statusConstants.COMPLETED
                },
                {
                    field: 'GreyReqQty', headerText: 'Grey Req Qty', width: 85, allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    field: 'GreyLeftOverQty', headerText: 'Grey Utilization Qty', width: 85, allowEditing: false, propField: 'GreyLeftOverQty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 40, propField: 'GreyLeftOverQty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findGreyLeftOverQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for grey left over qty" } }
                    ]
                },
                {
                    field: 'GreyProdQty', headerText: 'Grey Prod Qty', width: 95, allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: 'Dist Qty', textAlign: 'Center', visible: _isBDS != 2 && !isLabdipMenu(), allowEditing: isAllowEditingCell, width: 80, commands: [
                        { buttonOption: { type: 'UsesIn', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                    ]
                },
                {
                    headerText: 'Finishing Process', textAlign: 'Center', propField: 'finishingProcess', visible: (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation, allowEditing: isAllowEditingCell, width: 120, commands: [
                        { buttonOption: { type: 'finishingProcess', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-plus' } }
                    ]
                },
                {
                    field: 'Remarks', headerText: 'Remarks', allowEditing: _isRemarksEditable, visible: _isRemarksShow
                }
            ];
        }
        var additionalColumns = [
            {
                field: 'DeliveredQty', headerText: 'Delivered Qty', width: 85, allowEditing: false, visible: status == statusConstants.APPROVED && !isAdditionBulkBooking()
            },
            {
                field: 'DelivereyComplete', headerText: 'Is Delivered?', allowEditing: isAllowEditingCell, displayAsCheckBox: true, textAlign: 'Center', visible: status == statusConstants.APPROVED && !isAdditionBulkBooking()
            }
        ]
        columns.push.apply(columns, additionalColumns);

        if (_isBDS == 3) {
            var indexF = columns.findIndex(x => x.field == "MachineType");
            if (indexF > -1) columns.splice(indexF, 1);
            indexF = columns.findIndex(x => x.field == "TechnicalName");
            if (indexF > -1) columns.splice(indexF, 1);
        }

        var childColumns = [
            { field: 'YBChildItemID', isPrimaryKey: true, visible: false },
            { field: 'ConsumptionID', visible: false },
            { field: 'ShadeCode', headerText: 'Shade Code', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Distribution', headerText: 'Distribution', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'BookingQty', headerText: 'Booking Qty', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Allowance', headerText: 'Allowance', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'StitchLength', headerText: 'Stitch Length', width: 40, allowEditing: true, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'Specification', headerText: 'Specification', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', textAlign: 'Center', width: 40, allowEditing: false },
        ];
        ej.base.enableRipple(true);

        columns = setVisiblePropValue(columns, 1);
        columns = removeDuplicateColumns(columns);


        if (_isBDS == 2) {
            //var childColumns = await getChildColumnsForBDS2('Item', data, true);
            var childColumns = await getChildColumnsForBDS2(true);
            var contextMenuItems = [
                { text: 'Copy Information', target: '.e-content', id: 'copyBoth' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'pasteYarn' },
                { text: 'Paste Technical Information', target: '.e-content', id: 'pasteTech' },
                { text: 'Paste Finishing Process', target: '.e-content', id: 'pasteFinishingProcess' },
                { text: 'Paste Both', target: '.e-content', id: 'pasteBoth' }
            ];

            var isAllowEditing = true,
                isAllowAdding = true,
                isAllowDeleting = true,
                isChildGridAllowEditing = true,
                isChildGridAllowAdding = true,
                isChildGridAllowDeleting = true;

            var childGridToolbars = ['Add'];
            if (menuType == _paramType.BulkBookingYarnAllowance) {
                contextMenuItems = [];
                childGridToolbars = [];

                isAllowEditing = true;
                isAllowAdding = false;
                isAllowDeleting = false;

                isChildGridAllowAdding = false;
                isChildGridAllowDeleting = false;
            }
            else if (isAdditionBulkBooking()) {
                childGridToolbars = [
                    { text: 'Add Item', tooltipText: 'Add Item', prefixIcon: 'e-icons e-add', id: 'addItem' },
                    //{ text: 'Remove Item', tooltipText: 'Remove Item', prefixIcon: 'e-icons e-delete', id: 'removeItem' }
                ];

                columns.unshift({
                    headerText: 'Commands', textAlign: 'Center', width: 80, commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ]
                });
            }

            var queryStringValue = "";
            if (isYarnBookingAckMenu()) {
                queryStringValue = 'BookingChildID';
            }
            else if (status == statusConstants.PENDING || status == statusConstants.REJECT) {
                queryStringValue = "YBChildID";
            }
            else {
                queryStringValue = 'BookingChildID';
            }

            $tblChildEl = new ej.grids.Grid({
                dataSource: data,
                //allowGrouping: true,
                allowPaging: false,
                allowScrolling: false,
                allowResizing: true,
                columns: columns,
                editSettings: {
                    allowEditing: isAllowEditing,
                    allowAdding: isAllowAdding,
                    allowDeleting: isAllowDeleting,
                    mode: "Normal",
                    showDeleteConfirmDialog: true
                },
                childGrid: {
                    allowResizing: true,
                    autofitColumns: false,
                    queryString: queryStringValue,
                    //additionalQueryParams: "BookingID",
                    columns: childColumns,
                    toolbar: childGridToolbars,
                    editSettings: {
                        allowEditing: isChildGridAllowEditing,
                        allowAdding: isChildGridAllowAdding,
                        allowDeleting: isChildGridAllowDeleting,
                        mode: "Normal",
                        showDeleteConfirmDialog: true
                    },
                    toolbarClick: function (args) {

                        if (args.item.id === "addItem") {

                            var parentObj = this.parentDetails.parentRowData;
                            //if (parentObj.IsForFabric) {
                            //    toastr.error("This addition is for fabric, not for yarn (For yarn addition uncheck for fabric).");
                            //    return false;
                            //}
                            var index = $tblChildEl.getRowIndexByPrimaryKey(parentObj.BookingChildID);
                            var allYarns = _allYarnList.filter(x => x.BookingChildID == parentObj.BookingChildID);

                            if (allYarns.length == 0) {
                                toastr.error("No list found");
                                return false;
                            }
                            var currentYarns = parentObj.ChildItems;
                            var childItemIds = currentYarns.map(x => x.YBChildItemID).join(",");

                            var columns = getYarnCommanFinderColumns();
                            var fieldList = columns.map(x => x.Field).join(",");
                            var headerTextList = columns.map(x => x.HeaderText).join(",");
                            var widthList = columns.map(x => x.Width).join(",");
                            var finder = new commonFinder({
                                title: "Select Yarn",
                                pageId: pageId,
                                height: 320,
                                data: allYarns,
                                fields: fieldList,
                                headerTexts: headerTextList,
                                widths: widthList,
                                isMultiselect: true,
                                autofitColumns: true,
                                primaryKeyColumn: "YBChildItemID",
                                modalSize: "modal-lg",
                                top: "2px",
                                selectedIds: childItemIds,
                                onMultiselect: function (selectedRecords) {
                                    if (selectedRecords.length > 0) {
                                        parentObj.ChildItems = selectedRecords;
                                        $tblChildEl.updateRow(index, parentObj);
                                    }
                                }
                            });
                            finder.showModal();
                        }
                        else if (args.item.id === "removeItem") {

                        }
                    },
                    actionBegin: function (args) {
                        if (args.requestType === 'beginEdit') {
                            if (args.rowData.YDProductionMasterID > 0) {
                                toastr.error("Yarn Dyeing found, You cannot modify anything.");
                                args.cancel = true;
                            }

                        }
                        else if (args.requestType === "add") {

                            args.data.YBChildItemID = maxCol++; //getMaxIdForArray(masterData.Childs, "YBChildItemID");
                            args.data.YBChildID = this.parentDetails.parentRowData.YBChildID;
                            args.data.BookingChildID = this.parentDetails.parentRowData.BookingChildID;
                            args.data.ConsumptionID = this.parentDetails.parentRowData.ConsumptionID;


                            if (isBulkBookingKnittingInfoMenu()) {
                                args.data.GreyAllowance = this.parentDetails.parentRowData.YarnAllowance;
                                args.data.Allowance = args.data.GreyAllowance;
                            }

                            var totalDis = 0, remainDis = 0;
                            this.dataSource.forEach(l => {
                                totalDis += l.Distribution;
                            })
                            if (totalDis < 100) remainDis = 100 - totalDis;
                            else {
                                toastr.error("Distribution can not more then 100!!");
                                args.cancel = true;
                                return;
                            }
                            var netConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(remainDis) / 100);
                            var reqQty = netConsumption;
                            args.data.Distribution = remainDis;
                            args.data.BookingQty = netConsumption.toFixed(4);
                            args.data.Allowance = 0.00;

                            args.data.RequiredQty = reqQty.toFixed(2);

                            args.data.DisplayUnitDesc = "Kg";
                            args.data.SubGroupId = 1;

                            args.data.Segment1ValueId = 0;
                            args.data.Segment2ValueId = 0;
                            args.data.Segment3ValueId = 0;
                            args.data.Segment4ValueId = 0;
                            args.data.Segment5ValueId = 0;
                            args.data.Segment6ValueId = 0;
                            args.data.Segment7ValueId = 0;
                            args.data.Segment8ValueId = 0;

                            args.data = setYarnRelatedSingleField(args.data, this.parentDetails.parentRowData);

                            //getAllYarnList();
                        }
                        else if (args.requestType === "save") {

                            args.data = checkAndSetYarnValidSegmentCH(args.data, _yarnSegmentsMapping);
                            args.data = setNullIfIdNullYarnSegment(args.data);
                            if (!args.data.YD && args.data.YDAllowance > 0) {
                                args.data.YDAllowance = 0;
                                toastr.error("YD allowance only valid for Go for YD item.");
                            }
                            else if (args.data.YD && (args.data.YDAllowance < 0 || args.data.YDAllowance > 35)) {
                                toastr.error("YD allowance should be between 0 to 35.");
                                args.data.YDAllowance = 0;
                                return false;
                            }
                            //Saif_04_10_2023 END

                            args.data.GreyAllowance = getDefaultValueWhenInvalidN_Float(args.data.GreyAllowance);
                            args.data.YDAllowance = getDefaultValueWhenInvalidN_Float(args.data.YDAllowance);

                            args.data.Allowance = args.data.GreyAllowance + args.data.YDAllowance;
                            var parentObj = this.parentDetails.parentRowData;
                            if (typeof parentObj.YarnAllowance == 'undefined' && parentObj.YarnAllowance == null) {
                                parentObj.YarnAllowance = 0;
                            }
                            var reqQty = 0;
                            if (typeof parentObj.GreyProdQty != 'undefined' && parentObj.GreyProdQty != null && typeof args.data.Distribution != 'undefined' && args.data.Distribution != null) {
                                reqQty = (parentObj.GreyProdQty * (args.data.Distribution / 100)) / (1 + (parentObj.YarnAllowance / 100) - (0.5 / 100));
                            }
                            args.data.YarnReqQty = reqQty.toFixed(2);
                            if (isAdditionBulkBooking() && parentObj.IsForFabric == false) {
                                args.data.YarnReqQty = args.data.NetYarnReqQty;
                            }
                            args.data.GreyYarnUtilizationQty = 0;
                            args.data.DyedYarnUtilizationQty = 0;
                            if (typeof args.data.GreyAllowance == 'undefined' || args.data.GreyAllowance == null) {
                                args.data.GreyAllowance = 0;
                            }
                            if (typeof args.data.YDAllowance == 'undefined' || args.data.YDAllowance == null) {
                                args.data.YDAllowance = 0;
                            }

                            var isdistributionChenged = false;
                            if (args.data.Distribution != args.rowData.Distribution) {
                                isdistributionChenged = true;
                            }

                            if (isAdditionBulkBooking() && parentObj.IsForFabric == false) {
                                args.data = getYarnRelatedPropsAdditionalYarn(args.data, this.parentDetails.parentRowData, isdistributionChenged, true);
                            } else {
                                args.data = getYarnRelatedProps(args.data, this.parentDetails.parentRowData, isdistributionChenged, true);
                            }


                            var NetConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(args.data.Distribution) / 100);
                            //var reqQty = parseFloat(NetConsumption) + ((parseFloat(NetConsumption) * parseFloat(args.data.Allowance)) / 100);

                            //args.data.Distribution = args.rowData.Distribution;
                            args.data.YarnSubBrandIDs = args.rowData.YarnSubBrandIDs;
                            //args.data.YBChildID = this.parentDetails.parentRowData.YBChildID;
                            args.data.BookingQty = NetConsumption.toFixed(4);
                            args.data.RequiredQty = reqQty.toFixed(2);

                            /* Saif Stopped On 27-02-2024
                              args.rowData.Segment1ValueId = !args.data.Segment1ValueId ? 0 : args.data.Segment1ValueId;
                              args.rowData.Segment2ValueId = !args.data.Segment2ValueId ? 0 : args.data.Segment2ValueId;
                              args.rowData.Segment3ValueId = !args.data.Segment3ValueId ? 0 : args.data.Segment3ValueId;
                              args.rowData.Segment4ValueId = !args.data.Segment4ValueId ? 0 : args.data.Segment4ValueId;
                              args.rowData.Segment5ValueId = !args.data.Segment5ValueId ? 0 : args.data.Segment5ValueId;
                              args.rowData.Segment6ValueId = !args.data.Segment6ValueId ? 0 : args.data.Segment6ValueId;
                              args.rowData.Segment7ValueId = !args.data.Segment7ValueId ? 0 : args.data.Segment7ValueId;
                              args.rowData.Segment8ValueId = !args.data.Segment8ValueId ? 0 : args.data.Segment8ValueId;
                              */
                            args.data = setYarnRelatedSingleField(args.data, this.parentDetails.parentRowData);
                            args.data = setYarnSegDesc(args.data);
                            args = setSegmentValueFromRowDataToData(args);

                        }
                        else if (args.requestType === "delete") {
                            if (args.data[0].YDProductionMasterID > 0) {
                                toastr.error("Yarn Dyeing found, You cannot modify anything.");
                                args.cancel = true;
                            }
                            if (isAdditionBulkBooking()) {
                                var parentObj = this.parentDetails.parentRowData;
                                //if (parentObj.IsForFabric) {
                                //    toastr.error("This addition is for fabric, not for yarn (For yarn addition uncheck for fabric).");
                                //    var index = $tblChildEl.getRowIndexByPrimaryKey(parentObj.ConsumptionID);
                                //    parentObj.ChildItems = _allYarnList.filter(x => x.BookingChildID == parentObj.BookingChildID);
                                //    $tblChildEl.updateRow(index, parentObj);
                                //    return false;
                                //}
                            }
                        }
                    },
                    load: loadYarnBookingChildItems,
                    commandClick: childCommandClickChild2,
                },
                enableContextMenu: true,
                contextMenuItems: contextMenuItems,
                contextMenuClick: function (args) {
                    if (args.item.id === 'copyBoth') {

                        fabYarnItem.ParentInfo = objectCopy(args.rowInfo.rowData);
                        fabYarnItem.ChildItems = objectCopy(args.rowInfo.rowData.ChildItems);

                        if (typeof fabYarnItem.ChildItems === "undefined" || fabYarnItem.ChildItems == null || fabYarnItem.ChildItems.length == 0) {
                            fabYarnItem.ChildItems = [];
                        }

                        //Copy Finishing Process
                        var indexF = masterData.Childs.findIndex(x => x.BookingChildID == fabYarnItem.ParentInfo.BookingChildID);
                        if (indexF > -1) {
                            masterData.Childs[indexF].PreFinishingProcessChilds = getValidList(masterData.Childs[indexF].PreFinishingProcessChilds);
                            masterData.Childs[indexF].PostFinishingProcessChilds = getValidList(masterData.Childs[indexF].PostFinishingProcessChilds);

                            fabYarnItem.PreFinishingProcessChilds = DeepClone(masterData.Childs[indexF].PreFinishingProcessChilds);
                            fabYarnItem.PostFinishingProcessChilds = DeepClone(masterData.Childs[indexF].PostFinishingProcessChilds);
                        }
                    }
                    else if (args.item.id === 'pasteYarn') {
                        if (typeof fabYarnItem.ChildItems === "undefined" || fabYarnItem.ChildItems == null || fabYarnItem.ChildItems.length == 0) {
                            //toastr.error("Please copy yarn item first!!");
                            return;
                        }
                        for (var i = 0; i < fabYarnItem.ChildItems.length; i++) {
                            var copiedItem = objectCopy(fabYarnItem.ChildItems[i]);

                            copiedItem.YBChildItemID = _fbChildItemID++;
                            copiedItem.YBChildID = args.rowInfo.rowData.YBChildID;
                            copiedItem.YBookingID = args.rowInfo.rowData.YBookingID;

                            copiedItem.BookingChildID = args.rowInfo.rowData.BookingChildID;
                            copiedItem.BookingID = args.rowInfo.rowData.BookingID;

                            var parentInfo = {
                                GreyProdQty: 0
                            };
                            copiedItem = setYarnRelatedSingleField(copiedItem, parentInfo);
                            args.rowInfo.rowData.ChildItems.push(copiedItem);
                        }
                        $tblChildEl.refresh();
                    }
                    else if (args.item.id === 'pasteTech') {

                        if (typeof fabYarnItem.ParentInfo === "undefined" || fabYarnItem.ParentInfo == null) {
                            //toastr.error("Please copy technical info first!!");
                            return;
                        }

                        args.rowInfo.rowData.MachineTypeId = fabYarnItem.ParentInfo.MachineTypeId;
                        args.rowInfo.rowData.MachineType = fabYarnItem.ParentInfo.MachineType;
                        args.rowInfo.rowData.TechnicalNameId = fabYarnItem.ParentInfo.TechnicalNameId;
                        args.rowInfo.rowData.TechnicalName = fabYarnItem.ParentInfo.TechnicalName;
                        args.rowInfo.rowData.MachineGauge = fabYarnItem.ParentInfo.MachineGauge;
                        args.rowInfo.rowData.MachineDia = fabYarnItem.ParentInfo.MachineDia;
                        args.rowInfo.rowData.BrandID = fabYarnItem.ParentInfo.BrandID;
                        args.rowInfo.rowData.Brand = fabYarnItem.ParentInfo.Brand;

                        $tblChildEl.refresh();

                    }
                    else if (args.item.id === 'pasteFinishingProcess') {

                        if (typeof fabYarnItem.ParentInfo === "undefined" || fabYarnItem.ParentInfo == null) {
                            //toastr.error("Please copy technical info first!!");
                            return;
                        }
                        pasteFinishingProcess(args.rowInfo.rowData.BookingChildID, fabYarnItem);
                        $tblChildEl.refresh();
                    }
                    else if (args.item.id === 'pasteBoth') {
                        if (typeof fabYarnItem.ParentInfo === "undefined" || fabYarnItem.ParentInfo == null) {
                            //toastr.error("Please copy first!!");
                            return;
                        }

                        args.rowInfo.rowData.MachineTypeId = fabYarnItem.ParentInfo.MachineTypeId;
                        args.rowInfo.rowData.MachineType = fabYarnItem.ParentInfo.MachineType;
                        args.rowInfo.rowData.TechnicalNameId = fabYarnItem.ParentInfo.TechnicalNameId;
                        args.rowInfo.rowData.TechnicalName = fabYarnItem.ParentInfo.TechnicalName;
                        args.rowInfo.rowData.MachineGauge = fabYarnItem.ParentInfo.MachineGauge;
                        args.rowInfo.rowData.MachineDia = fabYarnItem.ParentInfo.MachineDia;
                        args.rowInfo.rowData.BrandID = fabYarnItem.ParentInfo.BrandID;
                        args.rowInfo.rowData.Brand = fabYarnItem.ParentInfo.Brand;

                        for (var i = 0; i < fabYarnItem.ChildItems.length; i++) {
                            var copiedItem = objectCopy(fabYarnItem.ChildItems[i]);

                            copiedItem.YBChildItemID = _fbChildItemID++;
                            copiedItem.YBChildID = args.rowInfo.rowData.YBChildID;
                            copiedItem.YBookingID = args.rowInfo.rowData.YBookingID;

                            copiedItem.BookingChildID = args.rowInfo.rowData.BookingChildID;
                            copiedItem.BookingID = args.rowInfo.rowData.BookingID;

                            var parentInfo = {
                                GreyProdQty: 0
                            };
                            copiedItem = setYarnRelatedSingleField(copiedItem, parentInfo);
                            args.rowInfo.rowData.ChildItems.push(copiedItem);
                        }

                        pasteFinishingProcess(args.rowInfo.rowData.BookingChildID, fabYarnItem);

                        $tblChildEl.refresh();
                    }
                },
                commandClick: childCommandClick2,
                actionBegin: function (args) {

                    if (args.requestType === "save") {

                        args.data.MachineTypeId = args.rowData.MachineTypeId;
                        args.data.MachineType = args.rowData.MachineType;
                        args.data.KTypeId = args.rowData.KTypeId;

                        args.data.TechnicalNameId = args.rowData.TechnicalNameId;
                        args.data.TechnicalName = args.rowData.TechnicalName;

                        args.data.MachineGauge = args.rowData.MachineGauge;
                        args.data.MachineDia = args.rowData.MachineDia;

                        if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
                            if (args.data.YarnAllowance < 0 || args.data.YarnAllowance > 35) {
                                args.data.YarnAllowance = 0;
                                toastr.error("Yarn allowance should be between 0 to 35.");
                                return false;
                            }
                            args.data.ChildItems.forEach(x => {
                                x.GreyAllowance = args.data.YarnAllowance;
                                x.Allowance = parseFloat(x.GreyAllowance) + parseFloat(x.YDAllowance);
                            });
                            args.data.GreyReqQty = (args.data.ReqFinishFabricQty * (1 + (args.data.YarnAllowance / 100) - (0.5 / 100))).toFixed(2);

                            args.data.GreyProdQty = (args.data.GreyReqQty - args.data.GreyLeftOverQty).toFixed(2);

                            setModifiedChildData(args);
                            $tblChildEl.updateRow(args.rowIndex, args.data);
                            setChildData(args.data, 1);
                        }
                        if (isAdditionBulkBooking()) {
                            args.data = getAdditionBulkBookingData(args.data, 1);
                            setAdditionalAllowance(args.data);
                            setChildData(args.data, 1);
                        }
                    }
                },
                actionComplete: function (args) {

                }
            });


        } else {
            $tblChildEl = new ej.grids.Grid({
                dataSource: data,

                //allowGrouping: true,
                allowPaging: false,
                allowScrolling: false,
                //height: 300, // Adjust this height based on your layout
                //frozenRows: 0, // Keep the header row fixed

                allowResizing: true,
                columns: columns,
                commandClick: childCommandClick,
                editSettings: { allowEditing: _isBDS == 3 ? false : true, allowAdding: _isBDS == 3 ? false : true, allowDeleting: _isBDS == 3 ? false : true, mode: "Normal", showDeleteConfirmDialog: true },
                recordClick: function (args) {

                    if (args.column && args.column.field == "TotalDays") {

                        if (_isLabDipAck_RnD) {
                            args.rowData.CriteriaNames = args.rowData.CriteriaNames.filter(x => x.CriteriaName == "Material" || x.CriteriaName == "Preprocess");
                        }
                        _oRow = args.rowData;

                        _index = args.rowIndex;
                        _modalFrom = subGroupNames.FABRIC;
                        // initPlanningTable(_oRow.FBAChildPlannings, _oRow.CriteriaIDs);
                        initCriteriaIDTable(_oRow.CriteriaNames, _oRow.FBAChildPlannings, _oRow.FBAChildPlanningsWithIds, _oRow.BookingChildID);
                        $modalCriteriaEl.modal('show');
                    } else if (args.column && args.column.field == "MachineType") {
                        var oRow = args.rowData;
                        var machineTypes = masterData.MCTypeForFabricList;
                        var aaa = args.rowData.MachineType;
                        // fields: { value: 'id', text: 'text' },
                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "save") {

                        args.data = setArgDataValues(args.data, args.rowData);
                    }
                },
                enableContextMenu: true,
                contextMenuItems: [
                    { text: 'Copy TNA', target: '.e-content', id: 'copy' },
                    { text: 'Paste TNA', target: '.e-content', id: 'paste' }
                ],
                contextMenuClick: function (args) {
                    if (args.item.id === 'copy') {
                        itemTNAInfo = objectCopy(args.rowInfo.rowData);
                        if (itemTNAInfo.length == 0) {
                            toastr.error("No TNA information found to copy!!");
                            return;
                        }
                        var selctedRowCriterias = idsList.filter(x => x.SubGroupWiseIndex == args.rowInfo.rowIndex && x.SubGroupName == "Fabric");
                        if (selctedRowCriterias) {
                            idsListCopyFabric = JSON.parse(JSON.stringify(selctedRowCriterias));
                        }
                    }
                    else if (args.item.id === 'paste') {
                        var rowIndex = args.rowInfo.rowIndex;
                        if (itemTNAInfo == null || itemTNAInfo.length == 0) {
                            toastr.error("Please copy first!!");
                            return;
                        } else {
                            var pasteObject = objectCopy(itemTNAInfo);
                            var preSubContactDays = 0,
                                subContactDays = 0;
                            if (pasteObject.IsSubContact) preSubContactDays = 14;
                            if (args.rowInfo.rowData.IsSubContact) subContactDays = 14;
                            args.rowInfo.rowData.TotalDays = pasteObject.TotalDays - pasteObject.StructureDays + args.rowInfo.rowData.StructureDays - preSubContactDays + subContactDays;
                            var dt = new Date();
                            dt.setDate(dt.getDate() + args.rowInfo.rowData.TotalDays);
                            args.rowInfo.rowData.DeliveryDate = dt;

                            idsListCopyFabric.forEach(x => {
                                var indexFCC = idsList.findIndex(y => y.SubGroupWiseIndex == args.rowInfo.rowIndex && y.SubGroupName == "Fabric" && y.CriteriaName == x.CriteriaName);
                                if (indexFCC > -1) {
                                    idsList[indexFCC].CriteriaIDs = x.CriteriaIDs;
                                }
                            });

                            //args.rowInfo.rowData.CriteriaIDs = pasteObject.CriteriaIDs;

                            args.rowInfo.rowData.CriteriaIDs = idsList.filter(x => x.SubGroupWiseIndex == args.rowInfo.rowIndex
                                && x.SubGroupName == "Fabric")
                                .filter(x => x.CriteriaIDs.length > 0)
                                .map(x => x.CriteriaIDs).join(",");

                            console.log("CIDs=" + args.rowInfo.rowData.CriteriaIDs);

                            _oRow = args.rowInfo.rowData;
                            updateCriteriaIDTable(_oRow, pasteObject);
                            //$tblChildEl.refresh();
                            $tblChildEl.updateRow(args.rowInfo.rowIndex, _oRow);

                        }
                    }
                }
            });
        }
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }

    async function childCommandClick2(e) {

        selectedBookingChildID = e.rowData.BookingChildID;
        if (e.commandColumn.buttonOption.type == 'findGreyLeftOverQty') {

            ////e.preventDefault();
            selectedCompositionId = e.rowData.CompositionId;
            selectedGSMId = e.rowData.GSMId

            // Alamin
            var childData = e.rowData;
            __GSMNumber = childData.GSM,
                __GSMId = childData.GSMId, __CompositionId = childData.CompositionId; __ConstructionId = childData.ConstructionId;
            __SubGroupID = childData.SubGroupID;


            if (GFUtilizationSummary == null) {
                GFUtilizationSummary = [];
            }

            GFUtilizationSummary = childData.GreyFabricUtilizationPopUpList;
            initGFUtilization(GFUtilizationSummary);
            $modalGFUtilizationInfoEL.modal('show');


        }
        else if (e.commandColumn.buttonOption.type == 'findRefSourceNo') {
            var response = await axios.get(`/api/bds-acknowledge/getRefSourceItem/${e.rowData.BookingID}/${e.rowData.ConsumptionID}`);
            var list = response.data.Items;

            if (list.length == 0) {
                return toastr.error("No Reference Found.");
            }

            var finder = new commonFinder({
                title: "Select Ref Source",
                pageId: pageId,
                height: 320,
                data: list,
                fields: "BookingNo,RefSourceNo,Composition,Construction,Color,GSM,RefSourceName",
                headerTexts: "BookingNo,RefSourceNo,Composition,Construction,Color,GSM,RefSourceName",
                widths: "100,100,100,100,100,100,100",
                editTypes: "text,text,text,text,text,text,text",
                isMultiselect: false,
                autofitColumns: true,
                primaryKeyColumn: "BRefDetailsID",
                modalSize: "modal-md",
                top: "2px",
                onSelect: function (record) {
                    finder.hideModal();

                    var childData = e.rowData;
                    childData.RefSourceID = record.rowData.RefSourceID;
                    childData.RefSourceNo = record.rowData.RefSourceNo;

                    var bookingNo = record.rowData.RefSourceNo;
                    var subGroupId = record.rowData.SubGroupID;
                    var consumptionID = record.rowData.ConsumptionID;
                    var itemMasterID = record.rowData.ItemMasterID;
                    var construction = record.rowData.Construction;


                    getRefInfos(childData, bookingNo, subGroupId, consumptionID, itemMasterID, construction);
                }
            });
            finder.showModal();
        }
        else if (e.commandColumn.buttonOption.type == 'finishingProcess') {
            //FinishFabricUtilizationQty

            $(pageIdWithHash).find('#divModalFP').modal('show');
            $(pageIdWithHash).find(".fpProps").each(function () {
                var propId = $(this).attr("id");
                var parentPropId = propId.slice(0, -2);
                $(this).val($formEl.find("#" + parentPropId).val());
            });

            var data = e.rowData;
            if (data.SubGroupId == 1 || data.SubGroupID == 1) {
                _fpBookingChildID = data.BookingChildID;
            }
            _fpRow = e.rowData;
            _fpBookingChildColorID = data.ColorID;
            data.MachineType = data.MachineType == "Empty" ? "" : data.MachineType;
            data.TechnicalName = data.TechnicalName == "Empty" ? "" : data.TechnicalName;

            $(pageIdWithHash).find("#MachineNameFP").val(data.MachineType);
            $(pageIdWithHash).find("#TechnicalNameFP").val(data.TechnicalName);
            $(pageIdWithHash).find("#GsmFP").val(data.GSM);
            $(pageIdWithHash).find("#CompositionFP").val(data.Composition);

            $(pageIdWithHash).find("#spnColorName").text(data.Color);

            masterData.Childs = typeof masterData.Childs === "undefined" ? masterData.FBookingChild : masterData.Childs;

            var indexF = -1;
            if (data.SubGroupId == 1 || data.SubGroupID == 1) {
                indexF = masterData.Childs.findIndex(x => x.BookingChildID == _fpBookingChildID);
            }
            else {
                indexF = masterData.Childs.findIndex(x => x.Construction == _fpRow.Construction && x.Composition == _fpRow.Composition && x.Color == _fpRow.Color);
            }
            //_fpBookingChildColorID
            if (indexF > -1) {
                if (typeof masterData.Childs[indexF].PreFinishingProcessChilds === "undefined") {
                    masterData.Childs[indexF].PreFinishingProcessChilds = [];
                }
                if (typeof masterData.Childs[indexF].PostFinishingProcessChilds === "undefined") {
                    masterData.Childs[indexF].PostFinishingProcessChilds = [];
                }
                initChildTableFP(masterData.Childs[indexF].PreFinishingProcessChilds, true);
                initChildTableColorFP(masterData.Childs[indexF].PostFinishingProcessChilds, true);
            }
            else {
                initChildTableFP([], true);
                initChildTableColorFP([], true);
            }
        }
        else if (e.commandColumn.buttonOption.type == 'findFinishFabricUtilizationQty') {

            if (isBulkBookingKnittingInfoMenu() ||
                isAdditionBulkBooking()) {
                var childData = e.rowData;

                __GSMNumber = childData.GSM,
                    __GSMId = childData.GSMId, __CompositionId = childData.CompositionId; __ConstructionId = childData.ConstructionId;
                __SubGroupID = childData.SubGroupID;

                if (FinishFabricUtilizationDataList == null) {
                    FinishFabricUtilizationDataList = [];
                }

                FinishFabricUtilizationDataList = childData.FinishFabricUtilizationPopUpList;
                FinishFabricUtilizationDataList = DeepClone(FinishFabricUtilizationDataList.filter(item => item.FinishFabricUtilizationQTYinkg != 0));
                initFinishFabricUtilizationTable(FinishFabricUtilizationDataList);
                $modalFFUtilizationInfoEL.modal('show');
            }

            //initStockInfo(null);
            //initStockSummary(null);

            //selectedChildIndex = e.rowIndex;
            /* if (_isYP == true && status == statusConstants.PENDING) {*/
            //$("#divYarnInfo").show();
            //axios.get(`/api/yarn-allocation/GetStock/${e.rowData.ItemMasterID}`)
            //    .then(function (response) {
            //        
            //        stockData = response.data;
            //        initStockInfo(stockData);
            //        //initStockInfo(null);
            //        stockSummary = masterData.Childs[selectedChildIndex].ChildItems;
            //        initStockSummary(stockSummary);
            //        //initStockSummary(null);
            //        $modalUtilizationPropoasalEl.modal('show');
            //    })
            //    .catch(function (error) {
            //        toastr.error(error.response.data.Message);
            //    });
            //}
            //else {
            //    $("#divYarnInfo").hide();;
            //    axios.get(`/api/yarn-allocation/GetAllocatedStock/${args.rowData.AllocationChildID}`)
            //        .then(function (response) {
            //            stockSummary = response.data;
            //            initStockSummary(stockSummary);
            //            $modalUtilizationPropoasalEl.modal('show');
            //        })
            //        .catch(function (error) {
            //            toastr.error(error.response.data.Message);
            //        });
            //}
        }

        else if (e.commandColumn.buttonOption.type == 'findAdditionalReplacementQty') {

            if (isAdditionBulkBooking()) {
                var childData = e.rowData;
                if (!childData.IsForFabric) {
                    return toastr.error(`This operation is only valid for fabric item.`);
                }

                __GSMNumber = childData.GSM;
                __GSMId = childData.GSMId;
                __CompositionId = childData.CompositionId;
                __ConstructionId = childData.ConstructionId;
                __SubGroupID = childData.SubGroupID;

                if (AdditionalReplacementDataList == null) AdditionalReplacementDataList = [];
                AdditionalReplacementDataList = childData.AdditionalReplacementPOPUPList;
                initAdditioalReplacementQTY(AdditionalReplacementDataList);
                $modalFBAckChildReplacementInfoEL.modal('show');
            }
        }
    }
    function removeDuplicateColumns(columns) {
        if (menuType == _paramType.BulkBookingCheck) {
            var fieldList = [];
            columns.map(c => {
                fieldList.push(c.field);
            });
            var indexList = fieldList.multiIndexOfSameItem("TechnicalName");
            if (indexList.length > 1) {
                var maxIndex = Math.max.apply(Math, indexList);
                if (maxIndex > 0) columns.splice(maxIndex, 1);
            }
            fieldList = [];
            columns.map(c => {
                fieldList.push(c.field);
            });
            indexList = fieldList.multiIndexOfSameItem("MachineType");
            if (indexList.length > 1) {
                var maxIndex = Math.max.apply(Math, indexList);
                if (maxIndex > 0) columns.splice(maxIndex, 1);
            }
            fieldList = [];
            columns.map(c => {
                fieldList.push(c.field);
            });
            indexList = fieldList.multiIndexOfSameItem("MachineGauge");
            if (indexList.length > 1) {
                var maxIndex = Math.max.apply(Math, indexList);
                if (maxIndex > 0) columns.splice(maxIndex, 1);
            }
            fieldList = [];
            columns.map(c => {
                fieldList.push(c.field);
            });
            indexList = fieldList.multiIndexOfSameItem("MachineDia");
            if (indexList.length > 1) {
                var maxIndex = Math.max.apply(Math, indexList);
                if (maxIndex > 0) columns.splice(maxIndex, 1);
            }
        }
        return columns;
    }

    function getAdditionBulkBookingData(obj, subGroupId) {
        if (subGroupId == 1) {
            if (!obj.IsForFabric) {
                masterData.FBookingChild.find(x => x.BookingChildID == obj.BookingChildID).AdditionalReplacementPOPUPList = [];
                masterData.FBookingChild.find(x => x.BookingChildID == obj.BookingChildID).BookingQty = 0;

                obj.AdditionalReplacementPOPUPList = [];
                obj.BookingQty = 0;
            }
        }
        else if (subGroupId == 11) {
            if (!obj.IsForFabric) {
                masterData.FBookingChildCollor.find(x => x.BookingChildID == obj.BookingChildID).AdditionalReplacementPOPUPList = [];
                masterData.FBookingChildCollor.find(x => x.BookingChildID == obj.BookingChildID).BookingQty = 0;

                obj.AdditionalReplacementPOPUPList = [];
                obj.BookingQty = 0;
            }
        }
        else if (subGroupId == 12) {
            if (!obj.IsForFabric) {
                masterData.FBookingChildCuff.find(x => x.BookingChildID == obj.BookingChildID).AdditionalReplacementPOPUPList = [];
                masterData.FBookingChildCuff.find(x => x.BookingChildID == obj.BookingChildID).BookingQty = 0;

                obj.AdditionalReplacementPOPUPList = [];
                obj.BookingQty = 0;
            }
        }
        return obj;
    }

    //function getChildFinishFabricUtilization(DataList, YBChildID) {
    //    var filterFinishFabricUtilizationList = $.grep(DataList, function (h) {
    //        return parseInt(h.YBChildID) == parseInt(YBChildID)
    //    });
    //    return filterFinishFabricUtilizationList;
    //}
    function initStockInfo(data) {
        if ($tblStockInfoEl) $tblStockInfoEl.destroy();

        $tblStockInfoEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowPaging: false,
            editSettings: { allowAdding: false, allowEditing: true, allowDeleting: false, mode: "Normal" },
            columns: [
                //{
                //    field: 'YarnStockSetId', headerText: 'YarnStockSetId', visible: false
                //},
                {
                    field: 'EWO', headerText: 'EWO/Order No', width: 100, allowEditing: false
                },
                {
                    field: 'BatchNo', headerText: 'Batch No', width: 100, allowEditing: false
                },
                {
                    field: 'Color', headerText: 'Color', width: 100, allowEditing: false
                },
                {
                    field: 'Buyer', headerText: 'Buyer', width: 100, allowEditing: false
                },
                {
                    field: 'FabricConstruction', headerText: 'Fabric Construction', width: 100, allowEditing: false
                },
                {
                    field: 'Width', headerText: 'Width', width: 100, allowEditing: false
                },
                {
                    field: 'GSM', headerText: 'GSM', width: 100, allowEditing: false
                },
                {
                    field: 'Utilization Qty', headerText: 'UtilizationQty', width: 100, allowEditing: false
                },
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {

                    if (args.data.AdvanceAllocationQty > args.data.AdvanceStockQty || args.data.SampleAllocationQty > args.data.SampleStockQty || args.data.LiabilitiesAllocationQty > args.data.LiabilitiesStockQty || args.data.LeftoverAllocationQty > args.data.LeftoverStockQty) {
                        toastr.error('Allocation Qty should not greater than Stock Qty!!!');
                        return false;
                    }
                    var indexF = -1;
                    if (stockSummary != null) {
                        indexF = stockSummary.findIndex(x => x.YarnStockSetId == args.data.YarnStockSetId);
                    }
                    else {
                        stockSummary = [];
                    }

                    if (indexF > -1) {
                        stockSummary.splice(indexF, 1);
                        stockSummary.push(DeepClone(args.data));
                    }
                    else {
                        stockSummary.push(DeepClone(args.data));
                    }

                    initStockSummary(stockSummary);
                    args.data.AllocatedQty = args.rowData.AllocatedQty - args.data.AllocatedQty;
                }

            },
        });
        $tblStockInfoEl.refreshColumns;
        $tblStockInfoEl.appendTo(tblStockInfoId);
    }
    function initStockSummary(data) {
        return false;
        if ($tblStockSummaryEl) $tblStockSummaryEl.destroy();
        //if (data.length == 0)
        //    data = null;
        /*var obj = {
            Spinner: 'A & A Fashion',
            PhysicalLot: 'AA546',
            PhysicalCount: '75D AA 122',
            NumericCount: '75',
            POCount: '75D AA 122',
            AllocatedQty: 300,
            OwnerUnit: 'EFL',
            ImportStatus: 'Local',
            Remarks: ''

        }
        var obj2 = {
            Spinner: 'SQUARE TEXTILES PLC.',
            PhysicalLot: 'BG435',
            PhysicalCount: '1120D',
            NumericCount: '1120',
            POCount: '1120D',
            AllocatedQty: 500,
            OwnerUnit: 'EFL',
            ImportStatus: 'Local',
            Remarks: ''

        }
        var obj3 = {
            Spinner: 'SPORT KING INDIA LTD.',
            PhysicalLot: 'FG453',
            PhysicalCount: '1350D',
            NumericCount: '1350',
            POCount: '1350D',
            AllocatedQty: 200,
            OwnerUnit: 'EFL',
            ImportStatus: 'Local',
            Remarks: ''

        }
        var obj4 = {
            Spinner: 'GHCL LTD.',
            PhysicalLot: 'HJ543',
            PhysicalCount: '3429D',
            NumericCount: '3429',
            POCount: '3429D',
            AllocatedQty: 350,
            OwnerUnit: 'EFL',
            ImportStatus: 'Local',
            Remarks: ''

        }
        var obj5 = {
            Spinner: 'GTN TEXTILES LTD.',
            PhysicalLot: 'KY956',
            PhysicalCount: '2435D',
            NumericCount: '2435',
            POCount: '2435D',
            AllocatedQty: 600,
            OwnerUnit: 'EFL',
            ImportStatus: 'Local',
            Remarks: ''

        }
        var obj6 = {
            Spinner: 'AA YARN MILLS LTD.',
            PhysicalLot: 'SD436',
            PhysicalCount: '7634D',
            NumericCount: '7634',
            POCount: '7634D',
            AllocatedQty: 550,
            OwnerUnit: 'EFL',
            ImportStatus: 'Local',
            Remarks: ''

        }
        data = [];
        //data.push(obj);
        data.push(obj2);
        //data.push(obj3);
        //data.push(obj4);
        data.push(obj5);
        //data.push(obj6);*/
        $tblStockSummaryEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowPaging: false,
            editSettings: { allowAdding: false, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Commands', width: 80, commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        //{ type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        //{ type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                {
                    field: 'YarnStockSetId', headerText: 'YarnStockSetId', visible: false
                },
                {
                    field: 'YarnCategory', headerText: 'Yarn Category', width: 100, allowEditing: false
                },
                {
                    field: 'NumericCount', headerText: 'Numeric Count', width: 100, allowEditing: false
                },
                {
                    field: 'Spinner', headerText: 'Spinner', width: 100, allowEditing: false
                },
                {
                    field: 'PhysicalCount', headerText: 'PhysicalCount', width: 100, allowEditing: false
                },
                {
                    field: 'PhysicalLot', headerText: 'PhysicalLot', width: 100, allowEditing: false
                },
                {
                    field: 'AdvanceStockQty', headerText: 'Advance Stock Qty', width: 100, visible: _isYP == true && status == statusConstants.PENDING, allowEditing: false
                },
                {
                    field: 'AdvanceAllocationQty', headerText: 'Advance Allocation Qty', width: 100, allowEditing: false
                },
                {
                    field: 'SampleStockQty', headerText: 'Sample Stock Qty', width: 100, visible: _isYP == true && status == statusConstants.PENDING, allowEditing: false
                },
                {
                    field: 'SampleAllocationQty', headerText: 'Sample Allocation Qty', width: 100, allowEditing: false
                },
                {
                    field: 'LeftoverStockQty', headerText: 'Leftover Stock Qty', width: 100, visible: _isYP == true && status == statusConstants.PENDING, allowEditing: false
                },
                {
                    field: 'LeftoverAllocationQty', headerText: 'Leftover Allocation Qty', width: 100, allowEditing: false
                },
                {
                    field: 'LiabilitiesStockQty', headerText: 'Liabilities Stock Qty', width: 100, visible: _isYP == true && status == statusConstants.PENDING, allowEditing: false
                },
                {
                    field: 'LiabilitiesAllocationQty', headerText: 'Liabilities Allocation Qty', width: 100, allowEditing: false
                },
                {
                    field: 'POPrice', headerText: 'PO Price', width: 100, allowEditing: false
                },
                {
                    field: 'YarnAge', headerText: 'Yarn Age', width: 100, allowEditing: false
                },
                {
                    field: 'TestResult', headerText: 'Test Result', width: 100, allowEditing: false
                },
                {
                    field: 'TestResultComments', headerText: 'Comments', width: 100, allowEditing: false
                },
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {

                }
                else if (args.requestType === "delete") {

                    var indexF = stockData.findIndex(x => x.YarnStockSetId == args.data[0].YarnStockSetId);
                    stockData[indexF].AdvanceAllocationQty = 0;
                    stockData[indexF].SampleAllocationQty = 0;
                    stockData[indexF].LeftoverAllocationQty = 0;
                    stockData[indexF].LiabilitiesAllocationQty = 0;
                    initStockInfo(stockData);
                }
            },
        });
        $tblStockSummaryEl.refreshColumns;
        $tblStockSummaryEl.appendTo(tblStockSummaryId);
    }

    function pasteFinishingProcess(bookingChildID, copiedItem) {
        var indexF = masterData.Childs.findIndex(x => x.BookingChildID == bookingChildID);
        if (indexF > -1) {
            masterData.Childs[indexF].PreFinishingProcessChilds = copiedItem.PreFinishingProcessChilds;
            masterData.Childs[indexF].PreFinishingProcessChilds.map(x => {
                x.FPChildID = _fbChildID++;
                x.BookingChildID = bookingChildID;
                x.FinishingProcessChildItems = [];
            });
            masterData.Childs[indexF].PostFinishingProcessChilds = copiedItem.PostFinishingProcessChilds;
            masterData.Childs[indexF].PostFinishingProcessChilds.map(x => {
                if (x.FMSID == null) x.FMSID = 0;
                x.FPChildID = _fbChildID++;
                x.BookingChildID = bookingChildID;
                x.FinishingProcessChildItems = [];
            });
        }
    }
    function pasteFinishingProcessCollarCuff(rowData, copiedItem) {
        //var indexF = masterData.Childs.findIndex(x => x.ConsumptionID == consumptionID);

        indexF = masterData.Childs.findIndex(x => x.Construction == rowData.Construction && x.Composition == rowData.Composition && x.Color == rowData.Color);

        if (indexF > -1) {
            masterData.Childs[indexF].PreFinishingProcessChilds = copiedItem.PreFinishingProcessChilds;
            masterData.Childs[indexF].PreFinishingProcessChilds.map(x => {
                x.FPChildID = _fbChildID++;
                x.BookingChildID = rowData.BookingChildID;
                x.FinishingProcessChildItems = [];
            });
            masterData.Childs[indexF].PostFinishingProcessChilds = copiedItem.PostFinishingProcessChilds;
            masterData.Childs[indexF].PostFinishingProcessChilds.map(x => {
                if (x.FMSID == null) x.FMSID = 0;
                x.FPChildID = _fbChildID++;
                x.BookingChildID = rowData.BookingChildID;
                x.FinishingProcessChildItems = [];
            });
        }
    }
    function getYarnCommanFinderColumns() {

        return [
            {
                HeaderText: "Composition",
                Field: "Segment1ValueDesc",
                Width: 100
            },
            {
                HeaderText: "Yarn Type",
                Field: "Segment2ValueDesc",
                Width: 100
            },
            {
                HeaderText: "Manufacturing Process",
                Field: "Segment3ValueDesc",
                Width: 100
            },
            {
                HeaderText: "Sub Process",
                Field: "Segment4ValueDesc",
                Width: 100
            },
            {
                HeaderText: "Quality Parameter",
                Field: "Segment5ValueDesc",
                Width: 100
            },
            {
                HeaderText: "Count",
                Field: "Segment6ValueDesc",
                Width: 100
            },
            {
                HeaderText: "Shade Code",
                Field: "ShadeCode",
                Width: 100
            },
            {
                HeaderText: "Stitch Length",
                Field: "StitchLength",
                Width: 100
            },
            {
                HeaderText: "Distribution",
                Field: "Distribution",
                Width: 100
            },
            {
                HeaderText: "Allowance",
                Field: "Allowance",
                Width: 100
            }
        ];
    }

    function setYarnSegDesc(obj) {
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
        obj = getYarnCategory(obj);
        return obj;
    }
    function setSegmentValueFromRowDataToData(args) {

        args.rowData.Segment1ValueId = !args.data.Segment1ValueId ? 0 : args.data.Segment1ValueId;
        args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
        args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
        args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
        args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
        args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
        args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
        args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

        //args.data.Segment1ValueDesc = args.rowData.Segment1ValueDesc;
        args.data.Segment2ValueDesc = args.rowData.Segment2ValueDesc;
        args.data.Segment3ValueDesc = args.rowData.Segment3ValueDesc;
        args.data.Segment4ValueDesc = args.rowData.Segment4ValueDesc;
        args.data.Segment5ValueDesc = args.rowData.Segment5ValueDesc;
        args.data.Segment6ValueDesc = args.rowData.Segment6ValueDesc;
        args.data.Segment7ValueDesc = args.rowData.Segment7ValueDesc;
        args.data.Segment7ValueDesc = args.rowData.Segment7ValueDesc;
        if (args.data.ShadeCode == null) {
            args.data.ShadeCode = '';
        }
        args.data.YarnCategory = GetYarnShortForm(args.data.Segment1ValueDesc, args.data.Segment2ValueDesc, args.data.Segment3ValueDesc, args.data.Segment4ValueDesc, args.data.Segment5ValueDesc, args.data.Segment6ValueDesc, args.data.ShadeCode);
        return args;
    }

    function setModifiedChildData(args) {

        //MCTypeForFabricList = Array(8) [Object, Object, Object, …]

        args.data.SubGroupId = getDefaultValueWhenInvalidN(args.data.SubGroupId);
        args.data.SubGroupID = getDefaultValueWhenInvalidN(args.data.SubGroupID);
        args.data.SubGroupName = getDefaultValueWhenInvalidS(args.data.SubGroupName);

        if (args.data.SubGroupId == 1 || args.data.SubGroupID == 1 || args.data.SubGroupName == "Fabric") {
            if (masterData.MCTypeForFabricList != null && masterData.MCTypeForFabricList.length > 0) {
                var objM = masterData.MCTypeForFabricList.find(x => x.id == args.rowData.MachineTypeId);
                if (typeof objM !== "undefined" && objM != null) {
                    args.data.MachineType = objM.text;
                } else {
                    args.data.MachineType = "";
                }
            }
        }
        else if (args.data.SubGroupId != 1) {
            if (masterData.MCTypeForOtherList != null && masterData.MCTypeForOtherList.length > 0) {
                var objM = masterData.MCTypeForOtherList.find(x => x.id == args.rowData.MachineTypeId);
                if (typeof objM !== "undefined" && objM != null) {
                    args.data.MachineType = objM.text;
                } else {
                    args.data.MachineType = "";
                }
            }
        }

        if (masterData.TechnicalNameList != null && masterData.TechnicalNameList.length > 0) {
            var objM = masterData.TechnicalNameList.find(x => x.id == args.rowData.TechnicalNameId);
            if (typeof objM !== "undefined" && objM != null) {
                args.data.TechnicalName = objM.text;
            } else {
                args.data.TechnicalName = "";
            }
        }
        if (masterData.CollarCuffBrandList != null && masterData.CollarCuffBrandList.length > 0) {
            var objM = masterData.CollarCuffBrandList.find(x => x.BrandID == args.data.BrandID);
            if (typeof objM !== "undefined" && objM != null) {
                args.data.Brand = objM.Brand;
            } else {
                args.data.Brand = "";
            }
        }
        return args;
    }
    function getValidStringValue(value) {
        if (typeof value === "undefined" || value == null) return "";
        return value;
    }
    function getYarnCategory(obj) {
        if (typeof obj.YarnCategory === "undefined" || obj.YarnCategory == null || obj.YarnCategory.trim() == "") {
            obj.Segment1ValueDesc = getValidStringValue(obj.Segment1ValueDesc);
            obj.Segment2ValueDesc = getValidStringValue(obj.Segment2ValueDesc);
            obj.Segment3ValueDesc = getValidStringValue(obj.Segment3ValueDesc);
            obj.Segment4ValueDesc = getValidStringValue(obj.Segment4ValueDesc);
            obj.Segment5ValueDesc = getValidStringValue(obj.Segment5ValueDesc);
            obj.Segment6ValueDesc = getValidStringValue(obj.Segment6ValueDesc);
            obj.ShadeCode = getValidStringValue(obj.ShadeCode);

            obj.YarnCategory = GetYarnShortForm(obj.Segment1ValueDesc,
                obj.Segment2ValueDesc,
                obj.Segment3ValueDesc,
                obj.Segment4ValueDesc,
                obj.Segment5ValueDesc,
                obj.Segment6ValueDesc,
                obj.ShadeCode);
        }
        return obj;
    }

    async function getRefInfos(childData, bookingNo, subGroupId, consumptionID, itemMasterId, construction) {
        //Yarns
        var response = await axios.get(`/api/yarn-booking/get-yarn-childs/${bookingNo}/${subGroupId}/${consumptionID}/${itemMasterId}/${construction}`);
        childData.ChildItems = response.data;
        childData.YBChildID = childData.BookingChildID;
        childData.ChildItems.map(x => {
            x.YBChildItemID = maxCol++;
            x.YBChildID = childData.BookingChildID;
            x.Distribution = (100 / childData.ChildItems.length).toFixed(2);
            x.Allowance = 0;
            x.YDItem = false;
            x.YD = false;

            x.Segment1ValueId = x.Segment1ValueID;
            x.Segment2ValueId = x.Segment2ValueID;
            x.Segment3ValueId = x.Segment3ValueID;
            x.Segment4ValueId = x.Segment4ValueID;
            x.Segment5ValueId = x.Segment5ValueID;
            x.Segment6ValueId = x.Segment6ValueID;
            x.Segment7ValueId = x.Segment7ValueID;
        });
        childData.YBChildID = childData.BookingChildID;
        childData.ChildItems = setYarnRelatedFields(childData.ChildItems, childData, false);

        //Yarns

        var response = await axios.get(`/api/yarn-booking/get-finishing-process/${bookingNo}/${subGroupId}/${consumptionID}/${itemMasterId}/${construction}`);
        var result = response.data;

        childData.PreFinishingProcessChilds = result.filter(x => x.IsPreProcess == true);
        childData.PostFinishingProcessChilds = result.filter(x => x.IsPreProcess == false);
        if (childData.PreFinishingProcessChilds.length > 0) {
            childData.MachineDia = childData.PreFinishingProcessChilds[0].MachineDia;
            childData.MachineGauge = childData.PreFinishingProcessChilds[0].MachineGauge;
        } else if (childData.PostFinishingProcessChilds.length > 0) {
            childData.MachineDia = childData.PostFinishingProcessChilds[0].MachineDia;
            childData.MachineGauge = childData.PostFinishingProcessChilds[0].MachineGauge;
        }

        var indexF = masterData.Childs.findIndex(x => x.ConsumptionID == childData.ConsumptionID);
        if (indexF > -1) {
            masterData.Childs[indexF].PreFinishingProcessChilds = childData.PreFinishingProcessChilds;
            masterData.Childs[indexF].PostFinishingProcessChilds = childData.PostFinishingProcessChilds;
        }

        var index = $tblChildEl.getRowIndexByPrimaryKey(childData.ConsumptionID);
        $tblChildEl.updateRow(index, childData);
    }

    async function childCommandClickChild2(e) {


        if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
            if (e.commandColumn.buttonOption.type == 'findGreyYarnUtilizationQty') {

                var yarnrowData = e.rowData;
                __ItemMasterID = yarnrowData.YItemMasterID;
                selectedYBChildItemID = yarnrowData.YBChildItemID;

                var parentObj = {};
                var parentChilds = $tblChildEl.getCurrentViewRecords();
                if (menuType == _paramType.YarnBookingAcknowledge) {
                    parentObj = parentChilds.find(x => x.BookingChildID == yarnrowData.BookingChildID)
                }
                else {
                    parentObj = this.parentDetails.parentRowData;
                }
                if (parentObj != null) {
                    __SubGroupID = parentObj.SubGroupID;
                    selectedBookingChildID = parentObj.BookingChildID;

                    __GSMNumber = parentObj.GSM,
                        __GSMId = parentObj.GSMId, __CompositionId = parentObj.CompositionId; __ConstructionId = parentObj.ConstructionId;

                }
                if (GreyYarnUtilizationSummary == null) {
                    GreyYarnUtilizationSummary = [];
                }
                else
                    GreyYarnUtilizationSummary = yarnrowData.GreyYarnUtilizationPopUpList.filter(item => item.TotalUtilization != 0);
                initGreyYarnUtilization(GreyYarnUtilizationSummary);
                $modalGeryYarnUtilizationInfoEL.modal('show');
                //$formEl.find("#divGreyYarnInfo").show();
            }
            else if (e.commandColumn.buttonOption.type == 'findDyedYarnUtilizationQty') {
                if (e.rowData.YD == false) {
                    toastr.error("Not a YD item.");
                    return
                }
                var yarnrowData = e.rowData;
                __ItemMasterID = yarnrowData.YItemMasterID;
                selectedYBChildItemID = yarnrowData.YBChildItemID;

                var parentObj = {};
                var parentChilds = $tblChildEl.getCurrentViewRecords();
                if (menuType == _paramType.YarnBookingAcknowledge) {
                    parentObj = parentChilds.find(x => x.BookingChildID == yarnrowData.BookingChildID)
                }
                else {
                    parentObj = this.parentDetails.parentRowData;
                }
                if (parentObj != null) {
                    __SubGroupID = parentObj.SubGroupID;
                    selectedBookingChildID = parentObj.BookingChildID;
                    __GSMNumber = parentObj.GSM,
                        __GSMId = parentObj.GSMId, __CompositionId = parentObj.CompositionId; __ConstructionId = parentObj.ConstructionId;
                }

                if (DyedYarnUtilizationSummary == null) {
                    DyedYarnUtilizationSummary = [];
                }
                else
                    DyedYarnUtilizationSummary = yarnrowData.DyedYarnUtilizationPopUpList.filter(item => item.DyedYarnUtilizationQty != 0);


                initDyedYarnUtilization(DyedYarnUtilizationSummary);
                $modalDyedYarnUtilizationInfoEL.modal('show');

            }
            else if (e.commandColumn.buttonOption.type == 'findNetYarnReqQty') {
                if (isAdditionBulkBooking()) {
                    var parentObj = this.parentDetails.parentRowData;
                    if (parentObj != null) {

                        if (parentObj.IsForFabric) {
                            return toastr.error(`This operation is not valid for fabric addition quantity.`);
                        }

                        __SubGroupID = parentObj.SubGroupID;
                        selectedBookingChildID = parentObj.BookingChildID;
                        __GSMNumber = parentObj.GSM;
                        __GSMId = parentObj.GSMId;
                        __CompositionId = parentObj.CompositionId;
                        __ConstructionId = parentObj.ConstructionId;
                    }

                    var yarnrowData = e.rowData;
                    __ItemMasterID = yarnrowData.YItemMasterID;
                    selectedYBChildItemID = yarnrowData.YBChildItemID;

                    if (AdditionalNetReqDataList == null) {
                        AdditionalNetReqDataList = [];
                    }

                    AdditionalNetReqDataList = yarnrowData.AdditionalNetReqPOPUPList;
                    initAdditioalNetReqQTY(AdditionalNetReqDataList);
                    $modalFBAckYarnNetYarnReqQtyInfoEL.modal('show');
                }
            }
        }
    }

    function resizeColumns(childColumns) {
        var cAry = ["Commands", "Segment1ValueId", "ShadeCode", "YarnSubBrandName", "Remarks", "SpinnerId", "PhysicalCount", "BatchNo", "YarnLotNo", "Specification", "YDItem", "YD", "Distribution", "BookingQty", "Allowance", "RequiredQty", "DisplayUnitDesc", "StitchLength"];
        cAry.map(c => {
            var indexF = -1;
            var widthValue = 80;

            if (c == "Commands") indexF = childColumns.findIndex(x => x.headerText == c);
            else indexF = childColumns.findIndex(x => x.field == c);

            if (c == "Commands") widthValue = 60;
            else if (c == "Segment1ValueId") widthValue = 180;

            if (indexF > -1) childColumns[indexF].width = widthValue;
        });
        return childColumns;
    }

    async function getChildColumnsForBDS2(isFabric) {
        var childColumns = [];
        var additionalColumns = [];
        var spinnerCell = {
            field: 'SpinnerId',
            headerText: isYarnBookingAcknowledgeMenu() ? 'Reference Spinner (if any)' : 'Spinner',
            valueAccessor: ej2GridDisplayFormatter,
            dataSource: masterData.SpinnerList,
            allowEditing: isAllowEditing,
            displayField: "text",
            edit: ej2GridDropDownObj({
            })
        };
        var shadeCodeCell = {
            field: 'ShadeCode',
            headerText: 'Shade Code',
            valueAccessor: ej2GridDisplayFormatter,
            dataSource: masterData.YarnShadeBooks,
            allowEditing: isAllowEditing,
            displayField: "ShadeCode",
            visible: !isYarnBookingAcknowledgeMenu(),
            edit: ej2GridDropDownObj({
            })
        };

        var isAllowEditing = true,
            isAllowEditingAllowance = false,
            isAllowForAdditionBB = false,
            isOnlyAllowDelete = false;

        if (isAdditionBulkBooking()) isAllowForAdditionBB = true;
        //if (menuType == _paramType.BulkBookingYarnAllowance || isAdditionBulkBooking()) {
        if (menuType == _paramType.BulkBookingYarnAllowance) {
            isAllowEditing = false;
            isOnlyAllowDelete = true;
            isAllowEditingAllowance = true;

            spinnerCell = { field: 'Spinner', headerText: isYarnBookingAcknowledgeMenu() ? 'Reference Spinner (if any)' : 'Spinner', allowEditing: isAllowEditing }
            shadeCodeCell = { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: isAllowEditing, visible: !isYarnBookingAcknowledgeMenu() }

        } else if (menuType == _paramType.BulkBookingCheck || menuType == _paramType.BulkBookingApprove) {
            isAllowEditingAllowance = true;
        }

        if (!isYarnBookingAcknowledgeMenu()) {
            if (isAllowEditing) {
                childColumns = await getYarnItemColumnsWithSearchDDLAsync(ch_getCountRelatedList(masterData, 2), isAllowEditing);

            } else {
                childColumns = [
                    { field: 'Segment1ValueDesc', headerText: 'Composition', allowEditing: isAllowEditing },
                    { field: 'Segment2ValueDesc', headerText: 'Yarn Type', allowEditing: isAllowEditing },
                    { field: 'Segment3ValueDesc', headerText: 'Manufacturing Process', allowEditing: isAllowEditing },
                    { field: 'Segment4ValueDesc', headerText: 'Sub Process', allowEditing: isAllowEditing },
                    { field: 'Segment5ValueDesc', headerText: 'Quality Parameter', allowEditing: isAllowEditing },
                    { field: 'Segment6ValueDesc', headerText: 'Count', allowEditing: isAllowEditing }
                ];
            }
        } else {
            childColumns = [];
        }

        var isStitchLengthShow = isFabric;
        var netYarnReqQtyName = "Net Yarn Req Qty";
        var YarnBalanceQtyName = "Yarn Balance Qty";
        if (isYarnBookingAcknowledgeMenu()) {
            netYarnReqQtyName = "Yarn Req Qty (KG)";
            YarnBalanceQtyName = "Net Yarn Qty (KG)";
            isStitchLengthShow = false;
        }
        if (isYarnBookingAcknowledgeMenu()) isStitchLengthShow = false;

        var isYarnPlyShow = true;
        if (isYarnBookingAcknowledgeMenu()) {
            isYarnPlyShow = false;
        }

        var isGreyAllowanceShow = true;
        if (!isYarnBookingAcknowledgeMenu()) {
            isGreyAllowanceShow = false;
        }

        var isYDShow = true;
        if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
            isYDShow = true;
        }

        additionalColumns = [
            { field: 'YBChildItemID', isPrimaryKey: true, visible: false, width: 20 },
            { field: 'YBChildID', visible: false },
            { field: 'ConsumptionID', visible: false },
            { field: 'YarnCategory', headerText: 'Yarn Description', allowEditing: false, width: 350, visible: true },
            { field: 'YarnPly', headerText: 'Yarn Ply', allowEditing: isAllowEditing, width: 100, visible: isYarnPlyShow },
            { field: 'StitchLength', headerText: 'Stitch Length', visible: isStitchLengthShow, textAlign: 'Center', width: 100, allowEditing: isAllowEditing, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'YarnLotNo', headerText: isYarnBookingAcknowledgeMenu() ? 'Reference Lot (if any)' : 'Lot No', allowEditing: isAllowEditing, visible: true },
            { field: 'Distribution', headerText: 'Yarn Distribution (%)', visible: true, allowEditing: isAllowEditing || isAdditionBulkBooking(), textAlign: 'Center', width: 100, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'GreyAllowance', headerText: 'Grey Allowance (%)', visible: isGreyAllowanceShow, textAlign: 'Center', allowEditing: false, width: 120, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'YDItem', headerText: 'YD Item?', textAlign: 'Center', width: 100, visible: false, allowEditing: isAllowEditing, displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center' },
            { field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100, visible: isYDShow, allowEditing: menuType != _paramType.BulkBookingYarnAllowance },
            //{ field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100, visible: isYDShow, allowEditing: menuType != _paramType.BulkBookingYarnAllowance },
            { field: 'YDAllowance', headerText: 'YD Allowance (%)', visible: true, textAlign: 'Center', allowEditing: isAllowEditingAllowance, width: 120, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            //{ field: 'YDAllowance', headerText: 'YD Allowance (%)', visible: !isYarnBookingAcknowledgeMenu(), textAlign: 'Center', allowEditing: isAllowEditingAllowance, width: 120, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'Allowance', headerText: 'Total Allowance (%)', visible: true, textAlign: 'Center', allowEditing: false, width: 120, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'YarnReqQty', headerText: 'Yarn Req Qty', textAlign: 'Center', width: 120, visible: !isBulkBookingKnittingInfoMenu() && !isAdditionBulkBooking(), allowEditing: isAllowForAdditionBB, params: { decimals: 0, format: "N2" } },
            { field: 'NetYarnReqQty', headerText: netYarnReqQtyName, textAlign: 'Center', propField: "NetYarnReqQty", width: 120, allowEditing: false, params: { decimals: 0, format: "N2" } },
            {
                headerText: '', textAlign: 'Center', visible: true, propField: "NetYarnReqQty", width: 40, commands: [
                    { buttonOption: { type: 'findNetYarnReqQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for Net Yarn Req Qty" } }
                ]
            },
            { field: 'GreyYarnUtilizationQty', headerText: 'Grey Yarn Utilization Qty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), propField: "GreyYarnUtilizationQty", textAlign: 'Center', width: 120, allowEditing: false, params: { decimals: 0, format: "N2" } },
            {
                headerText: '', textAlign: 'Center', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), propField: "GreyYarnUtilizationQty", width: 40, commands: [
                    { buttonOption: { type: 'findGreyYarnUtilizationQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for grey yarn utilization qty" } }
                ]
            },
            { field: 'DyedYarnUtilizationQty', headerText: 'Dyed Yarn Utilization Qty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), propField: "DyedYarnUtilizationQty", textAlign: 'Center', width: 120, allowEditing: false, params: { decimals: 0, format: "N2" } },
            {
                headerText: '', textAlign: 'Center', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), propField: "DyedYarnUtilizationQty", width: 40, commands: [
                    { buttonOption: { type: 'findDyedYarnUtilizationQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for dyed yarn utilization qty" } }
                ]
            },
            { field: 'YarnBalanceQty', headerText: YarnBalanceQtyName, textAlign: 'Center', width: 120, allowEditing: false, params: { decimals: 0, format: "N2" } },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: _isRemarksEditable, visible: _isRemarksShow }
        ];

        childColumns.push.apply(childColumns, additionalColumns);
        childColumns = resizeColumns(childColumns);

        var indexS = childColumns.findIndex(x => x.field == 'StitchLength');
        indexS = indexS + 1;
        childColumns.splice(indexS, 0, spinnerCell);

        indexS = childColumns.findIndex(x => x.field == 'YarnCategory');
        indexS = indexS - 1;
        childColumns.splice(indexS, 0, shadeCodeCell);

        if (isOnlyAllowDelete) {
            childColumns.unshift({
                headerText: 'Commands', textAlign: 'Center', width: 80, commands: [
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                ]
            });
        }
        childColumns = setVisiblePropValueYarn(childColumns);
        return childColumns;
    }

    async function childCommandClick(e) {
        childData = e.rowData;
        var distributiondatasource = [];
        distributiondatasource = masterData.FBookingAcknowledgeChildDistribution.filter(el => el.ConsumptionID == e.rowData.ConsumptionID);
        if (distributiondatasource != null) {
            distributiondatasource = distributiondatasource.map(function (el) {
                el.DeliveryDate = formatDateToDefault(el.DeliveryDate);
                return el
            });
        }

        if (e.commandColumn.buttonOption.type == 'UsesIn') {
            var finder = new commonFinder({
                title: "Select Distribution",
                pageId: pageId,
                height: 320,
                data: distributiondatasource,
                fields: "DeliveryDate,DistributionQty",
                headerTexts: "Distribution Date,Distribution Qty",
                isMultiselect: false,
                //selectedIds: childData.UsesInIDs,
                primaryKeyColumn: "BookingID",
                //onMultiselect: function (selectedRecords) {
                //    //childData.UsesInIDs = selectedRecords.map(function (el) { return el.BookingID }).toString();
                //    for (var i = 0; i < selectedRecords.length; i++) {

                //        childData.ChildsDistribution.push(selectedRecords[i]);
                //    }
                //}
            });
            finder.showModal();
        }
    }

    function setArgDataValues(argsData, argsRowData = null) {
        if (argsRowData == null) argsRowData = argsData;
        var totalCriteriaDays = 0, subcontactDays = 0;
        var materialDays = 0, preProcessDays = 0, batchPreparationDays = 0, dyeingDays = 0, finishDays = 0, testingDays = 0, qcDays = 0;
        for (var i = 0; i < argsRowData.CriteriaNames.length; i++) {
            totalCriteriaDays += argsRowData.CriteriaNames[i].TotalTime;
            if (argsRowData.CriteriaNames[i].CriteriaName === "Material") materialDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Preprocess") preProcessDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Batch Preparation") batchPreparationDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Finishing") finishDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Quality Check") qcDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Testing") testingDays = argsRowData.CriteriaNames[i].TotalTime;
            else if (argsRowData.CriteriaNames[i].CriteriaName === "Dyeing") dyeingDays = argsRowData.CriteriaNames[i].TotalTime;
        }
        if (argsData.IsSubContact) subcontactDays = 14;

        argsData.StructureDays = parseInt(argsRowData.TechnicalTime);
        argsData.MaterialDays = materialDays;
        argsData.KnittingDays = argsData.StructureDays + materialDays;
        argsData.BatchPreparationDays = argsData.StructureDays + materialDays + preProcessDays + batchPreparationDays;
        argsData.DyeingDays = argsData.StructureDays + materialDays + preProcessDays + batchPreparationDays + dyeingDays;
        argsData.FinishingDays = argsData.StructureDays + materialDays + preProcessDays + batchPreparationDays + dyeingDays + finishDays;
        argsData.TestReportDays = argsData.StructureDays + materialDays + preProcessDays + batchPreparationDays + dyeingDays + finishDays + testingDays;
        argsData.TotalDays = argsData.StructureDays + subcontactDays + totalCriteriaDays;

        var dt = new Date();
        dt.setDate(dt.getDate() + argsData.TotalDays);
        argsData.DeliveryDate = dt;

        argsData.MachineTypeId = argsRowData.MachineTypeId;
        argsData.MachineType = argsRowData.MachineType;
        argsData.KTypeId = argsRowData.KTypeId;
        argsData.TechnicalNameId = argsRowData.TechnicalNameId;
        argsData.TechnicalName = argsRowData.TechnicalName;
        argsData.TechnicalTime = argsRowData.TechnicalTime;
        argsData.YarnSourceID = argsRowData.YarnSourceID;
        argsData.BrandID = argsRowData.BrandID;
        argsData.Brand = argsRowData.Brand;

        return argsData;
    }

    async function updateCriteriaIDTable(_oRow, sourceData) {

        for (var i = 0; i < _oRow.CriteriaNames.length; i++) {
            var obj = sourceData.CriteriaNames.find(function (el) { return el.CriteriaName == _oRow.CriteriaNames[i].CriteriaName });
            if (obj) {
                _oRow.CriteriaNames[i].TotalTime = obj.TotalTime;
                _oRow.CriteriaNames[i].CriteriaIDs = obj.CriteriaIDs;
            }
            //var objCP = sourceData.FBAChildPlannings.find(function (el) { return el.CriteriaName == _oRow.CriteriaNames[i].CriteriaName });
            //if (objCP) {
            //    _oRow.FBAChildPlannings[i].ProcessTime = objCP.ProcessTime;
            //}
            var objCPs = sourceData.FBAChildPlannings.filter(function (el) { return el.CriteriaName == _oRow.CriteriaNames[i].CriteriaName });
            if (objCPs) {
                for (var j = 0; j < objCPs.length; j++) {
                    var objCP = _oRow.FBAChildPlannings.find(function (el) { return el.CriteriaName == objCPs[j].CriteriaName && el.OperationName == objCPs[j].OperationName });
                    if (objCP) {
                        objCPs[j].ProcessTime = objCP.ProcessTime;
                    }
                }
            }
        }
    }

    function loadYarnBookingChildItems() {
        var list = setYarnRelatedFields(this.parentDetails.parentRowData.ChildItems, this.parentDetails.parentRowData, false);
        list.map(ci => {
            ci = getYarnCategory(ci);
        });
        this.dataSource = list;
    }

    function diplayPlanningCriteria(field, data, column) {
        column.disableHtmlEncode = false;
        return `<a class="btn btn-xs btn-default" href="javascript:void(0)" title="Total Days">
                                     ${data[field] ? data[field] : 0}
                                </a>`;
    }

    function diplayPlanningCriteriaTime(field, data, column) {
        column.disableHtmlEncode = false;
        return `<a class="btn btn-xs btn-default" href="javascript:void(0)" title="Total Time">
                                     ${data[field] ? data[field] : 0}
                                </a>`;
    }

    async function initChildCollar(data, isDoCalculateFields = false) {
        /*data.forEach(x => {
            
            if (menuType == _paramType.AdditionalYarnBooking && status == statusConstants.APPROVED2) {
                if (_isFirstLoad) {
                    x.YarnAllowance = 0;
                    x.BookingQty = 0;
                    x.BookingQtyKG = 0;
                    x = setAdditionalAllowance(x);
                }
                if (typeof x.FinishFabricUtilizationQty == 'undefined' || x.FinishFabricUtilizationQty == null) {
                    x.FinishFabricUtilizationQty = 0;
                }
                if (typeof x.GreyLeftOverQty == 'undefined' || x.GreyLeftOverQty == null) {
                    x.GreyLeftOverQty = 0;
                }
            }
            x.ReqFinishFabricQty = x.BookingQtyKG - x.FinishFabricUtilizationQty;
            x = setBookingQtyKGRelatedFieldsValue(x, 11);
            x.ChildItems.forEach(y => {
                if (typeof x.GreyProdQty != 'undefined' && x.GreyProdQty != null && typeof y.Distribution != 'undefined' && y.Distribution != null && typeof x.YarnAllowance != 'undefined' && x.YarnAllowance != null) {
                    y.YarnReqQty = (x.GreyProdQty * (y.Distribution / 100)) / (1 + (x.YarnAllowance / 100) - (0.5 / 100));
                    y.YarnReqQty = y.YarnReqQty.toFixed(2);
                    //y.GreyYarnUtilizationQty = 0;
                    //y.DyedYarnUtilizationQty = 0;
                    if (typeof y.GreyAllowance == 'undefined' || y.GreyAllowance == null) {
                        y.GreyAllowance = 0;
                    }
                    if (typeof y.YDAllowance == 'undefined' || y.YDAllowance == null) {
                        y.YDAllowance = 0;
                    }
                    y = getYarnRelatedProps(y, x, false, isDoCalculateFields);
                }
            });
        });
        data = setCalculatedValues(data);*/

        GetCalculatedFBookingChildCollor(data, isDoCalculateFields);

        if ($tblChildCollarIdEl) $tblChildCollarIdEl.destroy();
        var isAllowEditingCell = true;
        var isAllowEditingAllowanceCell = false;
        if (menuType == _paramType.BulkBookingYarnAllowance) {

            isAllowEditingCell = false;
            isAllowEditingAllowanceCell = true;
        }
        if (menuType == _paramType.BulkBookingCheck || menuType == _paramType.BulkBookingApprove) {

            //isAllowEditingCell = false;
            isAllowEditingAllowanceCell = true;
        }
        var columns = [];
        if (isAdditionBulkBooking()) {
            columns = [
                { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
                { field: 'ConceptTypeID', visible: false },
                { field: 'SubGroupID', visible: false },
                {
                    field: 'Construction', headerText: 'Collar Description', allowEditing: false,
                },
                {
                    field: 'Composition', headerText: 'Collar Type', allowEditing: false,
                },
                {
                    field: 'MachineType', headerText: 'Machine Type', width: 80, allowEditing: false
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', width: 80, allowEditing: false
                },
                {
                    field: 'ExistingYarnAllowance', headerText: 'Existing Yarn Allowance', allowEditing: false
                },
                {
                    field: 'YarnAllowance', headerText: 'Add. Yarn Allowance', allowEditing: true
                },
                {
                    field: 'TotalYarnAllowance', headerText: 'Total Yarn Allowance', allowEditing: false
                },
                {
                    field: 'Brand', headerText: 'Brand', allowEditing: false,
                },
                {
                    field: 'IsSubContact', headerText: 'Sub-Contact?', visible: (status != statusConstants.ACTIVE && _isBDS == 1), allowEditing: isAllowEditingCell, displayAsCheckBox: true, editType: "booleanedit", width: 85, textAlign: 'Center'
                },
                {
                    field: 'TotalDays', headerText: 'Total Days', visible: (status != statusConstants.ACTIVE && _isBDS == 1), allowEditing: false, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria
                },
                {
                    field: 'StructureDays', headerText: 'Structure Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'FinishingDays', headerText: 'Finishing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'DyeingDays', headerText: 'Dyeing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'BatchPreparationDays', headerText: 'Batch Preparation Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'KnittingDays', headerText: 'Knitting Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'TestReportDays', headerText: 'Test Report Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'MaterialDays', headerText: 'Material Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'QualityDays', headerText: 'Quality Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'DeliveryDate', headerText: 'Delivery Date', visible: status != statusConstants.ACTIVE && _isBDS == 1, textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
                },
                {
                    field: 'Color', headerText: 'Color', width: 85, allowEditing: false
                },
                {
                    field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false
                },
                {
                    field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false
                },
                {
                    field: 'DyeingType', headerText: 'Dyeing Type', width: 85, allowEditing: false
                },
                {
                    field: 'Instruction', headerText: 'Instruction', allowEditing: false
                },
                {
                    field: 'LabDipNo', headerText: 'Lab Dip No', allowEditing: false
                },
                {
                    field: 'RefSourceNo', headerText: 'Ref No', width: 85, allowEditing: false
                },
                {
                    headerText: '', textAlign: 'Center', width: 40, visible: isBulkBookingKnittingInfoMenu(), allowEditing: isAllowEditingCell, commands: [
                        { buttonOption: { type: 'findRefSourceNo', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Ref Detail" } }
                    ]
                },
                {
                    field: 'ActualBookingQty', headerText: 'Booking Qty(Pcs)', width: 85, allowEditing: false
                },
                {
                    field: 'BookingQty', headerText: 'Replacement Qty(Pcs)', width: 120, allowEditing: false
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 40, commands: [
                        { buttonOption: { type: 'findAdditionalReplacementQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for Replacement qty" } }
                    ]
                },
                {
                    field: 'BookingQtyKG', headerText: 'Replacement Qty(KG)', width: 85, allowEditing: false
                },
                {
                    field: 'IsForFabric', headerText: 'For Fabric?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', allowEditing: true
                },
                {
                    field: 'FinishFabricUtilizationQty', headerText: 'Finish Fabric Utilization Qty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), width: 120, propField: 'FinishFabricUtilizationQty', allowEditing: false
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 40, propField: 'FinishFabricUtilizationQty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findFinishFabricUtilizationQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for finish fabric utilization qty" } }
                    ]
                },
                {
                    field: 'ReqFinishFabricQty', headerText: 'Req. Finish Fabric Qty', width: 120, allowEditing: false,
                },
                {
                    field: 'TotalQty', headerText: 'Total Qty', width: 85, allowEditing: false, visible: false //status == statusConstants.COMPLETED
                },
                {
                    field: 'GreyReqQty', headerText: 'Grey Req Qty', width: 85, allowEditing: false,
                },
                {
                    field: 'GreyLeftOverQty', headerText: 'Grey Utilization Qty', width: 85, propField: 'GreyLeftOverQty', allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, propField: 'GreyLeftOverQty', width: 40, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findGreyLeftOverQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for grey left over qty" } }
                    ]
                },
                {
                    field: 'GreyProdQty', headerText: 'Grey Prod Qty', width: 95, allowEditing: false,
                },
                {
                    headerText: 'Dist Qty', textAlign: 'Center', visible: _isBDS != 2, allowEditing: isAllowEditingCell, width: 80, commands: [
                        { buttonOption: { type: 'UsesIn', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                    ]
                },
                /*{
                    headerText: 'Finishing Process', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 120, commands: [
                        { buttonOption: { type: 'finishingProcess', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-plus' } }
                    ]
                }*/

                /*
                {
                    field: 'GreyReqQty', headerText: 'Grey Req Qty', width: 85, allowEditing: false
                },
                {
                    field: 'GreyLeftOverQty', headerText: 'Grey Left Over Qty', width: 85, allowEditing: false
                },
                {
                    headerText: '', textAlign: 'Center', width: 40, commands: [
                        { buttonOption: { type: 'findGreyLeftOverQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for grey left over qty" } }
                    ]
                },
                {
                    field: 'GreyProdQty', headerText: 'Grey Prod Qty', width: 95, allowEditing: false
                },
                {
                    headerText: 'Dist Qty', textAlign: 'Center', visible: _isBDS != 2 && !isLabdipMenu(), width: 80, commands: [
                        { buttonOption: { type: 'UsesIn', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                    ]
                }*/
            ];
        }
        else {

            columns = [
                { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
                { field: 'BookingID', visible: false },
                { field: 'ItemMasterID', visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'ConceptTypeID', visible: false },
                {
                    field: 'Construction', headerText: 'Collar Description', allowEditing: false, visible: true
                },
                {
                    field: 'Composition', headerText: 'Collar Type', allowEditing: false, visible: true
                },
                {
                    field: 'MachineType', headerText: 'Machine Type ', visible: (status != statusConstants.ACTIVE || _isBDS == 2) && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation && isAllowEditingCell == true, allowEditing: isAllowEditingCell, edit: {
                        create: function () {
                            machineTypeElem = document.createElement('input');
                            return machineTypeElem;
                        },
                        read: function () {
                            return machineTypeObj.value;
                        },
                        destroy: function () {
                            machineTypeObj.destroy();
                        },
                        write: function (e) {
                            machineTypeObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.MCTypeForOtherList,
                                fields: { value: 'id', text: 'text' },

                                placeholder: 'Select Machine Type',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                change: function (f) {
                                    //
                                    technicalNameObj.enabled = true;
                                    var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                    technicalNameObj.query = tempQuery;
                                    technicalNameObj.text = null;
                                    technicalNameObj.dataBind();

                                    e.rowData.MachineTypeId = f.itemData.id;
                                    e.rowData.MachineType = f.itemData.text;
                                    e.rowData.KTypeId = f.itemData.desc;
                                    e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);
                                },
                                placeholder: 'Select M/C Type',
                                floatLabelType: 'Never'
                            });
                            machineTypeObj.appendTo(machineTypeElem);
                        }
                    }
                },
                {
                    field: 'MachineType', headerText: 'Machine Type', visible: (status != statusConstants.ACTIVE || _isBDS == 2) && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation && isAllowEditingCell == false, allowEditing: isAllowEditingCell,
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', visible: (status != statusConstants.ACTIVE || _isBDS == 2) && isAllowEditingCell == true, allowEditing: isAllowEditingCell, edit: {
                        create: function () {
                            technicalNameElem = document.createElement('input');
                            return technicalNameElem;
                        },
                        read: function () {
                            return technicalNameObj.value;
                        },
                        destroy: function () {
                            technicalNameObj.destroy();
                        },
                        write: function (e) {
                            technicalNameObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.TechnicalNameList,
                                fields: { value: 'id', text: 'text' },
                                //enabled: false,
                                placeholder: 'Select Technical Name',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                change: function (f) {
                                    if (!f.isInteracted || !f.itemData) return false;
                                    e.rowData.TechnicalTime = parseInt(f.itemData.desc);
                                    e.rowData.TechnicalNameId = f.itemData.id;
                                    e.rowData.TechnicalName = f.itemData.text;
                                    e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);

                                    //$tblChildCollarIdEl.updateRow(e.row.rowIndex, e.rowData);
                                }
                            });
                            technicalNameObj.appendTo(technicalNameElem);
                        }
                    }
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', visible: (status != statusConstants.ACTIVE || _isBDS == 2) && isAllowEditingCell == false, allowEditing: isAllowEditingCell,
                },
                {
                    field: 'BrandID', headerText: 'Brand', displayField: "Brand", allowEditing: isAllowEditingCell, visible: (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) && isAllowEditingCell == true, valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            machineBrandElem = document.createElement('input');
                            return machineBrandElem;
                        },
                        read: function () {
                            return machineBrandObj.value;
                        },
                        destroy: function () {
                            machineBrandObj.destroy();
                        },
                        write: function (e) {
                            machineBrandObj = new ej.dropdowns.DropDownList({
                                dataSource: getMachineBrandList(masterData.CollarCuffBrandList, 0, 0, 11),
                                fields: { value: 'BrandID', text: 'Brand' },
                                //enabled: false,
                                placeholder: 'Select Machine Brand',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                change: function (f) {

                                    if (!f.isInteracted || !f.itemData) return false;
                                    e.rowData.BrandID = f.itemData.BrandID;
                                    e.rowData.Brand = f.itemData.Brand;

                                    $tblChildCollarIdEl.updateRow(e.row.rowIndex, e.rowData);
                                }
                            });
                            machineBrandObj.appendTo(machineBrandElem);
                        }
                    }
                },
                {
                    field: 'Brand', headerText: 'Brand', visible: (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) && isAllowEditingCell == false, allowEditing: isAllowEditingCell,
                },
                {
                    field: 'YarnAllowance', headerText: 'Yarn Allowance', visible: (isBulkBookingKnittingInfoMenu()) && menuType != _paramType.BulkBookingAck && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation, allowEditing: isAllowEditingAllowanceCell
                },
                //{
                //    field: 'MachineGauge', headerText: 'Gauge', visible: _isBDS == 2, width: 80, allowEditing: true, editType: "numericedit", params: { decimals: 0, format: "N", min: 0, validateDecimalOnType: true }
                //},
                //{
                //    field: 'Brand', headerText: 'Brand', visible: _isBDS == 2, edit: {
                //        create: function () {
                //            brandElem = document.createElement('input');
                //            return brandElem;
                //        },
                //        read: function () {
                //            return brandObj.text;
                //        },
                //        destroy: function () {
                //            brandObj.destroy();
                //        },
                //        write: function (e) {
                //            brandObj = new ej.dropdowns.DropDownList({
                //                dataSource: masterData.KnittingMachines,
                //                fields: { value: 'id', text: 'text' },
                //                change: function (f) {
                //                    e.rowData.BrandID = f.itemData.id;
                //                    e.rowData.Brand = f.itemData.text;
                //                },
                //                placeholder: 'Select Brand',
                //                floatLabelType: 'Never'
                //            });
                //            brandObj.appendTo(brandElem);
                //        }
                //    }
                //},
                {
                    field: 'IsSubContact', headerText: 'Sub-Contact?', visible: (status != statusConstants.ACTIVE && _isBDS == 1) || _isLabDipAck_RnD, allowEditing: isAllowEditingCell, displayAsCheckBox: true, editType: "booleanedit", width: 85, textAlign: 'Center'
                },
                {
                    field: 'TotalDays', headerText: 'Total Days', visible: (status != statusConstants.ACTIVE && _isBDS == 1) || _isLabDipAck_RnD, allowEditing: false, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria
                },
                {
                    field: 'StructureDays', headerText: 'Structure Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'FinishingDays', headerText: 'Finishing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'DyeingDays', headerText: 'Dyeing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'BatchPreparationDays', headerText: 'Batch Preparation Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'KnittingDays', headerText: 'Knitting Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'TestReportDays', headerText: 'Test Report Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'MaterialDays', headerText: 'Material Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'QualityDays', headerText: 'Quality Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'DeliveryDate', headerText: 'Delivery Date', visible: status != statusConstants.ACTIVE && _isBDS == 1, textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
                },
                {
                    field: 'Color', headerText: 'Color', width: 85, allowEditing: false, visible: true
                },
                {
                    field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'ReferenceSourceName', headerText: 'Reference Source', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
                },
                {
                    field: 'ReferenceNo', headerText: 'Ref No', visible: _isBDS == 1, width: 85, allowEditing: false
                },
                {
                    field: 'ColorReferenceNo', headerText: 'Color Ref No', visible: _isBDS == 1 ? true : false, allowEditing: false
                },
                {
                    field: 'ValueName', headerText: 'Yarn Source', visible: false, allowEditing: isAllowEditingCell/* status != statusConstants.ACTIVE*/, edit: {
                        create: function () {
                            YarnSourceNameElem = document.createElement('input');
                            return YarnSourceNameElem;
                        },
                        read: function () {
                            return YarnSourceNameobj.value;
                        },
                        destroy: function () {
                            YarnSourceNameobj.destroy();
                        },
                        write: function (e) {
                            YarnSourceNameobj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.YarnSourceNameList,

                                fields: { value: 'id', text: 'text' },
                                placeholder: 'Select Yarn Source',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                change: function (f) {
                                    technicalNameObj.enabled = true;
                                    technicalNameObj.text = null;
                                    technicalNameObj.dataBind();

                                    e.rowData.YarnSourceID = f.itemData.id;
                                    e.rowData.ValueName = f.itemData.text;
                                },
                                placeholder: 'Select one',
                                floatLabelType: 'Never'
                            });
                            YarnSourceNameobj.appendTo(YarnSourceNameElem);
                        }
                    }
                },
                {
                    field: 'DyeingType', headerText: 'Dyeing Type', width: 85, allowEditing: false, visible: true
                },
                {
                    field: 'DayValidDurationName', headerText: 'Yarn Sourcing Mode', width: 120, allowEditing: false, visible: menuType == _paramType.Projection
                },
                {
                    field: 'Instruction', headerText: 'Instruction', allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'LabDipNo', headerText: 'Lab Dip No', allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'RefSourceNo', headerText: 'Ref No', visible: isBulkBookingKnittingInfoMenu(), width: 85, allowEditing: false
                },
                {
                    headerText: '', textAlign: 'Center', width: 40, visible: isBulkBookingKnittingInfoMenu(), allowEditing: isAllowEditingCell, commands: [
                        { buttonOption: { type: 'findRefSourceNo', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Ref Detail" } }
                    ]
                },
                {
                    field: 'BookingQty', headerText: 'Booking Qty(Pcs)', width: 85, allowEditing: false, visible: true
                },
                {
                    field: 'BookingQtyKG', headerText: 'Booking Qty(KG)', width: 85, allowEditing: false, visible: true
                },
                {
                    field: 'FinishFabricUtilizationQty', headerText: 'Finish Fabric Utilization Qty', width: 120, propField: 'FinishFabricUtilizationQty', allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 40, propField: 'FinishFabricUtilizationQty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findFinishFabricUtilizationQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for finish fabric utilization qty" } }
                    ]
                },
                {
                    field: 'ReqFinishFabricQty', headerText: 'Req. Finish Fabric Qty', width: 120, allowEditing: false, visible: isBulkBookingKnittingInfoMenu()
                },
                {
                    field: 'TotalQty', headerText: 'Total Qty', width: 85, allowEditing: false, visible: false //status == statusConstants.COMPLETED
                },
                {
                    field: 'GreyReqQty', headerText: 'Grey Req Qty', width: 85, allowEditing: false, visible: isBulkBookingKnittingInfoMenu()
                },
                {
                    field: 'GreyLeftOverQty', headerText: 'Grey Utilization Qty', width: 85, propField: 'GreyLeftOverQty', allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, propField: 'GreyLeftOverQty', width: 40, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findGreyLeftOverQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for grey left over qty" } }
                    ]
                },
                {
                    field: 'GreyProdQty', headerText: 'Grey Prod Qty', width: 95, allowEditing: false, visible: isBulkBookingKnittingInfoMenu()
                },
                {
                    headerText: 'Dist Qty', textAlign: 'Center', visible: _isBDS != 2, allowEditing: isAllowEditingCell, width: 80, commands: [
                        { buttonOption: { type: 'UsesIn', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                    ]
                },
                {
                    headerText: 'Finishing Process', textAlign: 'Center', propField: 'finishingProcess', visible: isBulkBookingKnittingInfoMenu() && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation, allowEditing: isAllowEditingCell, width: 120, commands: [
                        { buttonOption: { type: 'finishingProcess', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-plus' } }
                    ]
                },
                {
                    field: 'Remarks', headerText: 'Remarks', allowEditing: _isRemarksEditable, visible: _isRemarksShow
                }
            ];
        }

        var additionalColumns = [
            {
                field: 'DeliveredQty', headerText: 'Delivered Qty(kg/pcs)', width: 85, allowEditing: false, visible: status == statusConstants.APPROVED && !isAdditionBulkBooking()
            },
            {
                field: 'DelivereyComplete', headerText: 'Is Delivered?', allowEditing: isAllowEditingCell, displayAsCheckBox: true, textAlign: 'Center', visible: status == statusConstants.APPROVED && !isAdditionBulkBooking()
            }
        ]
        columns.push.apply(columns, additionalColumns);
        var childColumns = [
            { field: 'YBChildItemID', isPrimaryKey: true, visible: false },
            { field: 'ShadeCode', headerText: 'Shade Code', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Distribution', headerText: 'Distribution', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'BookingQty', headerText: 'Booking Qty', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Allowance', headerText: 'Allowance', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'StitchLength', headerText: 'Stitch Length', width: 40, allowEditing: true, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'Specification', headerText: 'Specification', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', textAlign: 'Center', width: 40, allowEditing: false },
        ];
        ej.base.enableRipple(true);

        columns = setVisiblePropValue(columns, 11);
        columns = removeDuplicateColumns(columns);

        if (_isBDS == 2) {

            var childColumns = await getChildColumnsForBDS2(false);
            var contextMenuItems = [
                { text: 'Copy Information', target: '.e-content', id: 'copyBoth' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'pasteYarn' },
                { text: 'Paste Technical Information', target: '.e-content', id: 'pasteTech' },
                { text: 'Paste Finishing Process', target: '.e-content', id: 'pasteFinishingProcess' },
                { text: 'Paste Both', target: '.e-content', id: 'pasteBoth' }
            ];
            var isAllowEditing = true,
                isAllowAdding = true,
                isAllowDeleting = true,
                isChildGridAllowEditing = true,
                isChildGridAllowAdding = true,
                isChildGridAllowDeleting = true;

            var childGridToolbars = ['Add'];
            if (menuType == _paramType.BulkBookingYarnAllowance) {
                contextMenuItems = [];
                childGridToolbars = [];

                isAllowEditing = true;
                isAllowAdding = false;
                isAllowDeleting = false;

                isChildGridAllowAdding = false;
                isChildGridAllowDeleting = false;
            }
            else if (isAdditionBulkBooking()) {
                childGridToolbars = [
                    { text: 'Add Item', tooltipText: 'Add Item', prefixIcon: 'e-icons e-add', id: 'addItem' },
                    //{ text: 'Remove Item', tooltipText: 'Remove Item', prefixIcon: 'e-icons e-delete', id: 'removeItem' }
                ];

                columns.unshift({
                    headerText: 'Commands', textAlign: 'Center', width: 80, commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ]
                });
            }

            var queryStringValue = "";
            if (isYarnBookingAckMenu()) {
                queryStringValue = 'BookingChildID';
            }
            else if (status == statusConstants.PENDING || status == statusConstants.REJECT) {
                queryStringValue = "YBChildID";
            }
            else {
                queryStringValue = 'BookingChildID';
            }

            $tblChildCollarIdEl = new ej.grids.Grid({
                dataSource: data,

                //allowGrouping: true,
                allowPaging: false,
                allowScrolling: false,
                //height: 300, // Adjust this height based on your layout
                //frozenRows: 0, // Keep the header row fixed
                //toolbar: ['Update', 'Cancel'],
                allowResizing: true,
                columns: columns,
                commandClick: childCommandClick,
                editSettings: {
                    allowEditing: isAllowEditing,
                    allowAdding: isAllowAdding,
                    allowDeleting: isAllowDeleting,
                    mode: "Normal",
                    showDeleteConfirmDialog: true
                },
                recordClick: function (args) {
                    if (args.column && args.column.field == "TotalDays") {
                        _oRowCollar = args.rowData;
                        _indexCollar = args.rowIndex;
                        _modalFrom = subGroupNames.COLLAR;
                        // initPlanningTable(_oRowCollar.FBAChildPlannings, _oRowCollar.CriteriaIDs);
                        initCriteriaIDTable(_oRowCollar.CriteriaNames, _oRowCollar.FBAChildPlannings, _oRowCollar.FBAChildPlanningsWithIds, _oRowCollar.BookingChildID);
                        $modalCriteriaEl.modal('show');
                    }
                },
                /*
                actionBegin: function (args) {
                    if (args.requestType === "save") {
                        
                        args.data = setArgDataValues(args.data, args.rowData);
                    }
                },
                */
                childGrid: {
                    queryString: queryStringValue,
                    allowResizing: true,
                    autofitColumns: false,
                    editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: false },
                    columns: childColumns,
                    toolbar: childGridToolbars,
                    editSettings: {
                        allowEditing: isChildGridAllowEditing,
                        allowAdding: isChildGridAllowAdding,
                        allowDeleting: isChildGridAllowDeleting,
                        mode: "Normal",
                        showDeleteConfirmDialog: true
                    },
                    toolbarClick: function (args) {
                        if (args.item.id === "addItem") {

                            var parentObj = this.parentDetails.parentRowData;
                            //if (parentObj.IsForFabric) {
                            //    toastr.error("This addition is for fabric, not for yarn (For yarn addition uncheck for fabric).");
                            //    return false;
                            //}
                            var index = $tblChildCollarIdEl.getRowIndexByPrimaryKey(parentObj.BookingChildID);
                            var allYarns = _allYarnList.filter(x => x.BookingChildID == parentObj.BookingChildID);

                            if (allYarns.length == 0) {
                                toastr.error("No list found");
                                return false;
                            }
                            var currentYarns = parentObj.ChildItems;
                            var childItemIds = currentYarns.map(x => x.YBChildItemID).join(",");

                            var columns = getYarnCommanFinderColumns();
                            var fieldList = columns.map(x => x.Field).join(",");
                            var headerTextList = columns.map(x => x.HeaderText).join(",");
                            var widthList = columns.map(x => x.Width).join(",");
                            var finder = new commonFinder({
                                title: "Select Yarn",
                                pageId: pageId,
                                height: 320,
                                data: allYarns,
                                fields: fieldList,
                                headerTexts: headerTextList,
                                widths: widthList,
                                isMultiselect: true,
                                autofitColumns: true,
                                primaryKeyColumn: "YBChildItemID",
                                modalSize: "modal-lg",
                                top: "2px",
                                selectedIds: childItemIds,
                                onMultiselect: function (selectedRecords) {
                                    if (selectedRecords.length > 0) {
                                        parentObj.ChildItems = selectedRecords;
                                        $tblChildCollarIdEl.updateRow(index, parentObj);
                                    }
                                }
                            });
                            finder.showModal();
                        }
                    },
                    actionBegin: function (args) {

                        if (args.requestType === 'beginEdit') {
                            if (args.rowData.YDProductionMasterID > 0) {
                                toastr.error("Yarn Dyeing found, You cannot modify anything.");
                                args.cancel = true;
                            }
                        }
                        else if (args.requestType === "add") {
                            args.data.YBChildItemID = maxCol++; //getMaxIdForArray(masterData.Childs, "YBChildItemID");
                            args.data.YBChildID = this.parentDetails.parentRowData.YBChildID;
                            args.data.BookingChildID = this.parentDetails.parentRowData.BookingChildID;
                            args.data.ConsumptionID = this.parentDetails.parentRowData.ConsumptionID;

                            if (isBulkBookingKnittingInfoMenu()) {
                                args.data.GreyAllowance = this.parentDetails.parentRowData.YarnAllowance;
                                args.data.Allowance = args.data.GreyAllowance;
                            }

                            var totalDis = 0, remainDis = 0;
                            this.dataSource.forEach(l => {
                                totalDis += l.Distribution;
                            })
                            if (totalDis < 100) remainDis = 100 - totalDis;
                            else {
                                toastr.error("Distribution can not more then 100!!");
                                args.cancel = true;
                                return;
                            }
                            var netConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(remainDis) / 100);
                            var reqQty = netConsumption;
                            args.data.Distribution = remainDis;
                            args.data.BookingQty = netConsumption.toFixed(4);
                            args.data.Allowance = 0.00;
                            args.data.RequiredQty = reqQty.toFixed(2);

                            args.data.DisplayUnitDesc = "Kg";
                            args.data.SubGroupId = 1;

                            args.data.Segment1ValueId = 0;
                            args.data.Segment2ValueId = 0;
                            args.data.Segment3ValueId = 0;
                            args.data.Segment4ValueId = 0;
                            args.data.Segment5ValueId = 0;
                            args.data.Segment6ValueId = 0;
                            args.data.Segment7ValueId = 0;
                            args.data.Segment8ValueId = 0;
                            //getAllYarnList();
                        }
                        else if (args.requestType === "save") {
                            args.data = checkAndSetYarnValidSegmentCH(args.data, _yarnSegmentsMapping);

                            args.data = setNullIfIdNullYarnSegment(args.data);
                            if (!args.data.YD && args.data.YDAllowance > 0) {
                                args.data.YDAllowance = 0;
                                toastr.error("YD allowance only valid for Go For YD item.");
                            }
                            else if (args.data.YD && (args.data.YDAllowance < 0 || args.data.YDAllowance > 35)) {
                                toastr.error("YD allowance should be between 0 to 35.");
                                args.data.YDAllowance = 0;
                                return false;
                            }


                            args.data.GreyAllowance = getDefaultValueWhenInvalidN_Float(args.data.GreyAllowance);
                            args.data.YDAllowance = getDefaultValueWhenInvalidN_Float(args.data.YDAllowance);

                            args.data.Allowance = args.data.GreyAllowance + args.data.YDAllowance;
                            var parentObj = this.parentDetails.parentRowData;
                            if (typeof parentObj.YarnAllowance == 'undefined' && parentObj.YarnAllowance == null) {
                                parentObj.YarnAllowance = 0;
                            }
                            var reqQty = 0;
                            if (typeof parentObj.GreyProdQty != 'undefined' && parentObj.GreyProdQty != null && typeof args.data.Distribution != 'undefined' && args.data.Distribution != null) {
                                reqQty = (parentObj.GreyProdQty * (args.data.Distribution / 100)) / (1 + (parentObj.YarnAllowance / 100) - (0.5 / 100));
                            }
                            args.data.YarnReqQty = reqQty.toFixed(2);
                            if (isAdditionBulkBooking() && parentObj.IsForFabric == false) {
                                args.data.YarnReqQty = args.data.NetYarnReqQty;
                            }

                            args.data.GreyYarnUtilizationQty = 0;
                            args.data.DyedYarnUtilizationQty = 0;
                            if (typeof args.data.GreyAllowance == 'undefined' || args.data.GreyAllowance == null) {
                                args.data.GreyAllowance = 0;
                            }
                            if (typeof args.data.YDAllowance == 'undefined' || args.data.YDAllowance == null) {
                                args.data.YDAllowance = 0;
                            }

                            if (isAdditionBulkBooking() && parentObj.IsForFabric == false) {
                                args.data = getYarnRelatedPropsAdditionalYarn(args.data, this.parentDetails.parentRowData, false, true);
                            } else {
                                args.data = getYarnRelatedProps(args.data, this.parentDetails.parentRowData, false, true);
                            }

                            var NetConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(args.data.Distribution) / 100);
                            //var reqQty = parseFloat(NetConsumption) + ((parseFloat(NetConsumption) * parseFloat(args.data.Allowance)) / 100);

                            //args.data.Distribution = args.rowData.Distribution;
                            args.data.YarnSubBrandIDs = args.rowData.YarnSubBrandIDs;
                            //args.data.YBChildID = this.parentDetails.parentRowData.YBChildID;
                            args.data.YBChildID = args.rowData.YBChildID;
                            args.data.BookingQty = NetConsumption.toFixed(4);
                            args.data.RequiredQty = reqQty.toFixed(2);
                            /* Saif Stopped On 27-02-2024
                            args.rowData.Segment1ValueId = !args.data.Segment1ValueId ? 0 : args.data.Segment1ValueId;
                            args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                            args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                            args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                            args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                            args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                            args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                            args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;*/

                            args.data = setYarnSegDesc(args.data);

                            args = setSegmentValueFromRowDataToData(args);
                        }
                        else if (args.requestType === "delete") {
                            if (args.data[0].YDProductionMasterID > 0) {
                                toastr.error("Yarn Dyeing found, You cannot modify anything.");
                                args.cancel = true;
                            }
                            if (isAdditionBulkBooking()) {
                                var parentObj = this.parentDetails.parentRowData;
                                //if (parentObj.IsForFabric) {
                                //    toastr.error("This addition is for fabric, not for yarn (For yarn addition uncheck for fabric).");
                                //    var index = $tblChildCollarIdEl.getRowIndexByPrimaryKey(parentObj.ConsumptionID);
                                //    parentObj.ChildItems = _allYarnList.filter(x => x.BookingChildID == parentObj.BookingChildID);
                                //    $tblChildCollarIdEl.updateRow(index, parentObj);
                                //    return false;
                                //}
                            }
                        }
                    },
                    commandClick: childCommandClickChild2, // alamin
                    load: loadYarnBookingChildItems

                },
                enableContextMenu: true,
                contextMenuItems: contextMenuItems,
                contextMenuClick: function (args) {
                    if (args.item.id === 'copyBoth') {

                        collarCuffYarnItem.ParentInfo = objectCopy(args.rowInfo.rowData);
                        collarCuffYarnItem.ChildItems = objectCopy(args.rowInfo.rowData.ChildItems);

                        if (typeof collarCuffYarnItem.ChildItems === "undefined" || collarCuffYarnItem.ChildItems == null || collarCuffYarnItem.ChildItems.length == 0) {
                            collarCuffYarnItem.ChildItems = [];
                        }

                        //Copy Finishing Process
                        //var indexF = masterData.Childs.findIndex(x => x.BookingChildID == collarCuffYarnItem.ParentInfo.BookingChildID);

                        var indexF = masterData.Childs.findIndex(x => x.Construction == collarCuffYarnItem.ParentInfo.Construction && x.Composition == collarCuffYarnItem.ParentInfo.Composition && x.Color == collarCuffYarnItem.ParentInfo.Color);

                        if (indexF > -1) {
                            masterData.Childs[indexF].PreFinishingProcessChilds = getValidList(masterData.Childs[indexF].PreFinishingProcessChilds);
                            masterData.Childs[indexF].PostFinishingProcessChilds = getValidList(masterData.Childs[indexF].PostFinishingProcessChilds);

                            collarCuffYarnItem.PreFinishingProcessChilds = DeepClone(masterData.Childs[indexF].PreFinishingProcessChilds);
                            collarCuffYarnItem.PostFinishingProcessChilds = DeepClone(masterData.Childs[indexF].PostFinishingProcessChilds);
                        }

                    }
                    else if (args.item.id === 'pasteYarn') {
                        if (typeof collarCuffYarnItem.ChildItems === "undefined" || collarCuffYarnItem.ChildItems == null || collarCuffYarnItem.ChildItems.length == 0) {
                            //toastr.error("Please copy yarn item first!!");
                            return;
                        }
                        for (var i = 0; i < collarCuffYarnItem.ChildItems.length; i++) {
                            var copiedItem = objectCopy(collarCuffYarnItem.ChildItems[i]);

                            copiedItem.YBChildItemID = _fbChildItemID++;
                            copiedItem.YBChildID = args.rowInfo.rowData.YBChildID;
                            copiedItem.YBookingID = args.rowInfo.rowData.YBookingID;

                            copiedItem.BookingChildID = args.rowInfo.rowData.BookingChildID;
                            copiedItem.BookingID = args.rowInfo.rowData.BookingID;

                            var parentInfo = {
                                GreyProdQty: 0
                            };
                            copiedItem = setYarnRelatedSingleField(copiedItem, parentInfo);
                            args.rowInfo.rowData.ChildItems.push(copiedItem);
                        }
                        $tblChildCollarIdEl.refresh();
                    }
                    else if (args.item.id === 'pasteTech') {
                        if (typeof collarCuffYarnItem.ParentInfo === "undefined" || collarCuffYarnItem.ParentInfo == null) {
                            //toastr.error("Please copy technical info first!!");
                            return;
                        }

                        args.rowInfo.rowData.MachineTypeId = collarCuffYarnItem.ParentInfo.MachineTypeId;
                        args.rowInfo.rowData.MachineType = collarCuffYarnItem.ParentInfo.MachineType;
                        args.rowInfo.rowData.TechnicalNameId = collarCuffYarnItem.ParentInfo.TechnicalNameId;
                        args.rowInfo.rowData.TechnicalName = collarCuffYarnItem.ParentInfo.TechnicalName;
                        args.rowInfo.rowData.BrandID = collarCuffYarnItem.ParentInfo.BrandID;
                        args.rowInfo.rowData.Brand = collarCuffYarnItem.ParentInfo.Brand;

                        pasteFinishingProcess(args.rowInfo.rowData.BookingChildID, collarCuffYarnItem);

                        $tblChildCollarIdEl.refresh();

                    }
                    else if (args.item.id === 'pasteFinishingProcess') {

                        if (typeof collarCuffYarnItem.ParentInfo === "undefined" || collarCuffYarnItem.ParentInfo == null) {
                            toastr.error("Please copy technical info first!!");
                            return;
                        }
                        pasteFinishingProcessCollarCuff(args.rowInfo.rowData, collarCuffYarnItem);
                        $tblChildCollarIdEl.refresh();
                    }
                    else if (args.item.id === 'pasteBoth') {
                        if (typeof collarCuffYarnItem.ParentInfo === "undefined" || collarCuffYarnItem.ParentInfo == null) {
                            //toastr.error("Please copy first!!");
                            return;
                        }

                        args.rowInfo.rowData.MachineTypeId = collarCuffYarnItem.ParentInfo.MachineTypeId;
                        args.rowInfo.rowData.MachineType = collarCuffYarnItem.ParentInfo.MachineType;
                        args.rowInfo.rowData.TechnicalNameId = collarCuffYarnItem.ParentInfo.TechnicalNameId;
                        args.rowInfo.rowData.TechnicalName = collarCuffYarnItem.ParentInfo.TechnicalName;
                        args.rowInfo.rowData.BrandID = collarCuffYarnItem.ParentInfo.BrandID;
                        args.rowInfo.rowData.Brand = collarCuffYarnItem.ParentInfo.Brand;

                        for (var i = 0; i < collarCuffYarnItem.ChildItems.length; i++) {
                            var copiedItem = objectCopy(collarCuffYarnItem.ChildItems[i]);

                            copiedItem.YBChildItemID = _fbChildItemID++;
                            copiedItem.YBChildID = args.rowInfo.rowData.YBChildID;
                            copiedItem.YBookingID = args.rowInfo.rowData.YBookingID;

                            copiedItem.BookingChildID = args.rowInfo.rowData.BookingChildID;
                            copiedItem.BookingID = args.rowInfo.rowData.BookingID;

                            var parentInfo = {
                                GreyProdQty: 0
                            };
                            copiedItem = setYarnRelatedSingleField(copiedItem, parentInfo);
                            args.rowInfo.rowData.ChildItems.push(copiedItem);
                        }

                        pasteFinishingProcessCollarCuff(args.rowInfo.rowData, collarCuffYarnItem);

                        $tblChildCollarIdEl.refresh();
                    }
                },
                commandClick: childCommandClick2,
                actionBegin: function (args) {
                    if (args.requestType === "save") {

                        if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
                            //Saif_04_10_2023
                            if (args.data.YarnAllowance < 0 || args.data.YarnAllowance > 35) {
                                args.data.YarnAllowance = 0;
                                toastr.error("Yarn allowance should be between 0 to 35.");
                                return false;
                            }
                            //Saif_04_10_2023 END

                            args.data.ChildItems.forEach(x => {
                                x.GreyAllowance = args.data.YarnAllowance;
                                x.Allowance = parseFloat(x.GreyAllowance) + parseFloat(x.YDAllowance);
                            });
                            args.data.GreyReqQty = (args.data.ReqFinishFabricQty * (1 + (args.data.YarnAllowance / 100) - (0.5 / 100))).toFixed(0);
                            args.data.GreyProdQty = args.data.GreyReqQty - args.data.GreyLeftOverQty;

                            setModifiedChildData(args);
                            $tblChildCollarIdEl.updateRow(args.rowIndex, args.data);
                            setChildData(args.data, 11);
                        }
                        if (isAdditionBulkBooking()) {
                            args.data = getAdditionBulkBookingData(args.data, 11);
                            setAdditionalAllowance(args.data);
                            setChildData(args.data, 11);
                        }
                    }
                },
            });
        } else {
            $tblChildCollarIdEl = new ej.grids.Grid({
                dataSource: data,

                //allowGrouping: true,
                allowPaging: false,
                allowScrolling: false,
                //height: 300, // Adjust this height based on your layout
                //frozenRows: 0, // Keep the header row fixed

                allowResizing: true,
                columns: columns,
                commandClick: childCommandClick,
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                recordClick: function (args) {
                    if (args.column && args.column.field == "TotalDays") {
                        _oRowCollar = args.rowData;
                        _indexCollar = args.rowIndex;
                        _modalFrom = subGroupNames.COLLAR;
                        // initPlanningTable(_oRowCollar.FBAChildPlannings, _oRowCollar.CriteriaIDs);
                        initCriteriaIDTable(_oRowCollar.CriteriaNames, _oRowCollar.FBAChildPlannings, _oRowCollar.FBAChildPlanningsWithIds, _oRowCollar.BookingChildID);
                        $modalCriteriaEl.modal('show');
                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "save") {

                        args.data = setArgDataValues(args.data, args.rowData);
                    }
                },
                enableContextMenu: true,
                contextMenuItems: [
                    { text: 'Copy TNA', target: '.e-content', id: 'copy' },
                    { text: 'Paste TNA', target: '.e-content', id: 'paste' },
                    { text: 'Paste TNA To All', target: '.e-content', id: 'pasteAll' }
                ],
                contextMenuClick: function (args) {
                    if (args.item.id === 'copy') {
                        itemTNAInfoCollar = objectCopy(args.rowInfo.rowData);
                        if (itemTNAInfoCollar.length == 0) {
                            toastr.error("No TNA information found to copy!!");
                            return;
                        }
                        var selctedRowCriterias = idsList.filter(x => x.SubGroupWiseIndex == args.rowInfo.rowIndex && x.SubGroupName == "Collar");
                        if (selctedRowCriterias) {
                            idsListCopyCollarOrCuff = JSON.parse(JSON.stringify(selctedRowCriterias));
                        }
                    }
                    else if (args.item.id === 'paste') {
                        var rowIndex = args.rowInfo.rowIndex;
                        if (itemTNAInfoCollar == null || itemTNAInfoCollar.length == 0) {
                            toastr.error("Please copy first!!");
                            return;
                        } else {
                            var pasteObject = objectCopy(itemTNAInfoCollar);
                            var preSubContactDays = 0,
                                subContactDays = 0;
                            //if (pasteObject.IsSubContact) preSubContactDays = 14;
                            //if (args.rowInfo.rowData.IsSubContact) subContactDays = 14;
                            //args.rowInfo.rowData.TotalDays = pasteObject.TotalDays - pasteObject.StructureDays + args.rowInfo.rowData.StructureDays - preSubContactDays + subContactDays;

                            args.rowInfo.rowData.MachineTypeId = pasteObject.MachineTypeId;
                            args.rowInfo.rowData.MachineType = pasteObject.MachineType;
                            args.rowInfo.rowData.KTypeId = pasteObject.KTypeId;
                            args.rowInfo.rowData.TechnicalNameId = pasteObject.TechnicalNameId;
                            args.rowInfo.rowData.TechnicalName = pasteObject.TechnicalName;
                            args.rowInfo.rowData.TechnicalTime = pasteObject.TechnicalTime;
                            args.rowInfo.rowData.IsSubContact = pasteObject.IsSubContact;

                            //args.rowInfo.rowData = setTotalDaysAndDeliveryDate(args.rowInfo.rowData, args.rowInfo.rowData.CriteriaNames);
                            //var techTypeDesc = 0;
                            //var techType = masterData.TechnicalNameList.find(y => y.id == pasteObject.TechnicalNameId);
                            //if (typeof techType !== "undefined" && techType != null) techTypeDesc = parseInt(techType.desc) + parseInt(pasteObject.IsSubContact ? 14 : 0);
                            //args.rowInfo.rowData.TotalDays += parseInt(techTypeDesc);

                            args.rowInfo.rowData.TotalDays = pasteObject.TotalDays;
                            var dt = new Date();
                            dt.setDate(dt.getDate() + args.rowInfo.rowData.TotalDays);
                            args.rowInfo.rowData.DeliveryDate = dt;

                            //args.rowInfo.rowData.CriteriaIDs = pasteObject.CriteriaIDs;

                            idsListCopyCollarOrCuff.forEach(x => {
                                var indexFCC = idsList.findIndex(y => y.SubGroupWiseIndex == args.rowInfo.rowIndex && y.SubGroupName == "Collar" && y.CriteriaName == x.CriteriaName);
                                if (indexFCC > -1) {
                                    idsList[indexFCC].CriteriaIDs = x.CriteriaIDs;
                                }
                            });

                            args.rowInfo.rowData.CriteriaIDs = idsList.filter(x => x.SubGroupWiseIndex == args.rowInfo.rowIndex
                                && x.SubGroupName == "Collar")
                                .filter(x => x.CriteriaIDs.length > 0)
                                .map(x => x.CriteriaIDs).join(",");

                            console.log("CIDs Collar=" + args.rowInfo.rowData.CriteriaIDs);

                            _oRow = args.rowInfo.rowData;
                            updateCriteriaIDTable(_oRow, pasteObject);
                            //$tblChildCollarIdEl.refresh();
                            $tblChildCollarIdEl.updateRow(args.rowInfo.rowIndex, _oRow);
                        }
                    }
                    else if (args.item.id === 'pasteAll') {
                        var rowIndex = args.rowInfo.rowIndex;
                        if (itemTNAInfoCollar == null || itemTNAInfoCollar.length == 0) {
                            toastr.error("Please copy first!!");
                            return;
                        } else {
                            var rows = $tblChildCollarIdEl.getCurrentViewRecords();
                            for (var i = 0; i < rows.length; i++) {

                                var pasteObject = objectCopy(itemTNAInfoCollar);
                                //var preSubContactDays = 0,
                                //    subContactDays = 0;
                                //if (pasteObject.IsSubContact) preSubContactDays = 14;
                                //if (rows[i].IsSubContact) subContactDays = 14;
                                //rows[i].TotalDays = pasteObject.TotalDays - pasteObject.StructureDays + rows[i].StructureDays - preSubContactDays + subContactDays;

                                rows[i].MachineTypeId = pasteObject.MachineTypeId;
                                rows[i].MachineType = pasteObject.MachineType;
                                rows[i].KTypeId = pasteObject.KTypeId;
                                rows[i].TechnicalNameId = pasteObject.TechnicalNameId;
                                rows[i].TechnicalName = pasteObject.TechnicalName;
                                rows[i].TechnicalTime = pasteObject.TechnicalTime;
                                rows[i].IsSubContact = pasteObject.IsSubContact;

                                //rows[i] = setTotalDaysAndDeliveryDate(rows[i], rows[i].CriteriaNames);
                                //var techTypeDesc = 0;
                                //var techType = masterData.TechnicalNameList.find(y => y.id == pasteObject.TechnicalNameId);
                                //if (typeof techType !== "undefined" && techType != null) techTypeDesc = parseInt(techType.desc) + parseInt(pasteObject.IsSubContact ? 14 : 0);
                                //rows[i].TotalDays += parseInt(techTypeDesc);

                                rows[i].TotalDays = pasteObject.TotalDays;
                                var dt = new Date();
                                dt.setDate(dt.getDate() + rows[i].TotalDays);
                                rows[i].DeliveryDate = dt;


                                idsListCopyCollarOrCuff.forEach(x => {
                                    var indexFCC = idsList.findIndex(y => y.SubGroupWiseIndex == i && y.SubGroupName == "Collar" && y.CriteriaName == x.CriteriaName);
                                    if (indexFCC > -1) {
                                        idsList[indexFCC].CriteriaIDs = x.CriteriaIDs;
                                    }
                                });


                                //rows[i].CriteriaIDs = pasteObject.CriteriaIDs;

                                rows[i].CriteriaIDs = idsList.filter(x => x.SubGroupWiseIndex == i
                                    && x.SubGroupName == "Collar")
                                    .filter(x => x.CriteriaIDs.length > 0)
                                    .map(x => x.CriteriaIDs).join(",");

                                updateCriteriaIDTable(rows[i], pasteObject);
                            }
                            initChildCollar(rows);

                        }
                    }
                }
            });
        }
        $tblChildCollarIdEl.refreshColumns;
        $tblChildCollarIdEl.appendTo(tblChildCollarId);
        //$tblChildCollarIdEl.autoFitColumns();
    }

    function addDays(date, days) {
        var result = new Date(date);
        result.setDate(result.getDate() + days);
        return result;
    }

    function setTotalDaysAndDeliveryDate(currentData, criteriaNames) {
        var techTypeDesc = 0;
        var totalDays = 0;
        criteriaNames.map(y => {
            totalDays += parseInt(y.TotalTime);
        });
        var techType = masterData.TechnicalNameList.find(y => y.id == currentData.TechnicalNameId);
        if (typeof techType !== "undefined" && techType != null) techTypeDesc = parseInt(techType.desc) + parseInt(currentData.IsSubContact ? 14 : 0);
        currentData.TotalDays = parseInt(techTypeDesc) + totalDays;
        currentData.DeliveryDate = currentData.TotalDays > 0 ? addDays(new Date(), currentData.TotalDays) : "";
        return currentData;
    }

    async function initChildCuff(data, isDoCalculateFields = false) {
        /*data.forEach(x => {
            
            if (menuType == _paramType.AdditionalYarnBooking && status == statusConstants.APPROVED2) {
                if (_isFirstLoad) {
                    x.YarnAllowance = 0;
                    x.BookingQty = 0;
                    x.BookingQtyKG = 0;
                    x = setAdditionalAllowance(x);
                }
                if (typeof x.FinishFabricUtilizationQty == 'undefined' || x.FinishFabricUtilizationQty == null) {
                    x.FinishFabricUtilizationQty = 0;
                }
                if (typeof x.GreyLeftOverQty == 'undefined' || x.GreyLeftOverQty == null) {
                    x.GreyLeftOverQty = 0;
                }
            }
            x.ReqFinishFabricQty = x.BookingQtyKG - x.FinishFabricUtilizationQty;
            x = setBookingQtyKGRelatedFieldsValue(x, 12);
            x.ChildItems.forEach(y => {
                if (typeof x.GreyProdQty != 'undefined' && x.GreyProdQty != null && typeof y.Distribution != 'undefined' && y.Distribution != null && typeof x.YarnAllowance != 'undefined' && x.YarnAllowance != null) {
                    y.YarnReqQty = (x.GreyProdQty * (y.Distribution / 100)) / (1 + (x.YarnAllowance / 100) - (0.5 / 100));
                    y.YarnReqQty = y.YarnReqQty.toFixed(2);
                    //y.GreyYarnUtilizationQty = 0;
                    //y.DyedYarnUtilizationQty = 0;
                    if (typeof y.GreyAllowance == 'undefined' || y.GreyAllowance == null) {
                        y.GreyAllowance = 0;
                    }
                    if (typeof y.YDAllowance == 'undefined' || y.YDAllowance == null) {
                        y.YDAllowance = 0;
                    }
                    y = getYarnRelatedProps(y, x, false, isDoCalculateFields);
                }

            });
        });
        data = setCalculatedValues(data);*/
        GetCalculatedFBookingChildCuff(data, isDoCalculateFields)
        if ($tblChildCuffIdEl) $tblChildCuffIdEl.destroy();

        var columns = [];
        var isAllowEditingCell = true;
        var isAllowEditingAllowanceCell = false;
        if (menuType == _paramType.BulkBookingYarnAllowance) {
            isAllowEditingCell = false;
            isAllowEditingAllowanceCell = true;
        }
        if (menuType == _paramType.BulkBookingCheck || menuType == _paramType.BulkBookingApprove) {

            //isAllowEditingCell = false;
            isAllowEditingAllowanceCell = true;
        }

        if (isAdditionBulkBooking()) {
            columns = [
                { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
                { field: 'ConceptTypeID', visible: false },
                { field: 'SubGroupID', visible: false },
                {
                    field: 'Construction', headerText: 'Collar Description', allowEditing: false,
                },
                {
                    field: 'Composition', headerText: 'Collar Type', allowEditing: false,
                },
                {
                    field: 'MachineType', headerText: 'Machine Type', width: 80, allowEditing: false
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', width: 80, allowEditing: false
                },
                {
                    field: 'ExistingYarnAllowance', headerText: 'Existing Yarn Allowance', allowEditing: false
                },
                {
                    field: 'YarnAllowance', headerText: 'Add. Yarn Allowance', allowEditing: true
                },
                {
                    field: 'TotalYarnAllowance', headerText: 'Total Yarn Allowance', allowEditing: false
                },
                {
                    field: 'Brand', headerText: 'Brand', allowEditing: false,
                },
                {
                    field: 'IsSubContact', headerText: 'Sub-Contact?', visible: (status != statusConstants.ACTIVE && _isBDS == 1), allowEditing: isAllowEditingCell, displayAsCheckBox: true, editType: "booleanedit", width: 85, textAlign: 'Center'
                },
                {
                    field: 'TotalDays', headerText: 'Total Days', visible: (status != statusConstants.ACTIVE && _isBDS == 1), allowEditing: false, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria
                },
                {
                    field: 'StructureDays', headerText: 'Structure Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'FinishingDays', headerText: 'Finishing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'DyeingDays', headerText: 'Dyeing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'BatchPreparationDays', headerText: 'Batch Preparation Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'KnittingDays', headerText: 'Knitting Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'TestReportDays', headerText: 'Test Report Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'MaterialDays', headerText: 'Material Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'QualityDays', headerText: 'Quality Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'DeliveryDate', headerText: 'Delivery Date', visible: status != statusConstants.ACTIVE && _isBDS == 1, textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
                },
                {
                    field: 'Color', headerText: 'Color', width: 85, allowEditing: false
                },
                //{
                //    field: 'GSM', headerText: 'GSM', width: 85, allowEditing: false
                //},
                //{
                //    field: 'FabricWidth', headerText: 'Fabric Width', width: 85, allowEditing: false
                //},
                //{
                //    field: 'KnittingType', headerText: 'Knitting Type', width: 85, allowEditing: false
                //},
                {
                    field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false
                },
                {
                    field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false
                },
                {
                    field: 'DyeingType', headerText: 'Dyeing Type', width: 85, allowEditing: false
                },
                {
                    field: 'Instruction', headerText: 'Instruction', allowEditing: false
                },
                {
                    field: 'LabDipNo', headerText: 'Lab Dip No', allowEditing: false
                },
                {
                    field: 'RefSourceNo', headerText: 'Ref No', width: 85, allowEditing: false
                },
                {
                    headerText: '', textAlign: 'Center', width: 40, visible: isBulkBookingKnittingInfoMenu(), allowEditing: isAllowEditingCell, commands: [
                        { buttonOption: { type: 'findRefSourceNo', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Ref Detail" } }
                    ]
                },
                {
                    field: 'ActualBookingQty', headerText: 'Booking Qty(Pcs)', width: 85, allowEditing: false
                },
                {
                    field: 'BookingQty', headerText: 'Replacement Qty(Pcs)', width: 120, allowEditing: false
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 40, commands: [
                        { buttonOption: { type: 'findAdditionalReplacementQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for Replacement qty" } }
                    ]
                },
                {
                    field: 'BookingQtyKG', headerText: 'Replacement Qty(KG)', width: 85, allowEditing: false
                },
                {
                    field: 'IsForFabric', headerText: 'For Fabric?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', allowEditing: true
                },
                {
                    field: 'FinishFabricUtilizationQty', headerText: 'Finish Fabric Utilization Qty', width: 120, propField: 'FinishFabricUtilizationQty', allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 40, propField: 'FinishFabricUtilizationQty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findFinishFabricUtilizationQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for finish fabric utilization qty" } }
                    ]
                },
                {
                    field: 'ReqFinishFabricQty', headerText: 'Req. Finish Fabric Qty', width: 120, allowEditing: false,
                },
                {
                    field: 'TotalQty', headerText: 'Total Qty', width: 85, allowEditing: false, visible: false //status == statusConstants.COMPLETED
                },
                {
                    field: 'GreyReqQty', headerText: 'Grey Req Qty', width: 85, allowEditing: false,
                },
                {
                    field: 'GreyLeftOverQty', headerText: 'Grey Utilization Qty', width: 85, propField: 'GreyLeftOverQty', allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 40, propField: 'GreyLeftOverQty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findGreyLeftOverQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for grey left over qty" } }
                    ]
                },
                {
                    field: 'GreyProdQty', headerText: 'Grey Prod Qty', width: 95, allowEditing: false,
                },
                {
                    headerText: 'Dist Qty', textAlign: 'Center', visible: _isBDS != 2, allowEditing: isAllowEditingCell, width: 80, commands: [
                        { buttonOption: { type: 'UsesIn', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                    ]
                },
                {
                    headerText: 'Finishing Process', textAlign: 'Center', propField: 'finishingProcess', allowEditing: isAllowEditingCell, width: 120, commands: [
                        { buttonOption: { type: 'finishingProcess', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-plus' } }
                    ]
                }
                /*{
                    field: 'GreyReqQty', headerText: 'Grey Req Qty', width: 85, allowEditing: false
                },
                {
                    field: 'GreyLeftOverQty', headerText: 'Grey Left Over Qty', width: 85, allowEditing: false
                },
                {
                    headerText: '', textAlign: 'Center', width: 40, commands: [
                        { buttonOption: { type: 'findGreyLeftOverQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for grey left over qty" } }
                    ]
                },
                {
                    field: 'GreyProdQty', headerText: 'Grey Prod Qty', width: 95, allowEditing: false
                },
                {
                    headerText: 'Dist Qty', textAlign: 'Center', visible: _isBDS != 2 && !isLabdipMenu(), width: 80, commands: [
                        { buttonOption: { type: 'UsesIn', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                    ]
                }*/
            ];
        }
        else {

            columns = [
                { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
                { field: 'BookingID', visible: false },
                { field: 'ItemMasterID', visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'ConceptTypeID', visible: false },
                {
                    field: 'Construction', headerText: 'Cuff Description', allowEditing: false, visible: true
                },
                {
                    field: 'Composition', headerText: 'Cuff Type', allowEditing: false, visible: true
                },
                {
                    field: 'MachineType', headerText: 'Machine Type ', visible: (status != statusConstants.ACTIVE || _isBDS == 2) && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation && isAllowEditingCell == true, allowEditing: isAllowEditingCell, edit: {
                        create: function () {
                            machineTypeElem = document.createElement('input');
                            return machineTypeElem;
                        },
                        read: function () {
                            return machineTypeObj.value;
                        },
                        destroy: function () {
                            machineTypeObj.destroy();
                        },
                        write: function (e) {
                            machineTypeObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.MCTypeForOtherList,
                                fields: { value: 'id', text: 'text' },

                                placeholder: 'Select Machine Type',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                change: function (f) {
                                    technicalNameObj.enabled = true;
                                    var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                    technicalNameObj.query = tempQuery;
                                    technicalNameObj.text = null;
                                    technicalNameObj.dataBind();

                                    e.rowData.MachineTypeId = f.itemData.id;
                                    e.rowData.MachineType = f.itemData.text;
                                    e.rowData.KTypeId = f.itemData.desc;
                                    e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);
                                },
                                placeholder: 'Select M/C Type',
                                floatLabelType: 'Never'
                            });
                            machineTypeObj.appendTo(machineTypeElem);
                        }
                    }
                },
                {
                    field: 'MachineType', headerText: 'Machine Type', visible: (status != statusConstants.ACTIVE || _isBDS == 2) && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation && isAllowEditingCell == false, allowEditing: isAllowEditingCell,
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', visible: (status != statusConstants.ACTIVE || _isBDS == 2) && isAllowEditingCell == true, allowEditing: isAllowEditingCell, edit: {
                        create: function () {
                            technicalNameElem = document.createElement('input');
                            return technicalNameElem;
                        },
                        read: function () {
                            return technicalNameObj.value;
                        },
                        destroy: function () {
                            technicalNameObj.destroy();
                        },
                        write: function (e) {
                            technicalNameObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.TechnicalNameList,
                                fields: { value: 'id', text: 'text' },
                                //enabled: false,
                                placeholder: 'Select Technical Name',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                change: function (f) {
                                    if (!f.isInteracted || !f.itemData) return false;
                                    e.rowData.TechnicalTime = parseInt(f.itemData.desc);
                                    e.rowData.TechnicalNameId = f.itemData.id;
                                    e.rowData.TechnicalName = f.itemData.text;
                                    e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);

                                    //$tblChildCuffIdEl.updateRow(e.row.rowIndex, e.rowData);
                                }
                            });
                            technicalNameObj.appendTo(technicalNameElem);
                        }
                    }
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', visible: (status != statusConstants.ACTIVE || _isBDS == 2) && isAllowEditingCell == false, allowEditing: isAllowEditingCell,
                },
                {
                    field: 'BrandID', headerText: 'Brand', displayField: "Brand", allowEditing: isAllowEditingCell, visible: (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) && isAllowEditingCell == true, valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            machineBrandElem = document.createElement('input');
                            return machineBrandElem;
                        },
                        read: function () {
                            return machineBrandObj.value;
                        },
                        destroy: function () {
                            machineBrandObj.destroy();
                        },
                        write: function (e) {
                            machineBrandObj = new ej.dropdowns.DropDownList({
                                dataSource: getMachineBrandList(masterData.CollarCuffBrandList, 0, 0, 12),
                                fields: { value: 'BrandID', text: 'Brand' },
                                //enabled: false,
                                placeholder: 'Select Machine Brand',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                change: function (f) {

                                    if (!f.isInteracted || !f.itemData) return false;
                                    e.rowData.BrandID = f.itemData.BrandID;
                                    e.rowData.Brand = f.itemData.Brand;

                                    $tblChildCuffIdEl.updateRow(e.row.rowIndex, e.rowData);
                                }
                            });
                            machineBrandObj.appendTo(machineBrandElem);
                        }
                    }
                },
                {
                    field: 'Brand', headerText: 'Brand', visible: (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) && isAllowEditingCell == false, allowEditing: isAllowEditingCell,
                },
                {
                    field: 'YarnAllowance', headerText: 'Yarn Allowance', visible: (isBulkBookingKnittingInfoMenu()) && menuType != _paramType.BulkBookingAck && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation, allowEditing: isAllowEditingAllowanceCell
                },
                {
                    field: 'IsSubContact', headerText: 'Sub-Contact?', visible: (status != statusConstants.ACTIVE && _isBDS == 1) || _isLabDipAck_RnD, allowEditing: isAllowEditingCell, displayAsCheckBox: true, editType: "booleanedit", width: 85, textAlign: 'Center'
                },
                {
                    field: 'TotalDays', headerText: 'Total Days', visible: (status != statusConstants.ACTIVE && _isBDS == 1) || _isLabDipAck_RnD, allowEditing: false, textAlign: 'center', width: 85, valueAccessor: diplayPlanningCriteria
                },
                {
                    field: 'StructureDays', headerText: 'Structure Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'FinishingDays', headerText: 'Finishing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'DyeingDays', headerText: 'Dyeing Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'BatchPreparationDays', headerText: 'Batch Preparation Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'KnittingDays', headerText: 'Knitting Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'TestReportDays', headerText: 'Test Report Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'MaterialDays', headerText: 'Material Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'QualityDays', headerText: 'Quality Days', visible: false, width: 85, valueAccessor: diplayPlanningCriteriaTime
                },
                {
                    field: 'DeliveryDate', headerText: 'Delivery Date', visible: status != statusConstants.ACTIVE && _isBDS == 1, textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
                },
                {
                    field: 'Color', headerText: 'Color', width: 85, allowEditing: false, visible: true
                },
                {
                    field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'ReferenceSourceName', headerText: 'Reference Source', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
                },
                {
                    field: 'ReferenceNo', headerText: 'Ref No', visible: _isBDS == 1, width: 85, allowEditing: false
                },
                {
                    field: 'ColorReferenceNo', headerText: 'Color Ref No', visible: _isBDS == 1 ? true : false, width: 85, allowEditing: false
                },
                {
                    field: 'ValueName', headerText: 'Yarn Source', visible: false, allowEditing: isAllowEditingCell/* status != statusConstants.ACTIVE*/, edit: {
                        create: function () {
                            YarnSourceNameElem = document.createElement('input');
                            return YarnSourceNameElem;
                        },
                        read: function () {
                            return YarnSourceNameobj.value;
                        },
                        destroy: function () {
                            YarnSourceNameobj.destroy();
                        },
                        write: function (e) {
                            YarnSourceNameobj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.YarnSourceNameList,

                                placeholder: 'Select Yarn Source',
                                floatLabelType: 'Never',
                                allowFiltering: true,
                                popupWidth: 'auto',
                                filtering: async function (e) {

                                    var query = new ej.data.Query();
                                    query = (e.text != "") ? query.where(fields.text, "contains", e.text, true) : query;
                                    e.updateData(dataSource, query);
                                },

                                fields: { value: 'id', text: 'text' },
                                change: function (f) {
                                    technicalNameObj.enabled = true;
                                    technicalNameObj.text = null;
                                    technicalNameObj.dataBind();

                                    e.rowData.YarnSourceID = f.itemData.id;
                                    e.rowData.ValueName = f.itemData.text;
                                },
                                placeholder: 'Select one',
                                floatLabelType: 'Never'
                            });
                            YarnSourceNameobj.appendTo(YarnSourceNameElem);
                        }
                    }
                },
                {
                    field: 'DyeingType', headerText: 'Dyeing Type', width: 85, allowEditing: false, visible: true
                },
                {
                    field: 'DayValidDurationName', headerText: 'Yarn Sourcing Mode', width: 120, allowEditing: false, visible: menuType == _paramType.Projection
                },
                {
                    field: 'Instruction', headerText: 'Instruction', allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'LabDipNo', headerText: 'Lab Dip No', allowEditing: false, visible: menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation
                },
                {
                    field: 'RefSourceNo', headerText: 'Ref No', visible: isBulkBookingKnittingInfoMenu(), width: 85, allowEditing: false
                },
                {
                    headerText: '', textAlign: 'Center', width: 40, visible: isBulkBookingKnittingInfoMenu(), allowEditing: isAllowEditingCell, commands: [
                        { buttonOption: { type: 'findRefSourceNo', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Ref Detail" } }
                    ]
                },
                {
                    field: 'BookingQty', headerText: 'Booking Qty(Pcs)', width: 85, allowEditing: false, visible: true
                },
                {
                    field: 'BookingQtyKG', headerText: 'Booking Qty(KG)', width: 85, allowEditing: false, visible: true
                },
                {
                    field: 'FinishFabricUtilizationQty', headerText: 'Finish Fabric Utilization Qty', width: 120, propField: 'FinishFabricUtilizationQty', allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, propField: 'FinishFabricUtilizationQty', width: 40, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findFinishFabricUtilizationQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for finish fabric utilization qty" } }
                    ]
                },
                {
                    field: 'ReqFinishFabricQty', headerText: 'Req. Finish Fabric Qty', width: 120, allowEditing: false, visible: isBulkBookingKnittingInfoMenu()
                },
                {
                    field: 'TotalQty', headerText: 'Total Qty', width: 85, allowEditing: false, visible: false //status == statusConstants.COMPLETED 
                },
                {
                    field: 'GreyReqQty', headerText: 'Grey Req Qty', width: 85, allowEditing: false, visible: isBulkBookingKnittingInfoMenu()
                },
                {
                    field: 'GreyLeftOverQty', headerText: 'Grey Utilization Qty', width: 85, propField: 'GreyLeftOverQty', allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
                },
                {
                    headerText: '', textAlign: 'Center', allowEditing: isAllowEditingCell, width: 40, propField: 'GreyLeftOverQty', visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking(), commands: [
                        { buttonOption: { type: 'findGreyLeftOverQty', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select for grey left over qty" } }
                    ]
                },
                {
                    field: 'GreyProdQty', headerText: 'Grey Prod Qty', width: 95, allowEditing: false, visible: isBulkBookingKnittingInfoMenu()
                },
                {
                    headerText: 'Dist Qty', textAlign: 'Center', visible: _isBDS != 2, allowEditing: isAllowEditingCell, width: 80, commands: [
                        { buttonOption: { type: 'UsesIn', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                    ]
                },
                {
                    headerText: 'Finishing Process', textAlign: 'Center', propField: 'finishingProcess', visible: isBulkBookingKnittingInfoMenu() && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation, allowEditing: isAllowEditingCell, width: 120, commands: [
                        { buttonOption: { type: 'finishingProcess', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-plus' } }
                    ]
                },
                {
                    field: 'Remarks', headerText: 'Remarks', allowEditing: _isRemarksEditable, visible: _isRemarksShow
                }
            ];

        }
        var additionalColumns = [
            {
                field: 'DeliveredQty', headerText: 'Delivered Qty(kg/pcs)', width: 85, allowEditing: false, visible: status == statusConstants.APPROVED && !isAdditionBulkBooking()
            },
            {
                field: 'DelivereyComplete', headerText: 'Is Delivered?', displayAsCheckBox: true, textAlign: 'Center', visible: status == statusConstants.APPROVED && !isAdditionBulkBooking(), allowEditing: isAllowEditingCell
            }
        ]
        columns.push.apply(columns, additionalColumns);
        var childColumns = [
            { field: 'YBChildItemID', isPrimaryKey: true, visible: false },
            { field: 'ShadeCode', headerText: 'Shade Code', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Distribution', headerText: 'Distribution', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'BookingQty', headerText: 'Booking Qty', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Allowance', headerText: 'Allowance', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'StitchLength', headerText: 'Stitch Length', width: 40, allowEditing: true, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'Specification', headerText: 'Specification', textAlign: 'Center', width: 40, allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', textAlign: 'Center', width: 40, allowEditing: false },
        ];
        ej.base.enableRipple(true);

        columns = setVisiblePropValue(columns, 12);
        columns = removeDuplicateColumns(columns);

        if (_isBDS == 2) {
            var childColumns = await getChildColumnsForBDS2(false);
            var contextMenuItems = [
                { text: 'Copy Information', target: '.e-content', id: 'copyBoth' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'pasteYarn' },
                { text: 'Paste Technical Information', target: '.e-content', id: 'pasteTech' },
                { text: 'Paste Finishing Process', target: '.e-content', id: 'pasteFinishingProcess' },
                { text: 'Paste Both', target: '.e-content', id: 'pasteBoth' }
            ];
            var isAllowEditing = true,
                isAllowAdding = true,
                isAllowDeleting = true,
                isChildGridAllowEditing = true,
                isChildGridAllowAdding = true,
                isChildGridAllowDeleting = true;

            var childGridToolbars = ['Add'];
            if (menuType == _paramType.BulkBookingYarnAllowance) {
                contextMenuItems = [];
                childGridToolbars = [];

                isAllowEditing = true;
                isAllowAdding = false;
                isAllowDeleting = false;

                isChildGridAllowAdding = false;
                isChildGridAllowDeleting = false;
            }
            else if (isAdditionBulkBooking()) {
                childGridToolbars = [
                    { text: 'Add Item', tooltipText: 'Add Item', prefixIcon: 'e-icons e-add', id: 'addItem' },
                    //{ text: 'Remove Item', tooltipText: 'Remove Item', prefixIcon: 'e-icons e-delete', id: 'removeItem' }
                ];

                columns.unshift({
                    headerText: 'Commands', textAlign: 'Center', width: 80, commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ]
                });
            }

            var queryStringValue = "";
            if (isYarnBookingAckMenu()) {
                queryStringValue = 'BookingChildID';
            }
            else if (status == statusConstants.PENDING || status == statusConstants.REJECT) {
                queryStringValue = "YBChildID";
            }
            else {
                queryStringValue = 'BookingChildID';
            }

            $tblChildCuffIdEl = new ej.grids.Grid({
                dataSource: data,
                //allowGrouping: true,
                allowPaging: false,
                allowScrolling: false,
                //height: 300, // Adjust this height based on your layout
                //frozenRows: 0, // Keep the header row fixed
                //toolbar: ['Update', 'Cancel'],
                allowResizing: true,
                columns: columns,
                commandClick: childCommandClick,
                editSettings: {
                    allowEditing: isAllowEditing,
                    allowAdding: isAllowAdding,
                    allowDeleting: isAllowDeleting,
                    mode: "Normal",
                    showDeleteConfirmDialog: true
                },
                recordClick: function (args) {
                    if (args.column && args.column.field == "TotalDays") {
                        _oRowCuff = args.rowData;
                        _indexCuff = args.rowIndex;
                        _modalFrom = subGroupNames.CUFF;
                        // initPlanningTable(_oRowCuff.FBAChildPlannings, _oRowCuff.CriteriaIDs);
                        initCriteriaIDTable(_oRowCuff.CriteriaNames, _oRowCuff.FBAChildPlannings, _oRowCuff.FBAChildPlanningsWithIds, _oRowCuff.BookingChildID);
                        $modalCriteriaEl.modal('show');
                    }
                },
                /*
                actionBegin: function (args) {
                    if (args.requestType === "save") {
                        args.data = setArgDataValues(args.data, args.rowData);
                    }
                },
                */
                childGrid: {
                    queryString: queryStringValue,
                    allowResizing: true,
                    autofitColumns: false,
                    editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: false },
                    columns: childColumns,
                    toolbar: childGridToolbars,
                    editSettings: {
                        allowEditing: isChildGridAllowEditing,
                        allowAdding: isChildGridAllowAdding,
                        allowDeleting: isChildGridAllowDeleting,
                        mode: "Normal",
                        showDeleteConfirmDialog: true
                    },
                    toolbarClick: function (args) {
                        if (args.item.id === "addItem") {

                            var parentObj = this.parentDetails.parentRowData;
                            //if (parentObj.IsForFabric) {
                            //    toastr.error("This addition is for fabric, not for yarn (For yarn addition uncheck for fabric).");
                            //    return false;
                            //}
                            var index = $tblChildCuffIdEl.getRowIndexByPrimaryKey(parentObj.BookingChildID);
                            var allYarns = _allYarnList.filter(x => x.BookingChildID == parentObj.BookingChildID);

                            if (allYarns.length == 0) {
                                toastr.error("No list found");
                                return false;
                            }
                            var currentYarns = parentObj.ChildItems;
                            var childItemIds = currentYarns.map(x => x.YBChildItemID).join(",");

                            var columns = getYarnCommanFinderColumns();
                            var fieldList = columns.map(x => x.Field).join(",");
                            var headerTextList = columns.map(x => x.HeaderText).join(",");
                            var widthList = columns.map(x => x.Width).join(",");

                            var finder = new commonFinder({
                                title: "Select Yarn",
                                pageId: pageId,
                                height: 320,
                                data: allYarns,
                                fields: fieldList,
                                headerTexts: headerTextList,
                                widths: widthList,
                                isMultiselect: true,
                                autofitColumns: true,
                                primaryKeyColumn: "YBChildItemID",
                                modalSize: "modal-lg",
                                top: "2px",
                                selectedIds: childItemIds,
                                onMultiselect: function (selectedRecords) {
                                    if (selectedRecords.length > 0) {
                                        parentObj.ChildItems = selectedRecords;
                                        $tblChildCuffIdEl.updateRow(index, parentObj);
                                    }
                                }
                            });
                            finder.showModal();
                        }
                    },
                    actionBegin: function (args) {
                        if (args.requestType === 'beginEdit') {
                            if (args.rowData.YDProductionMasterID > 0) {
                                toastr.error("Yarn Dyeing found, You cannot modify anything.");
                                args.cancel = true;
                            }
                        }
                        else if (args.requestType === "add") {

                            args.data.YBChildItemID = maxCol++; //getMaxIdForArray(masterData.Childs, "YBChildItemID");
                            args.data.YBChildID = this.parentDetails.parentRowData.YBChildID;
                            args.data.BookingChildID = this.parentDetails.parentRowData.BookingChildID;
                            args.data.ConsumptionID = this.parentDetails.parentRowData.ConsumptionID;


                            if (isBulkBookingKnittingInfoMenu()) {
                                args.data.GreyAllowance = this.parentDetails.parentRowData.YarnAllowance;
                                args.data.Allowance = args.data.GreyAllowance;
                            }

                            var totalDis = 0, remainDis = 0;
                            this.dataSource.forEach(l => {
                                totalDis += l.Distribution;
                            })
                            if (totalDis < 100) remainDis = 100 - totalDis;
                            else {
                                toastr.error("Distribution can not more then 100!!");
                                args.cancel = true;
                                return;
                            }
                            var netConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(remainDis) / 100);
                            var reqQty = netConsumption;
                            args.data.Distribution = remainDis;
                            args.data.BookingQty = netConsumption.toFixed(4);
                            args.data.Allowance = 0.00;
                            args.data.RequiredQty = reqQty.toFixed(2);

                            args.data.DisplayUnitDesc = "Kg";
                            args.data.SubGroupId = 1;

                            args.data.Segment1ValueId = 0;
                            args.data.Segment2ValueId = 0;
                            args.data.Segment3ValueId = 0;
                            args.data.Segment4ValueId = 0;
                            args.data.Segment5ValueId = 0;
                            args.data.Segment6ValueId = 0;
                            args.data.Segment7ValueId = 0;
                            args.data.Segment8ValueId = 0;
                            //getAllYarnList();
                        }
                        else if (args.requestType === "save") {

                            args.data = checkAndSetYarnValidSegmentCH(args.data, _yarnSegmentsMapping);
                            args.data = setNullIfIdNullYarnSegment(args.data);
                            if (!args.data.YD && args.data.YDAllowance > 0) {
                                args.data.YDAllowance = 0;
                                toastr.error("YD allowance only valid for Go For YD item.");
                            }
                            else if (args.data.YD && (args.data.YDAllowance < 0 || args.data.YDAllowance > 35)) {
                                toastr.error("YD allowance should be between 0 to 35.");
                                args.data.YDAllowance = 0;
                                return false;
                            }
                            //Saif_04_10_2023 END

                            args.data.GreyAllowance = getDefaultValueWhenInvalidN_Float(args.data.GreyAllowance);
                            args.data.YDAllowance = getDefaultValueWhenInvalidN_Float(args.data.YDAllowance);

                            args.data.Allowance = args.data.GreyAllowance + args.data.YDAllowance;
                            var parentObj = this.parentDetails.parentRowData;
                            if (typeof parentObj.YarnAllowance == 'undefined' && parentObj.YarnAllowance == null) {
                                parentObj.YarnAllowance = 0;
                            }
                            var reqQty = 0;
                            if (typeof parentObj.GreyProdQty != 'undefined' && parentObj.GreyProdQty != null && typeof args.data.Distribution != 'undefined' && args.data.Distribution != null) {
                                reqQty = (parentObj.GreyProdQty * (args.data.Distribution / 100)) / (1 + (parentObj.YarnAllowance / 100) - (0.5 / 100));
                            }
                            args.data.YarnReqQty = reqQty.toFixed(2);
                            if (isAdditionBulkBooking() && parentObj.IsForFabric == false) {
                                args.data.YarnReqQty = args.data.NetYarnReqQty;
                            }
                            args.data.GreyYarnUtilizationQty = 0;
                            args.data.DyedYarnUtilizationQty = 0;
                            if (typeof args.data.GreyAllowance == 'undefined' || args.data.GreyAllowance == null) {
                                args.data.GreyAllowance = 0;
                            }
                            if (typeof args.data.YDAllowance == 'undefined' || args.data.YDAllowance == null) {
                                args.data.YDAllowance = 0;
                            }

                            if (isAdditionBulkBooking() && parentObj.IsForFabric == false) {
                                args.data = getYarnRelatedPropsAdditionalYarn(args.data, this.parentDetails.parentRowData, false, true);
                            } else {
                                args.data = getYarnRelatedProps(args.data, this.parentDetails.parentRowData, false, true);
                            }

                            if (typeof args.data.YarnReqQty === "undefined") args.data.YarnReqQty = 0;
                            if (isNaN(args.data.YarnReqQty)) args.data.YarnReqQty = 0;
                            if (typeof args.data.YarnBalanceQty === "undefined") args.data.YarnBalanceQty = 0;
                            if (isNaN(args.data.YarnBalanceQty)) args.data.YarnBalanceQty = 0;

                            var NetConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(args.data.Distribution) / 100);
                            //var reqQty = parseFloat(NetConsumption) + ((parseFloat(NetConsumption) * parseFloat(args.data.Allowance)) / 100);

                            //args.data.Distribution = args.rowData.Distribution;
                            args.data.YarnSubBrandIDs = args.rowData.YarnSubBrandIDs;
                            //args.data.YBChildID = this.parentDetails.parentRowData.YBChildID;
                            args.data.YBChildID = args.rowData.YBChildID;
                            args.data.BookingQty = NetConsumption.toFixed(4);
                            args.data.RequiredQty = reqQty.toFixed(2);
                            /* Saif Stopped On 27-02-2024
                            args.rowData.Segment1ValueId = !args.data.Segment1ValueId ? 0 : args.data.Segment1ValueId;
                            args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                            args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                            args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                            args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                            args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                            args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                            args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;
                            */
                            args.data = setYarnSegDesc(args.data);

                            args = setSegmentValueFromRowDataToData(args);
                        }
                        else if (args.requestType === "delete") {
                            if (args.data[0].YDProductionMasterID > 0) {
                                toastr.error("Yarn Dyeing found, You cannot modify anything.");
                                args.cancel = true;
                            }
                            if (isAdditionBulkBooking()) {
                                var parentObj = this.parentDetails.parentRowData;
                                //if (parentObj.IsForFabric) {
                                //    toastr.error("This addition is for fabric, not for yarn (For yarn addition uncheck for fabric).");
                                //    var index = $tblChildCuffIdEl.getRowIndexByPrimaryKey(parentObj.ConsumptionID);
                                //    parentObj.ChildItems = _allYarnList.filter(x => x.BookingChildID == parentObj.BookingChildID);
                                //    $tblChildCuffIdEl.updateRow(index, parentObj);
                                //    return false;
                                //}
                            }
                        }
                    },
                    commandClick: childCommandClickChild2, // alamin
                    load: loadYarnBookingChildItems
                },
                enableContextMenu: true,
                contextMenuItems: contextMenuItems,
                contextMenuClick: function (args) {
                    if (args.item.id === 'copyBoth') {

                        collarCuffYarnItem.ParentInfo = objectCopy(args.rowInfo.rowData);
                        collarCuffYarnItem.ChildItems = objectCopy(args.rowInfo.rowData.ChildItems);

                        if (typeof collarCuffYarnItem.ChildItems === "undefined" || collarCuffYarnItem.ChildItems == null || collarCuffYarnItem.ChildItems.length == 0) {
                            collarCuffYarnItem.ChildItems = [];
                        }

                        //Copy Finishing Process
                        //var indexF = masterData.Childs.findIndex(x => x.BookingChildID == collarCuffYarnItem.ParentInfo.BookingChildID);
                        var indexF = masterData.Childs.findIndex(x => x.Construction == collarCuffYarnItem.ParentInfo.Construction && x.Composition == collarCuffYarnItem.ParentInfo.Composition && x.Color == collarCuffYarnItem.ParentInfo.Color);
                        if (indexF > -1) {
                            masterData.Childs[indexF].PreFinishingProcessChilds = getValidList(masterData.Childs[indexF].PreFinishingProcessChilds);
                            masterData.Childs[indexF].PostFinishingProcessChilds = getValidList(masterData.Childs[indexF].PostFinishingProcessChilds);

                            collarCuffYarnItem.PreFinishingProcessChilds = DeepClone(masterData.Childs[indexF].PreFinishingProcessChilds);
                            collarCuffYarnItem.PostFinishingProcessChilds = DeepClone(masterData.Childs[indexF].PostFinishingProcessChilds);
                        }
                    }
                    else if (args.item.id === 'pasteYarn') {
                        if (typeof collarCuffYarnItem.ChildItems === "undefined" || collarCuffYarnItem.ChildItems == null || collarCuffYarnItem.ChildItems.length == 0) {
                            //toastr.error("Please copy yarn item first!!");
                            return;
                        }
                        for (var i = 0; i < collarCuffYarnItem.ChildItems.length; i++) {
                            var copiedItem = objectCopy(collarCuffYarnItem.ChildItems[i]);

                            copiedItem.YBChildItemID = _fbChildItemID++;
                            copiedItem.YBChildID = args.rowInfo.rowData.YBChildID;
                            copiedItem.YBookingID = args.rowInfo.rowData.YBookingID;

                            copiedItem.BookingChildID = args.rowInfo.rowData.BookingChildID;
                            copiedItem.BookingID = args.rowInfo.rowData.BookingID;

                            var parentInfo = {
                                GreyProdQty: 0
                            };
                            copiedItem = setYarnRelatedSingleField(copiedItem, parentInfo);
                            args.rowInfo.rowData.ChildItems.push(copiedItem);
                        }
                        $tblChildCuffIdEl.refresh();
                    }
                    else if (args.item.id === 'pasteTech') {
                        if (typeof collarCuffYarnItem.ParentInfo === "undefined" || collarCuffYarnItem.ParentInfo == null) {
                            //toastr.error("Please copy technical info first!!");
                            return;
                        }

                        args.rowInfo.rowData.MachineTypeId = collarCuffYarnItem.ParentInfo.MachineTypeId;
                        args.rowInfo.rowData.MachineType = collarCuffYarnItem.ParentInfo.MachineType;
                        args.rowInfo.rowData.TechnicalNameId = collarCuffYarnItem.ParentInfo.TechnicalNameId;
                        args.rowInfo.rowData.TechnicalName = collarCuffYarnItem.ParentInfo.TechnicalName;
                        args.rowInfo.rowData.BrandID = collarCuffYarnItem.ParentInfo.BrandID;
                        args.rowInfo.rowData.Brand = collarCuffYarnItem.ParentInfo.Brand;

                        pasteFinishingProcess(args.rowInfo.rowData.BookingChildID, collarCuffYarnItem);

                        $tblChildCuffIdEl.refresh();

                    }
                    else if (args.item.id === 'pasteFinishingProcess') {
                        if (typeof collarCuffYarnItem.ParentInfo === "undefined" || collarCuffYarnItem.ParentInfo == null) {
                            //toastr.error("Please copy technical info first!!");
                            return;
                        }
                        pasteFinishingProcessCollarCuff(args.rowInfo.rowData, collarCuffYarnItem);
                        $tblChildCuffIdEl.refresh();
                    }
                    else if (args.item.id === 'pasteBoth') {
                        if (typeof collarCuffYarnItem.ParentInfo === "undefined" || collarCuffYarnItem.ParentInfo == null) {
                            // toastr.error("Please copy first!!");
                            return;
                        }

                        args.rowInfo.rowData.MachineTypeId = collarCuffYarnItem.ParentInfo.MachineTypeId;
                        args.rowInfo.rowData.MachineType = collarCuffYarnItem.ParentInfo.MachineType;
                        args.rowInfo.rowData.TechnicalNameId = collarCuffYarnItem.ParentInfo.TechnicalNameId;
                        args.rowInfo.rowData.TechnicalName = collarCuffYarnItem.ParentInfo.TechnicalName;
                        args.rowInfo.rowData.BrandID = collarCuffYarnItem.ParentInfo.BrandID;
                        args.rowInfo.rowData.Brand = collarCuffYarnItem.ParentInfo.Brand;

                        for (var i = 0; i < collarCuffYarnItem.ChildItems.length; i++) {
                            var copiedItem = objectCopy(collarCuffYarnItem.ChildItems[i]);

                            copiedItem.YBChildItemID = _fbChildItemID++;
                            copiedItem.YBChildID = args.rowInfo.rowData.YBChildID;
                            copiedItem.YBookingID = args.rowInfo.rowData.YBookingID;

                            copiedItem.BookingChildID = args.rowInfo.rowData.BookingChildID;
                            copiedItem.BookingID = args.rowInfo.rowData.BookingID;

                            var parentInfo = {
                                GreyProdQty: 0
                            };
                            copiedItem = setYarnRelatedSingleField(copiedItem, parentInfo);
                            args.rowInfo.rowData.ChildItems.push(copiedItem);
                        }

                        pasteFinishingProcessCollarCuff(args.rowInfo.rowData, collarCuffYarnItem);

                        $tblChildCuffIdEl.refresh();
                    }
                },
                commandClick: childCommandClick2,
                actionBegin: function (args) {

                    if (args.requestType === "save") {
                        if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
                            //Saif_04_10_2023
                            if (args.data.YarnAllowance < 0 || args.data.YarnAllowance > 35) {
                                args.data.YarnAllowance = 0;
                                toastr.error("Yarn allowance should be between 0 to 35.");
                                return false;
                            }
                            //Saif_04_10_2023 END

                            args.data.ChildItems.forEach(x => {
                                x.GreyAllowance = args.data.YarnAllowance;
                                x.Allowance = parseFloat(x.GreyAllowance) + parseFloat(x.YDAllowance);
                            });
                            args.data.GreyReqQty = (args.data.ReqFinishFabricQty * (1 + (args.data.YarnAllowance / 100) - (0.5 / 100))).toFixed(0);
                            args.data.GreyProdQty = args.data.GreyReqQty - args.data.GreyLeftOverQty;

                            setModifiedChildData(args);
                            $tblChildCuffIdEl.updateRow(args.rowIndex, args.data);
                            setChildData(args.data, 12);
                        }
                        if (isAdditionBulkBooking()) {
                            args.data = getAdditionBulkBookingData(args.data, 12);
                            setAdditionalAllowance(args.data);
                            setChildData(args.data, 12);
                        }
                    }
                },
            });
        } else {
            $tblChildCuffIdEl = new ej.grids.Grid({
                dataSource: data,
                allowResizing: true,
                columns: columns,
                commandClick: childCommandClick,
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                recordClick: function (args) {
                    if (args.column && args.column.field == "TotalDays") {
                        _oRowCuff = args.rowData;
                        _indexCuff = args.rowIndex;
                        _modalFrom = subGroupNames.CUFF;
                        // initPlanningTable(_oRowCuff.FBAChildPlannings, _oRowCuff.CriteriaIDs);
                        initCriteriaIDTable(_oRowCuff.CriteriaNames, _oRowCuff.FBAChildPlannings, _oRowCuff.FBAChildPlanningsWithIds, _oRowCuff.BookingChildID);
                        $modalCriteriaEl.modal('show');
                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "save") {

                        args.data = setArgDataValues(args.data, args.rowData);
                    }
                },
                enableContextMenu: true,
                contextMenuItems: [
                    { text: 'Copy TNA', target: '.e-content', id: 'copy' },
                    { text: 'Paste TNA', target: '.e-content', id: 'paste' },
                    { text: 'Paste TNA To All', target: '.e-content', id: 'pasteAll' }
                ],
                contextMenuClick: function (args) {
                    if (args.item.id === 'copy') {
                        itemTNAInfoCollar = objectCopy(args.rowInfo.rowData);
                        if (itemTNAInfoCollar.length == 0) {
                            toastr.error("No TNA information found to copy!!");
                            return;
                        }
                        var selctedRowCriterias = idsList.filter(x => x.SubGroupWiseIndex == args.rowInfo.rowIndex && x.SubGroupName == "Cuff");
                        if (selctedRowCriterias) {
                            idsListCopyCollarOrCuff = JSON.parse(JSON.stringify(selctedRowCriterias));
                        }
                    }
                    else if (args.item.id === 'paste') {
                        var rowIndex = args.rowInfo.rowIndex;
                        if (itemTNAInfoCollar == null || itemTNAInfoCollar.length == 0) {
                            toastr.error("Please copy first!!");
                            return;
                        } else {
                            var pasteObject = objectCopy(itemTNAInfoCollar);
                            var preSubContactDays = 0,
                                subContactDays = 0;
                            //if (pasteObject.IsSubContact) preSubContactDays = 14;
                            //if (args.rowInfo.rowData.IsSubContact) subContactDays = 14;
                            //args.rowInfo.rowData.TotalDays = pasteObject.TotalDays - pasteObject.StructureDays + args.rowInfo.rowData.StructureDays - preSubContactDays + subContactDays;

                            args.rowInfo.rowData.MachineTypeId = pasteObject.MachineTypeId;
                            args.rowInfo.rowData.MachineType = pasteObject.MachineType;
                            args.rowInfo.rowData.KTypeId = pasteObject.KTypeId;
                            args.rowInfo.rowData.TechnicalNameId = pasteObject.TechnicalNameId;
                            args.rowInfo.rowData.TechnicalName = pasteObject.TechnicalName;
                            args.rowInfo.rowData.TechnicalTime = pasteObject.TechnicalTime;
                            args.rowInfo.rowData.IsSubContact = pasteObject.IsSubContact;

                            //args.rowInfo.rowData = setTotalDaysAndDeliveryDate(args.rowInfo.rowData, args.rowInfo.rowData.CriteriaNames);
                            //var techTypeDesc = 0;
                            //var techType = masterData.TechnicalNameList.find(y => y.id == pasteObject.TechnicalNameId);
                            //if (typeof techType !== "undefined" && techType != null) techTypeDesc = parseInt(techType.desc) + parseInt(pasteObject.IsSubContact ? 14 : 0);
                            //args.rowInfo.rowData.TotalDays += parseInt(techTypeDesc);

                            args.rowInfo.rowData.TotalDays = pasteObject.TotalDays;
                            var dt = new Date();
                            dt.setDate(dt.getDate() + args.rowInfo.rowData.TotalDays);
                            args.rowInfo.rowData.DeliveryDate = dt;

                            //args.rowInfo.rowData.CriteriaIDs = pasteObject.CriteriaIDs;

                            idsListCopyCollarOrCuff.forEach(x => {
                                var indexFCC = idsList.findIndex(y => y.SubGroupWiseIndex == args.rowInfo.rowIndex && y.SubGroupName == "Cuff" && y.CriteriaName == x.CriteriaName);
                                if (indexFCC > -1) {
                                    idsList[indexFCC].CriteriaIDs = x.CriteriaIDs;
                                }
                            });

                            args.rowInfo.rowData.CriteriaIDs = idsList.filter(x => x.SubGroupWiseIndex == args.rowInfo.rowIndex
                                && x.SubGroupName == "Cuff")
                                .filter(x => x.CriteriaIDs.length > 0)
                                .map(x => x.CriteriaIDs).join(",");

                            console.log("CIDs Cuff=" + args.rowInfo.rowData.CriteriaIDs);

                            _oRow = args.rowInfo.rowData;
                            updateCriteriaIDTable(_oRow, pasteObject);
                            //$tblChildCuffIdEl.refresh();
                            $tblChildCuffIdEl.updateRow(args.rowInfo.rowIndex, _oRow);
                        }
                    }
                    else if (args.item.id === 'pasteAll') {
                        var rowIndex = args.rowInfo.rowIndex;
                        if (itemTNAInfoCollar == null || itemTNAInfoCollar.length == 0) {
                            toastr.error("Please copy first!!");
                            return;
                        } else {

                            var rows = $tblChildCuffIdEl.getCurrentViewRecords();
                            for (var i = 0; i < rows.length; i++) {

                                var pasteObject = objectCopy(itemTNAInfoCollar);
                                //var preSubContactDays = 0,
                                //    subContactDays = 0;
                                //if (pasteObject.IsSubContact) preSubContactDays = 14;
                                //if (rows[i].IsSubContact) subContactDays = 14;
                                //rows[i].TotalDays = pasteObject.TotalDays - pasteObject.StructureDays + rows[i].StructureDays - preSubContactDays + subContactDays;

                                rows[i].MachineTypeId = pasteObject.MachineTypeId;
                                rows[i].MachineType = pasteObject.MachineType;
                                rows[i].KTypeId = pasteObject.KTypeId;
                                rows[i].TechnicalNameId = pasteObject.TechnicalNameId;
                                rows[i].TechnicalName = pasteObject.TechnicalName;
                                rows[i].TechnicalTime = pasteObject.TechnicalTime;
                                rows[i].IsSubContact = pasteObject.IsSubContact;

                                //rows[i] = setTotalDaysAndDeliveryDate(rows[i], rows[i].CriteriaNames);
                                //var techTypeDesc = 0;
                                //var techType = masterData.TechnicalNameList.find(y => y.id == pasteObject.TechnicalNameId);
                                //if (typeof techType !== "undefined" && techType != null) techTypeDesc = parseInt(techType.desc) + parseInt(pasteObject.IsSubContact ? 14 : 0);
                                //rows[i].TotalDays += parseInt(techTypeDesc);

                                rows[i].TotalDays = pasteObject.TotalDays;
                                var dt = new Date();
                                dt.setDate(dt.getDate() + rows[i].TotalDays);
                                rows[i].DeliveryDate = dt;


                                idsListCopyCollarOrCuff.forEach(x => {
                                    var indexFCC = idsList.findIndex(y => y.SubGroupWiseIndex == i && y.SubGroupName == "Cuff" && y.CriteriaName == x.CriteriaName);
                                    if (indexFCC > -1) {
                                        idsList[indexFCC].CriteriaIDs = x.CriteriaIDs;
                                    }
                                });


                                //rows[i].CriteriaIDs = pasteObject.CriteriaIDs;

                                rows[i].CriteriaIDs = idsList.filter(x => x.SubGroupWiseIndex == i
                                    && x.SubGroupName == "Cuff")
                                    .filter(x => x.CriteriaIDs.length > 0)
                                    .map(x => x.CriteriaIDs).join(",");

                                console.log("CIDs Cuff=" + rows[i].CriteriaIDs);

                                updateCriteriaIDTable(rows[i], pasteObject);

                            }
                            initChildCuff(rows);

                        }
                    }
                }
            });
        }
        $tblChildCuffIdEl.refreshColumns;
        $tblChildCuffIdEl.appendTo(tblChildCuffId);
        //$tblChildCuffIdEl.autoFitColumns();
    }

    async function initPlanningTable(data, criteriaIDs) {
        if ($tblPlanningEl) $tblPlanningEl.destroy();
        ej.base.enableRipple(true);

        var columns = [
            { field: 'CriteriaID', visible: false, isPrimaryKey: true },
            { field: 'CriteriaName', headerText: 'Criteria Name', allowEditing: false },
            { field: 'OperationName', headerText: 'Operation Name', allowEditing: false },
            { field: 'ProcessTime', headerText: 'Process Time', allowEditing: false }
        ];

        var tableOptions = {
            dataSource: data,
            allowResizing: true,
            editSettings: { allowEditing: false, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
        };

        var isMultiple = false;
        if (data[0].CriteriaName == 'Finishing' || data[0].CriteriaName == 'Material' || data[0].CriteriaName == 'Preprocess') {
            isMultiple = true;
            columns.unshift({ type: 'checkbox', width: 50 });
            tableOptions["columns"] = columns;
            tableOptions["selectionSettings"] = { type: "Multiple" };
            tableOptions["dataBound"] = function (args) {

                var ids = criteriaIDs.split(',');
                var selIds = [];
                for (var i = 0; i < ids.length; i++) {
                    selIds.push($tblPlanningEl.getRowIndexByPrimaryKey(parseInt(ids[i])));
                }
                //if (selIds.length > 0 && selIds[0] == -1) {
                //    if (data[0].CriteriaName == 'Finishing' || data[0].CriteriaName == 'Material') $tblPlanningEl.selectedRowIndex = 0;

                //}
                //else this.selectRows(selIds);

                this.selectRows(selIds);
            };
            /*
            tableOptions["rowSelected"] = function (args) {  //checkBoxChange//rowSelected//rowSelecting                
                var selectedRows = $tblPlanningEl.getSelectedRecords();
                var selIds = [];
                for (var i = 0; i < selectedRows.length; i++) {
                    selIds.push($tblPlanningEl.getRowIndexByPrimaryKey(selectedRows[i].CriteriaID));
                }

                if (args.data.OperationName == "Available Grey") {
                    //var obj = selectedRows.find(function (el) { return el.OperationName == "Yarn Dyed" });
                    var objs = selectedRows.filter(function (el) { return el.OperationName == "Yarn Dyed" || el.OperationName == "Yarn Twisted" || el.OperationName == "Purchase (Local)" || el.OperationName == "Purchase (Imported)" });
                    if (objs.length > 0) {
                        var index = selIds.indexOf(args.rowIndex);
                        if (index > -1) {
                            selIds.splice(index, 1);
                        }
                        $tblPlanningEl.selectRows(selIds);
                    }
                }
                else if (args.data.OperationName == "Yarn Dyed") {
                    var objs = selectedRows.filter(function (el) { return el.OperationName == "Available Grey" || el.OperationName == "Reconning" });
                    if (objs.length > 0) {
                        var index = selIds.indexOf(args.rowIndex);
                        if (index > -1) {
                            selIds.splice(index, 1);
                        }
                        $tblPlanningEl.selectRows(selIds);
                    }
                }
                else if (args.data.OperationName == "Yarn Twisted") {
                    var objs = selectedRows.filter(function (el) { return el.OperationName == "Available Grey" || el.OperationName == "Reconning" });
                    if (objs.length > 0) {
                        var index = selIds.indexOf(args.rowIndex);
                        if (index > -1) {
                            selIds.splice(index, 1);
                        }
                        $tblPlanningEl.selectRows(selIds);
                    }
                }
                else if (args.data.OperationName == "Purchase (Local)") {
                    var objs = selectedRows.filter(function (el) { return el.OperationName == "Available Grey" });
                    if (objs.length > 0) {
                        var index = selIds.indexOf(args.rowIndex);
                        if (index > -1) {
                            selIds.splice(index, 1);
                        }
                        $tblPlanningEl.selectRows(selIds);
                    }
                }
                else if (args.data.OperationName == "Purchase (Imported)") {
                    var objs = selectedRows.filter(function (el) { return el.OperationName == "Available Grey" });
                    if (objs.length > 0) {
                        var index = selIds.indexOf(args.rowIndex);
                        if (index > -1) {
                            selIds.splice(index, 1);
                        }
                        $tblPlanningEl.selectRows(selIds);
                    }
                }
                else if (args.data.OperationName == "Reconning") {
                    var objs = selectedRows.filter(function (el) { return el.OperationName == "Yarn Dyed" || el.OperationName == "Yarn Twisted" });
                    if (objs.length > 0) {
                        var index = selIds.indexOf(args.rowIndex);
                        if (index > -1) {
                            selIds.splice(index, 1);
                        }
                        $tblPlanningEl.selectRows(selIds);
                    }
                }
            };
            */
        }
        tableOptions["columns"] = columns;
        $tblPlanningEl = new ej.grids.Grid(
            tableOptions
        );
        if (!isMultiple) {
            if (data[0].CriteriaName == 'Testing' || data[0].CriteriaName == 'Dyeing') {
                var dIndex = -1;
                var splitCriteriaIDs = criteriaIDs.split(',');
                if (splitCriteriaIDs.length > 0) {
                    for (var iData = 0; iData < data.length; iData++) {
                        var findCIndex = splitCriteriaIDs.findIndex(x => x == parseInt(data[iData].CriteriaID));
                        if (findCIndex > -1) {
                            dIndex = iData;
                            break;
                        }
                    }
                    //var dIndex = data.findIndex(d => d.CriteriaID == criteriaIDs);
                    $tblPlanningEl.selectedRowIndex = dIndex;
                }
            }
        }
        $tblPlanningEl.refreshColumns;
        $tblPlanningEl.appendTo(tblPlanningId);

        //$formEl.find("#tblPlanningBDSAcknowledge-" + menuId).find(".e-table thead .e-columnheader").find(".e-headerchkcelldiv").hide();


        //$tblPlanningEl = new ej.grids.Grid({
        //    dataSource: data,
        //    allowResizing: true,
        //    columns: [
        //        { field: 'CriteriaID', visible: false, isPrimaryKey: true },
        //        { type: 'checkbox', width: 50 },
        //        { field: 'CriteriaName', headerText: 'Criteria Name', allowEditing: false },
        //        { field: 'OperationName', headerText: 'Operation Name', allowEditing: false },
        //        { field: 'ProcessTime', headerText: 'Process Time', allowEditing: false }
        //    ],
        //    editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
        //    selectionSettings: { type: "Multiple" },
        //    dataBound: function (args) {
        //        var ids = criteriaIDs.split(',');
        //        var selIds = [];
        //        for (var i = 0; i < ids.length; i++) {
        //            selIds.push($tblPlanningEl.getRowIndexByPrimaryKey(parseInt(ids[i])));
        //        }
        //        this.selectRows(selIds);
        //    }
        //});
        //$tblPlanningEl.refreshColumns;
        //$tblPlanningEl.appendTo(tblPlanningId);
    }

    async function initCriteriaIDTable(data, criteriaData, savedData, childId) {
        if (childId) {
            data.forEach(function (d) {
                var obj = savedData.find(function (el) { return d.CriteriaName == el.CriteriaName });
                if (obj) {
                    if (d.TotalTime == 0) {
                        d.TotalTime = obj.TotalTime;
                        d.CriteriaIDs = obj.CriteriaIDs;
                    }
                }
            });
        }

        if ($tblCriteriaIdEl) $tblCriteriaIdEl.destroy();

        ej.base.enableRipple(true);
        $tblCriteriaIdEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,

            columns: [
                { field: 'CriteriaName', headerText: 'Criteria Name', allowEditing: false },
                { field: 'TotalTime', headerText: 'Process Time', allowEditing: false, valueAccessor: diplayPlanningCriteria }
            ],
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            recordClick: function (args) {
                CriteriaName = (args.rowData.CriteriaName);
                if (args.column && args.column.field == "TotalTime" && CriteriaName != 'Batch Preparation' && CriteriaName != 'Quality Check') {
                    _oRowc = args.rowData;
                    _indexc = args.rowIndex;

                    var pChild = $.grep(criteriaData, function (h) { return h.CriteriaName == CriteriaName });
                    initPlanningTable(pChild, _oRowc.CriteriaIDs);
                    $modalPlanningEl.modal('show');
                }
            }
        });
        $tblCriteriaIdEl.refreshColumns;
        $tblCriteriaIdEl.appendTo(tblCriteriaId);
    }

    function getValidList(listItems) {
        if (typeof listItems === "undefined") return [];
        return listItems;
    }

    function backToListBulk() {

        //initBulkAckList(0);
        $divDetailsEl.hide();
        resetForm();
        $divTblEl.show();
    }
    function backToListBulk2() {

        if (!isAdditionBulkBooking()) {
            //initBulkAckList(menuType);
            //$tblMasterEl.refresh();
        }
        else {
            if (menuType == _paramType.AdditionalYarnBooking) {
                //initBulkAckList(menuType);
                //$tblMasterEl.refresh();
            } else {
                listLoadOperationBulkAddition();
            }
        }
        $divDetailsEl.hide();
        resetForm();
        $divTblEl.show();

        if (menuType == _paramType.YarnBookingAcknowledge) {
            initBulkAckList(_paramType.YarnBookingAcknowledge);
        }
    }

    function backToList() {

        if (isLabdipMenu()) initLabDipAckTable();
        else initMasterTable();

        $divDetailsEl.hide();
        resetForm();
        $divTblEl.show();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#FBAckID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(bookingId, isForRevise) {

        _isFirstLoad = true;
        var url = `/api/bds-acknowledge/new/${bookingId}`;
        if (isForRevise) {
            url = `/api/bds-acknowledge/new/forRevise/${bookingId}`;
        }
        if (isLabdipMenu()) {
            url = `/api/bds-acknowledge/new/labDip/${bookingId}`;
        }
        axios.get(url)
            .then(function (response) {
                if (isLabdipMenu()) {
                    $formEl.find("#btnSave,#btnUnAcknowledge").hide();
                    $formEl.find("#btnSaveLabDipAck,#btnSaveLabDipUnAck").show();
                }

                idsList = [];
                idsListCopyFabric = [];
                idsListCopyCollarOrCuff = [];

                itemTNAInfo = null;
                itemTNAInfoCollar = null;

                $divDetailsEl.show();
                $divTblEl.hide();
                masterData = response.data;
                /*
                if (masterData.IsYarnRevision) {
                    _IsYarnRevision = true;
                }
                else {
                    _IsYarnRevision = false;
                }
                */
                masterData.BookingDate = formatDateToDefault(masterData.BookingDate);

                setFormData($formEl, masterData);

                var fbIndex = -1;

                if (masterData.HasFabric) {
                    if (bmtArray.length > 0) {
                        var newFBookingChild = [];
                        var fabricIndex = -1;
                        masterData.FBookingChild.map(x => {
                            fabricIndex++;
                            fbIndex++;
                            var cnIndex = -1;
                            x.CriteriaNames.map(cn => {
                                cnIndex++;

                                idsList.push({
                                    RowIndex: fbIndex,
                                    CriteriaIndex: cnIndex,
                                    SubGroupWiseIndex: fabricIndex,
                                    SubGroupName: "Fabric",
                                    CriteriaName: cn.CriteriaName,
                                    CriteriaIDs: ""
                                });
                            });

                            var constructionId = x.ConstructionId;
                            if (constructionId != null && constructionId > 0) {
                                var bmtObj = bmtArray.find(x => x.ConstructionID == constructionId);
                                if (isValidValue(bmtObj)) {
                                    var technicalNameId = x.Composition.toUpperCase().indexOf("ELASTANE") > -1 ? bmtObj.TechnicalNameID_Elastane : bmtObj.TechnicalNameID;
                                    x.MachineTypeId = bmtObj.SubClassID;
                                    if (bmtObj.SubClassID > 0) {
                                        x.MachineType = masterData.MCTypeForFabricList.find(x => x.id == bmtObj.SubClassID).text;
                                        x.KTypeId = masterData.MCTypeForFabricList.find(x => x.id == bmtObj.SubClassID).desc;
                                    }

                                    if (technicalNameId != '0') {
                                        x.TechnicalNameId = technicalNameId;
                                        x.TechnicalName = masterData.TechnicalNameList.find(x => x.id == technicalNameId).text;
                                    }
                                }
                            }
                            newFBookingChild.push(x);
                        });
                        masterData.FBookingChild = newFBookingChild;
                    }
                    initChild(masterData.FBookingChild);
                    $formEl.find("#divFabricInfo").show();
                }
                else $formEl.find("#divFabricInfo").hide();

                if (masterData.HasCollar) {
                    var collarIndex = -1;
                    masterData.FBookingChildCollor.map(x => {
                        collarIndex++;
                        fbIndex++;
                        var cnIndex = -1;
                        x.CriteriaNames.map(cn => {
                            cnIndex++;

                            idsList.push({
                                RowIndex: fbIndex,
                                CriteriaIndex: cnIndex,
                                SubGroupWiseIndex: collarIndex,
                                SubGroupName: "Collar",
                                CriteriaName: cn.CriteriaName,
                                CriteriaIDs: ""
                            });
                        });
                    });

                    initChildCollar(masterData.FBookingChildCollor);
                    $formEl.find("#divCollarInfo").show();
                }
                else $formEl.find("#divCollarInfo").hide();

                if (masterData.HasCuff) {
                    var cuffIndex = -1;
                    masterData.FBookingChildCuff.map(x => {
                        cuffIndex++;
                        fbIndex++;
                        var cnIndex = -1;
                        x.CriteriaNames.map(cn => {
                            cnIndex++;
                            idsList.push({
                                RowIndex: fbIndex,
                                CriteriaIndex: cnIndex,
                                SubGroupWiseIndex: cuffIndex,
                                SubGroupName: "Cuff",
                                CriteriaName: cn.CriteriaName,
                                CriteriaIDs: ""
                            });
                        });
                    });
                    initChildCuff(masterData.FBookingChildCuff);
                    $formEl.find("#divCufInfo").show();
                }
                else $formEl.find("#divCufInfo").hide();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getNewBulk(bookingNo, isSample) {
        _isFirstLoad = true;

        axios.get(`/api/yarn-booking/forBulk/${bookingNo}/${isSample}`)
            .then(function (response) {
                masterData = response.data;
                //alert(masterData.Remarks);
                /*
                if (masterData.IsYarnRevision) {
                    _IsYarnRevision = true;
                }
                else {
                    _IsYarnRevision = false;
                }
                */
                $divDetailsEl.show();
                $divTblEl.hide();


                masterData.YBookingDate = formatDateToDefault(masterData.YBookingDate);
                masterData.BookingDate = formatDateToDefault(masterData.BookingDate);
                masterData.RequiredFabricDeliveryDate = formatDateToDefault(masterData.RequiredFabricDeliveryDate);

                setFormData($formEl, masterData);

                if (masterData.HasFabric) {
                    masterData.Fabrics.map(m => {
                        m = DeepClone(setGreyRelatedSingleField(m));
                        m.ChildItems = setYarnRelatedFields(m.ChildItems, m, true);
                    });

                    if (bmtArray.length > 0 && masterData.Fabrics.length > 0) {
                        masterData.Fabrics.map(x => {
                            var constructionId = x.ConstructionID;
                            if (constructionId != null && constructionId > 0) {
                                var bmtObj = bmtArray.find(x => x.ConstructionID == constructionId);
                                if (isValidValue(bmtObj)) {
                                    var technicalNameId = x.Composition.toUpperCase().indexOf("ELASTANE") > -1 ? bmtObj.TechnicalNameID_Elastane : bmtObj.TechnicalNameID;
                                    x.MachineTypeId = bmtObj.SubClassID;
                                    if (bmtObj.SubClassID > 0) {
                                        x.MachineType = masterData.MCTypeForFabricList.find(x => x.id == bmtObj.SubClassID).text;
                                        x.KTypeId = masterData.MCTypeForFabricList.find(x => x.id == bmtObj.SubClassID).desc;
                                    }
                                    if (technicalNameId != '0') {
                                        x.TechnicalNameId = technicalNameId;
                                        x.TechnicalName = masterData.TechnicalNameList.find(x => x.id == technicalNameId).text;
                                    }
                                }
                            }
                        });

                    }

                    initChild(masterData.Fabrics);
                    $formEl.find("#divFabricInfo").show();
                }
                else $formEl.find("#divFabricInfo").hide();

                if (masterData.HasCollar) {
                    masterData.Collars.map(m => {
                        m = DeepClone(setGreyRelatedSingleField(m));
                        m.ChildItems = setYarnRelatedFields(m.ChildItems, m, true);
                    });

                    initChildCollar(masterData.Collars);
                    $formEl.find("#divCollarInfo").show();
                }
                else $formEl.find("#divCollarInfo").hide();

                if (masterData.HasCuff) {
                    masterData.Cuffs.map(m => {
                        m = DeepClone(setGreyRelatedSingleField(m));
                        m.ChildItems = setYarnRelatedFields(m.ChildItems, m, true);
                    });

                    initChildCuff(masterData.Cuffs);
                    $formEl.find("#divCufInfo").show();
                }
                else $formEl.find("#divCufInfo").hide();
            })
            .catch(showResponseError);
    }

    function setGreyRelatedFields(items) {
        items.map(item => {
            item = setGreyRelatedSingleField(item);
        });
        return items;
    }
    function setGreyRelatedSingleField(item) {

        if (menuType == _paramType.BulkBookingAck) {
            if (status == statusConstants.PENDING || status == statusConstants.DRAFT) return item;
        }


        //var pL = 10; //percentage
        //var knitting = 0.5 //percentage
        var others = 9.5 / 100; //percentage

        if (!item.GreyReqQty) {
            item.GreyReqQty = item.BookingQty + (item.BookingQty * others);
        }


        if (typeof item.GreyLeftOverQty === "undefined") item.GreyLeftOverQty = 0;

        //if (item.GreyLeftOverQty || item.GreyLeftOverQty == 0) {
        //    item.GreyProdQty = item.GreyReqQty - item.GreyLeftOverQty;
        //} else {
        //    item.GreyProdQty = 0;
        //}

        item.GreyReqQty = parseFloat(item.GreyReqQty).toFixed(2);
        item.GreyLeftOverQty = parseFloat(item.GreyLeftOverQty).toFixed(2);
        //item.GreyProdQty = parseFloat(item.GreyProdQty).toFixed(2);
        if (!item.GreyYarnUtilizationQty) {
            item.GreyYarnUtilizationQty = 0;
        }
        item.GreyProdQty = item.GreyReqQty - item.GreyYarnUtilizationQty;

        return item;
    }

    function setYarnRelatedFields(items, parent, isDoCalculateFields) {

        items.map(item => {
            item = DeepClone(setYarnRelatedSingleField(item, parent, isDoCalculateFields));
        });
        return items;
    }
    function setYarnRelatedSingleField(item, parent, isDoCalculateFields = true) {

        if (!isAdditionBulkBooking()) {
            if (typeof parent.YarnAllowance == 'undefined' && parent.YarnAllowance == null) {
                parent.YarnAllowance = 0;
            }
        }
        if (isAdditionBulkBooking() && parent.IsForFabric == false) {
            item = getYarnRelatedPropsAdditionalYarn(item, parent, false, isDoCalculateFields);
        } else {
            item = getYarnRelatedProps(item, parent, false, isDoCalculateFields);
        }
        return item;
    }
    function getNetYarnReqQty(yarnDistribution, finishFabricUtilizationQty, greyUtilizationQty, dyedYarnUtilizationQty, totalAllowance, yDAllowance, greyYarnUtilizationQty, reqFinishFabricQty) {

        var yarnFFU = parseFloat(yarnDistribution) * (parseFloat(finishFabricUtilizationQty) / 100);
        yarnFFU = yarnFFU + (yarnFFU * parseFloat(totalAllowance)) / 100;

        var yarnGU = parseFloat(yarnDistribution) * (parseFloat(greyUtilizationQty) / 100);
        yarnGU = yarnGU + (yarnGU * (parseFloat(yDAllowance) + parseFloat(0.5))) / 100;

        //var yarnDYU = yarnDistribution * (dyedYarnUtilizationQty / 100);
        //yarnDYU = yarnDYU + (yarnDYU * (yDAllowance)) / 100;

        //var yarnGYU = greyYarnUtilizationQty;
        reqFinishFabricQty = (reqFinishFabricQty / 100) * yarnDistribution;
        var netReqQty = parseFloat(reqFinishFabricQty) + ((parseFloat(reqFinishFabricQty) * parseFloat(totalAllowance)) / 100) - yarnGU;
        //var netReqQty = parseFloat(reqFinishFabricQty) + ((parseFloat(reqFinishFabricQty) * parseFloat(totalAllowance)) / 100) - yarnFFU - yarnGU;

        return netReqQty;
    }
    function getYarnBalanceQty(netYarnReqQty, yarnDistribution, dyedYarnUtilizationQty, yDAllowance, greyYarnUtilizationQty) {

        var yarnDYU = parseFloat(yarnDistribution) * (parseFloat(dyedYarnUtilizationQty) / 100);
        yarnDYU = yarnDYU + (yarnDYU * (parseFloat(yDAllowance))) / 100;

        var yarnGYU = parseFloat(greyYarnUtilizationQty);

        var balanceQty = netYarnReqQty - yarnDYU - yarnGYU;

        return balanceQty;
    }
    function getYarnRelatedProps(obj, parent, isdistributionChenged = false, isDoCalculateFields = false) {

        obj.YarnReqQty = getDefaultValueWhenInvalidN(obj.YarnReqQty);
        obj.Allowance = getDefaultValueWhenInvalidN(obj.Allowance);
        obj.GreyAllowance = getDefaultValueWhenInvalidN(obj.GreyAllowance);
        obj.YDAllowance = getDefaultValueWhenInvalidN(obj.YDAllowance);
        obj.NetYarnReqQty = getDefaultValueWhenInvalidN(obj.NetYarnReqQty);
        obj.GreyYarnUtilizationQty = getDefaultValueWhenInvalidN(obj.GreyYarnUtilizationQty);
        obj.DyedYarnUtilizationQty = getDefaultValueWhenInvalidN(obj.DyedYarnUtilizationQty);
        obj.YarnLeftOverQty = getDefaultValueWhenInvalidN(obj.YarnLeftOverQty);
        obj.Distribution = getDefaultValueWhenInvalidN(obj.Distribution);

        parent.GreyProdQty = getDefaultValueWhenInvalidN(parent.GreyProdQty);
        parent.FinishFabricUtilizationQty = getDefaultValueWhenInvalidN(parent.FinishFabricUtilizationQty);
        parent.GreyLeftOverQty = getDefaultValueWhenInvalidN(parent.GreyLeftOverQty);
        parent.ReqFinishFabricQty = getDefaultValueWhenInvalidN(parent.ReqFinishFabricQty);
        //if (isDoCalculateFields) {

        var netYRQ = getNetYarnReqQty(obj.Distribution, parent.FinishFabricUtilizationQty, parent.GreyLeftOverQty, obj.DyedYarnUtilizationQty, obj.Allowance, obj.YDAllowance, obj.GreyYarnUtilizationQty, parent.ReqFinishFabricQty);


        //var NetYarnReqQty = 0;
        if (parent.GreyProdQty > 0 || isdistributionChenged == true) {
            obj.NetYarnReqQty = netYRQ;//((parent.GreyProdQty * (1 + (0.50 / 100))) * (obj.Distribution / 100) * (1 + (obj.YDAllowance / 100)));
            //NetYarnReqQty = ((parent.GreyProdQty * (1 + (0.50 / 100))) * (obj.Distribution / 100));
        }
        else {
            obj.YarnReqQty = netYRQ > 0 ? netYRQ : 0;//obj.NetYarnReqQty;
            //NetYarnReqQty = obj.NetYarnReqQty;
            obj.NetYarnReqQty = 0;
        }
        obj.YarnBalanceQty = getYarnBalanceQty(netYRQ, obj.Distribution, obj.DyedYarnUtilizationQty, obj.YDAllowance, obj.GreyYarnUtilizationQty);//parseFloat((parseFloat(NetYarnReqQty) - parseFloat(obj.DyedYarnUtilizationQty)) * (1 + (parseFloat(obj.YDAllowance) / 100)) - parseFloat(obj.GreyYarnUtilizationQty));


        //}

        //obj.YarnBalanceQty = parseFloat(obj.NetYarnReqQty - obj.GreyYarnUtilizationQty - obj.DyedYarnUtilizationQty);


        obj.YarnReqQty = parseFloat(obj.YarnReqQty).toFixed(2);
        obj.Allowance = parseFloat(obj.Allowance).toFixed(2);
        obj.GreyAllowance = parseFloat(obj.GreyAllowance).toFixed(2);
        obj.YDAllowance = parseFloat(obj.YDAllowance).toFixed(2);
        obj.NetYarnReqQty = parseFloat(obj.NetYarnReqQty).toFixed(2);
        obj.GreyYarnUtilizationQty = parseFloat(obj.GreyYarnUtilizationQty).toFixed(2);
        obj.DyedYarnUtilizationQty = parseFloat(obj.DyedYarnUtilizationQty).toFixed(2);
        obj.YarnLeftOverQty = parseFloat(obj.YarnLeftOverQty).toFixed(2);
        obj.YarnBalanceQty = parseFloat(obj.YarnBalanceQty).toFixed(2);

        return obj;
    }
    function getYarnRelatedPropsAdditionalYarn(obj, parent, isdistributionChenged = false, isDoCalculateFields = false) {

        obj.YarnReqQty = getDefaultValueWhenInvalidN(obj.YarnReqQty);
        obj.Allowance = getDefaultValueWhenInvalidN(obj.Allowance);
        obj.GreyAllowance = getDefaultValueWhenInvalidN(obj.GreyAllowance);
        obj.YDAllowance = getDefaultValueWhenInvalidN(obj.YDAllowance);
        obj.NetYarnReqQty = getDefaultValueWhenInvalidN(obj.NetYarnReqQty);
        obj.GreyYarnUtilizationQty = getDefaultValueWhenInvalidN(obj.GreyYarnUtilizationQty);
        obj.DyedYarnUtilizationQty = getDefaultValueWhenInvalidN(obj.DyedYarnUtilizationQty);
        obj.YarnLeftOverQty = getDefaultValueWhenInvalidN(obj.YarnLeftOverQty);
        obj.Distribution = getDefaultValueWhenInvalidN(obj.Distribution);

        parent.GreyProdQty = getDefaultValueWhenInvalidN(parent.GreyProdQty);
        parent.FinishFabricUtilizationQty = getDefaultValueWhenInvalidN(parent.FinishFabricUtilizationQty);
        parent.GreyLeftOverQty = getDefaultValueWhenInvalidN(parent.GreyLeftOverQty);
        parent.ReqFinishFabricQty = getDefaultValueWhenInvalidN(parent.ReqFinishFabricQty);
        //if (isDoCalculateFields) {

        //var netYRQ = getNetYarnReqQty(obj.Distribution, parent.FinishFabricUtilizationQty, parent.GreyLeftOverQty, obj.DyedYarnUtilizationQty, obj.Allowance, obj.YDAllowance, obj.GreyYarnUtilizationQty, parent.ReqFinishFabricQty);

        //if (parent.GreyProdQty > 0 || isdistributionChenged == true) {
        //    obj.NetYarnReqQty = netYRQ;
        //}
        //else {
        //    obj.YarnReqQty = netYRQ > 0 ? netYRQ : 0;//obj.NetYarnReqQty;
        //    obj.NetYarnReqQty = 0;
        //}
        obj.YarnBalanceQty = getYarnBalanceQty(obj.NetYarnReqQty, 100, obj.DyedYarnUtilizationQty, 0, obj.GreyYarnUtilizationQty);


        //}


        obj.YarnReqQty = parseFloat(obj.YarnReqQty).toFixed(2);
        obj.Allowance = parseFloat(obj.Allowance).toFixed(2);
        obj.GreyAllowance = parseFloat(obj.GreyAllowance).toFixed(2);
        obj.YDAllowance = parseFloat(obj.YDAllowance).toFixed(2);
        obj.NetYarnReqQty = parseFloat(obj.NetYarnReqQty).toFixed(2);
        obj.GreyYarnUtilizationQty = parseFloat(obj.GreyYarnUtilizationQty).toFixed(2);
        obj.DyedYarnUtilizationQty = parseFloat(obj.DyedYarnUtilizationQty).toFixed(2);
        obj.YarnLeftOverQty = parseFloat(obj.YarnLeftOverQty).toFixed(2);
        obj.YarnBalanceQty = parseFloat(obj.YarnBalanceQty).toFixed(2);

        return obj;
    }
    /*function getYarnRelatedProps(obj, parent, isdistributionChenged = false, isDoCalculateFields = false) {

        obj.YarnReqQty = getDefaultValueWhenInvalidN(obj.YarnReqQty);
        obj.Allowance = getDefaultValueWhenInvalidN(obj.Allowance);
        obj.GreyAllowance = getDefaultValueWhenInvalidN(obj.GreyAllowance);
        obj.YDAllowance = getDefaultValueWhenInvalidN(obj.YDAllowance);
        obj.NetYarnReqQty = getDefaultValueWhenInvalidN(obj.NetYarnReqQty);
        obj.GreyYarnUtilizationQty = getDefaultValueWhenInvalidN(obj.GreyYarnUtilizationQty);
        obj.DyedYarnUtilizationQty = getDefaultValueWhenInvalidN(obj.DyedYarnUtilizationQty);
        obj.YarnLeftOverQty = getDefaultValueWhenInvalidN(obj.YarnLeftOverQty);
        obj.Distribution = getDefaultValueWhenInvalidN(obj.Distribution);

        parent.GreyProdQty = getDefaultValueWhenInvalidN(parent.GreyProdQty);

        //if (isDoCalculateFields) {
        if (obj.YD) {
            var NetYarnReqQty = 0;
            if (parent.GreyProdQty > 0 || isdistributionChenged == true) {
                obj.NetYarnReqQty = ((parent.GreyProdQty * (1 + (0.50 / 100))) * (obj.Distribution / 100) * (1 + (obj.YDAllowance / 100)));
                NetYarnReqQty = ((parent.GreyProdQty * (1 + (0.50 / 100))) * (obj.Distribution / 100));
            }
            else {
                obj.YarnReqQty = obj.NetYarnReqQty;
                NetYarnReqQty = obj.NetYarnReqQty;
            }
            obj.YarnBalanceQty = parseFloat((parseFloat(NetYarnReqQty) - parseFloat(obj.DyedYarnUtilizationQty)) * (1 + (parseFloat(obj.YDAllowance) / 100)) - parseFloat(obj.GreyYarnUtilizationQty));
        }
        else {
            if (parent.GreyProdQty > 0 || isdistributionChenged == true) {
                obj.NetYarnReqQty = ((parent.GreyProdQty * (1 + (0.50 / 100))) * (obj.Distribution / 100));
            }
            else {
                obj.YarnReqQty = obj.NetYarnReqQty;
            }
            obj.YarnBalanceQty = parseFloat(parseFloat(obj.NetYarnReqQty) - (parseFloat(obj.GreyYarnUtilizationQty) + parseFloat(obj.DyedYarnUtilizationQty)));
        }

        //}

        //obj.YarnBalanceQty = parseFloat(obj.NetYarnReqQty - obj.GreyYarnUtilizationQty - obj.DyedYarnUtilizationQty);


        obj.YarnReqQty = parseFloat(obj.YarnReqQty).toFixed(2);
        obj.Allowance = parseFloat(obj.Allowance).toFixed(2);
        obj.GreyAllowance = parseFloat(obj.GreyAllowance).toFixed(2);
        obj.YDAllowance = parseFloat(obj.YDAllowance).toFixed(2);
        obj.NetYarnReqQty = parseFloat(obj.NetYarnReqQty).toFixed(2);
        obj.GreyYarnUtilizationQty = parseFloat(obj.GreyYarnUtilizationQty).toFixed(2);
        obj.DyedYarnUtilizationQty = parseFloat(obj.DyedYarnUtilizationQty).toFixed(2);
        obj.YarnLeftOverQty = parseFloat(obj.YarnLeftOverQty).toFixed(2);
        obj.YarnBalanceQty = parseFloat(obj.YarnBalanceQty).toFixed(2);

        return obj;
    }*/
    function isValidValue(value) {
        if (typeof value === "undefined" || value == null) return false;
        return true;
    }

    function getView(fbAckId, bookingNo, isSample, yBookingNo, isRevisionValid) {

        $formEl.find("#btnReviseBBKI").hide();
        _isRevise = false;
        _isFirstLoad = true;

        var isAllowYBookingNo = getIsAllowYBookingNo();

        var url = '';
        var isSavedAddition = (status == statusConstants.ADDITIONAL || status == statusConstants.ADDITIONAL_INTERNAL_REJECTION) && menuType == _paramType.AdditionalYarnBooking ? true : false;
        var isYarnRevisionMenu = isBulkBookingKnittingInfoRevisionMenu();

        var isFromYBAck = false;
        if (menuType == _paramType.YarnBookingAcknowledge && status == statusConstants.ACKNOWLEDGE) {
            isFromYBAck = true;
        }

        if (isBulkBookingKnittingInfoMenu()) {
            url = `/api/bds-acknowledge/bulk/${bookingNo}/${isSample}/${yBookingNo}/${isYarnRevisionMenu}/${isFromYBAck}`;
        }
        else if (isAdditionBulkBooking()) {
            url = `/api/bds-acknowledge/bulk/addition/${bookingNo}/${isSample}/${yBookingNo}/${isSavedAddition}/${isAllowYBookingNo}/${isFromYBAck}`;
        }
        else {
            url = `/api/bds-acknowledge/${fbAckId}`;
        }

        axios.get(url)
            .then(function (response) {
                $divDetailsEl.show();
                $divTblEl.hide();
                if (menuType == _paramType.BulkBookingAck && status == statusConstants.APPROVED_PMC) {
                    $formEl.find("#btnReviseBBKIYarn").show();
                }
                if (menuType == _paramType.AdditionalYarnBooking && status == statusConstants.ADDITIONAL_APPROVED_OPERATION_HEAD) {
                    $formEl.find("#btnReviseBBKIAdYarn").show();
                }
                if (menuType == _paramType.YarnBookingAcknowledge) {
                    $divDetailsEl.find(".addYarnComposition").hide();
                } else {
                    $divDetailsEl.find(".addYarnComposition").show();
                }
                masterData = response.data;

                if (menuType == _paramType.AdditionalYarnBooking && status == statusConstants.APPROVED2 && _isFirstLoad) {
                    masterData.FBookingChild.forEach(x => {
                        x.YarnAllowance = 0;
                        //x.TotalYarnAllowance = 0;
                        x.BookingQty = 0;
                        x.BookingQtyKG = 0;
                        x = setAdditionalAllowance(x);
                        x.ChildItems.forEach(y => {
                            y.YarnReqQty = 0;
                            y.NetYarnReqQty = 0;
                            y.YarnAllowance = 0;
                            y.Allowance = 0;
                            y = getYarnRelatedPropsAdditionalYarn(y, x, false, isDoCalculateFields);
                        });
                    });
                    masterData.FBookingChildCollor.forEach(x => {
                        x.YarnAllowance = 0;
                        //x.TotalYarnAllowance = 0;
                        x.BookingQty = 0;
                        x.BookingQtyKG = 0;
                        x = setAdditionalAllowance(x);
                        x.ChildItems.forEach(y => {
                            y.YarnReqQty = 0;
                            y.NetYarnReqQty = 0;
                            y.YarnAllowance = 0;
                            y.Allowance = 0;
                            y = getYarnRelatedPropsAdditionalYarn(y, x, false, isDoCalculateFields);
                        });
                    });
                    masterData.FBookingChildCuff.forEach(x => {
                        x.YarnAllowance = 0;
                        //x.TotalYarnAllowance = 0;
                        x.BookingQty = 0;
                        x.BookingQtyKG = 0;
                        x = setAdditionalAllowance(x);
                        x.ChildItems.forEach(y => {
                            y.YarnReqQty = 0;
                            y.NetYarnReqQty = 0;
                            y.YarnAllowance = 0;
                            y.Allowance = 0;
                            y = getYarnRelatedPropsAdditionalYarn(y, x, false, isDoCalculateFields);
                        });
                    });
                }

                masterData.BookingDate = formatDateToDefault(masterData.BookingDate);
                masterData.BookingDateFR = formatDateToDefault(masterData.BookingDateFR);

                masterData.RequiredFabricDeliveryDate = formatDateToDefault(masterData.RequiredFabricDeliveryDate);
                masterData.FirstShipmentDate = formatDateToDefault(masterData.FirstShipmentDate);

                masterData.GarmentsShipmentDate = formatDateToDefault(masterData.GarmentsShipmentDate);
                masterData.FabricRequireDate = formatDateToDefault(masterData.FabricRequireDate);
                masterData.FabricRequireDateEnd = formatDateToDefault(masterData.FabricRequireDateEnd);

                masterData.YarnRequiredDate = formatDateToDefault(masterData.YarnRequiredDate);
                masterData.FabricStartDate = formatDateToDefault(masterData.FabricStartDate);
                masterData.FabricEndDate = formatDateToDefault(masterData.FabricEndDate);
                masterData.YarnBookingDate = formatDateToDefault(masterData.YarnBookingDate);
                masterData.AddYarnBookingDate = formatDateToDefault(masterData.AddYarnBookingDate);

                masterData.YarnBookingDateActual = formatDateToDefault(masterData.YarnBookingDateActual);
                masterData.YarnBookingDateFR = formatDateToDefault(masterData.YarnBookingDateFR);
                masterData.YarnRequiredDateFR = formatDateToDefault(masterData.YarnRequiredDateFR);
                masterData.YarnRequiredDateBOYB = formatDateToDefault(masterData.YarnRequiredDateBOYB);
                masterData.RevisionDate = formatDateToDefault(masterData.RevisionDate);
                masterData.YarnBookingRevisionDate = formatDateToDefault(masterData.YarnBookingRevisionDate);


                var isDoCalculateFields = false;
                setFormData($formEl, masterData);

                if (masterData.FabricRequireDateEnd == '01/01/0001') {
                    masterData.FabricRequireDateEnd = new Date();
                    $formEl.find("#FabricRequireDateEnd").val("");
                }


                if (masterData.HasFabric) {
                    if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
                        masterData.FBookingChild.map(m => {
                            var constructionId = m.ConstructionId;
                            if (constructionId != null && constructionId > 0) {
                                var bmtObj = bmtArray.find(x => x.ConstructionID == constructionId);
                                if (isValidValue(bmtObj)) {
                                    var technicalNameId = m.Composition.toUpperCase().indexOf("ELASTANE") > -1 ? bmtObj.TechnicalNameID_Elastane : bmtObj.TechnicalNameID;
                                    if (m.MachineTypeId == 0 || m.MachineTypeId == null || typeof m.MachineTypeId == 'undefined') {
                                        m.MachineTypeId = bmtObj.SubClassID;
                                    }
                                    if (bmtObj.SubClassID > 0 && (typeof m.KTypeId === "undefined" || m.KTypeId == 0)) {
                                        m.MachineType = masterData.MCTypeForFabricList.find(x => x.id == bmtObj.SubClassID).text;
                                        m.KTypeId = masterData.MCTypeForFabricList.find(x => x.id == bmtObj.SubClassID).desc;
                                    }
                                    if (technicalNameId != '0' && (typeof m.TechnicalNameId === "undefined" || m.TechnicalNameId == 0)) {
                                        m.TechnicalNameId = technicalNameId;
                                        m.TechnicalName = masterData.TechnicalNameList.find(x => x.id == technicalNameId).text;
                                    }
                                }
                            }
                            m = DeepClone(setGreyRelatedSingleField(m));
                            m.ChildItems = setYarnRelatedFields(m.ChildItems, m, isDoCalculateFields);
                            _allYarnList.push(...m.ChildItems);
                        });
                    }
                    if (menuType == _paramType.AdditionalYarnBooking && status == statusConstants.APPROVED2) {
                        initChild([], isDoCalculateFields);
                    } else {
                        initChild(masterData.FBookingChild, isDoCalculateFields);
                    }
                    $formEl.find("#divFabricInfo").show();
                }
                else $formEl.find("#divFabricInfo").hide();

                if (masterData.HasCollar) {
                    if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
                        masterData.FBookingChildCollor.map(m => {
                            m = DeepClone(setGreyRelatedSingleField(m));
                            m.ChildItems = setYarnRelatedFields(m.ChildItems, m, isDoCalculateFields);
                            _allYarnList.push(...m.ChildItems);
                        });
                    }
                    if (menuType == _paramType.AdditionalYarnBooking && status == statusConstants.APPROVED2) {
                        initChildCollar([], isDoCalculateFields);
                    } else {
                        initChildCollar(masterData.FBookingChildCollor, isDoCalculateFields);
                    }
                    $formEl.find("#divCollarInfo").show();
                }
                else $formEl.find("#divCollarInfo").hide();

                if (masterData.HasCuff) {
                    if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
                        masterData.FBookingChildCuff.map(m => {
                            m = DeepClone(setGreyRelatedSingleField(m));
                            m.ChildItems = setYarnRelatedFields(m.ChildItems, m, isDoCalculateFields);
                            _allYarnList.push(...m.ChildItems);
                        });
                    }
                    if (menuType == _paramType.AdditionalYarnBooking && status == statusConstants.APPROVED2) {
                        initChildCuff([], isDoCalculateFields);
                    } else {
                        initChildCuff(masterData.FBookingChildCuff, isDoCalculateFields);
                    }
                    $formEl.find("#divCufInfo").show();
                }
                else $formEl.find("#divCufInfo").hide();

                if (status == statusConstants.INTERNAL_REJECTION) {
                    $formEl.find('#btnSave').html("Save & Send For Approval");
                    $formEl.find('#btnUnAcknowledge').html("Reject");
                    $formEl.find("#btnSave,#btnUnAcknowledge").show();
                }
                else if (isYarnBookingAckMenu("PENDING") || isYarnBookingAckMenu("REVISE")) {
                    $formEl.find("#btnAckYBA,#btnUnAckYBA").show();
                }

                if (isYarnBookingAckMenu()) {
                    if ($tblItemSummaryEl) $tblItemSummaryEl.destroy();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getRevise(bookingNo, isSample) {
        _isRevise = true;
        _isFirstLoad = true;
        var url = `/api/bds-acknowledge/bulk/revise/${bookingNo}/${isSample}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.show();
                $divTblEl.hide();
                masterData = response.data;

                masterData.BookingDate = formatDateToDefault(masterData.BookingDate);
                masterData.RequiredFabricDeliveryDate = formatDateToDefault(masterData.RequiredFabricDeliveryDate);
                setFormData($formEl, masterData);
                if (masterData.HasFabric) {
                    if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
                        masterData.FBookingChild.map(m => {
                            m = DeepClone(setGreyRelatedSingleField(m));
                            m.ChildItems = setYarnRelatedFields(m.ChildItems, m, false);
                        });
                    }
                    initChild(masterData.FBookingChild);
                    $formEl.find("#divFabricInfo").show();
                }
                else $formEl.find("#divFabricInfo").hide();

                if (masterData.HasCollar) {
                    if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
                        masterData.FBookingChildCollor.map(m => {
                            m = DeepClone(setGreyRelatedSingleField(m));
                            m.ChildItems = setYarnRelatedFields(m.ChildItems, m, false);
                        });
                    }
                    initChildCollar(masterData.FBookingChildCollor);
                    $formEl.find("#divCollarInfo").show();
                }
                else $formEl.find("#divCollarInfo").hide();

                if (masterData.HasCuff) {
                    if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
                        masterData.FBookingChildCuff.map(m => {
                            m = DeepClone(setGreyRelatedSingleField(m));
                            m.ChildItems = setYarnRelatedFields(m.ChildItems, m, false);
                        });
                    }
                    initChildCuff(masterData.FBookingChildCuff);
                    $formEl.find("#divCufInfo").show();
                }
                else $formEl.find("#divCufInfo").hide();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getViewLabDip(bookingId) {
        var isRnD = status == statusConstants.REVISE || status == statusConstants.UN_ACKNOWLEDGE ? true : _isLabDipAck_RnD;
        var url = `/api/bds-acknowledge/labDip/${bookingId}/${isRnD}`;
        if (status == statusConstants.REVISE_FOR_ACKNOWLEDGE && _isLabDipAck_RnD) {
            url = `/api/bds-acknowledge/labDip/revision/${bookingId}`;
        }
        if (status == statusConstants.REVISE && _isLabDipAck_RnD) {
            url = `/api/bds-acknowledge/labDip/acknowledgedData/${bookingId}/${isRnD}`;
        }
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.show();
                $divTblEl.hide();
                masterData = response.data;
                masterData.BookingDate = formatDateToDefault(masterData.BookingDate);
                masterData.FBookingChild.FBAChildPlannings = response.data.AllChildPlannings;
                setFormData($formEl, masterData);
                if (masterData.HasFabric) {
                    if (isBulkBookingKnittingInfoMenu()) {
                        masterData.FBookingChild.map(m => {
                            m = DeepClone(setGreyRelatedSingleField(m));
                            m.ChildItems = setYarnRelatedFields(m.ChildItems, m, false);
                        });
                    }
                    initChild(masterData.FBookingChild);
                    $formEl.find("#divFabricInfo").show();
                }
                else $formEl.find("#divFabricInfo").hide();

                if (masterData.HasCollar) {
                    if (isBulkBookingKnittingInfoMenu()) {
                        masterData.FBookingChildCollor.map(m => {
                            m = DeepClone(setGreyRelatedSingleField(m));
                            m.ChildItems = setYarnRelatedFields(m.ChildItems, m, false);
                        });
                    }
                    initChildCollar(masterData.FBookingChildCollor);
                    $formEl.find("#divCollarInfo").show();
                }
                else $formEl.find("#divCollarInfo").hide();

                if (masterData.HasCuff) {
                    if (isBulkBookingKnittingInfoMenu()) {
                        masterData.FBookingChildCuff.map(m => {
                            m = DeepClone(setGreyRelatedSingleField(m));
                            m.ChildItems = setYarnRelatedFields(m.ChildItems, m, false);
                        });
                    }
                    initChildCuff(masterData.FBookingChildCuff);
                    $formEl.find("#divCufInfo").show();
                }
                else $formEl.find("#divCufInfo").hide();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function checkPropValues(obj) {
        if (isNaN(obj.BrandID)) obj.BrandID = 0;
        //if (obj.BrandID == 0 && masterData.KnittingMachines.length > 0) obj.BrandID = (masterData.KnittingMachines.find(x => x.text == obj.Brand)).id;
        if (isNaN(obj.BatchPreparationDays)) obj.BatchPreparationDays = 0;
        if (isNaN(obj.DyeingDays)) obj.DyeingDays = 0;
        if (isNaN(obj.FinishingDays)) obj.FinishingDays = 0;
        if (isNaN(obj.KnittingDays)) obj.KnittingDays = 0;
        if (isNaN(obj.MaterialDays)) obj.MaterialDays = 0;
        if (isNaN(obj.StructureDays)) obj.StructureDays = 0;
        if (isNaN(obj.TestReportDays)) obj.TestReportDays = 0;
        if (isNaN(obj.TotalDays)) obj.TotalDays = 0;
        return obj;
    }

    function checkMachineTypeAndTechName(obj) {
        if (obj.MachineTypeId == null || obj.MachineTypeId == 0) {
            var machineType = masterData.MCTypeForFabricList.find(x => x.text == obj.MachineType);
            if (typeof machineType === "undefined") {
                machineType = masterData.MCTypeForOtherList.find(x => x.text == obj.MachineType);
            }
            if (machineType) {
                obj.MachineTypeId = machineType.id;
            }
        }
        if (obj.TechnicalNameId == null || obj.TechnicalNameId == 0) {
            var tObj = masterData.TechnicalNameList.find(x => x.text == obj.TechnicalName);
            if (tObj) obj.TechnicalNameId = tObj.id;
        }
        if (typeof obj.MachineDia === "undefined" || obj.MachineDia == null) {
            obj.MachineDia = 0;
        }
        if (typeof obj.MachineGauge === "undefined" || obj.MachineGauge == null) {
            obj.MachineGauge = 0;
        }
        return obj;
    }

    function checkTotalDays() {
        var fabrics = $tblChildEl.getCurrentViewRecords();
        var collars = $tblChildCollarIdEl.getCurrentViewRecords();
        var cuffs = $tblChildCuffIdEl.getCurrentViewRecords();

        if (fabrics && fabrics.length > 0) {
            for (var iFabric = 0; iFabric < fabrics.length; iFabric++) {
                var fabric = fabrics[iFabric];
                var totalDays = fabric.TotalDays;
                var criteriaIDs = fabric.CriteriaIDs;
            }
        }
    }

    function checkAndGetValueNumber(value) {
        if (isYarnBookingAcknowledgeMenu()) return 0;
        value = getDefaultValueWhenInvalidN_Float(value);
        return value;
    }
    function checkAndGetValueText(value) {
        value = getDefaultValueWhenInvalidS(value);
        return value;
    }

    function save(result = "", isCheck, isRejectByKnittingHead, isRejectByKnittingInput, isReviseLabdip, isReviseBBKI, btnId) {
        var acknowledgeList = [];
        var data = formElToJson($formEl);
        data.GridStatus = status;
        data.BtnId = btnId;
        data.AllCollarSizeList = masterData.AllCollarSizeList;
        data.AllCuffSizeList = masterData.AllCuffSizeList;
        data.IsIncreaseRevisionNo = masterData.IsIncreaseRevisionNo;
        data.PMCFinalApproveCount = masterData.PMCFinalApproveCount;



        if (!isReviseBBKI) {
            if (result) {
                data.UnAcknowledgeReason = result;
                data.IsUnAcknowledge = true;
            } else {
                data.IsUnAcknowledge = false;
            }
        }
        else {
            if (result) {
                data.RivisionReason = result;
                data.IsUnAcknowledge = false;
            }
        }


        var BulkBookingAckDraft = false;
        if (menuType == _paramType.BulkBookingAck && _saveType == "SaveAsDraft") {
            BulkBookingAckDraft = true;
        }

        //acknowledgeList
        if (masterData.HasFabric && (result == "" || isReviseBBKI)) {
            var fabrics = $tblChildEl.getCurrentViewRecords();
            for (var i = 0; i < fabrics.length; i++) {
                if (_isBDS != 3) {
                    fabrics[i] = checkMachineTypeAndTechName(fabrics[i]);
                    if (!_isLabDipAck) {
                        if (!fabrics[i].MachineTypeId) {
                            toastr.warning("Please enter machine type for each fabric!");
                            return;
                        }
                        if (!fabrics[i].TechnicalNameId) {
                            toastr.warning("Please enter technical name for each fabric!");
                            return;
                        }
                        if (_isBDS == 1 && _isLabDipAck_RnD) {
                            if (!fabrics[i].TotalDays) {
                                toastr.warning("Please enter total days for each fabric!");
                                return;
                            }
                        }
                        if (_isBDS == 1 && !isLabdipMenu()) {
                            if (!fabrics[i].CriteriaIDs) {
                                toastr.warning("Please select criteria properly for each fabric!");
                                return;
                            }
                        }
                    }
                    if (_isBDS == 2) {
                        fabrics[i] = checkPropValues(fabrics[i]);
                        if (fabrics[i].BrandID == null || fabrics[i].BrandID == 0) {
                            var bObj = masterData.KnittingMachines.find(x => x.text == fabrics[i].Brand);
                            if (bObj) fabrics[i].BrandID = bObj.id;
                        }
                    }
                }

                //for child distribution
                if (masterData.FBookingAcknowledgeChildDistribution != null) {
                    var childDistData = masterData.FBookingAcknowledgeChildDistribution.filter(el => el.ConsumptionID == fabrics[i].ConsumptionID);
                    for (var j = 0; j < childDistData.length; j++) {
                        fabrics[i].ChildsDistribution.push(childDistData[j]);
                    }
                }

                //end for child distribution
                acknowledgeList.push(fabrics[i]);
            }
        }

        if (masterData.HasCollar && (result == "" || isReviseBBKI)) {
            var collars = $tblChildCollarIdEl.getCurrentViewRecords();
            for (var i = 0; i < collars.length; i++) {
                if (_isBDS != 3) {
                    collars[i] = checkMachineTypeAndTechName(collars[i]);
                    if (!_isLabDipAck && !_isLabDipAck_RnD && !isBulkBookingKnittingInfoMenu() && !isAdditionBulkBooking()) {
                        if (!collars[i].MachineTypeId) {
                            toastr.warning("Please enter machine type for each collar!");
                            return;
                        }
                        if (!collars[i].TechnicalNameId) {
                            toastr.warning("Please enter technical name for each collar!");
                            return;
                        }
                        if (_isBDS == 1 && !_isLabDipAck) {
                            if (!collars[i].CriteriaIDs) {
                                toastr.warning("Please select criteria properly for each collar!");
                                return;
                            }
                        }
                    }
                    if (_isBDS == 2) {
                        collars[i] = checkPropValues(collars[i]);
                        if (collars[i].BrandID == null || collars[i].BrandID == 0) {
                            var bObj = masterData.KnittingMachines.find(x => x.text == collars[i].Brand);
                            if (bObj) collars[i].BrandID = bObj.id;
                        }
                        if (!collars[i].BrandID) {
                            collars[i].BrandID = getDefaultValueWhenInvalidN(collars[i].BrandID);
                            //toastr.warning("Select brand for each collar!");
                            //return;
                        }
                    }
                }
                //for child distribution
                if (masterData.FBookingAcknowledgeChildDistribution != null) {
                    var childDistData = masterData.FBookingAcknowledgeChildDistribution.filter(el => el.ConsumptionID == collars[i].ConsumptionID);
                    for (var j = 0; j < childDistData.length; j++) {
                        collars[i].ChildsDistribution.push(childDistData[j]);
                    }
                }
                //end for child distribution
                acknowledgeList.push(collars[i]);
            }
        }
        if (masterData.HasCuff && (result == "" || isReviseBBKI)) {
            var cuffs = $tblChildCuffIdEl.getCurrentViewRecords();
            for (var i = 0; i < cuffs.length; i++) {
                if (_isBDS != 3) {
                    cuffs[i] = checkMachineTypeAndTechName(cuffs[i]);
                    if (!_isLabDipAck && !_isLabDipAck_RnD && !isBulkBookingKnittingInfoMenu() && !isAdditionBulkBooking()) {
                        if (!cuffs[i].MachineTypeId) {
                            toastr.warning("Please enter machine type for each cuff!");
                            return;
                        }
                        if (!cuffs[i].TechnicalNameId) {
                            toastr.warning("Please enter technical name for each cuff!");
                            return;
                        }
                        if (_isBDS == 1 && !_isLabDipAck) {
                            if (!cuffs[i].CriteriaIDs) {
                                toastr.warning("Please select criteria properly for each cuff!");
                                break;
                            }
                        }
                    }
                    if (_isBDS == 2) {
                        cuffs[i] = checkPropValues(cuffs[i]);
                        if (cuffs[i].BrandID == null || cuffs[i].BrandID == 0) {
                            var bObj = masterData.KnittingMachines.find(x => x.text == cuffs[i].Brand);
                            if (bObj) cuffs[i].BrandID = bObj.id;
                        }
                        if (!cuffs[i].BrandID) {
                            cuffs[i].BrandID = getDefaultValueWhenInvalidN(cuffs[i].BrandID);
                            //toastr.warning("Select brand for each cuff!");
                            //return;
                        }
                    }
                }
                //for child distribution
                if (masterData.FBookingAcknowledgeChildDistribution != null) {
                    var childDistData = masterData.FBookingAcknowledgeChildDistribution.filter(el => el.ConsumptionID == cuffs[i].ConsumptionID);
                    for (var j = 0; j < childDistData.length; j++) {
                        cuffs[i].ChildsDistribution.push(childDistData[j]);
                    }
                }
                //end for child distribution
                acknowledgeList.push(cuffs[i]);
            }
        }
        data.WithoutOB = masterData.WithoutOB;
        if (_isBDS == 2) {
            data.FBAckID = CheckNull(data.FBAckID);
            data.BookingQty = CheckNull(data.BookingQty);
            data.BookingBy = CheckNull(data.BookingBy);
            data.PreRevisionNo = CheckNull(data.PreRevisionNo);
            acknowledgeList.map(x => {
                x.YarnSubBrandID = CheckNull(x.YarnSubBrandID);
                x.LabdipUpdateDate = null;
            });
        }
        data.FBookingChild = acknowledgeList;

        if (isAdditionBulkBooking() && data.FBookingChild.length == 0) {
            toastr.error("No item found for save.");
            return false;
        }

        if (isLabdipMenu()) {
            data.FBookingChild = acknowledgeList.filter(x => x.IsFabricReq == true);
            if (typeof data.IsUnAcknowledge === "undefined" || data.IsUnAcknowledge == null) {
                data.IsUnAcknowledge = true;
            }
            if (data.FBookingChild.length == 0 && !data.IsUnAcknowledge) {
                toastr.error("No fabric required item found for acknowledge");
                return false;
            }
            for (var i = 0; i < data.FBookingChild.length; i++) {
                var child = data.FBookingChild[i];
                if (child.BookingQty == null || child.BookingQty == 0) {
                    toastr.warning("Give booking qty where faric is require");
                    return false;
                }
            }
        }
        if (isBulkBookingKnittingInfoMenu() && !isAdditionBulkBooking() && !BulkBookingAckDraft) {
            if (!isValidYarnItems(data.FBookingChild)) return false;
        }

        data.grpConceptNo = $formEl.find('#GroupConceptNo').val();
        data.IsBDS = _isBDS;
        data.IsRevised = masterData.PreRevisionNo > 0 ? true : false;
        if (menuType != _paramType.BulkBookingAck && status == statusConstants.DRAFT) {
            data.IsYarnRevision = masterData.PreRevisionNo > 0 ? true : false;
        }
        else if (menuType == _paramType.BulkBookingAck && status == statusConstants.DRAFT) {
            data.IsYarnRevision = masterData.PMCFinalApproveCount > 0 ? true : false;
        }
        data.PreRevisionNo = masterData.PreRevisionNo;
        data.PageName = isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking() ? "BulkBookingKnittingInfo" : pageName;

        if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
            data.IsBulkBooking = true;
        }
        else {
            data.IsBulkBooking = false;
        }
        if (result.length == 0) {
            if (menuType == _paramType.BulkBookingAck && (status == statusConstants.PENDING || status == statusConstants.DRAFT || status == statusConstants.INTERNAL_REJECTION)) {
                if (typeof data.CollarSizeID === "undefined" || data.CollarSizeID == null) data.CollarSizeID = "";
                if (typeof data.CollarWeightInGm === "undefined" || data.CollarWeightInGm == null || data.CollarWeightInGm == "") data.CollarWeightInGm = 0;

                if (masterData.HasCollar && (data.CollarSizeID.length == 0 || data.CollarWeightInGm == 0)) {
                    toastr.error("Select collar size & give consumption (gm)");
                    return;
                }

                if (typeof data.CuffSizeID === "undefined" || data.CuffSizeID == null) data.CuffSizeID = "";
                if (typeof data.CuffWeightInGm === "undefined" || data.CuffWeightInGm == null || data.CuffWeightInGm == "") data.CuffWeightInGm = 0;

                if (masterData.HasCuff && (data.CuffSizeID.length == 0 || data.CuffWeightInGm == 0)) {
                    toastr.error("Select cuff size & give consumption (gm)");
                    return;
                }

                //var list = data.FBookingChild.filter(x => x.SubGroupId == 1 && x.BookingQty == 0);
                //if (list.length > 0) {
                //    toastr.error("Fabric booking qty missing.");
                //    return;
                //}
                var list = data.FBookingChild.filter(x => x.SubGroupId == 11 && x.BookingQtyKG == 0);
                if (list.length > 0) {
                    toastr.error("Give Booking qty (KG) in collar.");
                    return;
                }
                list = data.FBookingChild.filter(x => x.SubGroupId == 12 && x.BookingQtyKG == 0);
                if (list.length > 0) {
                    toastr.error("Give Booking qty (KG) in cuff.");
                    return;
                }
            }
        }

        data.ParamTypeId = menuType;
        var saveURL = "/api/bds-acknowledge/save";
        var hasError = false;
        if (isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()) {
            if (_saveType == "SaveAsDraft" || _saveType == "Save" || _saveType == "UnAcknowledge") {
                data.OrderQtyKG = 0;
                data.IsSample = masterData.IsSample;
                if (_saveType == "SaveAsDraft" || _saveType == "UnAcknowledge") {
                    saveURL = "/api/bds-acknowledge/bulk/save";
                    if (isCheck || isRejectByKnittingHead) {
                        saveURL = "/api/bds-acknowledge/bulk/checkKnittingHead";
                    }
                }

                if (_saveType == "Save") {
                    if (masterData.IsYarnRevision) {
                        saveURL = "/api/bds-acknowledge/bulk/saveWithFreeConceptWithRevision";
                    } else {
                        saveURL = "/api/bds-acknowledge/bulk/saveWithFreeConcept";
                    }

                }

                if (menuType == _paramType.AdditionalYarnBooking) {
                    saveURL = "/api/bds-acknowledge/bulk/saveAddition";
                    if (status == statusConstants.ADDITIONAL_INTERNAL_REJECTION) {
                        data.IsUpdateAddition = true;
                        data.IsAdditionalRevise = true;
                    }
                }

                data.IsBDS = 2;
                data.IsReviseBBKI = isReviseBBKI;

                if (_saveType == "Save") {
                    data.IsKnittingComplete = true;
                } else {
                    data.IsKnittingComplete = false;
                }

                data.IsRevised = _isRevise;
                data.ParentYBookingNo = masterData.ParentYBookingNo;

                if (menuType == _paramType.AdditionalYarnBooking) {
                    data.IsAddition = true;
                }

                if (isRejectByKnittingInput) {
                    data.IsUnAcknowledge = isRejectByKnittingInput;
                    data.UnAcknowledgeReason = $(pageIdWithHash).find("#txtRejectReason").val();
                }

                for (var iChild = 0; iChild < data.FBookingChild.length; iChild++) {
                    var child = data.FBookingChild[iChild];

                    data.FBookingChild[iChild].SubGroupId = getDefaultValueWhenInvalidN(data.FBookingChild[iChild].SubGroupId);
                    data.FBookingChild[iChild].SubGroupID = getDefaultValueWhenInvalidN(data.FBookingChild[iChild].SubGroupID);
                    data.FBookingChild[iChild].MachineGauge = getDefaultValueWhenInvalidN(data.FBookingChild[iChild].MachineGauge);
                    data.FBookingChild[iChild].MachineDia = getDefaultValueWhenInvalidN(data.FBookingChild[iChild].MachineDia);
                    data.FBookingChild[iChild].BrandID = getDefaultValueWhenInvalidN(data.FBookingChild[iChild].BrandID);

                    child.SubGroupId = getDefaultValueWhenInvalidN(child.SubGroupId);
                    child.SubGroupID = getDefaultValueWhenInvalidN(child.SubGroupID);
                    child.MachineGauge = getDefaultValueWhenInvalidN(child.MachineGauge);
                    child.MachineDia = getDefaultValueWhenInvalidN(child.MachineDia);
                    child.BrandID = getDefaultValueWhenInvalidN(child.BrandID);

                    if (child.SubGroupId > 0 && child.SubGroupID == 0) {
                        child.SubGroupID = child.SubGroupId;
                        data.FBookingChild[iChild].SubGroupID = data.FBookingChild[iChild].SubGroupId;
                    }
                    if (child.SubGroupID > 0 && child.SubGroupId == 0) {
                        child.SubGroupId = child.SubGroupID;
                        data.FBookingChild[iChild].SubGroupId = data.FBookingChild[iChild].SubGroupID;
                    }


                    var childIndex = -1;
                    if (child.SubGroupId == 1) {
                        childIndex = masterData.Childs.findIndex(x => x.BookingChildID == child.BookingChildID);
                    }
                    else {
                        childIndex = masterData.Childs.findIndex(x => x.Construction == child.Construction && x.Composition == child.Composition && x.Color == child.Color);
                    }

                    if (childIndex > -1) {
                        child.PreFinishingProcessChilds = masterData.Childs[childIndex].PreFinishingProcessChilds;
                        child.PostFinishingProcessChilds = masterData.Childs[childIndex].PostFinishingProcessChilds;
                    }
                    //Validations

                    if (!BulkBookingAckDraft) {
                        if (child.MachineGauge == 0 && child.SubGroupId == 1) {
                            toastr.error("Select Gauge.");
                            hasError = true;
                            break;
                        }
                        if (child.MachineDia == 0 && child.SubGroupId == 1) {
                            toastr.error("Select Dia.");
                            hasError = true;
                            break;
                        }
                        if (child.ChildItems.length == 0) {
                            toastr.error("Give yarn information.");
                            hasError = true;
                            break;
                        }
                    }
                    if (hasError) break;
                    var totalDistribution = 0;

                    for (var iChildItem = 0; iChildItem < child.ChildItems.length; iChildItem++) {
                        var childItem = child.ChildItems[iChildItem];
                        totalDistribution = totalDistribution + parseFloat(childItem.Distribution);
                        totalDistribution = parseFloat(totalDistribution.toFixed(2));

                        childItem.StitchLength = checkAndGetValueNumber(childItem.StitchLength);
                        childItem.SpinnerId = checkAndGetValueNumber(childItem.SpinnerId);
                        childItem.YarnLotNo = checkAndGetValueText(childItem.YarnLotNo);

                        //Validations
                        if (BulkBookingAckDraft == false) {
                            if (checkAndGetValueNumber(childItem.Segment1ValueId) == 0) {
                                toastr.error("Select Composition.");
                                hasError = true;
                                break;
                            }

                            if (checkAndGetValueNumber(childItem.Segment2ValueId) == 0) {
                                toastr.error("Select Yarn Type.");
                                hasError = true;
                                break;
                            }
                            if (checkAndGetValueNumber(childItem.Segment3ValueId) == 0) {
                                toastr.error("Select Manufacturing Process.");
                                hasError = true;
                                break;
                            }
                            if (checkAndGetValueNumber(childItem.Segment4ValueId) == 0) {
                                toastr.error("Select Sub Process.");
                                hasError = true;
                                break;
                            }
                            if (checkAndGetValueNumber(childItem.Segment5ValueId) == 0) {
                                toastr.error("Select Quality Parameter.");
                                hasError = true;
                                break;
                            }
                            if (checkAndGetValueNumber(childItem.Segment6ValueId) == 0) {
                                toastr.error("Select Count.");
                                hasError = true;
                                break;
                            }
                            if (isYarnBookingAcknowledgeMenu()) {
                                childItem.StitchLength = 0;
                            }
                            else if (child.SubGroupId == 1) {
                                //if (checkAndGetValueNumber(childItem.StitchLength) == 0) {
                                //    toastr.error("Select Stitch Length.");
                                //    hasError = true;
                                //    break;
                                //}
                            } else {
                                childItem.StitchLength = 0;
                            }
                        }



                        if (menuType == _paramType.BulkBookingCheck) {
                            childItem.Allowance = getDefaultValueWhenInvalidN_Float(childItem.Allowance);
                            if (childItem.Allowance < 1 || childItem.Allowance > 35) {
                                toastr.error("Allowance must be within 1 to 35");
                                hasError = true;
                                break;
                            }
                        }

                        for (var indexSeg = 1; indexSeg <= 6; indexSeg++) {
                            var segIdProp = "Segment" + indexSeg + "ValueId";
                            var segDescProp = "Segment" + indexSeg + "ValueDesc";
                            var listName = "Segment" + indexSeg + "ValueList";

                            if (childItem[segIdProp] > 0 && (typeof childItem[segDescProp] === "undefined" || childItem[segDescProp] == "")) {
                                var objSeg = _yarnSegments[listName].find(s => s.id == childItem[segIdProp]);
                                if (objSeg) {
                                    childItem[segDescProp] = objSeg.text;
                                }
                            }
                        }
                    }
                    if (hasError) break;
                    if (BulkBookingAckDraft == false) {
                        if (parseFloat(totalDistribution) != parseFloat(100) && !isAdditionBulkBooking()) {
                            toastr.error("Yarn distribution must be 100%.");
                            hasError = true;
                            break;
                        }
                    }
                }
                if (!hasError) {
                    if (isCheck) {
                        data.BookingNo = masterData.BookingNo;
                        data.IsCheckByKnittingHead = isCheck;
                    } else if (isRejectByKnittingHead) {
                        data.BookingNo = masterData.BookingNo;
                        data.IsRejectByKnittingHead = isRejectByKnittingHead;
                        data.RejectReasonKnittingHead = $.trim($(pageIdWithHash).find("#txtRejectReason").val());
                    }
                }
            } else {
                data.IsKnittingComplete = true;
            }
        }


        if (isLabdipMenu()) {
            if (status == statusConstants.REVISE) {
                data.IsRevised = isReviseLabdip;
            }
            data.StatusText = _statusText + 'LabDip';
            data.IsLabdip = true;
        }
        else {
            data.IsLabdip = false;
        }

        for (var i = 0; i < data.FBookingChild.length; i++) {
            for (var j = 0; j < data.FBookingChild[i].ChildItems.length; j++) {
                if (data.FBookingChild[i].ChildItems[j].Segment1ValueId == null) {
                    data.FBookingChild[i].ChildItems[j].Segment1ValueId = 0;
                }
                if (data.FBookingChild[i].ChildItems[j].Segment2ValueId == null) {
                    data.FBookingChild[i].ChildItems[j].Segment2ValueId = 0;
                }
                if (data.FBookingChild[i].ChildItems[j].Segment3ValueId == null) {
                    data.FBookingChild[i].ChildItems[j].Segment3ValueId = 0;
                }
                if (data.FBookingChild[i].ChildItems[j].Segment4ValueId == null) {
                    data.FBookingChild[i].ChildItems[j].Segment4ValueId = 0;
                }
                if (data.FBookingChild[i].ChildItems[j].Segment5ValueId == null) {
                    data.FBookingChild[i].ChildItems[j].Segment5ValueId = 0;
                }
                if (data.FBookingChild[i].ChildItems[j].Segment6ValueId == null) {
                    data.FBookingChild[i].ChildItems[j].Segment6ValueId = 0;
                }
                data.FBookingChild[i].ChildItems[j].YarnPly = getDefaultValueWhenInvalidN(data.FBookingChild[i].ChildItems[j].YarnPly);
                data.FBookingChild[i].ChildItems[j].StitchLength = getDefaultValueWhenInvalidN(data.FBookingChild[i].ChildItems[j].StitchLength);
            }
        }
        data = setDateDefault(data);

        if (data.IsYarnRevision == true && _saveType == "Save") {
            var finder = new commonFinder({
                title: "Select Revision Reason",
                pageId: pageId,
                apiEndPoint: `/api/bds-acknowledge/get-yarn-revision-reason`,
                fields: "ReasonName",
                headerTexts: "Reason Name",
                isMultiselect: true,
                allowPaging: false,
                primaryKeyColumn: "ReasonID",
                onMultiselect: function (selectedRecords) {
                    if (selectedRecords.length > 0) {
                        selectedRecords.forEach(function (value) {

                            value.YBookingNo = data.YBookingNo;

                        });
                        data.RevisionReasonList = selectedRecords;
                    }
                    else {
                        data.RevisionReasonList = [];
                    }
                    savePost(hasError, saveURL, data);
                }
            });
            finder.showModal();
        }
        else {
            data.RevisionReasonList = [];
            savePost(hasError, saveURL, data);
        }

    }
    function savePost(hasError, saveURL, data) {

        if (!hasError) {
            axios.post(saveURL, data)
                .then(function (response) {
                    if (_isBDS == 2) {
                        if (menuType == _paramType.AdditionalYarnBooking && status == statusConstants.APPROVED2) {
                            showBootboxAlert("Yarn Booking No: <b>" + response.data.YBookingNo + "</b> saved successfully.");
                        }
                        if (_saveType == "Save") {

                            response.data.PageName = "BulkBookingKnittingInfo";
                            response.data.IsBulkBooking = true;
                            response.data.IsBDS = 2;
                            //saveBDSAck("/api/bds-acknowledge/save", response.data)
                            toastr.success("Saved successfully.");
                            initBulkAckList(0);
                            backToListBulk();
                        } else {
                            toastr.success("Saved successfully.");
                            initBulkAckList(0);
                            backToListBulk();
                        }
                    }
                    //else if (menuType == _paramType.BDSAcknowledge)
                    //{
                    //    showBootboxAlert("Booking No: <b>" + response.data.BookingNo + "</b> saved successfully.");
                    //    backToList();
                    //}
                    else {
                        toastr.success("Saved successfully.");
                        backToList();
                    }
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }

    }
    function setDateDefault(obj) {
        var dateProps = ["GarmentsShipmentDate", "YarnRequiredDate", "FabricStartDate", "FabricRequireDate", "FabricRequireDateEnd", "YarnBookingDateActual", "YarnBookingDateFR", "YarnRequiredDateFR", "YarnRequiredDateBOYB"];
        dateProps.map(d => {
            if (typeof obj[d] === "undefined" || obj[d] == null || obj[d] == "") {
                obj[d] = formatDateToDefault(new Date());
            }
        });
        obj.TNACalender = 0;
        obj.YarnRevisionNo = 0;
        return obj;
    }

    function saveBDSAck(saveURL, data) {
        axios.post(saveURL, data)
            .then(function () {
                toastr.success("Saved successfully.");
                initBulkAckList(0);
                backToListBulk();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function isInvalidSegment(segId) {
        if (typeof segId === "undefined" || segId == null || segId == 0) return true;
        return false;
    }
    function isValidYarnItems(fBookingChild) {

        var hasError = false;
        for (var i = 0; i < fBookingChild.length; i++) {
            var pChild = fBookingChild[i];
            for (var j = 0; j < pChild.ChildItems.length; j++) {
                var child = pChild.ChildItems[j];
                if (isInvalidSegment(child.Segment1ValueId)) {
                    toastr.error("Select composition");
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment2ValueId)) {
                    toastr.error("Select yarn type");
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment3ValueId)) {
                    toastr.error("Select manufacturing process");
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment4ValueId)) {
                    toastr.error("Select sub process");
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment5ValueId)) {
                    toastr.error("Select quality parameter");
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment6ValueId)) {
                    toastr.error("Select count");
                    hasError = true;
                    break;
                }
                if ((child.Segment5ValueDesc.toLowerCase() == "melange" || child.Segment5ValueDesc.toLowerCase() == "color melange") && (child.ShadeCode == null || child.ShadeCode == "")) {
                    toastr.error("Select shade code for color melange");
                    hasError = true;
                    break;
                }
            }
            if (hasError) break;
        }
        if (hasError) return false;
        return true;
    }

    function Receive() {
        var data = formElToJson($formEl);
        axios.post("/api/bds-acknowledge/Receive", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function Received() {
        var data = formElToJson($formEl);
        axios.post("/api/bds-acknowledge/Received", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function CheckNull(value) {
        if (value == null || value == "") return 0;
        return value;
    }

    function setTechnicalTime(data) {
        var obj = masterData.TechnicalNameList.find(x => x.additionalValue == data.MachineTypeId
            && x.id == data.TechnicalNameId);
        if (obj != null) return obj.desc;
        return 0;
    }

    function setCalculatedValues(dataList) {
        if (dataList != null && dataList.length > 0 && (status == statusConstants.PENDING || status == statusConstants.REJECT)) {
            dataList.map(x => {
                x.TechnicalTime = setTechnicalTime(x);
                x = setArgDataValues(x);
            });
        }
        return dataList;
    }

    function cancelSave(result = "") {
        var acknowledgeList = [];
        var data = formElToJson($formEl);
        if (result) {
            data["UnAcknowledgeReason"] = result;
            data["IsUnAcknowledge"] = true;
        }

        if (masterData.HasFabric && result == "") {
            var fabrics = $tblChildEl.getCurrentViewRecords();
            for (var i = 0; i < fabrics.length; i++) {
                if (_isBDS != 3) {
                    fabrics[i] = checkMachineTypeAndTechName(fabrics[i]);
                    if (!_isLabDipAck && !_isLabDipAck_RnD) {
                        if (!fabrics[i].MachineTypeId) {
                            toastr.warning("Please enter machine type for each fabric!");
                            return;
                        }
                        if (!fabrics[i].TechnicalNameId) {
                            toastr.warning("Please enter technical name for each fabric!");
                            return;
                        }
                        if (_isBDS == 1 && !isLabdipMenu()) {
                            if (!fabrics[i].CriteriaIDs) {
                                toastr.warning("Please select criteria properly for each fabric!");
                                return;
                            }
                        }
                    }
                    if (_isBDS == 2) {
                        fabrics[i] = checkPropValues(fabrics[i]);
                        if (fabrics[i].BrandID == null || fabrics[i].BrandID == 0) {
                            var bObj = masterData.KnittingMachines.find(x => x.text == fabrics[i].Brand);
                            if (bObj) fabrics[i].BrandID = bObj.id;
                        }
                        if (!fabrics[i].BrandID) {
                            fabrics[i].BrandID = getDefaultValueWhenInvalidN(fabrics[i].BrandID);
                            //toastr.warning("Select brand for each fabric!");
                            //return;
                        }
                    }
                }
                acknowledgeList.push(fabrics[i]);
            }
        }
        if (masterData.HasCollar && result == "") {
            var collars = $tblChildCollarIdEl.getCurrentViewRecords();
            for (var i = 0; i < collars.length; i++) {
                if (_isBDS != 3) {
                    collars[i] = checkMachineTypeAndTechName(collars[i]);
                    if (!_isLabDipAck && !_isLabDipAck_RnD && !isBulkBookingKnittingInfoMenu() && !isAdditionBulkBooking()) {
                        if (!collars[i].MachineTypeId) {
                            toastr.warning("Please enter machine type for each collar!");
                            return;
                        }
                        if (!collars[i].TechnicalNameId) {
                            toastr.warning("Please enter technical name for each collar!");
                            return;
                        }
                        if (_isBDS == 1 && !_isLabDipAck) {
                            if (!collars[i].CriteriaIDs) {
                                toastr.warning("Please select criteria properly for each collar!");
                                return;
                            }
                        }
                    }
                    if (_isBDS == 2) {
                        collars[i] = checkPropValues(collars[i]);
                        if (collars[i].BrandID == null || collars[i].BrandID == 0) {
                            var bObj = masterData.KnittingMachines.find(x => x.text == collars[i].Brand);
                            if (bObj) collars[i].BrandID = bObj.id;
                        }
                        if (!collars[i].BrandID) {
                            collars[i].BrandID = getDefaultValueWhenInvalidN(collars[i].BrandID);
                            //toastr.warning("Select brand for each fabric!");
                            //return;
                        }
                    }
                }
                acknowledgeList.push(collars[i]);
            }
        }
        if (masterData.HasCuff && result == "") {
            var cuffs = $tblChildCuffIdEl.getCurrentViewRecords();
            for (var i = 0; i < cuffs.length; i++) {
                if (_isBDS != 3) {
                    cuffs[i] = checkMachineTypeAndTechName(cuffs[i]);
                    if (!_isLabDipAck && !_isLabDipAck_RnD && !isBulkBookingKnittingInfoMenu() && !isAdditionBulkBooking()) {
                        if (!cuffs[i].MachineTypeId) {
                            toastr.warning("Please enter machine type for each cuff!");
                            return;
                        }
                        if (!cuffs[i].TechnicalNameId) {
                            toastr.warning("Please enter technical name for each cuff!");
                            return;
                        }
                        if (_isBDS == 1 && !_isLabDipAck) {
                            if (!cuffs[i].CriteriaIDs) {
                                toastr.warning("Please select criteria properly for each cuff!");
                                isValid = false;
                                break;
                            }
                        }
                    }
                    if (_isBDS == 2) {
                        cuffs[i] = checkPropValues(cuffs[i]);
                        if (cuffs[i].BrandID == null || cuffs[i].BrandID == 0) {
                            var bObj = masterData.KnittingMachines.find(x => x.text == cuffs[i].Brand);
                            if (bObj) cuffs[i].BrandID = bObj.id;
                        }
                        if (!cuffs[i].BrandID) {
                            cuffs[i].BrandID = getDefaultValueWhenInvalidN(cuffs[i].BrandID);
                            //toastr.warning("Select brand for each cuff!");
                            //return;
                        }
                    }
                }
                acknowledgeList.push(cuffs[i]);
            }
        }
        data.WithoutOB = masterData.WithoutOB;
        if (_isBDS == 2) {
            data.FBAckID = CheckNull(data.FBAckID);
            data.BookingQty = CheckNull(data.BookingQty);
            data.BookingBy = CheckNull(data.BookingBy);
            data.PreRevisionNo = CheckNull(data.PreRevisionNo);
            acknowledgeList.map(x => {
                x.YarnSubBrandID = CheckNull(x.YarnSubBrandID),
                    x.LabdipUpdateDate = null;
            });
        }

        data["FBookingChild"] = acknowledgeList;
        data.grpConceptNo = $formEl.find('#GroupConceptNo').val();
        data.IsBDS = _isBDS;

        axios.post("/api/bds-acknowledge/cancel-save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                if (_isBDS == 2) {
                    initBulkAckList(0);
                    backToListBulk();
                }
                else backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    /*Don't Delete
    function checkByKnittingHead(isReject) {
        var data = {
            FBAckID: masterData.FBAckID,
            BookingNo: masterData.BookingNo,
            IsRejectByKnittingHead: isReject,
            RejectReasonKnittingHead: $.trim($(pageIdWithHash).find("#txtRejectReason").val())
        }
        var saveURL = "/api/bds-acknowledge/bulk/checkKnittingHead";
        axios.post(saveURL, data)
            .then(function () {
                if (isReject) toastr.success("Rejected successfully.");
                else toastr.success("Checked successfully.");
                backToListBulk();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    */

    function approveRejectOperationBBKI(isReject, isInternalRevise, isYarnReviseFromFinalApproveList) {

        var isAllowance = menuType == _paramType.BulkBookingYarnAllowance;
        var isUtilizationPropose = menuType == _paramType.BulkBookingUtilizationProposal;
        var isUtilizationConfirmation = menuType == _paramType.BulkBookingUtilizationConfirmation;
        //var isKnittingHeadApprove = menuType == _paramType.BulkBookingCheck;
        //var isProductionHeadApprove = menuType == _paramType.BulkBookingApprove;
        var isKnittingHeadApprove = menuType == _paramType.BulkBookingApprove;
        var isFinalApprovePMC = menuType == _paramType.BulkBookingFinalApprove;
        var isYarnRevise = isYarnReviseFromFinalApproveList;

        var hasError = false;
        var data = {
            FBAckID: masterData.FBAckID,
            BookingNo: masterData.BookingNo,
            CollarSizeID: masterData.CollarSizeID,
            CollarWeightInGm: masterData.CollarWeightInGm,
            CuffSizeID: masterData.CuffSizeID,
            CuffWeightInGm: masterData.CuffWeightInGm,
            AllCollarSizeList: masterData.AllCollarSizeList,
            AllCuffSizeList: masterData.AllCuffSizeList,
            IsYarnRevision: masterData.IsYarnRevision
        }
        if (masterData.IsIncreaseRevisionNo == true) {
            data.CollarSizeID = $(pageIdWithHash).find("#CollarSizeID").val();
            data.CollarWeightInGm = $(pageIdWithHash).find("#CollarWeightInGm").val();
            data.CuffSizeID = $(pageIdWithHash).find("#CuffSizeID").val();
            data.CuffWeightInGm = $(pageIdWithHash).find("#CuffWeightInGm").val();
        }
        switch (true) {
            //case isKnittingHeadApprove:

            //    data.IsRejectByKnittingHead = isReject;
            //    data.RejectReasonKnittingHead = isReject ? $.trim($(pageIdWithHash).find("#txtRejectReason").val()) : "";
            //    data.YChilds = [];
            //    data.FBookingChild = [];
            //    data.URL = "/api/bds-acknowledge/bulk/checkKnittingHead";

            //    break;
            case isKnittingHeadApprove:

                data.IsRejectByProdHead = isReject;
                data.RejectReasonProdHead = isReject ? $.trim($(pageIdWithHash).find("#txtRejectReason").val()) : "";
                data.YChilds = [];
                data.FBookingChild = [];
                data.ChildItems = [];
                data.URL = "/api/bds-acknowledge/bulk/approveProdHead";

                break;
            case isAllowance:

                data.IsRejectByAllowance = isReject;
                data.RejectReasonAllowance = isReject ? $.trim($(pageIdWithHash).find("#txtRejectReason").val()) : "";
                data.YChilds = [];
                data.FBookingChild = [];
                data.ChildItems = [];
                data.URL = "/api/bds-acknowledge/bulk/approveAllowance";

                break;
            case isUtilizationPropose:

                data.YChilds = [];
                data.FBookingChild = [];
                data.ChildItems = [];
                data.URL = "/api/bds-acknowledge/bulk/utilizationProposalSend";

                break;
            case isUtilizationConfirmation:

                data.YChilds = [];
                data.FBookingChild = [];
                data.ChildItems = [];
                data.URL = "/api/bds-acknowledge/bulk/utilizationProposalConfirmed";

                break;
            case isFinalApprovePMC:
                data.IsRejectByPMC = isReject;
                data.RejectReasonPMC = isReject ? $.trim($(pageIdWithHash).find("#txtRejectReason").val()) : "";
                data.ChildItems = [];
                data.IsInternalRevise = isInternalRevise;
                data.InternalReviseReason = isInternalRevise ? $.trim($(pageIdWithHash).find("#txtRejectReason").val()) : "";
                data.URL = "/api/bds-acknowledge/bulk/approvePMC";

                break;
            case isYarnRevise:
                data.IsRevisedYarn = true;
                data.YChilds = [];
                data.FBookingChild = [];
                data.ChildItems = [];
                data.URL = "/api/bds-acknowledge/bulk/YarnBookingRevision";
                data.RevisionReasonList = masterData.RevisionReasonList;
                break;
            default:
            // code block
        }

        if (isKnittingHeadApprove
            || isYarnRevise
            || isAllowance
            || isUtilizationPropose
            || isUtilizationConfirmation) {

            hasError = false;
            if (masterData.HasFabric) {
                var fabrics = $tblChildEl.getCurrentViewRecords();
                for (var iChild = 0; iChild < fabrics.length; iChild++) {
                    fabrics[iChild].LengthInch = parseInt(fabrics[iChild].LengthInch);
                    fabrics[iChild].LabdipUpdateDate = new Date();
                    fabrics[iChild].ToItemMasterId = 0;

                    fabrics[iChild].BrandID = getDefaultValueWhenInvalidN(fabrics[iChild].BrandID);

                    var totalDistribution = 0;
                    var childItems = fabrics[iChild].ChildItems;
                    for (var iChildItem = 0; iChildItem < childItems.length; iChildItem++) {
                        var childItem = childItems[iChildItem];

                        hasError = isValidYarnItemSingle(childItem);
                        if (hasError) break;

                        if ((typeof fabrics[iChild].YBookingID == "undefined" || fabrics[iChild].YBookingID == 0) && childItem.YBookingID > 0) {
                            fabrics[iChild].YBookingID = childItem.YBookingID;
                        }
                        if ((typeof fabrics[iChild].YBChildID == "undefined" || fabrics[iChild].YBChildID == 0) && childItem.YBChildID > 0) {
                            fabrics[iChild].YBChildID = childItem.YBChildID;
                        }

                        totalDistribution = totalDistribution + parseFloat(childItem.Distribution);
                        totalDistribution = parseFloat(totalDistribution.toFixed(2));

                        childItem.Allowance = getDefaultValueWhenInvalidN_Float(childItem.Allowance);
                        if (childItem.Allowance < 1 || childItem.Allowance > 35) {
                            toastr.error("Allowance must be within 1 to 35");
                            hasError = true;
                            break;
                        }
                        if (childItem.YD && menuType == _paramType.BulkBookingYarnAllowance && (childItem.YDAllowance < 1 || childItem.YDAllowance > 35)) {
                            toastr.error("Go for YD must have YD Allowance between 1 to 35");
                            hasError = true;
                            break;
                        }

                        if (childItem.SpinnerId == null) {
                            childItem.SpinnerId = 0;
                        }
                    }
                    if (hasError) break;

                    if (parseFloat(totalDistribution) != parseFloat(100) && !isAdditionBulkBooking()) {
                        toastr.error("Yarn distribution must be 100%.");
                        hasError = true;
                        break;
                    }

                    data.YChilds.push(fabrics[iChild]);
                }
            }
            if (hasError) return false;
            hasError = false;
            if (masterData.HasCollar) {

                var collars = $tblChildCollarIdEl.getCurrentViewRecords();
                for (var iChild = 0; iChild < collars.length; iChild++) {
                    collars[iChild].LengthInch = parseInt(collars[iChild].LengthInch);
                    collars[iChild].LabdipUpdateDate = new Date();
                    collars[iChild].ToItemMasterId = 0;

                    var totalDistribution = 0;
                    var childItems = collars[iChild].ChildItems;
                    for (var iChildItem = 0; iChildItem < childItems.length; iChildItem++) {
                        var childItem = childItems[iChildItem];

                        hasError = isValidYarnItemSingle(childItem);
                        if (hasError) break;

                        if ((typeof collars[iChild].YBookingID == "undefined" || collars[iChild].YBookingID == 0) && childItem.YBookingID > 0) {
                            collars[iChild].YBookingID = childItem.YBookingID;
                        }
                        if ((typeof collars[iChild].YBChildID == "undefined" || collars[iChild].YBChildID == 0) && childItem.YBChildID > 0) {
                            collars[iChild].YBChildID = childItem.YBChildID;
                        }

                        totalDistribution = totalDistribution + parseFloat(childItem.Distribution);
                        totalDistribution = parseFloat(totalDistribution.toFixed(2));

                        childItem.Allowance = getDefaultValueWhenInvalidN_Float(childItem.Allowance);
                        if (childItem.Allowance < 1 || childItem.Allowance > 35) {
                            toastr.error("Allowance must be within 1 to 35");
                            hasError = true;
                            break;
                        }
                        if (childItem.YD && menuType == _paramType.BulkBookingYarnAllowance && (childItem.YDAllowance < 1 || childItem.YDAllowance > 35)) {
                            toastr.error("Go for YD must have YD Allowance between 1 to 35");
                            hasError = true;
                            break;
                        }

                        if (childItem.SpinnerId == null) {
                            childItem.SpinnerId = 0;
                        }
                    }
                    if (hasError) break;

                    if (parseFloat(totalDistribution) != parseFloat(100) && !isAdditionBulkBooking()) {
                        toastr.error("Yarn distribution must be 100%.");
                        hasError = true;
                        break;
                    }
                    data.YChilds.push(collars[iChild]);
                }
            }
            if (hasError) return false;
            hasError = false;
            if (masterData.HasCuff) {
                var cuffs = $tblChildCuffIdEl.getCurrentViewRecords();
                for (var iChild = 0; iChild < cuffs.length; iChild++) {
                    cuffs[iChild].LengthInch = parseInt(cuffs[iChild].LengthInch);
                    cuffs[iChild].LabdipUpdateDate = new Date();
                    cuffs[iChild].ToItemMasterId = 0;

                    var totalDistribution = 0;
                    var childItems = cuffs[iChild].ChildItems;
                    for (var iChildItem = 0; iChildItem < childItems.length; iChildItem++) {
                        var childItem = childItems[iChildItem];

                        hasError = isValidYarnItemSingle(childItem);
                        if (hasError) break;

                        if ((typeof cuffs[iChild].YBookingID == "undefined" || cuffs[iChild].YBookingID == 0) && childItem.YBookingID > 0) {
                            cuffs[iChild].YBookingID = childItem.YBookingID;
                        }
                        if ((typeof cuffs[iChild].YBChildID == "undefined" || cuffs[iChild].YBChildID == 0) && childItem.YBChildID > 0) {
                            cuffs[iChild].YBChildID = childItem.YBChildID;
                        }

                        var tDis = parseFloat(childItem.Distribution).toFixed(2);
                        totalDistribution += parseFloat(parseFloat(tDis).toFixed(2));
                        childItem.Allowance = getDefaultValueWhenInvalidN_Float(childItem.Allowance);
                        if (childItem.Allowance < 1 || childItem.Allowance > 35) {
                            toastr.error("Allowance must be within 1 to 35");
                            hasError = true;
                            break;
                        }
                        if (childItem.YD && menuType == _paramType.BulkBookingYarnAllowance && (childItem.YDAllowance < 1 || childItem.YDAllowance > 35)) {
                            toastr.error("Go for YD must have YD Allowance between 1 to 35");
                            hasError = true;
                            break;
                        }

                        if (childItem.SpinnerId == null) {
                            childItem.SpinnerId = 0;
                        }
                    }
                    if (hasError) break;

                    if (parseFloat(totalDistribution) != parseFloat(100) && !isAdditionBulkBooking()) {
                        toastr.error("Yarn distribution must be 100%.");
                        hasError = true;
                        break;
                    }
                    data.YChilds.push(cuffs[iChild]);
                }
            }
        }
        if (hasError) return false;

        var acknowledgeList = [];

        if (isKnittingHeadApprove) {
            var result = "";
            if (masterData.HasFabric && result == "") {
                var fabrics = $tblChildEl.getCurrentViewRecords();
                for (var i = 0; i < fabrics.length; i++) {
                    fabrics[i].SubGroupId = 1;
                    fabrics[i].SubGroupID = 1;

                    if (_isBDS != 3) {
                        fabrics[i] = checkMachineTypeAndTechName(fabrics[i]);
                        if (!_isLabDipAck) {
                            if (!fabrics[i].MachineTypeId) {
                                toastr.warning("Please enter machine type for each fabric!");
                                return;
                            }
                            if (!fabrics[i].TechnicalNameId) {
                                toastr.warning("Please enter technical name for each fabric!");
                                return;
                            }
                        }
                        if (_isBDS == 2) {
                            fabrics[i] = checkPropValues(fabrics[i]);
                            if (fabrics[i].BrandID == null || fabrics[i].BrandID == 0) {
                                var bObj = masterData.KnittingMachines.find(x => x.text == fabrics[i].Brand);
                                if (bObj) fabrics[i].BrandID = bObj.id;
                            }
                        }
                    }

                    //for child distribution
                    if (masterData.FBookingAcknowledgeChildDistribution != null) {
                        var childDistData = masterData.FBookingAcknowledgeChildDistribution.filter(el => el.ConsumptionID == fabrics[i].ConsumptionID);
                        for (var j = 0; j < childDistData.length; j++) {
                            fabrics[i].ChildsDistribution.push(childDistData[j]);
                        }
                    }
                    //end for child distribution
                    acknowledgeList.push(fabrics[i]);
                }
            }
            if (masterData.HasCollar && result == "") {
                var collars = $tblChildCollarIdEl.getCurrentViewRecords();
                for (var i = 0; i < collars.length; i++) {
                    collars[i].SubGroupId = 11;
                    collars[i].SubGroupID = 11;

                    if (_isBDS != 3) {
                        collars[i] = checkMachineTypeAndTechName(collars[i]);
                        if (!_isLabDipAck && !_isLabDipAck_RnD && !isBulkBookingKnittingInfoMenu() && !isAdditionBulkBooking()) {
                            if (!collars[i].MachineTypeId) {
                                toastr.warning("Please enter machine type for each collar!");
                                return;
                            }
                            if (!collars[i].TechnicalNameId) {
                                toastr.warning("Please enter technical name for each collar!");
                                return;
                            }
                            if (_isBDS == 1 && !_isLabDipAck) {
                                if (!collars[i].CriteriaIDs) {
                                    toastr.warning("Please select criteria properly for each collar!");
                                    return;
                                }
                            }
                        }
                        if (_isBDS == 2) {
                            collars[i] = checkPropValues(collars[i]);
                            if (collars[i].BrandID == null || collars[i].BrandID == 0) {
                                var bObj = masterData.KnittingMachines.find(x => x.text == collars[i].Brand);
                                if (bObj) collars[i].BrandID = bObj.id;
                            }
                            if (!collars[i].BrandID) {
                                collars[i].BrandID = getDefaultValueWhenInvalidN(collars[i].BrandID);
                                //toastr.warning("Select brand for each collars!");
                                //return;
                            }
                        }
                    }
                    //for child distribution
                    if (masterData.FBookingAcknowledgeChildDistribution != null) {
                        var childDistData = masterData.FBookingAcknowledgeChildDistribution.filter(el => el.ConsumptionID == collars[i].ConsumptionID);
                        for (var j = 0; j < childDistData.length; j++) {
                            collars[i].ChildsDistribution.push(childDistData[j]);
                        }
                    }
                    //end for child distribution
                    acknowledgeList.push(collars[i]);
                }
            }
            if (masterData.HasCuff && result == "") {
                var cuffs = $tblChildCuffIdEl.getCurrentViewRecords();
                for (var i = 0; i < cuffs.length; i++) {
                    cuffs[i].SubGroupId = 12;
                    cuffs[i].SubGroupID = 12;

                    if (_isBDS != 3) {
                        cuffs[i] = checkMachineTypeAndTechName(cuffs[i]);
                        if (!_isLabDipAck && !_isLabDipAck_RnD && !isBulkBookingKnittingInfoMenu() && !isAdditionBulkBooking()) {
                            if (!cuffs[i].MachineTypeId) {
                                toastr.warning("Please enter machine type for each cuff!");
                                return;
                            }
                            if (!cuffs[i].TechnicalNameId) {
                                toastr.warning("Please enter technical name for each cuff!");
                                return;
                            }
                            if (_isBDS == 1 && !_isLabDipAck) {
                                if (!cuffs[i].CriteriaIDs) {
                                    toastr.warning("Please select criteria properly for each cuff!");
                                    break;
                                }
                            }
                        }
                        if (_isBDS == 2) {
                            cuffs[i] = checkPropValues(cuffs[i]);
                            if (cuffs[i].BrandID == null || cuffs[i].BrandID == 0) {
                                var bObj = masterData.KnittingMachines.find(x => x.text == cuffs[i].Brand);
                                if (bObj) cuffs[i].BrandID = bObj.id;
                            }
                            //if (!cuffs[i].BrandID) {
                            //    toastr.warning("Select brand for each cuff!");
                            //    return;
                            //}
                        }
                    }
                    //for child distribution
                    if (masterData.FBookingAcknowledgeChildDistribution != null) {
                        var childDistData = masterData.FBookingAcknowledgeChildDistribution.filter(el => el.ConsumptionID == cuffs[i].ConsumptionID);
                        for (var j = 0; j < childDistData.length; j++) {
                            cuffs[i].ChildsDistribution.push(childDistData[j]);
                        }
                    }
                    //end for child distribution
                    acknowledgeList.push(cuffs[i]);
                }
            }

            data.FBookingChild = acknowledgeList;
        }

        //if (!isKnittingHeadApprove) {
        if (typeof data.FBookingChild === "undefined") data.FBookingChild = [];
        if (masterData.HasFabric) {
            var fabrics = $tblChildEl.getCurrentViewRecords();
            fabrics.map(x => {
                x = DeepClone(x);
                x.SubGroupId = 1;
                x.SubGroupID = 1;
                data.FBookingChild.push(x);
            });
            fabrics.map(x => {
                x.ChildItems.map(x => x.SubGroupId = 1);
                data.ChildItems.push(...x.ChildItems);
            });
        }
        if (masterData.HasCollar) {
            var collars = $tblChildCollarIdEl.getCurrentViewRecords();
            collars.map(x => {
                x = DeepClone(x);
                x.SubGroupId = 11;
                x.SubGroupID = 11;
                data.FBookingChild.push(x);
            });
            collars.map(x => {
                x.ChildItems.map(x => x.SubGroupId = 11);
                data.ChildItems.push(...x.ChildItems);
            });
        }
        if (masterData.HasCuff) {
            var cuffs = $tblChildCuffIdEl.getCurrentViewRecords();
            cuffs.map(x => {
                x = DeepClone(x);
                x.SubGroupId = 12;
                x.SubGroupID = 12;
                data.FBookingChild.push(x);
            });
            cuffs.map(x => {
                x.ChildItems.map(x => x.SubGroupId = 12);
                data.ChildItems.push(...x.ChildItems);
            });
        }
        //}

        var isCheckAllowanceRange = false;
        if (isFinalApprovePMC && !isInternalRevise) {
            isCheckAllowanceRange = true;
        } else if (isAllowance
            || isYarnRevise
            || isUtilizationPropose
            || isUtilizationConfirmation
            || isFinalApprovePMC) {
            isCheckAllowanceRange = true;

        }
        if (isCheckAllowanceRange) {
            hasError = false;

            for (var iChildItem = 0; iChildItem < data.ChildItems.length; iChildItem++) {
                var childItem = data.ChildItems[iChildItem];
                childItem.Allowance = getDefaultValueWhenInvalidN_Float(childItem.Allowance);
                if (childItem.Allowance < 1 || childItem.Allowance > 35) {

                    toastr.error("Allowance must be within 1 to 35");
                    hasError = true;
                    break;
                }
            }
        }

        if (hasError) return false;


        data.YBookingNo = masterData.YBookingNo;
        data.BuyerName = masterData.BuyerName;
        data.BuyerTeamName = masterData.BuyerTeamName;
        data.RevisionNo = masterData.RevisionNo;

        if (data.YChilds != null && typeof data.YChilds != "undefined") {
            data.YChilds.forEach(x => {
                x.ChildItems.forEach(y => {
                    if (y.SpinnerId == null) {
                        y.SpinnerId = 0;
                    }
                });
            });
        }
        axios.post(data.URL, data)
            .then(function () {
                if (isReject) toastr.success("Rejected successfully.");
                else toastr.success("Approved successfully.");

                initBulkAckList(0);
                backToListBulk();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    function initChildTableFP(data, isDestroy) {
        if ($tblChildElFP || isDestroy) {
            $tblChildElFP.destroy();
            $("#" + tblChildIdFP).html("");
        }
        ej.base.enableRipple(true);
        $tblChildElFP = new ej.grids.Grid({
            dataSource: data,
            allowRowDragAndDrop: true,
            selectionSettings: { type: 'Multiple' },
            editSettings: { allowAdding: true, allowDeleting: true, allowEditing: true },  //allowAdding: true, allowEditing: true,
            //commandClick: commandClickFP,
            columns: [
                {
                    headerText: '', width: 80, commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                    ]
                },
                { field: 'FPChildID', isPrimaryKey: true, visible: false },
                { field: 'ProcessID', visible: false },
                { field: 'ProcessTypeID', visible: false },
                { field: 'ProcessName', headerText: 'Process Name', width: 100, allowEditing: false },
                { field: 'ProcessType', headerText: 'Process Type', width: 100, allowEditing: false },
                { field: 'MachineName', headerText: 'Machine Name', width: 100, allowEditing: false },
                { field: 'MachineNo', headerText: 'Machine No', width: 80, allowEditing: false },
                {
                    headerText: '...', width: 50, commands: [
                        { buttonOption: { type: 'dmachine', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                    ]
                },
                { field: 'UnitName', headerText: 'Unit', width: 80, allowEditing: false },
                { field: 'BrandName', headerText: 'Brand', width: 80, allowEditing: false },
                { field: 'Param1Value', headerText: 'Param 1 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param2Value', headerText: 'Param 2 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param3Value', headerText: 'Param 3 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param4Value', headerText: 'Param 4 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param5Value', headerText: 'Param 5 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param6Value', headerText: 'Param 6 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param7Value', headerText: 'Param 7 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param8Value', headerText: 'Param 8 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param9Value', headerText: 'Param 9 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param10Value', headerText: 'Param 10 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param11Value', headerText: 'Param 11 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param12Value', headerText: 'Param 12 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param13Value', headerText: 'Param 13 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param14Value', headerText: 'Param 14 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param15Value', headerText: 'Param 15 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param16Value', headerText: 'Param 16 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param17Value', headerText: 'Param 17 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param18Value', headerText: 'Param 18 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param19Value', headerText: 'Param 19 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param20Value', headerText: 'Param 20 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Remarks', headerText: 'Remarks', width: 100, allowEditing: true }
            ],
            childGrid: {
                queryString: 'FPChildID',
                allowResizing: true,
                editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: [
                    {
                        headerText: 'Action', width: 60, visible: status == (statusConstants.PRE_PENDING || statusConstants.POST_PENDING), commands: [
                            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                        ]
                    },
                    { field: 'ItemMasterID', isPrimaryKey: true, visible: false },
                    { field: 'text', headerText: 'Item Name', textAlign: 'Center', allowEditing: false },
                    { field: 'Qty', headerText: 'Qty(gm/l)' }
                ],
                //load: loadFirstLevelChildGridFP
            }
        });
        $tblChildElFP.refreshColumns;
        $tblChildElFP.appendTo("#" + tblChildIdFP);
    }
    function initChildTableColorFP(data, isDestroy) {
        if ($tblColorChildElFP || isDestroy) {
            $tblColorChildElFP.destroy();
            $("#" + tblColorChildIdFP).html("");
        }
        ej.base.enableRipple(true);

        $tblColorChildElFP = new ej.grids.Grid({
            dataSource: data,
            allowRowDragAndDrop: true,
            selectionSettings: { type: 'Multiple' },
            editSettings: { allowAdding: true, allowDeleting: true, allowEditing: true },
            toolbar: [
                { text: 'Add Item', tooltipText: 'Add Item', prefixIcon: 'e-icons e-add', id: 'addItem' }
            ],
            columns: [
                {
                    headerText: '', width: 80, commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                    ]
                },
                { field: 'FPChildID', isPrimaryKey: true, visible: false },
                { field: 'ColorID', headerText: 'ColorID', width: 100, allowEditing: false, visible: false },
                { field: 'ProcessName', headerText: 'Process Name', width: 100, allowEditing: false },
                { field: 'ProcessType', headerText: 'Process Type', width: 100, allowEditing: false },
                { field: 'MachineName', headerText: 'Machine Name', width: 100, allowEditing: false },
                { field: 'MachineNo', headerText: 'Machine No', width: 100, allowEditing: false },
                {
                    headerText: '...', width: 30, commands: [
                        { buttonOption: { type: 'machine', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                    ]
                },
                { field: 'UnitName', headerText: 'Unit', width: 80, allowEditing: false },
                { field: 'BrandName', headerText: 'Brand', width: 80, allowEditing: false },
                { field: 'Param1Value', headerText: 'Param 1 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param2Value', headerText: 'Param 2 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param3Value', headerText: 'Param 3 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param4Value', headerText: 'Param 4 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param5Value', headerText: 'Param 5 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param6Value', headerText: 'Param 6 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param7Value', headerText: 'Param 7 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param8Value', headerText: 'Param 8 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param9Value', headerText: 'Param 9 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param10Value', headerText: 'Param 10 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param11Value', headerText: 'Param 11 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param12Value', headerText: 'Param 12 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param13Value', headerText: 'Param 13 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param14Value', headerText: 'Param 14 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param15Value', headerText: 'Param 15 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param16Value', headerText: 'Param 16 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param17Value', headerText: 'Param 17 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param18Value', headerText: 'Param 18 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param19Value', headerText: 'Param 19 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Param20Value', headerText: 'Param 20 Value', width: 100, allowEditing: false, visible: false },
                { field: 'Remarks', headerText: 'Remarks', width: 100, allowEditing: true }
            ],
            toolbarClick: function (args) {
                var dataS = args;
                var postProcessList = [];
                if (args.item.id === "addItem") {

                    var processType = 'Post Set';
                    var finder = new commonFinder({
                        title: "Select Process",
                        pageId: pageId,
                        apiEndPoint: `/api/finishing-process/get-finishing-process/${processType}`,
                        fields: "ProcessName,ProcessType,MachineName",
                        headerTexts: "Process Name, Process Type,Machine Name",
                        isMultiselect: true,
                        allowPaging: false,
                        primaryKeyColumn: "ProcessID",
                        onMultiselect: function (selectedRecords) {

                            var aaa = dataS;
                            var postProcessList = $tblColorChildElFP.getCurrentViewRecords();
                            for (var i = 0; i < selectedRecords.length; i++) {
                                var oPreProcess = {
                                    FPChildID: getMaxIdForArray(postProcessList, "FPChildID"),
                                    FPMasterID: 0,
                                    ProcessID: selectedRecords[i].ProcessID,
                                    ProcessTypeID: selectedRecords[i].ProcessTypeID,
                                    ProcessName: selectedRecords[i].ProcessName,
                                    ProcessType: selectedRecords[i].ProcessType,
                                    MachineName: selectedRecords[i].MachineName,
                                    FMCMasterID: selectedRecords[i].FMCMasterID,
                                    MachineNo: "",
                                    UnitName: "",
                                    BrandName: "",
                                    Remarks: "",
                                    ColorID: _fpBookingChildColorID,
                                    IsPreProcess: false,
                                    FMSID: null,
                                    Param1Value: null,
                                    Param2Value: null,
                                    Param3Value: null,
                                    Param4Value: null,
                                    Param5Value: null,
                                    Param6Value: null,
                                    Param7Value: null,
                                    Param8Value: null,
                                    Param9Value: null,
                                    Param10Value: null,
                                    Param11Value: null,
                                    Param12Value: null,
                                    Param13Value: null,
                                    Param14Value: null,
                                    Param15Value: null,
                                    Param16Value: null,
                                    Param17Value: null,
                                    Param18Value: null,
                                    Param19Value: null,
                                    Param20Value: null,
                                    PreFinishingProcessChildItems: []
                                }
                                var indexF = -1;
                                if (postProcessList.length > 0) {
                                    indexF = postProcessList.findIndex(y => y.ProcessName == oPreProcess.ProcessName && y.ProcessType == oPreProcess.ProcessType && y.MachineName == oPreProcess.MachineName);
                                }
                                if (indexF == -1) {
                                    postProcessList.push(oPreProcess);
                                }
                            }
                            initChildTableColorFP(postProcessList, true);
                        }
                    });
                    finder.showModal();
                }
            },
            actionBegin: function (args) {
                if (args.requestType === "add") {
                }
                else if (args.requestType === "delete") {
                }
            },
            actionComplete: function (args) {
                if (args.requestType === "add") {

                }
            }
        });
        $tblColorChildElFP.refreshColumns;
        $tblColorChildElFP.appendTo("#" + tblColorChildIdFP);
    }
    function hasValue(value) {
        if (typeof value === "undefined" || value == null || value == "") return false;
        return true;
    }

    function acknowledgeLabDip(result = "", isLabdipAcknowledge, isRevise, isReviseFromUnAckList) {
        if (_isLabDipAck) {
            var acknowledgeList = [];
            var data = formElToJson($formEl);
            if (result) {
                data.LabdipUnAcknowledgeReason = result;
            }
            if (masterData.HasFabric) {
                var fabrics = $tblChildEl.getCurrentViewRecords();
                for (var i = 0; i < fabrics.length; i++) {
                    fabrics[i] = checkMachineTypeAndTechName(fabrics[i]);
                    if (fabrics[i].IsFabricReq && fabrics[i].BookingQty == 0) {
                        toastr.warning("Give booking qty where faric is require");
                        return;
                    }
                    //if (!fabrics[i].MachineTypeId) {
                    //    toastr.warning("Please enter machine type for each fabric!");
                    //    return;
                    //}
                    //if (!fabrics[i].TechnicalNameId) {
                    //    toastr.warning("Please enter technical name for each fabric!");
                    //    return;
                    //}
                    acknowledgeList.push(fabrics[i]);
                }
            }
            if (masterData.HasCollar) {
                var collars = $tblChildCollarIdEl.getCurrentViewRecords();
                for (var i = 0; i < collars.length; i++) {
                    collars[i] = checkMachineTypeAndTechName(collars[i]);
                    if (collars[i].IsFabricReq && collars[i].BookingQty == 0) {
                        toastr.warning("Give booking qty where faric is require");
                        return;
                    }
                    //if (!collars[i].MachineTypeId) {
                    //    toastr.warning("Please enter machine type for each collar!");
                    //    return;
                    //}
                    //if (!collars[i].TechnicalNameId) {
                    //    toastr.warning("Please enter technical name for each collar!");
                    //    return;
                    //}
                    acknowledgeList.push(collars[i]);
                }
            }
            if (masterData.HasCuff) {
                var cuffs = $tblChildCuffIdEl.getCurrentViewRecords();
                for (var i = 0; i < cuffs.length; i++) {
                    cuffs[i] = checkMachineTypeAndTechName(cuffs[i]);
                    if (cuffs[i].IsFabricReq && cuffs[i].BookingQty == 0) {
                        toastr.warning("Give booking qty where faric is require");
                        return;
                    }
                    //if (!cuffs[i].MachineTypeId) {
                    //    toastr.warning("Please enter machine type for each cuff!");
                    //    return;
                    //}
                    //if (!cuffs[i].TechnicalNameId) {
                    //    toastr.warning("Please enter technical name for each cuff!");
                    //    return;
                    //}
                    acknowledgeList.push(cuffs[i]);
                }
            }
            data.WithoutOB = masterData.WithoutOB;
            data.FBookingChild = acknowledgeList.filter(x => x.IsFabricReq == true);
            if (data.FBookingChild.length == 0 && isLabdipAcknowledge) {
                toastr.error("No fabric required item found for acknowledge");
                return false;
            }
            data.IsLabdipAcknowledge = isLabdipAcknowledge;
            data["FBookingAcknowledgeList"] = masterData.FBookingAcknowledgeList;

            var url = '/api/bds-acknowledge/labDip/acknowledge';
            if (isRevise) data.IsRevised = isRevise;

            if (isReviseFromUnAckList) {
                data.IsRevised = isReviseFromUnAckList;
                url = '/api/bds-acknowledge/labDip/formulation/revision';
            }

            axios.post(url, data)
                .then(function (response) {
                    toastr.success("Saved successfully.");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }
    }

    function approveAddition(isReject, RejectReason, paramTypeId, isYarnReviseFromFinalApproveList) {

        var data = formElToJson($formEl);
        data.YBookingNo = $formEl.find("#YBookingNo").val();

        data.IsIncreaseRevisionNo = masterData.IsIncreaseRevisionNo;
        data.PMCFinalApproveCount = masterData.PMCFinalApproveCount;

        if (data.YBookingNo.trim().length == 0) return toastr.error('Invalid Yarn Booking No.');

        var childs = [];
        if (masterData.HasFabric) {
            var fabrics = $tblChildEl.getCurrentViewRecords();
            childs.push(...fabrics);
        }
        if (masterData.HasCollar) {
            var collars = $tblChildCollarIdEl.getCurrentViewRecords();
            childs.push(...collars);
        }
        if (masterData.HasCuff) {
            var cuffs = $tblChildCuffIdEl.getCurrentViewRecords();
            childs.push(...cuffs);
        }

        data.FBookingChild = [];
        /*for (var i = 0; i < childs.length; i++) {
            var obj = childs[i];
            var fBookingChild = {
                IsForFabric: obj.IsForFabric,
                BookingChildID: obj.BookingChildID,
                BookingQty: obj.IsForFabric ? obj.BookingQty : 0,
                ChildItems: obj.IsForFabric ? [] : obj.ChildItems
            };
            data.FBookingChild.push(fBookingChild);
        }*/
        for (var i = 0; i < childs.length; i++) {
            var obj = childs[i];
            obj.BookingQty = obj.IsForFabric ? obj.BookingQty : 0;
            obj.SubGroupId = obj.SubGroupId == 0 ? obj.SubGroupID : obj.SubGroupId;
            data.FBookingChild.push(obj);
        }

        data.ParamTypeId = paramTypeId;
        data.IsReject = isReject;
        data.IsApprove = !isReject;

        data.BuyerName = masterData.BuyerName;
        data.BuyerTeamName = masterData.BuyerTeamName;
        data.RevisionNo = masterData.RevisionNo;
        data.IsAddition = true;
        data.IsUpdateAddition = true;
        data.ParentYBookingNo = masterData.ParentYBookingNo;
        if (isYarnReviseFromFinalApproveList) {
            data.RevisionReasonList = masterData.RevisionReasonList;
            data.IsRevisedYarn = true;
        }
        if (isReject) {
            if (paramTypeId == _paramType.AYBQtyFinalizationPMC) {
                data.IsQtyFinalizationPMCReject = true;
                data.QtyFinalizationPMCRejectReason = RejectReason;
            }
            else if (paramTypeId == _paramType.AYBProdHeadApproval) {
                data.IsProdHeadReject = true;
                data.ProdHeadRejectReason = RejectReason;
            }
            else if (paramTypeId == _paramType.AYBTextileHeadApproval) {
                data.IsTextileHeadReject = true;
                data.TextileHeadRejectReason = RejectReason;
            }
            else if (paramTypeId == _paramType.AYBKnittingUtilization) {
                data.IsKnittingUtilizationReject = true;
                data.KnittingUtilizationRejectReason = RejectReason;
            }
            else if (paramTypeId == _paramType.AYBKnittingHeadApproval) {
                data.IsKnittingHeadReject = true;
                data.KnittingHeadRejectReason = RejectReason;
            }
            else if (paramTypeId == _paramType.AYBOperationHeadApproval) {
                data.IsOperationHeadReject = true;
                data.OperationHeadRejectReason = RejectReason;
            }

        }
        //axios.post("/api/bds-acknowledge/bulk/addition/approveOrReject", data)
        axios.post("/api/bds-acknowledge/bulk/saveAddition", data)
            .then(function (response) {
                if (data.IsReject) toastr.success("Successfully Rejected.");
                else toastr.success("Successfully Approved.");
                backToListBulk2();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function acknowledgeYBA(isUnack, result) {

        var data = {
            BookingNo: masterData.BookingNo,
            YBookingNo: masterData.YBookingNo,
            Acknowledge: isUnack ? false : true,
            UnAcknowledge: isUnack,
            UnAckReason: result,
            IsRevice: status == statusConstants.REVISE ? true : false
        };
        axios.post("/api/yarn-booking/bulk/ackOrUnack", data)
            .then(function (response) {
                if (isUnack) toastr.success("Successfully unacknowledged.");
                else toastr.success("Successfully acknowledged.");
                backToListBulk2();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function displayYarnBookingSummaryReport() {
        var yBookingNo = $.trim($formEl.find("#YBookingNo").val());
        if (yBookingNo.length == 0) {
            toastr.error("Yarn booking no missing");
            return false;
        }
        window.open(`/reports/InlinePdfView?ReportName=YarnBookingSummary_New.rdl&YBookingNo=${yBookingNo}`, '_blank');
    }
    async function loadItemSummary() {
        if (isYarnBookingAckMenu()) {
            var yarns = [];

            if (typeof $tblChildEl !== "undefined") {
                var fabrics = $tblChildEl.getCurrentViewRecords();
                fabrics.map(x => {
                    yarns.push(...x.ChildItems);
                });
            }
            if (typeof $tblChildCollarIdEl !== "undefined") {
                var collars = $tblChildCollarIdEl.getCurrentViewRecords();
                collars.map(x => {
                    yarns.push(...x.ChildItems);
                });
            }
            if (typeof $tblChildCuffIdEl !== "undefined") {
                var cuffs = $tblChildCuffIdEl.getCurrentViewRecords();
                cuffs.map(x => {
                    yarns.push(...x.ChildItems);
                });
            }


            var columns = await getChildColumnsForBDS2(true);
            columns.shift();

            columns.unshift({
                field: 'SerialNo', headerText: 'Serial', textAlign: 'center', allowEditing: false
            });

            columns = columns.filter(x => x.field != 'YarnReqQty');
            columns[columns.findIndex(x => x.field == "YarnBalanceQty")].headerText = "Net Yarn Required Qty After Utilization (Kg)";
            columns[columns.findIndex(x => x.field == "DyedYarnUtilizationQty")].headerText = "Dyed Yarn Utilization Qty (Kg)";
            columns[columns.findIndex(x => x.field == "GreyYarnUtilizationQty")].headerText = "Sample/Liabilities/Unusable/Leftover Yarn Utilization Qty (KG)";
            columns[columns.findIndex(x => x.field == "NetYarnReqQty")].headerText = "Yarn Required Quantity (KG)";

            var indexF = columns.findIndex(x => x.field == "NetYarnReqQty");
            indexF = indexF + 1 //Popup of NetYarnReqQty
            columns.splice(indexF, 1);

            var serialNo = 1;
            yarns.map(x => {
                x.SerialNo = serialNo++;
            });

            if ($tblItemSummaryEl) $tblItemSummaryEl.destroy();
            $tblItemSummaryEl = new initEJ2Grid({
                tableId: tblItemSummary,
                data: yarns,
                allowFiltering: false,
                allowPaging: false,
                allowScrolling: false,
                allowResizing: true,
                columns: columns,
                aggregates: [
                    {
                        columns: [
                            {
                                type: 'Sum',
                                field: 'YarnReqQty',
                                decimals: 2,
                                format: "N2",
                                footerTemplate: '${Sum}'
                            },
                            {
                                type: 'Sum',
                                field: 'NetYarnReqQty',
                                decimals: 2,
                                format: "N2",
                                footerTemplate: '${Sum}'
                            },
                            {
                                type: 'Sum',
                                field: 'GreyYarnUtilizationQty',
                                decimals: 2,
                                format: "N2",
                                footerTemplate: '${Sum}'
                            },
                            {
                                type: 'Sum',
                                field: 'DyedYarnUtilizationQty',
                                decimals: 2,
                                format: "N2",
                                footerTemplate: '${Sum}'
                            },
                            {
                                type: 'Sum',
                                field: 'YarnBalanceQty',
                                decimals: 2,
                                format: "N2",
                                footerTemplate: '${Sum}'
                            }
                        ]
                    }
                ],
                commandClick: childCommandClickChild2
            });
            //$tblItemSummaryEl.refreshColumns;
            //$tblItemSummaryEl.appendTo(tblItemSummary);
        }
    }

    function getIsAllowYBookingNo() {
        if (isAdditionBulkBooking() && status != statusConstants.APPROVED2) return true;
        return false;
    }
    //Addition Bulk Booking
    function isAdditionBulkBooking() {
        if (menuType == _paramType.AdditionalYarnBooking ||
            menuType == _paramType.AYBQtyFinalizationPMC ||
            menuType == _paramType.AYBProdHeadApproval ||
            menuType == _paramType.AYBTextileHeadApproval ||
            menuType == _paramType.AYBKnittingUtilization ||
            menuType == _paramType.AYBKnittingHeadApproval ||
            menuType == _paramType.AYBOperationHeadApproval) return true;
        return false;
    }
    function changeButtonName() {
        if (menuType == _paramType.AdditionalYarnBooking) $formEl.find("#spnSaveAsDraft").html("Save");
    }
    function listLoadOperationBulkAddition() {
        if (status == statusConstants.REJECT) {
            $toolbarEl.find("#btnRejectBBKIList").click();
        }
        else if (_isPendingList) {
            $toolbarEl.find("#btnPendingList").click();
        }
        else if (!_isPendingList) {
            $toolbarEl.find("#btnApproveBBKIList").click();
        }
    }

    function getBookingQtyKG(length, width, bookingQtyPcs, subGroupId) {

        var size = "";
        var gm = 0;
        if (subGroupId == 11) {
            gm = parseFloat($(pageIdWithHash).find("#CollarWeightInGm").val());
            size = $(pageIdWithHash).find("#CollarSizeID").val();
        }
        else if (subGroupId == 12) {
            gm = parseFloat($(pageIdWithHash).find("#CuffWeightInGm").val());
            size = $(pageIdWithHash).find("#CuffSizeID").val();
        }

        var selectedLength = parseFloat(size.split(' X ')[0]);
        var selectedWidth = parseFloat(size.split(' X ')[1]);
        //var perWeight = parseFloat(gm) / (parseFloat(selectedLength) * parseFloat(selectedWidth));

        var bookingQtyKG = getDefaultValueWhenInvalidN_Float(bookingQtyPcs * ((gm * length * width) / (selectedLength * selectedWidth)));
        var result = getDefaultValueWhenInvalidN_Float(bookingQtyKG / 1000);
        return result;
    }
    function getBookingQtyKGFromPcs(length, width, bookingQtyKG, subGroupId) {
        var size = "";
        var gm = 0;
        if (subGroupId == 11) {
            gm = parseFloat($(pageIdWithHash).find("#CollarWeightInGm").val());
            size = $(pageIdWithHash).find("#CollarSizeID").val();
        }
        else if (subGroupId == 12) {
            gm = parseFloat($(pageIdWithHash).find("#CuffWeightInGm").val());
            size = $(pageIdWithHash).find("#CuffSizeID").val();
        }

        var selectedLength = parseFloat(size.split(' X ')[0]);
        var selectedWidth = parseFloat(size.split(' X ')[1]);
        //var perWeight = parseFloat(gm) / (parseFloat(selectedLength) * parseFloat(selectedWidth));

        //var bookingQtyKG = getDefaultValueWhenInvalidN_Float(((gm * length * width) / (selectedLength * selectedWidth)));
        //var result = getDefaultValueWhenInvalidN_Float(bookingQtyKG / 1000);
        //var finalResultForPCS = getDefaultValueWhenInvalidN_Float((result*1000) / bookingQtyKG);

        var finalResultForPCS = (bookingQtyKG * 1000) / ((gm * length * width) / (selectedLength * selectedWidth));
        return finalResultForPCS;
    }

    function SetCollarBookingWeightKG() {

        masterData.Collars.forEach(x => {

            var Sizelist = masterData.AllCollarSizeList.filter(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color);
            var BookingWeightGM = 0;
            var BookingWeightGMPCS = 0;

            Sizelist.forEach(z => {

                BookingWeightGM += getBookingQtyKG(z.Length, z.Width, z.BookingQty, 11);
                z.BookingQtyKG = getBookingQtyKG(z.Length, z.Width, z.BookingQty, 11);
                z = setBookingQtyKGRelatedFieldsValue(z, 11);
            });

            x.BookingQtyKG = getDefaultValueWhenInvalidN_Float(BookingWeightGM);
            x = setBookingQtyKGRelatedFieldsValue(x, 11);
            Sizelist.forEach(z => {

                BookingWeightGMPCS += getBookingQtyKGFromPcs(z.Length, z.Width, z.GreyProdQty, 11);
            });

            var ff = BookingWeightGMPCS;
        });
    }
    function SetCuffBookingWeightKG() {
        masterData.Cuffs.forEach(x => {
            var Sizelist = masterData.AllCuffSizeList.filter(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color);
            var BookingWeightGM = 0;
            Sizelist.forEach(z => {
                BookingWeightGM += getBookingQtyKG(z.Length, z.Width, z.BookingQty, 12);
            });
            x.BookingQtyKG = getDefaultValueWhenInvalidN_Float(BookingWeightGM);
            x = setBookingQtyKGRelatedFieldsValue(x, 12);
        });
    }
    function SetCollarBookingWeightKGAfterSave() {

        masterData.FBookingChildCollor.forEach(x => {

            var Sizelist = masterData.AllCollarSizeList.filter(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color);
            var BookingWeightGM = 0;
            Sizelist.forEach(z => {
                BookingWeightGM += getBookingQtyKG(z.Length, z.Width, z.BookingQty, 11);
            });
            x.BookingQtyKG = getDefaultValueWhenInvalidN_Float(BookingWeightGM);
            x = setBookingQtyKGRelatedFieldsValue(x, 11);
        });
    }
    function SetCuffBookingWeightKGAfterSave() {
        masterData.FBookingChildCuff.forEach(x => {
            var Sizelist = masterData.AllCuffSizeList.filter(y => y.Construction == x.Construction && y.Composition == x.Composition && y.Color == x.Color);
            var BookingWeightGM = 0;
            Sizelist.forEach(z => {
                BookingWeightGM += getBookingQtyKG(z.Length, z.Width, z.BookingQty, 12);
            });
            x.BookingQtyKG = getDefaultValueWhenInvalidN_Float(BookingWeightGM);
            x = setBookingQtyKGRelatedFieldsValue(x, 12);
        });
    }
    function setBookingQtyKGRelatedFieldsValue(item, subGroupId) {

        var bookingQtyProp = subGroupId == 1 ? "BookingQty" : "BookingQtyKG";
        if (!item.FinishFabricUtilizationQty) item.FinishFabricUtilizationQty = 0;
        item.ReqFinishFabricQty = getDefaultValueWhenInvalidN_Float(item[bookingQtyProp] - item.FinishFabricUtilizationQty);

        item.GreyReqQty = (item.ReqFinishFabricQty * (1 + (item.YarnAllowance / 100) - (0.5 / 100))).toFixed(2);
        item.GreyReqQty = item.YarnAllowance == 0 ? item[bookingQtyProp] : item.GreyReqQty;
        item.GreyReqQty = getDefaultValueWhenInvalidN_Float(item.GreyReqQty);

        item.GreyLeftOverQty = getDefaultValueWhenInvalidN_Float(item.GreyLeftOverQty);

        item.GreyProdQty = getDefaultValueWhenInvalidN_Float(item.GreyReqQty - item.GreyLeftOverQty); //Grey Utilization Qty = GreyLeftOverQty
        item.BookingQtyKG = getDefaultValueWhenInvalidN_Float(item.BookingQtyKG);

        return item;
    }
    function setAdditionalAllowance(child) {
        //child.TotalYarnAllowance = child.YarnAllowance + child.ExistingYarnAllowance;
        child.TotalYarnAllowance = child.YarnAllowance;
        return child;
    }
    function isEnableSizeConsumption(isEnable) {
        $formEl.find(".divForWeight").show();
        $formEl.find(".SizeWithConsumption").prop("disabled", !isEnable);
    }
    function getMachineBrandList(allList, gauge, dia, subGroupId) {
        if (subGroupId == 1) {
            var brandList = allList.filter(bb => bb.GG == gauge && bb.Dia == dia);
            if (brandList.length == 0) return [];

            var brandIds = brandList.map(x => x.BrandID);
            brandIds = [...new Set(brandIds)];

            brandList = [];
            brandIds.map(bbId => {
                var brandName = allList.find(cc => cc.BrandID == bbId).Brand;
                brandList.push({
                    BrandID: bbId,
                    Brand: brandName
                });
            });
            return brandList;
        }
        return allList;
    }
    function tootBarButtonHideShow() {
        $toolbarEl.find(".btnToolbar").hide();

        if (isBulkBookingKnittingInfoMenu()) {
            var btnIds = "";
            switch (menuType) {
                case _paramType.BulkBookingAck:
                    btnIds = "#btnPendingList,#btnDraftList,#btnBookingList,#btnUnAcknowledgedList,#btnFinalApprovaledList,#btnInternalRejectionList,#btnAllBulkBookingList,#btnPendingExport,#btnExport";
                    break;
                case _paramType.BulkBookingYarnAllowance:
                    btnIds = "#btnPendingAllowanceList,#btnAllowanceList,#btnRejectedAllowanceList,#btnAllBulkBookingList";
                    break;
                case _paramType.BulkBookingUtilizationProposal:
                    btnIds = "#btnPendingUtilizationProposalList,#btnAllBulkBookingList";
                    break;
                case _paramType.BulkBookingUtilizationConfirmation:
                    btnIds = "#btnPendingUtilizationConfirmationList,#btnUtilizationConfirmedList,#btnAllBulkBookingList";
                    break;
                case _paramType.BulkBookingCheck:
                    btnIds = "#btnPendingCheckList,#btnCheckedList,#btnRejectedCheckList,#btnAllBulkBookingList";
                    break;
                case _paramType.BulkBookingApprove:
                    btnIds = "#btnPendingApprovalList,#btnApprovedList,#btnRejectedApproveList,#btnAllBulkBookingList";
                    break;
                case _paramType.BulkBookingFinalApprove:
                    btnIds = "#btnPendingFinalApprovalList,#btnFinalApprovaledList,#btnRejectedFinalApprovalList,#btnAllBulkBookingList";
                    break;
                case _paramType.YarnBookingAcknowledge:
                    btnIds = "#btnPendingYBAList,#btnRevisionYarnYBAList,#btnRevisionBookingYBAList,#btnAckYBAList,#btnUnAckYBAList";
                    break;
                default:
                    // code block
                    break;
            }

            if (btnIds.length > 0) $toolbarEl.find(btnIds).show();
        }
        else if (isAdditionBulkBooking) {
            var btnIds = "";
            switch (menuType) {
                case _paramType.AdditionalYarnBooking:
                    btnIds = "#btnPendingList,#btnDraftList,#btnInternalRejectionList";
                    break;
                default:
                    // code block
                    break;
            }

            if (btnIds.length > 0) $toolbarEl.find(btnIds).show();
        }
    }


    // Grey Febric Utilization

    function LoadGreyFabricUtilization() {

        var finder = new commonFinder({
            title: "Select Item",
            pageId: pageId,
            height: 350,
            apiEndPoint: `/api/bds-acknowledge/bulk/GetGreyFabricUtilizationItem?GSMId=${__GSMId}&GSM=${__GSMNumber}&CompositionId=${__CompositionId}&ConstructionId=${__ConstructionId}&SubGroupID=${__SubGroupID}`,
            //apiEndPoint: `/api/bds-acknowledge/GetGreyFabricUtilizationItem/${selectedCompositionId}/${selectedGSMId}`,
            fields: "ExportOrderNo,ColorName,FabricType,FabricStyle,GSM,Buyer",
            headerTexts: "Order No,Color,Fabric Type,Fabric Style,GSM,Buyer",
            widths: "80,60,60,60,40,40",
            isMultiselect: true,
            autofitColumns: true,
            primaryKeyColumn: "RowNumber",
            onMultiselect: function (selectedRecords) {
                selectedRecords.forEach(function (value) {
                    var indexF = -1;
                    if (GFUtilizationSummary != null) {
                        indexF = GFUtilizationSummary.findIndex(x => x.ExportOrderID == value.ExportOrderID && x.ItemMasterID == value.ItemMasterID && x.BuyerID == value.BuyerID && x.FabricTypeID == value.FabricTypeID && x.ColorID == value.ColorID && x.CompositionID == value.CompositionID && x.GSMID == value.GSMID);
                    }
                    else {
                        GFUtilizationSummary = [];
                    }

                    if (indexF == -1) {
                        GFUtilizationSummary.push(DeepClone(value));
                    }

                    initGFUtilization(GFUtilizationSummary);

                });


            }
        });
        finder.showModal();
    }
    function initGFUtilization(data) {
        if ($tblGFUtilizationEL) $tblGFUtilizationEL.destroy();

        $tblGFUtilizationEL = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowPaging: false,
            editSettings: { allowAdding: false, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 100, textAlign: 'Left', visible: true, commands: [
                        { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ]
                },
                { field: 'ExportOrderID', visible: false },
                { field: 'ItemMasterID', visible: false },
                { field: 'BuyerID', visible: false },
                { field: 'FabricTypeID', visible: false },
                { field: 'ColorID', visible: false },
                { field: 'CompositionID', visible: false },
                { field: 'GSMID', visible: false },
                { field: 'ExportOrderNo', headerText: 'Export Order No', allowEditing: false },
                { field: 'ColorName', headerText: 'Color', width: 100, allowEditing: false },
                { field: 'FabricType', headerText: 'Fabric Type', width: 100, allowEditing: false },
                { field: 'FabricStyle', headerText: 'Fabric Style', width: 100, allowEditing: true },
                { field: 'GSM', headerText: 'GSM', width: 100, allowEditing: false },
                { field: 'GreyFabricUtilizationQTYinkg', headerText: 'GF Utilization QTY', width: 100, allowEditing: true }
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {

                    args.rowData.FabricStyle = args.data.FabricStyle;
                    args.data.GreyFabricUtilizationQTYinkg = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.GreyFabricUtilizationQTYinkg);
                    args.rowData.GreyFabricUtilizationQTYinkg = args.data.GreyFabricUtilizationQTYinkg;
                }
                else if (args.requestType === "delete") {

                }
            },
        });
        $tblGFUtilizationEL.refreshColumns;
        $tblGFUtilizationEL.appendTo(tblGFUtilizationId);
    }

    // Finish Febric Utilization

    function LoadFinishingFabricUtilizationPopUp() {
        var finder = new commonFinder({
            title: "Select Info",
            pageId: pageId,
            height: 350,
            apiEndPoint: `/api/bds-acknowledge/bulk/finishFabricUtilization?GSMId=${__GSMId}&GSM=${__GSMNumber}&CompositionId=${__CompositionId}&ConstructionId=${__ConstructionId}&SubGroupID=${__SubGroupID}`,
            fields: "ExportOrderNo,BatchNo,ColorName,Buyer,FabricConstruction,Width,GSM,ExcessQtyKg,RejectQtyKg,BookingQtyDecreasedbyMerchantQtyKg,AfterProductionOrderCancelledbyMerchantQtyKg",
            headerTexts: "EWO,Batch No,Color,Buyer,Fabric Construction,Width,GSM,Excess Qty Kg,Reject Qty Kg,Booking Qty Decreased by Merchant Qty InKg,After Production Order Cancelled by Merchant Qty InKg",
            //customFormats: ",,,ej2GridColorFormatter",
            widths: "100,100,100,100,100,80,80,100,100,350,400",
            hiddenFields: "R_No_New",
            isMultiselect: true,
            autofitColumns: true,
            primaryKeyColumn: "R_No_New",
            onMultiselect: function (selectedRecords) {
                selectedRecords.forEach(function (value) {
                    var indexF = -1;
                    if (FinishFabricUtilizationDataList != null) {
                        indexF = FinishFabricUtilizationDataList.findIndex(x => x.ExportOrderID == value.ExportOrderID && x.ItemMasterID == value.ItemMasterID && x.WeightSheetNo == value.WeightSheetNo && x.CompositionID == value.CompositionID && x.GSMID == value.GSMID && x.ConstructionID == value.ConstructionID);
                    }
                    else {
                        FinishFabricUtilizationDataList = [];
                    }

                    if (indexF == -1) {
                        FinishFabricUtilizationDataList.push(DeepClone(value));
                    }
                    initFinishFabricUtilizationTable(FinishFabricUtilizationDataList);
                });
            }
        });
        finder.showModal();
    }

    function initFinishFabricUtilizationTable(DataList) {
        var columns = [
            {
                headerText: 'Command', width: 100, textAlign: 'Left', visible: true, commands: [
                    { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                ]
            },
            //{
            //    field: 'BookingNo', headerText: 'Booking No', isPrimaryKey: true
            //},

            { field: 'ExportOrderNo', headerText: 'Export Order No', visible: true, allowEditing: false },
            { field: 'BatchNo', headerText: 'Batch No', visible: true, allowEditing: false },
            { field: 'ColorName', headerText: 'Color', visible: true, allowEditing: false },
            { field: 'Buyer', headerText: 'Buyer', visible: true, allowEditing: false },
            { field: 'FabricConstruction', headerText: 'Fabric Construction', visible: true, allowEditing: false },
            { field: 'Width', headerText: 'Width', visible: true, allowEditing: false },
            { field: 'GSM', headerText: 'GSM', visible: true, allowEditing: false },
            //{ field: 'TotalStockQtyinkg', headerText: 'Total Stock Qty in kg', visible: true, allowEditing: false },
            { field: 'ExcessQtyKg', headerText: 'Excess Qty InKg', width: 100, allowEditing: false },
            { field: 'FinishFabricExcessQtyKg', headerText: 'Finish Fabric Excess Utilization Qty Kg', width: 250, allowEditing: true },

            { field: 'RejectQtyKg', headerText: 'Reject Qty InKg', width: 100, allowEditing: false },
            { field: 'FinishFabricRejectQtyKg', headerText: 'Finish Fabric Reject Utilization Qty InKg', width: 350, allowEditing: true },

            { field: 'BookingQtyDecreasedbyMerchantQtyKg', headerText: 'Booking Qty Decreased by Merchant Qty InKg', width: 350, allowEditing: false },
            { field: 'FinishFabricBookingQtyDecreasedbyMerchantQtyKg', headerText: 'Finish Fabric Booking Qty Decreased by Merchant Utilization Qty InKg', width: 400, allowEditing: true },

            { field: 'AfterProductionOrderCancelledbyMerchantQtyKg', headerText: 'After Production Order Cancelled by Merchant Qty InKg', width: 350, allowEditing: false },
            { field: 'FinishFabricAfterProductionOrderCancelledbyMerchantQtyKg', headerText: 'Finish Fabric After Production Order Cancelled by Merchant Utilization Qty InKg', width: 450, allowEditing: true },

            { field: 'FinishFabricUtilizationQTYinkg', headerText: 'Finish Fabric Utilization QTY', width: 200, allowEditing: false },
            { field: 'UserName', headerText: 'Modified/Added by', width: 200, allowEditing: false },

        ];

        if ($tblFFUtilizationEL) $tblFFUtilizationEL.destroy();
        $tblFFUtilizationEL = new ej.grids.Grid({
            dataSource: DataList,
            allowResizing: true,
            allowPaging: false,
            //allowGrouping: true,
            editSettings: { allowAdding: false, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {

                    //args.data.FinishFabricUtilizationQTYinkg = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.FinishFabricUtilizationQTYinkg);
                    args.data.FinishFabricExcessQtyKg = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.FinishFabricExcessQtyKg);
                    args.data.FinishFabricRejectQtyKg = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.FinishFabricRejectQtyKg);
                    args.data.FinishFabricBookingQtyDecreasedbyMerchantQtyKg = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.FinishFabricBookingQtyDecreasedbyMerchantQtyKg);
                    args.data.FinishFabricAfterProductionOrderCancelledbyMerchantQtyKg = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.FinishFabricAfterProductionOrderCancelledbyMerchantQtyKg);

                    //args.rowData.FinishFabricUtilizationQTYinkg = args.data.FinishFabricUtilizationQTYinkg;
                    var FinishFabricUtilizationQTYinkg = args.data.FinishFabricExcessQtyKg + args.data.FinishFabricRejectQtyKg + args.data.FinishFabricBookingQtyDecreasedbyMerchantQtyKg + args.data.FinishFabricAfterProductionOrderCancelledbyMerchantQtyKg;


                    args.rowData.FinishFabricExcessQtyKg = args.data.FinishFabricExcessQtyKg;
                    args.rowData.FinishFabricRejectQtyKg = args.data.FinishFabricRejectQtyKg;
                    args.rowData.FinishFabricBookingQtyDecreasedbyMerchantQtyKg = args.data.FinishFabricBookingQtyDecreasedbyMerchantQtyKg;
                    args.rowData.FinishFabricAfterProductionOrderCancelledbyMerchantQtyKg = args.data.FinishFabricAfterProductionOrderCancelledbyMerchantQtyKg;

                    args.data.FinishFabricUtilizationQTYinkg = getDefaultValueWhenInvalidN_FloatWithFourDigit(FinishFabricUtilizationQTYinkg);
                    args.rowData.FinishFabricUtilizationQTYinkg = args.data.FinishFabricUtilizationQTYinkg;
                }
                else if (args.requestType === "delete") {

                }
            },
        });
        $tblFFUtilizationEL.refreshColumns;
        $tblFFUtilizationEL.appendTo(tblFFUtilizationId);
    }

    // Grey Yarn utilization
    function initGreyYarnUtilization(data) {
        if ($tblGeryYarnUtilizationEL) $tblGeryYarnUtilizationEL.destroy();

        $tblGeryYarnUtilizationEL = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowPaging: false,
            editSettings: { allowAdding: false, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 100, textAlign: 'Left', visible: true, commands: [
                        { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ]
                },
                { field: 'YarnStockSetID', visible: false, isPrimaryKey: true },
                { field: 'Spinner', headerText: 'Spinner', allowEditing: false },
                { field: 'PhysicalLot', headerText: 'Physical Lot', width: 100, allowEditing: false },
                { field: 'PhysicalCount', headerText: 'Physical Count', width: 100, allowEditing: false },
                { field: 'NumaricCount', headerText: 'Numaric Count', width: 100, allowEditing: false },
                { field: 'YarnDetails', headerText: 'Yarn Details', width: 100, allowEditing: false },
                { field: 'SampleStockQty', headerText: 'Sample Stock', width: 100, allowEditing: false, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
                { field: 'UtilizationSampleStock', headerText: 'Utilization Sample Stock', width: 100, allowEditing: true, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
                { field: 'LiabilitiesStockQty', headerText: 'Liabilities Stock', width: 100, allowEditing: false, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
                { field: 'UtilizationLiabilitiesStock', headerText: 'Utilization Liabilities Sock', width: 100, allowEditing: true, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
                { field: 'UnusableStockQty', headerText: 'Unusable Stock', width: 100, allowEditing: false, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
                { field: 'UtilizationUnusableStock', headerText: 'Utilization Unusable Stock', width: 100, allowEditing: true, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
                { field: 'LeftoverStockQty', headerText: 'Leftover Stock', width: 100, allowEditing: false, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
                { field: 'UtilizationLeftoverStock', headerText: 'Utilization Leftover Stock', width: 100, allowEditing: true, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
                { field: 'TotalUtilization', headerText: 'Total Utilization', width: 100, allowEditing: false, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {
                    var SampleStockQty = 0;
                    var UtilizationSampleStock = 0;
                    var LiabilitiesStockQty = 0;
                    var UtilizationLiabilitiesStock = 0;
                    var UnusableStockQty = 0;
                    var UtilizationUnusableStock = 0;
                    var LeftoverStockQty = 0;
                    var UtilizationLeftoverStock = 0;
                    var TotalUtilization = 0;

                    args.data.SampleStockQty = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.rowData.SampleStockQty);
                    args.data.UtilizationSampleStock = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.UtilizationSampleStock);

                    args.data.LiabilitiesStockQty = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.rowData.LiabilitiesStockQty);
                    args.data.UtilizationLiabilitiesStock = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.UtilizationLiabilitiesStock);

                    args.data.UnusableStockQty = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.rowData.UnusableStockQty);
                    args.data.UtilizationUnusableStock = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.UtilizationUnusableStock);

                    args.data.LeftoverStockQty = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.rowData.LeftoverStockQty);
                    args.data.UtilizationLeftoverStock = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.UtilizationLeftoverStock);

                    // alamin
                    if (args.data.SampleStockQty < args.data.UtilizationSampleStock) {
                        //return toastr.error("Invalid: You cannot enter more than Sample Stock");
                    }
                    else if (args.data.LiabilitiesStockQty < args.data.UtilizationLiabilitiesStock) {
                        //return toastr.error("Invalid: You cannot enter more than Utilization Sample Stock");
                    }
                    else if (args.data.UnusableStockQty < args.data.UtilizationUnusableStock) {
                        //return toastr.error("Invalid: You cannot enter more than Unusable Stock");
                    }
                    else if (args.data.LeftoverStockQty < args.data.UtilizationLeftoverStock) {
                        //return toastr.error("Invalid: You cannot enter more than Leftover Stock");
                    }
                    else {
                        args.data.TotalUtilization += args.data.UtilizationSampleStock + args.data.UtilizationLiabilitiesStock + args.data.UtilizationUnusableStock + args.data.UtilizationLeftoverStock;
                    }

                    args.data.TotalUtilization = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.TotalUtilization);
                    args.rowData = args.data;
                }
                else if (args.requestType === "delete") {

                }
            },
        });
        $tblGeryYarnUtilizationEL.refreshColumns;
        $tblGeryYarnUtilizationEL.appendTo(tblGeryYarnUtilizationId);
    }
    function LoadGreyYarnUtilization() {

        var finder = new commonFinder({
            title: "Select Item",
            pageId: pageId,
            height: 350,
            apiEndPoint: `/api/bds-acknowledge/bulk/GetGreyYarnUtilizationItem?ItemMasterID=${__ItemMasterID}`,
            fields: "Spinner,PhysicalLot,PhysicalCount,NumaricCount,YarnDetails,SampleStockQty,LiabilitiesStockQty,UnusableStockQty,LeftoverStockQty",
            headerTexts: "Spinner,Physical Lot,Physical Count,Numaric Count,Yarn Details,Sample Stock Qty,Liabilities Stock Qty,Unusable Stock Qty,Leftover Stock Qty",
            widths: "100,100,140,100,140,140,140,140,140",

            isMultiselect: true,
            autofitColumns: true,
            primaryKeyColumn: "YarnStockSetID",
            onMultiselect: function (selectedRecords) {

                selectedRecords.forEach(function (value) {
                    var indexF = -1;
                    if (GreyYarnUtilizationSummary != null) {
                        indexF = GreyYarnUtilizationSummary.findIndex(x => x.YarnStockSetID == value.YarnStockSetID && x.ItemMasterID == value.ItemMasterID);
                    }
                    else {
                        GreyYarnUtilizationSummary = [];
                    }

                    if (indexF == -1) {
                        GreyYarnUtilizationSummary.push(DeepClone(value));
                    }

                });

                initGreyYarnUtilization(GreyYarnUtilizationSummary);


            }
        });
        finder.showModal();
    }


    // Dyed Yarn Utilization 

    function initDyedYarnUtilization(data) {
        if ($tblDyedYarnUtilizationEL) $tblDyedYarnUtilizationEL.destroy();

        $tblDyedYarnUtilizationEL = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowPaging: false,
            editSettings: { allowAdding: false, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 100, textAlign: 'Left', visible: true, commands: [
                        { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ]
                },
                //{ field: 'YarnStockSetId', visible: false, isPrimaryKey: true },
                { field: 'ExportOrderNo', headerText: 'EWO', allowEditing: false },
                { field: 'Buyer', headerText: 'Buyer', width: 100, allowEditing: false },
                { field: 'ColorName', headerText: 'Fabric Color', width: 100, allowEditing: false },
                { field: 'PhysicalCount', headerText: 'Physical Count', width: 100, allowEditing: true },
                { field: 'DyedYarnUtilizationQty', headerText: 'Dyed Yarn UtilizationQty', width: 100, allowEditing: true, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {

                    args.rowData.PhysicalCount = args.data.PhysicalCount;
                    args.data.DyedYarnUtilizationQty = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.DyedYarnUtilizationQty);
                    args.rowData.DyedYarnUtilizationQty = args.data.DyedYarnUtilizationQty;
                }
                else if (args.requestType === "delete") {

                }
            },
        });
        $tblDyedYarnUtilizationEL.refreshColumns;
        $tblDyedYarnUtilizationEL.appendTo(tblDyedYarnUtilizationId);
    }

    function LoadDyedYarnUtilization() {
        var finder = new commonFinder({
            title: "Select Item",
            pageId: pageId,
            height: 350,
            apiEndPoint: `/api/bds-acknowledge/bulk/GetDyedYarnUtilizationItem?GSMId=${__GSMId}&CompositionId=${__CompositionId}&ConstructionId=${__ConstructionId}&SubGroupID=${__SubGroupID}`,
            fields: "ExportOrderNo,Buyer,ColorName",
            headerTexts: "Export Order No,Buyer,Fabric Color",
            widths: "140,140,140",

            isMultiselect: true,
            autofitColumns: true,
            primaryKeyColumn: "ColorID",
            onMultiselect: function (selectedRecords) {
                selectedRecords.forEach(function (value) {
                    var indexF = -1;
                    if (DyedYarnUtilizationSummary != null) {
                        indexF = DyedYarnUtilizationSummary.findIndex(x => x.ColorID == value.ColorID && x.ExportOrderID == value.ExportOrderID);
                    }
                    else {
                        DyedYarnUtilizationSummary = [];
                    }

                    if (indexF == -1) {
                        DyedYarnUtilizationSummary.push(DeepClone(value));
                    }
                });
                initDyedYarnUtilization(DyedYarnUtilizationSummary);
            }
        });
        finder.showModal();
    }



    /// Replacement QTY Additoinal booking

    function initAdditioalReplacementQTY(data) {
        if ($tblFBAckChildReplacementInfoEL) $tblFBAckChildReplacementInfoEL.destroy();

        $tblFBAckChildReplacementInfoEL = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowPaging: false,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 100, textAlign: 'Left', visible: true, commands: [
                        { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ]
                },
                //{ field: '', visible: false, isPrimaryKey: true },
                {
                    field: 'ReasonID',
                    headerText: 'Reason',
                    valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.AdditionalYarnBookingReason,
                    allowEditing: true,
                    displayField: "Reason",
                    edit: ej2GridDropDownObj({
                    })
                },
                {
                    field: 'DepertmentID',
                    headerText: 'Related Department',
                    valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.AdditionalEFLCompanyList,
                    allowEditing: true,
                    displayField: "Department",
                    edit: ej2GridDropDownObj({
                    })
                },
                { field: 'ReplacementQTY', headerText: 'Add. QTY', width: 100, allowEditing: true, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
                { field: 'Remarks', headerText: 'Remarks', width: 100, allowEditing: true },
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {
                    //args.rowData.PhysicalCount = args.data.PhysicalCount;
                    //args.data.DyedYarnUtilizationQty = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.DyedYarnUtilizationQty);
                    //args.rowData.DyedYarnUtilizationQty = args.data.DyedYarnUtilizationQty;
                }
                else if (args.requestType === "delete") {

                }
            },
        });
        $tblFBAckChildReplacementInfoEL.refreshColumns;
        $tblFBAckChildReplacementInfoEL.appendTo(tblFBAckChildReplacementInfoId);
    }
    // Yarn Req Qty Finder For Addition
    function initAdditioalNetReqQTY(data) {
        if ($tblFBAckYarnNetYarnReqQtyInfoEL) $tblFBAckYarnNetYarnReqQtyInfoEL.destroy();

        $tblFBAckYarnNetYarnReqQtyInfoEL = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            allowPaging: false,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 100, textAlign: 'Left', visible: true, commands: [
                        { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ]
                },
                //{ field: '', visible: false, isPrimaryKey: true },
                {
                    field: 'ReasonID',
                    headerText: 'Reason',
                    valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.AdditionalYarnBookingReason,
                    allowEditing: true,
                    displayField: "Reason",
                    edit: ej2GridDropDownObj({
                    })
                },
                {
                    field: 'DepertmentID',
                    headerText: 'Related Department',
                    valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.AdditionalEFLCompanyList,
                    allowEditing: true,
                    displayField: "Department",
                    edit: ej2GridDropDownObj({
                    })
                },
                { field: 'ReplacementQTY', headerText: 'Add. QTY', width: 100, allowEditing: true, edit: { params: { showSpinButton: false, decimals: 4, format: "N4" } } },
                { field: 'Remarks', headerText: 'Remarks', width: 100, allowEditing: true },
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {
                    //args.rowData.PhysicalCount = args.data.PhysicalCount;
                    //args.data.DyedYarnUtilizationQty = getDefaultValueWhenInvalidN_FloatWithFourDigit(args.data.DyedYarnUtilizationQty);
                    //args.rowData.DyedYarnUtilizationQty = args.data.DyedYarnUtilizationQty;
                }
                else if (args.requestType === "delete") {

                }
            },
        });
        $tblFBAckYarnNetYarnReqQtyInfoEL.refreshColumns;
        $tblFBAckYarnNetYarnReqQtyInfoEL.appendTo(tblFBAckYarnNetYarnReqQtyInfoId);
    }


    function getExportData(bookingNo, isSample) {
        _isFirstLoad = true;

        axios.get(`/api/yarn-booking/forBulkExportData/${bookingNo}/${isSample}`)
            .then(function (response) {
                var json_str = JSON.parse(JSON.stringify(response.data));

                if (json_str.length > 10) {
                    var fileName = 'ExportData_' + bookingNo + '.txt';

                    saveFile(fileName, "data:text/plain", new Blob([json_str], { type: "" }));
                }

            })
            .catch(showResponseError);
    }

    function isFieldShowFabric(fieldName) {
        var isShow = true;
        switch (fieldName) {
            case "YarnAllowance":
                return isColumnShowFabric(fieldName);
                break;
            case "Construction":
            case "YarnType":
            case "YarnProgram":
            case "LabDipNo":
            case "Instruction":
                return isColumnShowFabric(fieldName);
                break;
            case "ReferenceNo":
                return isColumnShowFabric(fieldName);
                break;
            case "GreyLeftOverQty":
                if (isAdditionBulkBooking()) return true;
                else return isColumnShowFabric(fieldName);
                break;
            default:
                isShow = isColumnShowFabric(fieldName);
                return isShow;
        }
        return true;
    }

    function isColumnShowFabric(fieldName) {
        if (isColumnFilteredMenu()) {
            var fieldList = [];
            if (menuType == _paramType.BulkBookingYarnAllowance) {
                fieldList = ["YarnAllowance", "Construction", "Composition", "Color", "GSM", "FabricWidth", "DyeingType",
                    "ReferenceNo", "ReqFinishFabricQty", "YarnType", "YarnProgram", "LabDipNo", "Instruction", "Remarks"];
            }
            else if (menuType == _paramType.BulkBookingCheck) {
                return true;
            }
            else if (menuType == _paramType.BulkBookingApprove) {
                fieldList = ["YarnAllowance", "Construction", "Composition", "Color", "GSM", "FabricWidth", "DyeingType",
                    "ReferenceNo", "ReqFinishFabricQty", "YarnType", "YarnProgram", "LabDipNo", "Instruction", "FinishFabricUtilizationQty",
                    "GreyLeftOverQty", "Remarks"];
            }
            var indexFN = fieldList.findIndex(x => x == fieldName);
            if (indexFN > -1) return true;
        }
        return false;
    }
    function isColumnShowCollarCuff(fieldName) {
        if (isColumnFilteredMenu()) {
            var fieldList = [];
            if (menuType == _paramType.BulkBookingYarnAllowance) {
                fieldList = ["Construction", "Composition", "YarnAllowance", "Color", "YarnType", "YarnProgram", "DyeingType",
                    "Instruction", "LabDipNo", "ReferenceNo", "BookingQty", "BookingQtyKG"];
            }
            else if (menuType == _paramType.BulkBookingCheck) {
                return true;
            }
            else if (menuType == _paramType.BulkBookingApprove) {
                fieldList = ["Construction", "Composition", "YarnAllowance", "Color", "YarnType", "YarnProgram", "DyeingType",
                    "Instruction", "LabDipNo", "ReferenceNo", "BookingQty", "BookingQtyKG", "FinishFabricUtilizationQty",
                    "GreyLeftOverQty", "Remarks"];
            }
            var indexFN = fieldList.findIndex(x => x == fieldName);
            if (indexFN > -1) return true;
        }
        return false;
    }


    function isFieldShowCollarCuff(fieldName) {
        var isShow = true;
        switch (fieldName) {
            case "YarnAllowance":
                if ((isBulkBookingKnittingInfoMenu()) && menuType != _paramType.BulkBookingAck && menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation) return true;
                else return isColumnShowCollarCuff(fieldName);
                break;
            case "YarnType":
            case "YarnProgram":
            case "Instruction":
            case "LabDipNo":
                if (menuType != _paramType.BulkBookingUtilizationProposal && menuType != _paramType.BulkBookingUtilizationConfirmation) return true;
                else return isColumnShowCollarCuff(fieldName);
                break;
            case "ReferenceNo":
                if (_isBDS == 1) return true;
                else return isColumnShowCollarCuff(fieldName);
                break;
            default:
                isShow = isColumnShowCollarCuff(fieldName);
                return isShow;
        }
        return true;
    }
    function isFieldShowYarn(fieldName) {
        var isShow = true;
        switch (fieldName) {
            case "StitchLength":
                isStitchLengthShow = isColumnShowYarn(fieldName);
                return isStitchLengthShow;
                break;
            case "YarnPly":
            case "GreyAllowance":
                isShow = isColumnShowYarn(fieldName);
                return isShow;
                break;
            case "YD":
                isShow = isColumnShowYarn(fieldName);
                return isShow;
                break;
            case "YDAllowance":
                isShow = isColumnShowYarn(fieldName);
                return isShow;
                break;
            case "YarnReqQty":
                isShow = isColumnShowYarn(fieldName);
                return isShow;
                break;
            case "NetYarnReqQty":
                isShow = isColumnShowYarn(fieldName);
                return isShow;
                break;
            case "GreyYarnUtilizationQty":
            case "DyedYarnUtilizationQty":
                isShow = isColumnShowYarn(fieldName);
                return isShow;
                break;
            default:
                isShow = isColumnShowYarn(fieldName);
                return isShow;
        }
        return true;
    }
    function isColumnShowYarn(fieldName) {
        if (isBulkBookingKnittingInfoMenu()) {
            var fieldList = [];
            if (menuType == _paramType.BulkBookingYarnAllowance) {
                fieldList = ["YarnCategory", "Distribution", "YarnPly", "GreyAllowance", "YD", "YDAllowance", "Allowance",
                    "NetYarnReqQty", "Remarks"];
            }
            else if (menuType == _paramType.BulkBookingCheck) {
                fieldList = ["YarnCategory", "Distribution", "YarnPly", "Spinner", "PhysicalLot", "StitchLength", "GreyAllowance",
                    "YD", "YDAllowance", "Allowance", "NetYarnReqQty", "GreyYarnUtilizationQty", "DyedYarnUtilizationQty", "Remarks",
                    "YarnLotNo", "Spinner", "SpinnerId"];
            }
            else if (menuType == _paramType.BulkBookingApprove) {
                fieldList = ["YarnCategory", "Distribution", "YarnPly", "Spinner", "PhysicalLot", "StitchLength", "GreyAllowance",
                    "YD", "YDAllowance", "Allowance", "NetYarnReqQty", "GreyYarnUtilizationQty", "DyedYarnUtilizationQty", "Remarks",
                    "YarnLotNo", "Spinner", "SpinnerId"];
            }
            if (fieldList.length > 0) {
                var indexFN = fieldList.findIndex(x => x == fieldName);
                if (indexFN > -1) return true;
                else return false;
            }
        }
        return true;
    }
    function isColumnFilteredMenu() {
        if (menuType == _paramType.BulkBookingYarnAllowance ||
            menuType == _paramType.BulkBookingCheck ||
            menuType == _paramType.BulkBookingApprove) return true;
        return false;
    }
    function setVisiblePropValue(columns, subGroupId) {
        if (isColumnFilteredMenu()) {
            var validColumns = DeepClone(columns.filter(x => x.visible == true));
            var idText = "ID";
            validColumns.filter(c => typeof c.field !== "undefined").map(c => {
                if (!c.field.toUpperCase().includes(idText) || c.field == "BrandID") {
                    var field = c.field;
                    if (subGroupId == 1) {
                        c.visible = isFieldShowFabric(field);
                    } else {
                        c.visible = isFieldShowCollarCuff(field);
                    }
                    var countField = columns.filter(x => x.field == field).length;
                    if (countField == 1) {
                        var indexF = columns.findIndex(x => x.field == field);
                        if (indexF > -1) {
                            columns[indexF].visible = c.visible;
                        }
                    }
                    else if (countField > 1) {
                        var indexList = [];
                        $.each(columns, function (index, item) {
                            if (item.field == field) {
                                indexList.push(index);
                            }
                        });
                        indexList.map(i => {
                            columns[i].visible = c.visible;
                        });
                    }
                }
            });

            validColumns.filter(c => typeof c.propField !== "undefined").map(c => {

                var field = c.propField;
                if (subGroupId == 1) {
                    c.visible = isFieldShowFabric(field);
                } else {
                    c.visible = isFieldShowCollarCuff(field);
                }

                var countField = columns.filter(x => x.propField == field).length;
                if (countField == 1) {
                    var indexF = columns.findIndex(x => x.propField == field);
                    if (indexF > -1) {
                        columns[indexF].visible = c.visible;
                    }
                }
                else if (countField > 1) {
                    var indexList = [];
                    $.each(columns, function (index, item) {
                        if (item.propField == field) {
                            indexList.push(index);
                        }
                    });
                    indexList.map(i => {
                        columns[i].visible = c.visible;
                    });
                }
            });
        }
        return columns;
    }

    function setVisiblePropValueYarn(columns) {
        if (isColumnFilteredMenu()) {
            var validColumns = DeepClone(columns.filter(x => x.visible != false));
            var idText = "ID";
            validColumns.filter(c => typeof c.field !== "undefined").map(c => {
                var field = c.field;
                c.visible = isFieldShowYarn(field);

                var countField = columns.filter(x => x.field == field).length;
                if (countField == 1) {
                    var indexF = columns.findIndex(x => x.field == field);
                    if (indexF > -1) {
                        columns[indexF].visible = c.visible;
                    }
                }
                else if (countField > 1) {
                    var indexList = [];
                    $.each(columns, function (index, item) {
                        if (item.field == field) {
                            indexList.push(index);
                        }
                    });
                    indexList.map(i => {
                        columns[i].visible = c.visible;
                    });
                }
            });

            validColumns.filter(c => typeof c.propField !== "undefined").map(c => {
                var field = c.propField;
                c.visible = isFieldShowYarn(field);

                var countField = columns.filter(x => x.propField == field).length;
                if (countField == 1) {
                    var indexF = columns.findIndex(x => x.propField == field);
                    if (indexF > -1) {
                        columns[indexF].visible = c.visible;
                    }
                }
                else if (countField > 1) {
                    var indexList = [];
                    $.each(columns, function (index, item) {
                        if (item.propField == field) {
                            indexList.push(index);
                        }
                    });
                    indexList.map(i => {
                        columns[i].visible = c.visible;
                    });
                }
            });
        }
        return columns;
    }

    function FBookingAckRevisionPendingValidation(BookingNo, ExportOrderID) {
        return new Promise(function (resolve, reject) {
            _isFirstLoad = true;

            axios.get(`/api/bds-acknowledge/ForAckRevisionPendingValidation/${BookingNo}/${ExportOrderID}`)
                .then(function (response) {
                    if (response.data !== "") {

                        reject(response.data);
                    } else {

                        resolve(); // Resolve the promise if validation succeeds
                    }
                })
                .catch(function (error) {

                    showResponseError(error);
                    reject(error); // Reject the promise with the error
                });
        });
    }


    function isValidAllYarnItems(yarnItems) {
        var hasError2 = false;
        for (var iY = 0; iY < yarnItems.length; iY++) {
            hasError2 = isValidYarnItemSingle(yarnItems[iY]);
            if (hasError2) break;
        }
        return hasError2;
    }
    function isValidYarnItemSingle(yarnItem) {
        yarnItem.Segment1ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment1ValueId);
        yarnItem.Segment2ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment2ValueId);
        yarnItem.Segment3ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment3ValueId);
        yarnItem.Segment4ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment4ValueId);
        yarnItem.Segment5ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment5ValueId);
        yarnItem.Segment6ValueId = getDefaultValueWhenInvalidN(yarnItem.Segment6ValueId);

        yarnItem.Segment5ValueDesc = getDefaultValueWhenInvalidS(yarnItem.Segment5ValueDesc);
        yarnItem.ShadeCode = getDefaultValueWhenInvalidS(yarnItem.ShadeCode);

        if (yarnItem.Segment1ValueId == 0) {
            toastr.error("Select composition");
            return true;
        }
        if (yarnItem.Segment2ValueId == 0) {
            toastr.error("Select yarn type");
            return true;
        }
        if (yarnItem.Segment3ValueId == 0) {
            toastr.error("Select manufacturing process");
            return true;
        }
        if (yarnItem.Segment4ValueId == 0) {
            toastr.error("Select sub process");
            return true;
        }
        if (yarnItem.Segment5ValueId == 0) {
            toastr.error("Select quality parameter");
            return true;
        }
        if (yarnItem.Segment6ValueId == 0) {
            toastr.error("Select count");
            return true;
        }
        if ((yarnItem.Segment5ValueDesc.toLowerCase() == "melange" || yarnItem.Segment5ValueDesc.toLowerCase() == "color melange") && (yarnItem.ShadeCode == "")) {
            toastr.error("Select shade code for color melange");
            return true;
        }
        return false;
    }
    function loadFCCItem(subGroupId) {
        var modalObj = ch_GenerateBasicModal($formEl, true, "btnOk_FCCItem");
        var $tblTempEl;

        var unit = "KG";
        if (subGroupId != 1) unit = "PCS";

        var columnList = [
            { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
            { field: 'ConceptTypeID', visible: false },
            {
                field: 'MachineType', headerText: 'Machine Type', width: 80, allowEditing: false
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name', width: 80, allowEditing: false
            },
            {
                field: 'ExistingYarnAllowance', headerText: 'Existing Yarn Allowance', allowEditing: false,
            },
            {
                field: 'YarnAllowance', headerText: 'Add. Yarn Allowance', allowEditing: false,
            },
            {
                field: 'TotalYarnAllowance', headerText: 'Total Yarn Allowance', allowEditing: false,
            },
            {
                field: 'MachineGauge', headerText: 'Gauge', width: 80, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } }
            },
            {
                field: 'MachineDia', headerText: 'Dia', width: 80, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } }
            },
            {
                field: 'Brand', headerText: 'Brand', allowEditing: false
            },
            {
                field: 'Construction', headerText: 'Construction', allowEditing: false
            },
            {
                field: 'Composition', headerText: 'Composition', allowEditing: false
            },
            {
                field: 'Color', headerText: 'Color', width: 85, allowEditing: false
            },
            {
                field: 'GSM', headerText: 'GSM', width: 85, allowEditing: false
            },
            {
                field: 'FabricWidth', headerText: 'Fabric Width', width: 85, allowEditing: false
            },
            {
                field: 'KnittingType', headerText: 'Knitting Type', width: 85, allowEditing: false
            },
            {
                field: 'YarnType', headerText: 'Yarn Type', width: 85, allowEditing: false
            },
            {
                field: 'YarnProgram', headerText: 'Yarn Program', width: 85, allowEditing: false
            },
            {
                field: 'DyeingType', headerText: 'Dyeing Type', width: 85, allowEditing: false
            },
            {
                field: 'Instruction', headerText: 'Instruction', allowEditing: false
            },
            {
                field: 'LabDipNo', headerText: 'Lab Dip No', allowEditing: false
            },
            {
                field: 'RefSourceNo', headerText: 'Ref No', width: 85, allowEditing: false
            },
            {
                field: 'ActualBookingQty', headerText: 'Booking Qty(' + unit + ')', width: 85, allowEditing: false
            },
            {
                field: 'BookingQty', headerText: 'Replacement Qty(' + unit + ')', width: 120, allowEditing: false
            },
            {
                field: 'FinishFabricUtilizationQty', headerText: 'Finish Fabric Utilization Qty', width: 120, propField: 'FinishFabricUtilizationQty', allowEditing: false
            },
            {
                field: 'ReqFinishFabricQty', headerText: 'Req. Finish Fabric Qty', width: 120, allowEditing: false
            },
            {
                field: 'TotalQty', headerText: 'Total Qty', width: 85, allowEditing: false, visible: status == false
            },
            {
                field: 'GreyReqQty', headerText: 'Grey Req Qty', width: 85, allowEditing: false
            },
            {
                field: 'GreyLeftOverQty', headerText: 'Grey Utilization Qty', width: 85, propField: 'GreyLeftOverQty', allowEditing: false
            },
            {
                field: 'GreyProdQty', headerText: 'Grey Prod Qty', width: 95, allowEditing: false, visible: isBulkBookingKnittingInfoMenu() || isAdditionBulkBooking()
            }
        ];

        var dataList = [];
        if (subGroupId == 1) dataList = masterData.FBookingChild;
        else if (subGroupId == 11) dataList = masterData.FBookingChildCollor;
        else if (subGroupId == 12) dataList = masterData.FBookingChildCuff;

        columnList.unshift({ type: 'checkbox', width: 50 });

        if ($tblTempEl) $tblTempEl.destroy();
        $tblTempEl = new initEJ2Grid({
            tableId: modalObj.modalTableId,
            autofitColumns: true,
            data: dataList,
            columns: columnList,
            allowSorting: true,
            editSettings: {
                allowAdding: false,
                allowEditing: false,
                allowDeleting: false,
                mode: "Normal"
            },
        });

        $formEl.find("#btnOk_FCCItem").click(function () {
            var selectedItems = $tblTempEl.getSelectedRecords();
            if (selectedItems.length == 0) {
                toastr.error("Please select row(s)!");
                return;
            }
            var itemList = [];
            if (subGroupId == 1) {
                //$tblChildEl
                selectedItems.map(x => {
                    var item = masterData.FBookingChild.find(y => y.BookingChildID == x.BookingChildID);
                    itemList.push(DeepClone(item));
                });
                initChild(itemList, false);
            }
            else if (subGroupId == 11) {
                //$tblChildCollarIdEl
                selectedItems.map(x => {
                    var item = masterData.FBookingChildCollor.find(y => y.BookingChildID == x.BookingChildID);
                    itemList.push(DeepClone(item));
                });
                initChildCollar(itemList, false);
            }
            else if (subGroupId == 12) {
                //$tblChildCuffIdEl
                selectedItems.map(x => {
                    var item = masterData.FBookingChildCuff.find(y => y.BookingChildID == x.BookingChildID);
                    itemList.push(DeepClone(item));
                });
                initChildCuff(itemList, false);
            }
            $(modalObj.modalId).modal('hide');
        });
    }
    function getSelectedItems(subGroupId) {
        var items = [];
        var itemList = [];

        if (subGroupId == 1) {
            if (isAdditionBulkBooking()) {
                var items = $tblChildEl.getCurrentViewRecords();
                items.map(x => {
                    var item = masterData.FBookingChild.find(y => y.BookingChildID == x.BookingChildID);
                    item.ExistingYarnAllowance = x.ExistingYarnAllowance;
                    item.YarnAllowance = x.YarnAllowance;
                    item.TotalYarnAllowance = x.TotalYarnAllowance;
                    item.ChildItems.forEach(y => {
                        y.YD = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).YD;
                        y.YDItem = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).YDItem;
                        y.Allowance = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).Allowance;
                        y.GreyAllowance = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).GreyAllowance;
                        y.YDAllowance = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).YDAllowance;
                    });
                    itemList.push(item);
                });
                return itemList;
            }
            else {
                return masterData.FBookingChild;
            }
        }
        else if (subGroupId == 11) {
            if (isAdditionBulkBooking()) {
                var items = $tblChildCollarIdEl.getCurrentViewRecords();
                var itemList = [];
                items.map(x => {
                    var item = masterData.FBookingChildCollor.find(y => y.BookingChildID == x.BookingChildID);
                    item.ExistingYarnAllowance = x.ExistingYarnAllowance;
                    item.YarnAllowance = x.YarnAllowance;
                    item.TotalYarnAllowance = x.TotalYarnAllowance;
                    item.ChildItems.forEach(y => {
                        y.YD = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).YD;
                        y.YDItem = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).YDItem;
                        y.Allowance = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).Allowance;
                        y.GreyAllowance = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).GreyAllowance;
                        y.YDAllowance = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).YDAllowance;
                    });
                    itemList.push(item);
                });
                return itemList;
            } else {
                return masterData.FBookingChildCollor;
            }
        }
        else if (subGroupId == 12) {

            if (isAdditionBulkBooking()) {
                var items = $tblChildCuffIdEl.getCurrentViewRecords();
                var itemList = [];
                items.map(x => {
                    var item = masterData.FBookingChildCuff.find(y => y.BookingChildID == x.BookingChildID);
                    item.ExistingYarnAllowance = x.ExistingYarnAllowance;
                    item.YarnAllowance = x.YarnAllowance;
                    item.TotalYarnAllowance = x.TotalYarnAllowance;
                    item.ChildItems.forEach(y => {
                        y.YD = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).YD;
                        y.YDItem = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).YDItem;
                        y.Allowance = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).Allowance;
                        y.GreyAllowance = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).GreyAllowance;
                        y.YDAllowance = x.ChildItems.find(ci => ci.YBChildItemID == y.YBChildItemID).YDAllowance;
                    });
                    itemList.push(item);
                });
                return itemList;
            } else {
                return masterData.FBookingChildCuff;
            }
        }
        return [];
    }
    function GetCalculatedFBookingChildCollor(FBookingChildCollor, isDoCalculateFields) {

        FBookingChildCollor.forEach(x => {

            if (menuType == _paramType.AdditionalYarnBooking && status == statusConstants.APPROVED2) {
                //if (_isFirstLoad) {
                //    x.YarnAllowance = 0;
                //    x.BookingQty = 0;
                //    x.BookingQtyKG = 0;
                //    x = setAdditionalAllowance(x);
                //}
                if (typeof x.FinishFabricUtilizationQty == 'undefined' || x.FinishFabricUtilizationQty == null) {
                    x.FinishFabricUtilizationQty = 0;
                }
                if (typeof x.GreyLeftOverQty == 'undefined' || x.GreyLeftOverQty == null) {
                    x.GreyLeftOverQty = 0;
                }
            }
            x.ReqFinishFabricQty = x.BookingQtyKG - x.FinishFabricUtilizationQty;
            x = setBookingQtyKGRelatedFieldsValue(x, 11);
            x.ChildItems.forEach(y => {
                if (typeof x.GreyProdQty != 'undefined' && x.GreyProdQty != null && typeof y.Distribution != 'undefined' && y.Distribution != null && typeof x.YarnAllowance != 'undefined' && x.YarnAllowance != null) {
                    y.YarnReqQty = (x.GreyProdQty * (y.Distribution / 100)) / (1 + (x.YarnAllowance / 100) - (0.5 / 100));
                    if (isAdditionBulkBooking() && x.IsForFabric == false) {
                        y.YarnReqQty = y.NetYarnReqQty;
                    }
                    y.YarnReqQty = parseFloat(y.YarnReqQty).toFixed(2);
                    //y.GreyYarnUtilizationQty = 0;
                    //y.DyedYarnUtilizationQty = 0;
                    if (typeof y.GreyAllowance == 'undefined' || y.GreyAllowance == null) {
                        y.GreyAllowance = 0;
                    }
                    if (typeof y.YDAllowance == 'undefined' || y.YDAllowance == null) {
                        y.YDAllowance = 0;
                    }
                    if (isAdditionBulkBooking() && x.IsForFabric == false) {
                        y = getYarnRelatedPropsAdditionalYarn(y, x, false, isDoCalculateFields);
                    }
                    else {
                        y = getYarnRelatedProps(y, x, false, isDoCalculateFields);
                    }
                }
            });
        });
        FBookingChildCollor = setCalculatedValues(FBookingChildCollor);
    }
    function GetCalculatedFBookingChildCuff(FBookingChildCuff, isDoCalculateFields) {
        FBookingChildCuff.forEach(x => {

            if (menuType == _paramType.AdditionalYarnBooking && status == statusConstants.APPROVED2) {
                //if (_isFirstLoad) {
                //    x.YarnAllowance = 0;
                //    x.BookingQty = 0;
                //    x.BookingQtyKG = 0;
                //    x = setAdditionalAllowance(x);
                //}
                if (typeof x.FinishFabricUtilizationQty == 'undefined' || x.FinishFabricUtilizationQty == null) {
                    x.FinishFabricUtilizationQty = 0;
                }
                if (typeof x.GreyLeftOverQty == 'undefined' || x.GreyLeftOverQty == null) {
                    x.GreyLeftOverQty = 0;
                }
            }
            x.ReqFinishFabricQty = x.BookingQtyKG - x.FinishFabricUtilizationQty;
            x = setBookingQtyKGRelatedFieldsValue(x, 12);
            x.ChildItems.forEach(y => {
                if (typeof x.GreyProdQty != 'undefined' && x.GreyProdQty != null && typeof y.Distribution != 'undefined' && y.Distribution != null && typeof x.YarnAllowance != 'undefined' && x.YarnAllowance != null) {
                    y.YarnReqQty = (x.GreyProdQty * (y.Distribution / 100)) / (1 + (x.YarnAllowance / 100) - (0.5 / 100));
                    if (isAdditionBulkBooking() && x.IsForFabric == false) {
                        y.YarnReqQty = y.NetYarnReqQty;
                    }
                    y.YarnReqQty = parseFloat(y.YarnReqQty).toFixed(2);
                    //y.GreyYarnUtilizationQty = 0;
                    //y.DyedYarnUtilizationQty = 0;
                    if (typeof y.GreyAllowance == 'undefined' || y.GreyAllowance == null) {
                        y.GreyAllowance = 0;
                    }
                    if (typeof y.YDAllowance == 'undefined' || y.YDAllowance == null) {
                        y.YDAllowance = 0;
                    }
                    if (isAdditionBulkBooking() && x.IsForFabric == false) {
                        y = getYarnRelatedPropsAdditionalYarn(y, x, false, isDoCalculateFields);
                    }
                    else {
                        y = getYarnRelatedProps(y, x, false, isDoCalculateFields);
                    }
                }

            });
        });

        FBookingChildCuff = setCalculatedValues(FBookingChildCuff);
    }
    function GetQtyFromPer(bookingQty, distributionPer, allowancePer) {
        var PLoss = 0.50;
        var YarnReqQty = (bookingQty * (distributionPer / 100)) / (1 + (allowancePer / 100) - (PLoss / 100));
        return YarnReqQty;
    }
})();