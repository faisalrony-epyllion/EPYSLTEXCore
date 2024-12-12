
(function () {
    'use strict'
    var masterData = {}, currentChildRowData = null;
    var menuId, pageName;
    var toolbarId, pageId, pageIdWithHash;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var status;
    var prMasterId, companyId;
    var childData;
    var hasChildCommands = true;
    var selectedIds;
    var isApprovePage = false;
    var _poForList = [];
    var _yarnSegments = [];
    var _ignoreValidationPOIds = [];
    var _ypoMasterID = 0;
    var _draftEditing = false;
    var StatusPIPO = "";
    var _maxYPOChildID = 9999999;

    var validationConstraints = {
        SupplierId: {
            presence: true
        },
        IncoTermsId: {
            presence: true
        }
    }
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

        pageIdWithHash = "#" + pageId;
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        //$divDetailsEl.find("#CompanyId").prop("disabled", true);
        if (pageName == "YarnPOApprovalV2") {
            status = statusConstants.PROPOSED; // Peding for approval
            initMasterTable();
            $toolbarEl.find("#btnYPONew").hide();
            $toolbarEl.find("#btnPendingList").hide();
            $toolbarEl.find("#btnAwaitingProposeList").hide();
            $toolbarEl.find("#btnRevisionList").hide();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingApprovalList"), $toolbarEl);
            $toolbarEl.find("#btnAddPR").fadeOut();
            $toolbarEl.find("#btnSaveYPOShip").hide();
            //$formEl.find("#RevisionArea").fadeOut();
            //$formEl.find("#divTermsCondition").fadeOut();
        } else {
            status = statusConstants.PENDING;
            initMasterTable();
            $toolbarEl.find("#btnYPONew").show();
            $toolbarEl.find("#btnPendingList").show();
            $toolbarEl.find("#btnAwaitingProposeList").show();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);
            $toolbarEl.find("#btnAddPR").fadeIn();
            $toolbarEl.find("#btnSaveYPOShip").show();
        }

        $toolbarEl.find("#btnYPONew").click(btnNewClick);

        $toolbarEl.find("#btnPendingList").click(btnPendingListClick);

        $toolbarEl.find("#btnAwaitingProposeList").click(btnAwaitingProposeListClick);

        $toolbarEl.find("#btnPendingApprovalList").click(btnPendingApprovalListClick);

        $toolbarEl.find("#btnApprovedList").click(btnApprovedListClick);

        $toolbarEl.find("#btnRevisionList").click(btnRevisionListClick);

        $toolbarEl.find("#btnRejectList").click(btnRejectListClick);
        $toolbarEl.find("#btnCancelList").click(btnCancelListClick);

        $toolbarEl.find("#btnAllList").click(btnAllListClick);

        $formEl.find('#IsRevision').click(function () {
            if ($(this).is(':checked')) {
                controlReset(true);
                if (pageName == "YarnPOV2") {
                    initChildTable(masterData.YarnPOChilds);
                }
            } else {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnPropose").fadeOut();
                $formEl.find("#btnSaveYPO").fadeOut();
                $formEl.find("#btnSaveAndProposeYPO").fadeOut();
            }

            addPRItemBtnHideShow();
        });

        $formEl.find('#IsRevision').change(function () {
            if ($(this).is(':checked')) {
                controlReset(true);
            }

            addPRItemBtnHideShow();
        });
        $formEl.find('#IsCancel').click(function () {
            if (StatusPIPO != "") {
                $formEl.find("#IsCancel").prop("checked", false);
                toastr.error(StatusPIPO);
                return false;
            }
            if ($(this).is(':checked')) {
                controlReset(false);
                $formEl.find("#btnSaveAndProposeYPO").fadeOut();
            } else {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnPropose").fadeOut();
                $formEl.find("#btnSaveYPO").fadeOut();
                $formEl.find("#btnSaveAndProposeYPO").fadeOut();
            }
        });
        $formEl.find('#IsCancel').change(function () {
            if ($(this).is(':checked')) {
                controlReset(false);
            }
        });
        function controlReset(revision) {
            if (revision) {
                $formEl.find("#IsCancel").prop("checked", false);
                $formEl.find("#RevisionReason").fadeIn();
                $formEl.find("#lblRevisionReason").fadeIn();
                $formEl.find("#CancelReason").fadeOut();
                $formEl.find("#lblCancelReason").fadeOut();
                $formEl.find("#btnSaveYPOShip").fadeOut();
            } else {
                $formEl.find("#IsRevision").prop("checked", false);
                $formEl.find("#RevisionReason").fadeOut();
                $formEl.find("#lblRevisionReason").fadeOut();
                $formEl.find("#CancelReason").fadeIn();
                $formEl.find("#lblCancelReason").fadeIn();
            }
            $formEl.find("#btnSaveYPO").fadeIn();
            $formEl.find("#btnSaveYPOShip").fadeOut();
            $formEl.find("#btnSaveAndProposeYPO").fadeIn();
        }
        $formEl.find("#SupplierId").on("select2:select", function (e) {
            if (masterData.IsYarnReceived) {
                toastr.error('Already yarn received, supplier cannot be changed.');
                $formEl.find("#SupplierId").val(masterData.SupplierId).trigger('change');
                return false;
            }

            //var isRevise = $formEl.find('#IsRevision').is(":checked");
            //if (isRevise && masterData.IsReceivedPO) {

            //    toastr.error("Can't change supplier as PO already received!");
            //    return false;
            //}

            var url = `/api/ypo/get-supplier-info/${e.params.data.id}`;
            axios.get(url)
                .then(function (response) {
                    

                    response.data.PRMasterID = masterData.PRMasterID;
                    response.data.PRDate = masterData.PRDate;
                    response.data.PRNO = masterData.PRNO;
                    response.data.Remarks = masterData.Remarks;
                    response.data.CompanyId = masterData.CompanyId;
                    response.data.CompanyName = masterData.CompanyName;
                    response.data.YarnPOChilds = masterData.YarnPOChilds;
                    setPOForItems(masterData.YarnPOChilds);
                    response.data.SupplierList = masterData.SupplierList;
                    response.data.BuyerList = masterData.BuyerList;
                    response.data.YarnPOChildOrders = masterData.YarnPOChildOrders;
                    response.data.PIForList = masterData.PIForList;
                    response.data.ConceptNo = masterData.ConceptNo;
                    $formEl.find("#SupplierId").val(e.params.data.id);

                    var yarnPOChilds = [];
                    if (typeof $tblChildEl != 'undefined') {
                        yarnPOChilds = $tblChildEl.getCurrentViewRecords();
                    }
                    yarnPOChilds.map(x => {
                        //x.ShadeCode = "";
                        x = setYarnSegDesc(x);
                    });
                    initChildTable(yarnPOChilds);

                    masterData = response.data;
                    masterData.PoDate = formatDateToDefault(masterData.PoDate);

                    if (response.data.InLand == false) {
                        masterData.DeliveryStartDate = formatDateToDefault($("#DeliveryStartDate").val(masterData.DeliveryStartDate));
                        masterData.DeliveryEndDate = formatDateToDefault($("#DeliveryEndDate").val(masterData.DeliveryEndDate));
                        $("#DeliveryEndDateLeb").empty();
                        $("#DeliveryEndDateLeb").append("ETD");
                        $("#YarnInhouseDateLeb").empty();
                        $("#YarnInhouseDateLeb").append("ETA");
                    } else {
                        masterData.DeliveryStartDate = formatDateToDefault($("#DeliveryStartDate").val(masterData.DeliveryStartDate));
                        masterData.DeliveryEndDate = formatDateToDefault($("#DeliveryEndDate").val(masterData.DeliveryEndDate));
                        $("#DeliveryEndDateLeb").empty();
                        $("#DeliveryEndDateLeb").append("Delivery End Date");
                        $("#YarnInhouseDateLeb").empty();
                        $("#YarnInhouseDateLeb").append("Yarn Inhouse Date");
                    }

                    masterData.QuotationRefDate = formatDateToDefault(masterData.QuotationRefDate);
                    masterData.InHouseDate = formatDateToDefault(masterData.InHouseDate);
                    setFormData($formEl, masterData);

                    $("#PODateCurrent").text(masterData.DeliveryStartDate);
                    $pageEl.find("#SFToPLDate").text(formatDateToDefault(masterData.SFToPLDate));
                    $pageEl.find("#SFToPLDays").text(masterData.SFToPLDays);
                    $pageEl.find("#PLToPDDate").text(formatDateToDefault(masterData.PLToPDDate));
                    $pageEl.find("#PLToPDDays").text(masterData.PLToPDDays);
                    $pageEl.find("#PDToCFDate").text(formatDateToDefault(masterData.PDToCFDate));
                    $pageEl.find("#PDToCFDays").text(masterData.PDToCFDays);
                    $pageEl.find("#InHouseDays").text(masterData.InHouseDays);
                    tollerance = (masterData.ShippingTolerance)
                })
                .catch(showResponseError)
        });

        $formEl.find("#PaymentTermsId").on("select2:select", function (e) {
            if (e.params.data.id == "1") showHideLCSection(false);
            else showHideLCSection(true);
        });

        $formEl.find("#TypeOfLcId").on("select2:select", function (e) {
            if (e.params.data.id == "1") $formEl.find("#formGroupCreditDays").fadeOut();
            else $formEl.find("#formGroupCreditDays").fadeIn();
        });

        $formEl.find('#DeliveryStartDate').datepicker()
            .on('changeDate', function (ev) {
                //startDate = new Date(ev.date.getFullYear(), ev.date.getMonth(), ev.date.getDate(), 0, 0, 0);
                if (new Date($formEl.find('#PoDate').val()) != null && new Date($formEl.find('#PoDate').val()) != 'undefined') {
                    if (new Date($formEl.find('#DeliveryStartDate').val()) < new Date($formEl.find('#PoDate').val())) {
                        bootbox.alert({
                            size: "small",
                            title: "Alert !!!",
                            message: "Start Date can't less than PO Date.",
                            callback: function () {
                                $formEl.find("#DeliveryStartDate").val("");
                            }
                        })
                    }
                }
                if (new Date($formEl.find('#DeliveryEndDate').val()) != null && new Date($formEl.find('#DeliveryEndDate').val()) != 'undefined') {
                    if (new Date($formEl.find('#DeliveryEndDate').val()) < new Date($formEl.find('#DeliveryStartDate').val())) {
                        bootbox.alert({
                            size: "small",
                            title: "Alert !!!",
                            message: "End Date can't less than Start Date.",
                            callback: function () {
                                $formEl.find("#DeliveryStartDate").val("");
                            }
                        })
                    }
                }
            });

        $formEl.find("#DeliveryEndDate").datepicker()
            .on("changeDate", function (ev) {
                //endDate = new Date(ev.date.getFullYear(), ev.date.getMonth(), ev.date.getDate(), 0, 0, 0);
                if (new Date($formEl.find('#DeliveryStartDate').val()) != null && new Date($formEl.find('#DeliveryStartDate').val()) != 'undefined') {
                    if (new Date($formEl.find('#DeliveryEndDate').val()) < new Date($formEl.find('#DeliveryStartDate').val())) {
                        bootbox.alert({
                            size: "small",
                            title: "Alert !!!",
                            message: "End Date can't less than Start Date.",
                            callback: function () {
                                $formEl.find("#DeliveryEndDate").val("");
                            }
                        })
                    }
                }
            });

        $formEl.find("#btnViewDetailsTNA").click(function (e) {
            e.preventDefault();
            $formEl.find("#modal-child-Yarn-TNA").modal('show');
        });

        $formEl.find("#btnEditCancelYarnPO").click(function (e) {
            e.preventDefault();
            backToList();
        });

        $formEl.find("#btnSaveYPO").click(function (e) {
            e.preventDefault();
            saveYPO(false);
        });

        $formEl.find("#btnSaveAndProposeYPO").click(function (e) {
            e.preventDefault();
            saveYPO(true);
        });
        $formEl.find("#btnSaveYPOShip").click(function (e) {
            e.preventDefault();
            saveYPO(true);
        });

        $formEl.find("#btnApproveYPO").click(function (e) {
            e.preventDefault();

            var url = "/api/ypo/approve-ypo/" + $formEl.find("#YPOMasterID").val();
            axios.post(url)
                .then(function () {
                    toastr.success(constants.PROPOSE_SUCCESSFULLY);
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        });

        $formEl.find("#btnRejectYPO").click(function (e) {
            e.preventDefault();

            showBootboxPrompt("Reject Yarn PO", "Are you sure you want to Reject this PO?", function (result) {
                if (result) {
                    var data = {
                        YPOMasterID: $formEl.find("#YPOMasterID").val(),
                        UnapproveReason: result
                    };

                    axios.post("/api/ypo/reject-ypo", data)
                        .then(function () {
                            toastr.success(constants.REJECT_SUCCESSFULLY);
                            backToList();
                        })
                        .catch(function (error) {
                            toastr.error(error.response.data.Message);
                        });
                }

            });
        });

        $formEl.find("#btnRevisionYPO").click(function (e) {
            e.preventDefault();

            $formEl.find(':checkbox').each(function () {
                this.value = this.checked;
            });

            var data = formDataToJson($formEl.serializeArray());
            if (!data.CompanyId) data.CompanyId = companyId;

            if (status == "addNewItem")
                data.IsItemGenerate = true;

            data.YarnPOChilds = $tblChildEl.getCurrentViewRecords();

            //data.Proposed = isPropose ? true : false;
            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);

            var isValidItemInfo = false;
            $.each(data.YarnPOChilds, function (i, el) {
                if (el.HSCode == "" || el.HSCode == null) {
                    toastr.error("HS Code must be entry");
                    isValidItemInfo = true;
                }
                else if (el.PoQty == "" || el.PoQty == null || el.PoQty <= 0) {
                    toastr.error("Yarn PO Qty must be greater than zero");
                    isValidItemInfo = true;
                }
                else if (el.Rate == "" || el.Rate == null || el.Rate <= 0) {
                    toastr.error("Yarn Rate must be greater than zero");
                    isValidItemInfo = true;
                }
            });

            if (isValidItemInfo) return;

            //var url = "/api/ypov2/revision-ypo/" + $formEl.find("#YPOMasterID").val();
            //axios.post(url)
            axios.post("/api/ypo/revision-ypo", data)
                .then(function () {
                    toastr.success("Revision Successfylly!");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        });

        $formEl.find("#btnAddChild").click(addNewItem);
        $formEl.find("#btnAddFromPRChild").click(addFromPRChild);

        $toolbarEl.find("#btnAddPR").on("click", addPOFromPR);

        getYarnSegments();


        //$formEl.find("#btnAddChild").on("click", function (e) {
        //    e.preventDefault();
        //    var childIDs = $tblChildEl.getCurrentViewRecords().map(function (el) { return el.PRChildID }).toString();
        //    var finder = new commonFinder({
        //        title: "Select",
        //        pageId: pageId,
        //        height: 320,
        //        modalSize: "modal-lg",
        //        apiEndPoint: `/api/ypov2/pr-child-list?status=${status}&childIDs=${childIDs}&companyId=${masterData.CompanyId}`,
        //        fields: "YarnPRNo,Segment1ValueDesc,Segment2ValueDesc,Segment3ValueDesc,Segment4ValueDesc,Segment5ValueDesc,Segment6ValueDesc,Segment7ValueDesc,Segment8ValueDesc",
        //        headerTexts: "PR No,Composition,Yarn Type,Process,Sub Process,Quality Parameter,Shade,Count,No of Ply",
        //        isMultiselect: true,
        //        primaryKeyColumn: "PRChildID",
        //        onMultiselect: function (selectedRecords) {
        //            selectedRecords.forEach(function (value) {
        //                var exists = $tblChildEl.getCurrentViewRecords().find(function (el) { return el.PRChildID == value.PRChildID });
        //                if (!exists) $tblChildEl.getCurrentViewRecords().unshift(value);
        //            });
        //            initChildTable($tblChildEl.getCurrentViewRecords());
        //            $tblChildEl.refresh();
        //        }
        //    });

        //    finder.showModal();
        //});
    });

    async function getYarnSegments() {
        var response = await axios.get(getYarnItemsApiUrl([]));
        _yarnSegments = response.data;
    }

    function btnNewClick(e) {
        _draftEditing = false;
        _ypoMasterID = 0;
        status = "addNewItem";
        $divDetailsEl.find("#CompanyId").prop("disabled", false);
        $toolbarEl.find("#btnAddPR,#btnSaveYPOShip").fadeOut();
        $formEl.find("#RevisionArea").fadeOut();
        e.preventDefault();
        getNewData();
    }

    function btnPendingListClick(e) {
        e.preventDefault();
        _draftEditing = false;
        _ypoMasterID = 0;
        status = statusConstants.PENDING;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddPR").fadeIn();
        $divDetailsEl.find("#CompanyId").prop("disabled", false);
    }

    function btnAwaitingProposeListClick(e) {
        e.preventDefault();
        _draftEditing = true;
        _ypoMasterID = 0;
        status = statusConstants.AWAITING_PROPOSE;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddPR").fadeOut();
        $divDetailsEl.find("#CompanyId").prop("disabled", false);
    }

    function btnPendingApprovalListClick(e) {
        e.preventDefault();
        _draftEditing = false;
        _ypoMasterID = 0;
        status = statusConstants.PROPOSED;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddPR").fadeOut();
        $toolbarEl.find("#btnSaveYPOShip").fadeOut();

        $formEl.find("#divTermsCondition").fadeIn();
        $divDetailsEl.find("#CompanyId").prop("disabled", true);
    }

    function btnRevisionListClick(e) {
        e.preventDefault();
        _draftEditing = false;
        _ypoMasterID = 0;
        status = statusConstants.REVISE;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddPR").fadeOut();
        $divDetailsEl.find("#CompanyId").prop("disabled", true);

    }
    function btnApprovedListClick(e) {
        e.preventDefault();
        _draftEditing = false;
        _ypoMasterID = 0;
        status = statusConstants.APPROVED;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $toolbarEl.find("#btnAddPR").fadeOut();
        $toolbarEl.find("#btnSaveYPOShip").fadeOut();
        $formEl.find("#divTermsCondition").fadeIn();
        //
        if (pageName == "YarnPOV2") {
            $formEl.find("#RevisionArea").fadeIn();
            //$formEl.find("#divTermsCondition").fadeOut();
        }
        $divDetailsEl.find("#CompanyId").prop("disabled", true);

    }

    function btnRejectListClick(e) {
        e.preventDefault();
        _draftEditing = true;
        _ypoMasterID = 0;
        status = statusConstants.UN_APPROVE;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddPR").fadeOut();
        $toolbarEl.find("#btnSaveYPOShip").fadeOut();
        $divDetailsEl.find("#CompanyId").prop("disabled", true);
    }
    function btnCancelListClick(e) {
        e.preventDefault();
        _draftEditing = true;
        _ypoMasterID = 0;
        status = statusConstants.RETURN;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddPR").fadeOut();
        $toolbarEl.find("#btnSaveYPOShip").fadeOut();
        $divDetailsEl.find("#CompanyId").prop("disabled", true);
    }

    function btnAllListClick(e) {
        e.preventDefault();
        _draftEditing = false;
        _ypoMasterID = 0;
        status = statusConstants.ALL;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $toolbarEl.find("#btnSaveYPOShip").fadeIn();
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddPR").fadeOut();
        $divDetailsEl.find("#CompanyId").prop("disabled", true);
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#YPOMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        //initMasterTable();
        $tblMasterEl.refresh();
        showHideLCSection(false);
        showHideSupplierRegionSection(false);
        StatusPIPO = "";
        $toolbarEl.find("#btnAddPR").fadeIn();
    }

    function showHideLCSection(show) {
        if (show) {
            $formEl.find("#formGroupTypeOfLcId").show();
            $formEl.find("#formGroupCalculationofTenure").show();
        }
        else {
            $formEl.find("#formGroupTypeOfLcId").hide();
            $formEl.find("#formGroupCalculationofTenure").hide();
        }
    }

    function showHideSupplierRegionSection(show) {  // Supplier was local or foreign
        if (show) {
            $formEl.find("#formGroupPortofLoading").show();
            $formEl.find("#formGroupPortofDischarge").show();
            $formEl.find("#formGroupQuantityApprovalProcedure").show();
        }
        else {
            $formEl.find("#formGroupPortofLoading").hide();
            $formEl.find("#formGroupPortofDischarge").hide();
            $formEl.find("#formGroupQuantityApprovalProcedure").hide();
        }
    }

    function getNewData() {
        var url = "/api/ypo/new/";
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                
                masterData = response.data;
                masterData.PoDate = formatDateToDefault(masterData.PoDate);
                masterData.DeliveryStartDate = formatDateToDefault(masterData.DeliveryStartDate);
                masterData.DeliveryEndDate = formatDateToDefault(masterData.DeliveryEndDate);
                masterData.InHouseDate = formatDateToDefault(masterData.InHouseDate);
                masterData.QuotationRefDate = formatDateToDefault(masterData.QuotationRefDate);
                setFormData($formEl, masterData);
                initChildTable([]);
                if (status !== statusConstants.RETURN) {
                    $formEl.find("#btnSaveYPO").fadeIn();
                    $formEl.find("#btnSaveAndProposeYPO").fadeIn();
                }

                $formEl.find("#btnSaveYPOShip").fadeOut();
                HoldOn.close();
            })
            .catch(showResponseError)
    }

    function initMasterTable() {
        var commands = [];

        switch (status) {
            case statusConstants.PENDING:
                commands = [
                    { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }
                ]
                break;
            case statusConstants.AWAITING_PROPOSE:
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Propose', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
                break;
            case statusConstants.PROPOSED:
                if (pageName == "YarnPOApprovalV2") {
                    commands = [
                        { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'Approve', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                        { type: 'Reject', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-ban' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                } else {
                    commands = [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                }
                break;
            case statusConstants.APPROVED:
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
                break;
            case statusConstants.ALL:
                commands = [
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
                break;
            case statusConstants.UN_APPROVE:
                if (pageName == "YarnPOApprovalV2" || pageName == "YarnPOV2") {
                    commands = [
                        { type: 'Revision', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                } else {
                    commands = [
                        { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                }
                break;
            case statusConstants.RETURN:
                if (pageName == "YarnPOApprovalV2" || pageName == "YarnPOV2") {
                    commands = [
                        //{ type: 'Revision', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        //{ type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                } else {
                    commands = [
                        { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        //{ type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                }
                break;
            case statusConstants.REVISE:
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Propose', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
                break;
            default:
                break;
        }

        var columns = [
            {
                headerText: '', textAlign: 'Center', visible: status != statusConstants.PENDING, commands: commands,
                textAlign: 'Center', width: 100, minWidth: 100, maxWidth: 100
            },
            {
                field: 'PRNO', headerText: 'PR No', visible: status == statusConstants.PENDING
            },
            {
                field: 'PRDate', headerText: 'PR Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status == statusConstants.PENDING
            },
            {
                field: 'ConceptNo', headerText: 'Concept No', width: 150
            },
            {
                field: 'PRRequiredDate', headerText: 'PR Req. Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status == statusConstants.PENDING
            },
            {
                field: 'PRByUser', headerText: 'PR By', visible: status == statusConstants.PENDING
            },
            {
                field: 'DayValidDurationName', headerText: 'Yarn Sourcing Mode', visible: status == statusConstants.PENDING
            },
            {
                field: 'PoNo', headerText: 'PO No', visible: status != statusConstants.PENDING
            },
            {
                field: 'PoDate', headerText: 'PO Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'AddedByName', headerText: 'PO Created by', visible: status != statusConstants.PENDING
            },
            {
                field: 'RevisionNo', headerText: 'Revision No', textAlign: 'Center', visible: status == statusConstants.APPROVED || status == statusConstants.PROPOSED
            },
            {
                field: 'RevisionDate', headerText: 'Revision Date', textAlign: 'Center', type: 'date', format: _ch_date_format_7, visible: status == statusConstants.APPROVED || status == statusConstants.PROPOSED
            },
            {
                field: 'YarnPRChildID', visible: false
            },
            {
                field: 'CompanyId', visible: false
            },
            {
                field: 'CompanyName', headerText: 'Company'
            },
            {
                field: 'SupplierName', headerText: 'Supplier', visible: status != statusConstants.PENDING
            },
            {
                field: 'BuyerName', headerText: 'Buyer'
            },
            {
                field: 'QuotationRefNo', headerText: 'Ref No', visible: status != statusConstants.PENDING
            },
            {
                field: 'DeliveryStartDate', headerText: 'Delivery Start', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'DeliveryEndDate', headerText: 'Delivery End', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'POStatus', headerText: 'Status', visible: status != statusConstants.PENDING
            },
            {
                field: 'ReceivedCompleted', headerText: 'Shipment Status', displayAsCheckBox: true, visible: status == statusConstants.APPROVED
            },
            {
                field: 'InHouseDate', headerText: 'In-House Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'Segment1ValueDesc', headerText: 'Composition', visible: status == statusConstants.PENDING
            },
            {
                field: 'Segment2ValueDesc', headerText: 'Yarn Type', visible: status == statusConstants.PENDING
            },
            {
                field: 'Segment3ValueDesc', headerText: 'Process', visible: status == statusConstants.PENDING
            },
            {
                field: 'Segment4ValueDesc', headerText: 'Sub Process', visible: status == statusConstants.PENDING
            },
            {
                field: 'Segment5ValueDesc', headerText: 'Quality Parameter', visible: status == statusConstants.PENDING
            },
            {
                field: 'Segment6ValueDesc', headerText: 'Count', visible: status == statusConstants.PENDING
            },
            {
                field: 'Segment7ValueDesc', headerText: 'No of Poly', visible: status == statusConstants.PENDING
            },
            {
                field: 'ShadeCode', headerText: 'Shade Code', visible: status == statusConstants.PENDING
            },
            {
                field: 'ReqQty', headerText: 'Req Qty', visible: status == statusConstants.PENDING
            },
            {
                field: 'POQty', headerText: 'PO Qty', visible: status == statusConstants.PENDING
            },
            {
                field: 'BalanceQTY', headerText: 'Balance For PO QTY', visible: status == statusConstants.PENDING
            },
            {
                field: 'IsRevision', headerText: 'IsRevision', visible: false
            }
        ];

        if (status == statusConstants.PENDING) {
            columns.unshift({ type: 'checkbox', width: 50 });
            var selectionType = "Multiple";
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: status != statusConstants.PENDING,
            apiEndPoint: `/api/ypo/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands,
            aggregates: [
                {
                    columns: [
                        {
                            type: 'Sum',
                            field: 'ShadeCode',
                            footerTemplate: 'Total'
                        },
                        {
                            type: 'Sum',
                            field: 'ReqQty',
                            decimals: 2,
                            format: "N2",
                            footerTemplate: '${Sum}'
                        },
                        {
                            type: 'Sum',
                            field: 'POQty',
                            decimals: 2,
                            format: "N2",
                            footerTemplate: '${Sum}'
                        },
                        {
                            type: 'Sum',
                            field: 'BalanceQTY',
                            decimals: 2,
                            format: "N2",
                            footerTemplate: '${Sum}'
                        }
                    ]
                }
            ],
        });
    }

    function handleCommands(args) {
        
        hasChildCommands = args.commandColumn.type == 'Add' || args.commandColumn.type == 'Edit' || args.commandColumn.type == 'View';
        if (args.commandColumn.type == 'Add') {
            //addPOFromPR(args.rowData);
        } else if (args.commandColumn.type == 'Edit' || args.commandColumn.type == 'View') {
            getDetails(args.rowData);


            if (status == statusConstants.APPROVED && !isApprovePage) {
                $formEl.find("#btnSaveYPOShip").fadeOut();
            }
            else {
                $formEl.find("#btnSaveYPOShip").fadeOut();
            }
            //if (status == statusConstants.UN_APPROVE && !isApprovePage) {
            //    $formEl.find("#btnRevisionYPO").fadeIn();
            //}

        }
        else if (args.commandColumn.type == 'Propose') {
            proposePO(args.rowData);
        } else if (args.commandColumn.type == 'Approve') {
            approvePO(args.rowData);
        } else if (args.commandColumn.type == 'Reject') {
            rejectPO(args.rowData);
        }
        else if (args.commandColumn.type == 'Revision') {
            getDetails(args.rowData);
            if (status == statusConstants.UN_APPROVE && pageName == "YarnPOApprovalV2") {
                $formEl.find("#btnSaveYPOShip").fadeOut();
                $formEl.find("#btnRevisionYPO").fadeIn();
            }
            if (status == statusConstants.UN_APPROVE && pageName == "YarnPOV2") {
                $formEl.find("#btnSaveYPOShip").fadeOut();
                $formEl.find("#btnRevisionYPO").fadeIn();
            }
            //if (pageName == "YarnPOApprovalV2") {
            //    $formEl.find("#btnSaveYPO").fadeOut();
            //    $formEl.find("#btnSaveAndProposeYPO").fadeOut();
            //    $formEl.find("#btnApproveYPO").fadeOut();
            //    $formEl.find("#btnRejectYPO").fadeOut();
            //    $formEl.find("#btnSaveYPOShip").fadeOut();
            //    $formEl.find("#btnRevisionYPO").fadeIn();

            //}
        }
        else if (args.commandColumn.type == 'Report') {
            var a = document.createElement('a');
            a.href = "/reports/InlinePdfView?ReportName=YarnPOV2.rdl&PONo=" + args.rowData.PoNo;
            a.setAttribute('target', '_blank');
            a.setAttribute('title', 'Purchase Order Report');
            a.click();
        }
    }

    function addPOFromPR(row) {
        //Khan
        if ($tblMasterEl.getSelectedRecords().length == 0) {
            toastr.error("Please select row(s)!");
            return;
        }
        var uniqueAry = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "CompanyId");
        if (uniqueAry.length != 1) {
            toastr.error("Selected row(s) company name should be same!");
            return;
        }

        var prMasterIDs = $tblMasterEl.getSelectedRecords().map(function (el) {
            return el.PRMasterID
        }).toString();

        var yarnPRChildIDs = $tblMasterEl.getSelectedRecords().map(function (el) {
            return el.YarnPRChildID
        }).toString();

        var conceptNos = $tblMasterEl.getSelectedRecords().map(function (el) {
            return el.ConceptNo
        }).toString();

        //$("#PRMasterID").val(row.PRMasterID);
        //prMasterId = row.PRMasterID;
        companyId = uniqueAry[0].CompanyId;

        HoldOn.open({
            theme: "sk-circle"
        });
        var url = `/api/ypo/new/${prMasterIDs}/${yarnPRChildIDs}/${companyId}`;
        axios.get(url)
            .then(function (response) {

                addPRItemBtnHideShow();

                masterData = response.data;
                $divTblEl.fadeOut();
                if (status !== statusConstants.RETURN) {
                    $formEl.find("#btnSaveYPO").fadeIn();
                    $formEl.find("#btnSaveAndProposeYPO").fadeIn();
                    $formEl.find("#SupplierTNA").fadeIn();
                }
                $divDetailsEl.fadeIn();
                $formEl.find("#btnApproveYPO").fadeOut();
                $formEl.find("#btnRejectYPO").fadeOut();
                $formEl.find("#RevisionArea").fadeOut();
                $formEl.find("#btnSaveYPOShip").fadeOut();

                masterData.PoDate = formatDateToDefault(masterData.PoDate);
                masterData.DeliveryStartDate = formatDateToDefault(masterData.DeliveryStartDate);
                masterData.DeliveryEndDate = formatDateToDefault(masterData.DeliveryEndDate);
                masterData.QuotationRefDate = formatDateToDefault(masterData.QuotationRefDate);
                masterData.InHouseDate = formatDateToDefault(masterData.InHouseDate);
                masterData.ConceptNo = conceptNos;

                setFormData($formEl, masterData);
                companyId = masterData.CompanyId;
                $divDetailsEl.find("#CompanyId").prop("disabled", false);

                if (masterData.PaymentTermsId === 2) showHideLCSection(true);
                else showHideLCSection(false);

                if (masterData.PortofLoadingID === 105) showHideSupplierRegionSection(false);
                else showHideSupplierRegionSection(true);

                if (masterData.TypeOfLcId === 2) $formEl.find("#formGroupCreditDays").fadeIn();
                else $formEl.find("#formGroupCreditDays").fadeOut();

                $("#PODateCurrent").text(masterData.DeliveryStartDate);
                $pageEl.find("#SFToPLDate").text(formatDateToDefault(masterData.SFToPLDate));
                $pageEl.find("#SFToPLDays").text(masterData.SFToPLDays);
                $pageEl.find("#PLToPDDate").text(formatDateToDefault(masterData.PLToPDDate));
                $pageEl.find("#PLToPDDays").text(masterData.PLToPDDays);
                $pageEl.find("#PDToCFDate").text(formatDateToDefault(masterData.PDToCFDate));
                $pageEl.find("#PDToCFDays").text(masterData.PDToCFDays);
                $pageEl.find("#InHouseDays").text(masterData.InHouseDays);

                initChildTable(masterData.YarnPOChilds);

                HoldOn.close();
            })
            .catch(showResponseError)
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
    function getValidStringValue(value) {
        if (typeof value === "undefined" || value == null) return "";
        return value;
    }
    function getYarnCategory(obj) {

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
        return obj;
    }
    async function initChildTable(data) {
        data.filter(x => x.QuotationRefDate == null).map(x => {
            x.QuotationRefDate = Date.parse(new Date(), "dd/MM/yyyy");
        });
        var columns = [
            { field: 'YPOChildID', isPrimaryKey: true, visible: false },
            { field: 'PRChildID', visible: false }
        ];
        
        var isAllowEditing = false;
        if ($formEl.find('#RevisionArea').is(':visible') && $formEl.find('#IsRevision').is(':checked')) {
            isAllowEditing = true;
        }
        else if (_draftEditing == true) {
            isAllowEditing = true;
        }

        if (data.length > 0) {
            if (data[0].PRChildID > 0) {
                isAllowEditing = false;
            }
        }

        var itemColumns = [];
        
        if (isAllowEditing) {
            itemColumns = await getYarnItemColumnsWithSearchDDLAsync(ch_getCountRelatedList(data, 0), isAllowEditing);
        }
        else if (status != "addNewItem") {
            if (hasChildCommands) {
                columns.push({
                    headerText: 'Command', width: 100, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ]
                })
            }
            itemColumns = await getYarnItemColumnsForDisplayOnly(currentChildRowData);
        } else {
            if (hasChildCommands) {
                columns.push({
                    headerText: 'Command', width: 100, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ]
                })
            }
            itemColumns = await getYarnItemColumnsWithSearchDDLAsync(ch_getCountRelatedList(data, 0), isAllowEditing);
        }
        columns.push.apply(columns, itemColumns);
        columns.push.apply(columns, [
            { field: 'YarnPRNo', headerText: 'PR No', allowEditing: false, visible: status != "addNewItem" },
            { field: 'ConceptNo', headerText: 'Concept No', allowEditing: false, visible: status != "addNewItem" }
        ]);
        var contactID = $formEl.find("#SupplierId").val();
        //var shadeCodes = masterData.ShadeList.filter(x => x.additionalValue == contactID);
        var shadeCodes = masterData.ShadeList;
        var additionalColumns = [
            //{ field: 'ShadeCode', headerText: 'Shade Code', allowEditing: isAllowEditing || status == "addNewItem" }
            {
                field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false,
                valueAccessor: ej2GridDisplayFormatterV2,
                dataSource: shadeCodes,
                edit: ej2GridDropDownObj({
                })
            },
            { field: 'DayValidDurationName', headerText: 'Yarn Sourcing Mode', allowEditing: false },
            { field: 'YarnCategory', headerText: 'Yarn Category', allowEditing: false, width: 350 },
            { field: 'ReqQty', headerText: 'PR Qty', allowEditing: false, visible: status != "addNewItem", editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'PoQty', headerText: 'PO Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'BalanceQTY', headerText: 'Balance For PO QTY', allowEditing: false, visible: status != "addNewItem", editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'ReqCone', headerText: 'PR Cone Qty', allowEditing: isAllowEditing, visible: status != "addNewItem", editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'POCone', headerText: 'Po Cone Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },
            { field: 'Rate', headerText: 'Rate', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2 } } },
            { field: 'PIValue', headerText: 'Total Value', allowEditing: false },
            { field: 'HSCode', headerText: 'HS Code' },
            { field: 'QuotationRefNo', headerText: 'Quotation Ref. No' },
            { field: 'QuotationRefDate', headerText: 'Quotation Ref. Date', type: 'date', format: _ch_date_format_1, editType: 'datepickeredit', width: 40, textAlign: 'Center' },
            { field: 'BookingNo', headerText: 'Booking No', allowEditing: false, visible: status != "addNewItem" },
            //{
            //    field: 'BuyerNames', headerText: 'Buyer', width: 120, minWidth: 120, maxWidth: 120,
            //    edit: ej2GridMultipleDropDownObj({
            //        dataSource: masterData.BuyerList,
            //        displayField: "YarnChildPoBuyerIds",
            //        valueFieldName: "YarnChildPoBuyerIds",
            //        onChange: function (selectedData, currentRowData) {
            //            currentChildRowData = currentRowData;
            //        }
            //    })
            //},
            {
                field: 'BuyerID',
                headerText: 'Buyer',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.BuyerList,
                displayField: "text",
                width: 80,
                edit: ej2GridDropDownObj({
                })
            },
            { field: 'YarnChildPoExportIds', headerText: 'EWO', visible: false },
            { field: 'YarnChildPoEWOs', headerText: 'EWO', width: 120, minWidth: 120, maxWidth: 120, allowEditing: false },
            {
                headerText: '', textAlign: 'Center', width: 40, commands: [
                    {
                        buttonOption: {
                            type: 'AddEWO', content: '', cssClass: 'btn btn-success btn-xs',
                            iconCss: 'fa fa-search'
                        }
                    }
                ]
            },

            //{
            //    field: 'YarnChildPoEWOs', headerText: 'EWO', allowEditing: false,
            //    edit: ej2GridMultipleDropDownObj({
            //        dataSource: masterData.ExportOrderList,
            //        valueFieldName: "YarnChildPoExportIds",
            //        onChange: function (selectedData, currentRowData) {
            //            currentChildRowData = currentRowData;
            //        }
            //    })
            //},
            { field: 'Remarks', headerText: 'Special Specifications' },
            {
                field: 'ReceivedCompleted', headerText: 'Shipment Status?', displayAsCheckBox: true,
                editType: "booleanedit", textAlign: 'Center', visible: status == statusConstants.APPROVED ? true : false
            },
            { field: 'IsYarnReceive', allowEditing: false, visible: false },
            { field: 'ReceiveQty', allowEditing: false, visible: false }
        ];

        //----------------------------------------------------------------------
        var purchaseForObj = {};
        if (masterData.PRMasterID > 0) {
            purchaseForObj = { field: 'POFor', headerText: 'Purchase For', allowEditing: false }
        } else {
            purchaseForObj = {
                field: 'PoForId',
                headerText: 'Purchase For',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.BaseTypes,
                displayField: "POFor",
                valueFieldName: "id",
                allowEditing: true,
                width: 100,
                edit: ej2GridDropDownObj({
                })
            }
        }
        var indexF = additionalColumns.findIndex(x => x.field == 'YarnChildPoEWOs');
        additionalColumns.splice(indexF, 0, purchaseForObj);
        //----------------------------------------------------------------------

        columns.push.apply(columns, additionalColumns);

        if ($tblChildEl) $tblChildEl.destroy();
        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            data: DeepClone(data),
            columns: columns,

            aggregates: [
                {
                    columns: [
                        {
                            type: 'Sum',
                            field: 'ShadeCode',
                            footerTemplate: 'Total'
                        },
                        {
                            type: 'Sum',
                            field: 'ReqQty',
                            decimals: 2,
                            format: "N2",
                            footerTemplate: '${Sum}'
                        }
                        ,
                        {
                            type: 'Sum',
                            field: 'PoQty',
                            decimals: 2,
                            format: "N2",
                            footerTemplate: '${Sum}'
                        },
                        {
                            type: 'Sum',
                            field: 'ReqCone',
                            decimals: 2,
                            format: "N2",
                            footerTemplate: '${Sum}'
                        },

                        {
                            type: 'Sum',
                            field: 'PIValue',
                            decimals: 2,
                            format: "N2",
                            footerTemplate: '${Sum}'
                        }
                    ]
                }
            ],

            actionBegin: function (args) {
                if (args.requestType === 'beginEdit') {

                }
                else if (args.requestType === "save") {
                    
                    if (args.data.ReqQty > 0 && args.data.PoQty > args.data.MaxPOQty) {
                        toastr.error("PO Qty(" + args.data.PoQty + ") should not greater than " + args.data.MaxPOQty + " !!");
                        //if (args.data.NoOfCartoon == null) args.data.NoOfCartoon = 0;
                        if (args.data.POCone == null) args.data.POCone = 0;
                        //if (args.data.ChallanQty == null) args.data.ChallanQty = 0;
                        args.data.PoQty = 0;
                        return;
                    }

                    if (status != "addNewItem") {

                    } else {
                        //args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;
                    }
                    if (args.data.IsYarnReceive) {
                        args.data = setDefaultValue(args.data);
                        args.previousData = setDefaultValue(args.previousData);

                        if (args.data.Segment1ValueId != args.previousData.Segment1ValueId
                            || args.data.Segment2ValueId != args.previousData.Segment2ValueId
                            || args.data.Segment3ValueId != args.previousData.Segment3ValueId
                            || args.data.Segment4ValueId != args.previousData.Segment4ValueId
                            || args.data.Segment5ValueId != args.previousData.Segment5ValueId
                            || args.data.Segment6ValueId != args.previousData.Segment6ValueId
                            || args.data.ShadeCode != args.previousData.ShadeCode
                        ) {

                            toastr.error(`Yarn already received. This yarn item can't change or delete.`);
                            args.data = args.previousData;
                            args.rowData = args.previousData;
                        }
                        else if (args.data.PoQty < args.data.ReceiveQty) {
                            toastr.error(`PO Qty ${args.data.PoQty} can't be less then receive Qty ${args.data.ReceiveQty}`);
                            args.data.PoQty = args.previousData.ReceiveQty;
                            args.rowData.PoQty = args.previousData.ReceiveQty;
                        }
                    }
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.YPOChildID);

                    args.data.PIValue = (args.data.PoQty * args.data.Rate).toFixed(4);

                    if (args.rowData.YarnChildPoBuyerIds != undefined) {
                        args.data.YarnChildPoBuyerIds = args.rowData.YarnChildPoBuyerIds.toString() + ',';
                        args.data.YarnChildPoBuyerIds = args.data.YarnChildPoBuyerIds.substring(0, args.data.YarnChildPoBuyerIds.length - 1);
                    }
                    args.rowData.BuyerNames = args.data.BuyerNames;

                    if (args.rowData.BuyerNames != null && args.rowData.BuyerNames.length > 0) {
                        var buyerNames = args.rowData.BuyerNames.split(',');
                        if (buyerNames.length > 0) {
                            var buyerIds = [];
                            buyerNames.map(x => {
                                var obj = masterData.BuyerList.find(y => y.text.trim() == x.trim());
                                if (obj != null) {
                                    buyerIds.push(obj.id);
                                }
                            });
                            args.rowData.YarnChildPoBuyerIds = buyerIds.join(',');
                        }
                    }

                    if (args.previousData.BuyerNames != null && args.previousData.BuyerNames.length > 0
                        && args.data.BuyerNames != null && args.data.BuyerNames.length > 0) {

                        var previousBuyerNames = getBuyerNames(args.previousData.BuyerNames);
                        var currentBuyerNames = getBuyerNames(args.data.BuyerNames);

                        if (JSON.stringify(previousBuyerNames) != JSON.stringify(currentBuyerNames)) {
                            args.rowData.YarnChildPoEWOs = "";
                        }
                    }

                    args.data.YarnChildPoBuyerIds = args.rowData.YarnChildPoBuyerIds;

                    if (args.rowData.YarnChildPoExportIds != undefined) {
                        args.data.YarnChildPoExportIds = args.rowData.YarnChildPoExportIds.toString() + ',';
                        args.data.YarnChildPoExportIds = args.data.YarnChildPoExportIds.substring(0, args.data.YarnChildPoExportIds.length - 1);
                    }
                    args.data.YarnChildPoEWOs = args.rowData.YarnChildPoEWOs;

                    //args.data.PoForId = args.rowData.PoForId;
                    //args.data.POFor = args.rowData.POFor;
                    //args.rowData.PoForId = args.data.POForID;

                    args.data = setYarnSegDesc(args.data);
                    args.rowData = DeepClone(args.data);

                    if (!isIgnoreValidation(masterData.YPOMasterID)) {
                        var currentRow = parseInt(index) + 1;
                        var rowMessgae = " at row (" + currentRow + ")";
                        if (isInvalidSegment(args.data.Segment1ValueId)) {
                            toastr.error("Select composition" + rowMessgae);
                            return false;
                        }
                        if (isInvalidSegment(args.data.Segment2ValueId)) {
                            toastr.error("Select yarn type" + rowMessgae);
                            return false;
                        }
                        if (isInvalidSegment(args.data.Segment3ValueId)) {
                            toastr.error("Select manufacturing process" + rowMessgae);
                            return false;
                        }
                        if (isInvalidSegment(args.data.Segment4ValueId)) {
                            toastr.error("Select sub process" + rowMessgae);
                            return false;
                        }
                        if (isInvalidSegment(args.data.Segment5ValueId)) {
                            toastr.error("Select quality parameter" + rowMessgae);
                            return false;
                        }
                        if (isInvalidSegment(args.data.Segment6ValueId)) {
                            toastr.error("Select count" + rowMessgae);
                            return false;
                        }
                        if ((args.data.Segment5ValueDesc.toLowerCase() == "melange" || args.data.Segment5ValueDesc.toLowerCase() == "color melange") && !isValidValue(args.data.ShadeCode)) {
                            toastr.error("Select shade code for color melange " + rowMessgae);
                            hasError = true;
                            return false;
                        }
                    }
                    args.data = getPOForName(args.data);
                    masterData.YarnPOChilds[parseInt(index)] = args.data;

                    args.rowData = args.data;

                    //$tblChildEl.updateRow(args.rowIndex, args.data);
                }
                else if (args.requestType.toLowerCase() === "delete") {
                    
                    if (args.data[0].IsYarnReceive) {
                        toastr.error(`Yarn already received. This yarn item can't change or delete`);
                        args.cancel = true;
                    }
                    else if (args.data[0].IsYarnReceiveByPI) {
                        toastr.error(`PI already received. This yarn item can't change or delete.`);
                        args.cancel = true;
                    }
                }

            },
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            commandClick: childCommandClick,
            editSettings: {
                allowAdding: true,
                allowEditing: true,
                allowDeleting: true,
                mode: "Normal",
                showDeleteConfirmDialog: true
            }
        });
    }
    async function childCommandClick(e) {
        
        childData = e.rowData;
        if (e.commandColumn.buttonOption.type == 'AddEWO') {
            //if (childData.YarnChildPoBuyerIds.length < 0) {
            //    toastr.info("You must select buyer first.");
            //    return;
            //}
            if (childData.BuyerID == 0) {
                toastr.info("Select buyer.");
                return;
            }
            childData.YarnChildPoBuyerIds = childData.BuyerID;
            var finder = new commonFinder({
                title: "Select EWO",
                pageId: pageId,
                height: 320,
                //apiEndPoint: `/api/ypo/ewo-list/${childData.YarnChildPoBuyerIds}`,
                apiEndPoint: `/api/ypo/ewo-list/${childData.YarnChildPoBuyerIds}`,
                fields: "EWONo,IsSample,BuyerName,BuyerTeam",
                headerTexts: "EWO,Is Sample?,Buyer,Buyer Team",
                isMultiselect: true,
                autofitColumns: true,
                primaryKeyColumn: "EWONo",
                selectedIds: childData.YarnChildPoEWOs,
                seperateSelection: false,
                onMultiselect: function (selectedRecords) {
                    finder.hideModal();

                    childData.YarnPOChildOrders = [];
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var selectedValue = selectedRecords[i];
                        var yarnPOChildOrder = {
                            YPOChildID: childData.YPOChildID,
                            ExportOrderID: selectedValue.ExportOrderId,
                            EWONo: selectedValue.EWONo,
                            IsSample: selectedValue.IsSample,
                            BuyerID: selectedValue.BuyerID,
                            BuyerTeamID: selectedValue.BuyerTeamID,
                            BuyerName: selectedValue.BuyerName
                        }
                        childData.YarnPOChildOrders.push(yarnPOChildOrder);
                    }
                    childData.YarnChildPoEWOs = childData.YarnPOChildOrders.map(function (item) {
                        return item.EWONo
                    }).join(",");
                    childData.YarnChildPoExportIds = childData.YarnPOChildOrders.map(function (item) {
                        return item.ExportOrderID
                    }).join(",");

                    var index = $tblChildEl.getRowIndexByPrimaryKey(childData.YPOChildID);
                    $tblChildEl.updateRow(index, childData);
                }
            });
            finder.showModal();
        }
    }

    function setDefaultValue(obj) {
        var maxSegment = 6;
        for (var iS = 0; iS <= maxSegment; iS++) {
            obj['Segment' + iS + 'ValueId'] = obj['Segment' + iS + 'ValueId'] == null ? 0 : obj['Segment' + iS + 'ValueId'];
        }
        obj['ShadeCode'] = obj['ShadeCode'] == null ? "" : obj['ShadeCode'];
        return obj;
    }

    function getBuyerNames(buyerNames) {
        var buyerNameList = [];
        if (buyerNames != null && buyerNames.length > 0) {
            var newList = [];
            var buyerNameList = buyerNames.split(',');
            buyerNameList.map(x => {
                newList.push($.trim(x));
            });
            buyerNameList = newList;
            buyerNameList = buyerNameList.sort();
        }
        return buyerNameList;
    }

    function addNewItem(e) {
        e.preventDefault();
        if (_poForList.length > 0) {
            masterData.YarnPOChilds.map(x => {
                if (x.POFor == null || x.POFor == "") {
                    var child = _poForList.find(y => y.PoForId == x.PoForId);
                    if (child) {
                        x.POFor = child.POFor;
                    }
                }
            });
        }

        //masterData.YarnPOChilds.splice(masterData.YarnPOChilds.length - 1, 1);
        masterData.YarnPOChilds.push(addNewItemObj(null));
        initChildTable(masterData.YarnPOChilds);
    }
    function getSinglePropValue(obj, propName, isNumber, defaultValue = "") {
        if (obj == null) {
            if (isNumber) return 0;
            else return defaultValue.length > 0 ? defaultValue : "";
        } else {
            if (isNumber) {
                obj[propName] = getDefaultValueWhenInvalidN_Float(obj[propName]);
            } else {
                obj[propName] = getDefaultValueWhenInvalidS(obj[propName]);
            }
        }
        return obj[propName];
    }
    function addNewItemObj(obj) {
        var objMain = {
            YPOChildID: _maxYPOChildID++, //getMaxIdForArray(masterData.YarnPOChilds, "YPOChildID"),
            YPOMasterID: 0, //masterData.YPOMasterID, // No Need
            UnitID: 28,
            DisplayUnitDesc: "Kg",
            EntityState: 4,
            IsYarnReceive: false,
            YarnPRNo: "",
            PRNO: ""
        };
        //Imrez
        var strFields = ["YarnSubProgramIds"
            , "YarnSubProgramNames"
            , "YarnCategory"
            , "YarnLotNo"
            , "Remarks"
            , "HSCode"
            , "PRNO"
            , "YarnPRNo"
            , "YarnProgram"
            , "DayValidDurationName"
            , "YarnChildPoEWOs"
            , "EWOOthers"
            , "BuyerNames"
            , "POFor"
            , "ShadeCode"
            , "ConceptNo"
            , "BookingNo"
        ];

        var numFields = ["NoOfThread"
            , "POQty"
            , "PoQty"
            , "Rate"
            , "value"
            , "SubGroupId"
            , "ItemMasterID"
            , "ReqQty"
            , "BalanceQTY"
            , "POCone"
            , "YarnProgramId"
            , "PoForId"
            , "ReceiveQty"
            , "ReqCone"
            , "YarnPRChildID"
            , "PRChildID"
            , "PRMasterID"
            , "BaseTypeId"
            , "DayValidDurationId"
            , "ConceptID"
        ];

        strFields.map(propName => {
            objMain[propName] = obj == null ? "" : getDefaultValueWhenInvalidS(obj[propName]);
            var defaultValue = "";
            if (propName == "DayValidDurationName") defaultValue = "Empty";
            else if (propName == "PRNO" && objMain.YarnPRNo.length > 0) objMain.PRNO = objMain.YarnPRNo;
            else if (propName == "YarnPRNo" && objMain.PRNO.length > 0) objMain.YarnPRNo = objMain.PRNO;
            objMain[propName] == getSinglePropValue(obj, propName, false, defaultValue);
        });
        numFields.map(propName => {
            objMain[propName] = obj == null ? 0 : getDefaultValueWhenInvalidN_Float(obj[propName]);
            objMain[propName] == getSinglePropValue(obj, propName, true);
        });

        objMain.PRChildID = getDefaultValueWhenInvalidN(objMain.PRChildID);
        objMain.YarnPRChildID = getDefaultValueWhenInvalidN(objMain.YarnPRChildID);

        var maxPRChildId = Math.max(objMain.PRChildID, objMain.YarnPRChildID);
        objMain.YarnPRChildID = maxPRChildId;
        objMain.PRChildID = maxPRChildId;

        objMain.PRMasterID = getDefaultValueWhenInvalidN(objMain.PRMasterID);
        objMain.YarnPRMasterID = getDefaultValueWhenInvalidN(objMain.YarnPRMasterID);

        var maxPRMasterID = Math.max(objMain.PRMasterID, objMain.YarnPRMasterID);
        objMain.YarnPRMasterID = maxPRMasterID;
        objMain.PRMasterID = maxPRMasterID;

        for (var iS = 1; iS <= 6; iS++) {
            var propName = "Segment" + iS + "ValueId";
            objMain[propName] = obj == null ? 0 : getDefaultValueWhenInvalidN(obj[propName]);
            objMain[propName] = getSinglePropValue(obj, propName, true);

            var propName1 = "Segment" + iS + "ValueDesc";
            objMain[propName1] = obj == null ? "" : getDefaultValueWhenInvalidS(obj[propName1]);
            objMain[propName1] = getSinglePropValue(obj, propName1, false);

            //_yarnSegments
            var list = _yarnSegments["Segment" + iS + "ValueList"];
            if (list == null) list = [];
            if (list.length > 0) {
                if (objMain[propName] == 0 && objMain[propName1].length > 0) {
                    objMain[propName] = parseInt(list.find(x => x.text == objMain[propName1]).id);
                }
                if (objMain[propName] > 0 && objMain[propName1].length == 0) {
                    objMain[propName1] = list.find(x => x.id == objMain[propName]).text;
                }
            }
        }

        objMain.YarnCategory = GetYarnShortForm(objMain.Segment1ValueDesc
            , objMain.Segment2ValueDesc
            , objMain.Segment3ValueDesc
            , objMain.Segment4ValueDesc
            , objMain.Segment5ValueDesc
            , objMain.Segment6ValueDesc
            , objMain.ShadeCode);
        return objMain;
    }

    function getColumns() {
        var columns = [
            {
                field: 'PRNO', headerText: 'PR No'
            },
            {
                field: 'PRDate', headerText: 'PR Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'PRRequiredDate', headerText: 'PR Req. Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'PRByUser', headerText: 'PR By'
            },
            {
                field: 'ConceptID', headerText: 'ConceptID', visible: false
            },
            {
                field: 'BaseTypeId', headerText: 'BaseTypeId', visible: false
            },
            {
                field: 'YarnPRNo', headerText: 'PR No', visible: false
            },
            {
                field: 'DayValidDurationId', headerText: 'DayValidDurationId', visible: false
            },
            {
                field: 'DayValidDurationName', headerText: 'Yarn Sourcing Mode'
            },
            {
                field: 'RevisionNo', headerText: 'Revision No', textAlign: 'Center', visible: status == statusConstants.APPROVED || status == statusConstants.PROPOSED
            },
            {
                field: 'RevisionDate', headerText: 'Revision Date', textAlign: 'Center', type: 'date', format: _ch_date_format_7, visible: status == statusConstants.APPROVED || status == statusConstants.PROPOSED
            },
            {
                field: 'YarnPRChildID', visible: false
            },
            {
                field: 'CompanyId', visible: false
            },
            {
                field: 'CompanyName', headerText: 'Company'
            },
            {
                field: 'SupplierName', headerText: 'Supplier', visible: status != statusConstants.PENDING
            },
            {
                field: 'BuyerName', headerText: 'Buyer'
            },
            {
                field: 'QuotationRefNo', headerText: 'Ref No', visible: status != statusConstants.PENDING
            },
            {
                field: 'DeliveryStartDate', headerText: 'Delivery Start', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'DeliveryEndDate', headerText: 'Delivery End', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'POStatus', headerText: 'Status', visible: status != statusConstants.PENDING
            },
            {
                field: 'ReceivedCompleted', headerText: 'Shipment Status', displayAsCheckBox: true, visible: status == statusConstants.APPROVED
            },
            {
                field: 'InHouseDate', headerText: 'In-House Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'Segment1ValueDesc', headerText: 'Composition'
            },
            {
                field: 'Segment2ValueDesc', headerText: 'Yarn Type'
            },
            {
                field: 'Segment3ValueDesc', headerText: 'Process'
            },
            {
                field: 'Segment4ValueDesc', headerText: 'Sub Process'
            },
            {
                field: 'Segment5ValueDesc', headerText: 'Quality Parameter'
            },
            {
                field: 'Segment6ValueDesc', headerText: 'Count'
            },
            {
                field: 'Segment7ValueDesc', headerText: 'No of Poly'
            },
            {
                field: 'ShadeCode', headerText: 'Shade Code'
            },
            {
                field: 'ReqQty', headerText: 'Req Qty'
            },
            {
                field: 'POQty', headerText: 'PO Qty'
            },
            {
                field: 'BalanceQTY', headerText: 'Balance For PO QTY'
            },
            {
                field: 'POFor', headerText: 'PO For'
            }
        ];
        columns.unshift({ type: 'checkbox', width: 50 });
        return columns;
    }

    function addFromPRChild(e) {
        e.preventDefault();

        masterData.CompanyId = getDefaultValueWhenInvalidN(masterData.CompanyId);
        if (masterData.CompanyId == 0) {
            return toastr.error("Select Company !!!");
        }
        var prChildIds = "";
        var prChilds = [];
        var currentPRChilds = $tblChildEl.getCurrentViewRecords();
        masterData.YarnPOChilds.map(x => {
            var indexF = currentPRChilds.findIndex(y => y.PRChildID == x.PRChildID);
            if (indexF == -1) prChilds.push(x.PRChildID);
        });
        if (prChilds.length > 0) {
            prChildIds = prChilds.join(",");
        }
        prChildIds = replaceInvalidChar(prChildIds);

        var modalObj = ch_GenerateBasicModal($formEl, true, "btnSelectPRChild");
        var $tblMasterEl_Modal;
        if ($tblMasterEl_Modal) $tblMasterEl_Modal.destroy();
        $tblMasterEl_Modal = new initEJ2Grid({
            tableId: modalObj.modalTableId,
            apiEndPoint: `/api/ypo/get-pr-items/${masterData.CompanyId}/${prChildIds}`,
            columns: getColumns()
        });
        $formEl.find("#btnSelectPRChild").click(function () {
            var modalSelectedData = $tblMasterEl_Modal.getSelectedRecords();
            var childGridData = $tblChildEl.getCurrentViewRecords();

            var newChildList = [];
            modalSelectedData.map(m => {
                var indexF = childGridData.findIndex(x => x.PRChildID == m.YarnPRChildID);
                if (indexF == -1) {
                    m = addNewItemObj(m);
                    m.POQty = getDefaultValueWhenInvalidN_Float(m.POQty);
                    m.PIValue = getDefaultValueWhenInvalidN_Float(m.PIValue);
                    m.Rate = getDefaultValueWhenInvalidN_Float(m.Rate);
                    m.PIValue = m.POQty + m.Rate;
                    newChildList.push(m);
                }
            });
            childGridData.map(m => {
                var indexF = modalSelectedData.findIndex(x => x.YarnPRChildID == m.PRChildID);
                if (indexF > -1) {
                    var obj = modalSelectedData[indexF];
                    m.POQty = getDefaultValueWhenInvalidN_Float(m.POQty);
                    m.PIValue = getDefaultValueWhenInvalidN_Float(m.PIValue);
                    m.Rate = getDefaultValueWhenInvalidN_Float(m.Rate);
                    obj.POQty = getDefaultValueWhenInvalidN_Float(m.POQty);
                    m.POQty = m.POQty + obj.POQty;
                    m.PIValue = m.POQty + childGridData[indexF].Rate;
                }
                newChildList.push(m);
            });
            $(modalObj.modalId).modal('hide');
            initChildTable(newChildList);
        });
    }

    function getDetails(row) {
        
        HoldOn.open({
            theme: "sk-circle"
        });
        switch (status) {
            case statusConstants.AWAITING_PROPOSE:
                $formEl.find("#btnApproveYPO").fadeOut();
                $formEl.find("#btnRejectYPO").fadeOut();
                //$formEl.find("#btnRevisionYPO").fadeOut();
                $formEl.find("#btnSaveYPO").fadeIn();
                $formEl.find("#btnSaveYPOShip").fadeOut();
                $formEl.find("#btnSaveAndProposeYPO").fadeIn();
                break;
            case statusConstants.PROPOSED:
                if (pageName == "YarnPOApprovalV2") {
                    $formEl.find("#btnSaveYPO").fadeOut();
                    $formEl.find("#btnSaveAndProposeYPO").fadeOut();
                    $formEl.find("#btnRevisionYPO").fadeOut();
                    $formEl.find("#btnApproveYPO").fadeIn();
                    $formEl.find("#btnRejectYPO").fadeIn();
                }
                else {
                    $formEl.find("#btnApproveYPO").fadeOut();
                    $formEl.find("#btnRejectYPO").fadeOut();
                    $formEl.find("#btnSaveYPO").fadeOut();
                    $formEl.find("#btnSaveAndProposeYPO").fadeOut();
                    //$formEl.find("#btnRevisionYPO").fadeOut();
                }
                break;
            case statusConstants.UN_APPROVE:
                if (pageName == "YarnPOApprovalV2") {
                    $formEl.find("#btnSaveYPO").fadeOut();
                    $formEl.find("#btnSaveAndProposeYPO").fadeOut();
                    $formEl.find("#btnApproveYPO").fadeOut();
                    $formEl.find("#btnRejectYPO").fadeOut();
                    $formEl.find("#btnSaveYPOShip").fadeOut();
                    $formEl.find("#btnRevisionYPO").fadeIn();

                }
            case statusConstants.RETURN:
                if (pageName == "YarnPOApprovalV2") {
                    $formEl.find("#btnSaveYPO").fadeOut();
                    $formEl.find("#btnSaveAndProposeYPO").fadeOut();
                    $formEl.find("#btnApproveYPO").fadeOut();
                    $formEl.find("#btnRejectYPO").fadeOut();
                    $formEl.find("#btnSaveYPOShip").fadeOut();
                    $formEl.find("#btnRevisionYPO").fadeOut();

                }
            case statusConstants.APPROVED:
                $formEl.find("#btnApproveYPO").fadeOut();
                $formEl.find("#btnRejectYPO").fadeOut();
                $formEl.find("#btnSaveYPO").fadeOut();
                $formEl.find("#btnRevisionYPO").fadeOut();
                $formEl.find("#btnSaveAndProposeYPO").fadeOut();
                break;
            default:
                break;
        }
        StatusPIPO = row.StatusPIPO;

        var url = "";
        if (row.IsRevision) {
            url = `/api/ypo/revision/${row.YPOMasterID}`;
        }
        else {
            url = `/api/ypo/${row.YPOMasterID}`;
        }
        axios.get(url)
            .then(function (response) {
                $divTblEl.fadeOut();
                $divDetailsEl.fadeIn();

                addPRItemBtnHideShow();
                $formEl.find("#SupplierTNA").fadeIn();

                masterData = response.data;
                masterData.PoDate = formatDateToDefault(masterData.PoDate);
                masterData.DeliveryStartDate = formatDateToDefault(masterData.DeliveryStartDate);
                masterData.DeliveryEndDate = formatDateToDefault(masterData.DeliveryEndDate);
                masterData.QuotationRefDate = formatDateToDefault(masterData.QuotationRefDate);
                masterData.InHouseDate = formatDateToDefault(masterData.InHouseDate);
                setFormData($formEl, masterData);
                _ignoreValidationPOIds = masterData.IgnoreValidationPOIds;
                _ypoMasterID = masterData.YPOMasterID;

                //$divDetailsEl.find("#CompanyId").prop("disabled", false);
                if (masterData.PaymentTermsId === 2) showHideLCSection(true);
                else showHideLCSection(false);

                if (masterData.PortofLoadingID === 105) showHideSupplierRegionSection(false);
                else showHideSupplierRegionSection(true);

                if (masterData.TypeOfLcId === 2) $formEl.find("#formGroupCreditDays").fadeIn();
                else $formEl.find("#formGroupCreditDays").fadeOut();

                $("#PODateCurrent").text(masterData.DeliveryStartDate);
                $pageEl.find("#SFToPLDate").text(formatDateToDefault(masterData.SFToPLDate));
                $pageEl.find("#SFToPLDays").text(masterData.SFToPLDays);
                $pageEl.find("#PLToPDDate").text(formatDateToDefault(masterData.PLToPDDate));
                $pageEl.find("#PLToPDDays").text(masterData.PLToPDDays);
                $pageEl.find("#PDToCFDate").text(formatDateToDefault(masterData.PDToCFDate));
                $pageEl.find("#PDToCFDays").text(masterData.PDToCFDays);
                $pageEl.find("#InHouseDays").text(masterData.InHouseDays);

                setPOForItems(masterData.YarnPOChilds);
                initChildTable(masterData.YarnPOChilds);

                HoldOn.close();
            })
            .catch(showResponseError)
    }

    function proposePO(row) {
        showBootboxConfirm("Propose Yarn PO", "Are you sure you want to propose this PO?", function (yes) {
            if (yes) {
                var url = "/api/ypo/propose-ypo/" + row.YPOMasterID;
                axios.post(url)
                    .then(function () {
                        toastr.success(constants.PROPOSE_SUCCESSFULLY);
                        initMasterTable();
                    })
                    .catch(function (error) {
                        toastr.error(error.response.data.Message);
                    });
            }
        });
    }

    function approvePO(row) {
        showBootboxConfirm("Approve Yarn PO", "Are you sure you want to approve this PO?", function (yes) {
            if (yes) {
                var url = "/api/ypo/approve-ypo/" + row.YPOMasterID;
                axios.post(url)
                    .then(function () {
                        toastr.success(constants.APPROVE_SUCCESSFULLY);
                        initMasterTable();
                    })
                    .catch(function (error) {
                        toastr.error(error.response.data.Message);
                    });
            }
        });
    }

    function rejectPO(row) {
        showBootboxPrompt("Reject Yarn PO", "Are you sure you want to Reject this PO?", function (result) {
            if (result) {
                var data = {
                    YPOMasterID: row.YPOMasterID,
                    UnapproveReason: result,
                    CompanyId: 0,
                    SupplierId: 0,
                    IncoTermsId: 0,
                    PaymentTermsId: 0,
                    CountryOfOriginId: 0,
                    OfferValidity: 0
                };

                axios.post("/api/ypo/reject-ypo", data)
                    .then(function () {
                        toastr.success(constants.REJECT_SUCCESSFULLY);
                        initMasterTable();
                    })
                    .catch(function (error) {
                        toastr.error(error.response.data.Message);
                    });
            }
        });
    }

    function saveYPO(isPropose) {
        $formEl.find(':checkbox').each(function () {
            this.value = this.checked;
        });
        var data = formDataToJson($formEl.serializeArray());
        if (isPropose) {
            data.YPOMasterID = _ypoMasterID;
        }
        if (!data.CompanyId || parseInt(data.CompanyId) == 0) {
            toastr.error("Select company");
            return false;
        }
        if (!data.CompanyId) data.CompanyId = companyId;

        if (status == "addNewItem") data.IsItemGenerate = true;

        data.YarnPOChilds = $tblChildEl.getCurrentViewRecords();

        if (data.YarnPOChilds.length == 0) {
            toastr.error("No Item Information Found");
            return false;
        }

        data.Proposed = isPropose ? true : false;
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        var isRevise = $formEl.find('#IsRevision').is(":checked");

        var hasError = false;
        for (var i = 0; i < data.YarnPOChilds.length; i++) {
            var currentRow = parseInt(i) + 1;
            var rowMessgae = " at row (" + currentRow + ")";
            var child = data.YarnPOChilds[i];

            if (!isIgnoreValidation(masterData.YPOMasterID)) {
                if (isInvalidSegment(child.Segment1ValueId)) {
                    toastr.error("Select composition" + rowMessgae);
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment2ValueId)) {
                    toastr.error("Select yarn type" + rowMessgae);
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment3ValueId)) {
                    toastr.error("Select manufacturing process" + rowMessgae);
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment4ValueId)) {
                    toastr.error("Select sub process" + rowMessgae);
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment5ValueId)) {
                    toastr.error("Select quality parameter" + rowMessgae);
                    hasError = true;
                    break;
                }
                if (isInvalidSegment(child.Segment6ValueId)) {
                    toastr.error("Select count" + rowMessgae);
                    hasError = true;
                    break;
                }
                if ((child.Segment5ValueDesc.toLowerCase() == "melange" || child.Segment5ValueDesc.toLowerCase() == "color melange") && !isValidValue(child.ShadeCode)) {
                    toastr.error("Select shade code for color melange " + rowMessgae);
                    hasError = true;
                    break;
                }
            }
            if (child.HSCode == "" || child.HSCode == null) {
                toastr.error("Give HS Code" + rowMessgae);
                hasError = true;
                break;
            }
            
            if (child.PoQty == "" || child.PoQty == null || child.PoQty <= 0) {
                toastr.error("Yarn PO Qty must be greater than zero" + rowMessgae);
                hasError = true;
                break;
            }
            if (child.Rate == "" || child.Rate == null || child.Rate <= 0) {
                toastr.error("Yarn Rate must be greater than zero" + rowMessgae);
                hasError = true;
                break;
            }

            if (typeof child.PRChildID === "undefined") child.PRChildID = 0;
            if (typeof child.PoForId === "undefined") child.PoForId = 0;

            if (child.PoForId == 0) {
                toastr.error("Select purchase for " + rowMessgae);
                hasError = true;
                break;
            }
            if (child.PRChildID == 0 && (child.PoForId == 2163 || child.PoForId == 2164)) //Order Based Bulk OR Order Based Sample
            {
                //if (!child.BuyerNames) {
                //    toastr.error("Select buyer for " + child.POFor + " " + rowMessgae);
                //    hasError = true;
                //    break;
                //}
                if (child.BuyerID == 0) {
                    toastr.error("Select buyer for " + child.POFor + " " + rowMessgae);
                    hasError = true;
                    break;
                }
                if (!child.YarnChildPoEWOs) {
                    toastr.error("Select EWO for " + child.POFor + " " + rowMessgae);
                    hasError = true;
                    break;
                }
            }

            if (status == statusConstants.PENDING || status == statusConstants.AWAITING_PROPOSE) {
                if (child.IsInvalidItem) {
                    toastr.error(`Invalid item ` + rowMessgae);
                    hasError = true;
                    break;
                }
            }

            if (child.DayValidDurationIdPR > 0 && child.DayValidDurationIdPR != child.DayValidDurationId && masterData.IsCheckDVD) {
                toastr.error(`PR's yarn sourcing mode ${child.DayValidDurationIdPRName} is mismatched with PO yarn sourcing mode ${child.DayValidDurationName} at row ${currentRow}.`);
                hasError = true;
                break;
            }
        }
      
        if (hasError) return false;
        axios.post("/api/ypo/save", data)
            .then(function (response) {
                showBootboxAlert("Yarn PO No: <b>" + response.data + "</b> saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function isValidValue(value) {
        if (typeof value === "undefined" || value == null || value == "" || value == 0) return false;
        return true;
    }
    function isInvalidSegment(segId) {
        if (typeof segId === "undefined" || segId == null || segId == 0) return true;
        return false;
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    function setPOForItems(yarnPOChilds) {
        if (yarnPOChilds.length > 0) {
            yarnPOChilds.map(x => {
                var indexF = _poForList.findIndex(po => po.PoForId == x.PoForId);
                if (indexF == -1 && (x.POFor != null || x.POFor != "")) {
                    _poForList.push({
                        PoForId: x.PoForId,
                        POFor: x.POFor
                    });
                }
            });
        }
    }
    function isIgnoreValidation(poId) {
        if (poId > 0) {
            var indexV = _ignoreValidationPOIds.findIndex(x => x == poId);
            if (indexV > -1) return true;
        }
        return false;
    }
    function getPOForName(obj) {
        /*
        if (obj.PoForId > 0 && masterData.BaseTypes.length > 0) {
            var objBT = masterData.BaseTypes.find(x => x.id == obj.PoForId);
            if (objBT) obj.POFor = objBT.text;
        }
        */
        return obj;
    }
    function addPRItemBtnHideShow() {
        var isRevised = $formEl.find('#IsRevision').is(':checked');
        $formEl.find("#btnAddFromPRChild").hide();
        if (status == statusConstants.AWAITING_PROPOSE ||
            status == statusConstants.PENDING ||
            (status == statusConstants.APPROVED && isRevised)) {
            $formEl.find("#btnAddFromPRChild").show();
        }
    }
})();

