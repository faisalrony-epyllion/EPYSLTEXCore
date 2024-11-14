(function () {
    var menuId, pageName;
    var status = statusConstants.PENDING;

    var toolbarId, pageId, $pageEl, $formEl, $divTblEl, $divDetailsEl, $toolbarEl,
        $tblMasterEl, tblMasterId,
        $tblChildEl, tblChildId,
        $tblOtherItemEl, tblOtherItemId, $tblRollEl, $divRollEl, $tblKProductionE1;

    var masterData;
    var isApprovePage = false, isSampleDelChallanPage = false, isAcknowledgePage = false,
        isSampleGatepassPage = false, isSampleGatepassApprovePage = false, SampleGatepassCheckoutPage = false;
    var cTypeID = 0;
    var vIsBDS = 0;
    var ChildFabricItemList = new Array();
    var ChildOthersItemList = new Array();
    var _subGroup = 1;
    var totalQty;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblOtherItemId = "#tblOtherItem" + pageId;
        tblChildSetId = "#tblChildSet" + pageId;
        tblChildSetDetailsId = "#tblChildSetDetails" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblCreateCompositionId = `#tblCreateComposition-${pageId}`;
        tblMachineParamId = $("#tblMachineParam" + pageId);
        $tblRollEl = $("#tblRoll" + pageId);
        $divRollEl = $("#divRoll" + pageId);
        $tblKProductionE1 = $("#tblKProduction" + pageId);

        $formEl.find("#Non-Organic").prop("checked", false);

        isSampleDelChallanPage = convertToBoolean($(`#${pageId}`).find("#SampleDelChallanPage").val());
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());

        isSampleGatepassPage = convertToBoolean($(`#${pageId}`).find("#SampleGatepassPage").val());
        isSampleGatepassApprovePage = convertToBoolean($(`#${pageId}`).find("#SampleGatepassApprovePage").val());
        isSampleGatepassCheckoutPage = convertToBoolean($(`#${pageId}`).find("#SampleGatepassCheckoutPage").val());

        $toolbarEl.find("#btnAddDC").fadeOut();
        $formEl.find("#Non-Organic").prop("disabled", true);
        $formEl.find("#Organic").prop("disabled", true);
        $formEl.find("#Fair-Trade").prop("disabled", true);
        $formEl.find("#IsReDelivery").prop("disabled", true);

        if (isApprovePage) {
            $toolbarEl.find("#btnSampleList,#btnFreeConceptList").show();
            $toolbarEl.find("#btnProposedDCList,#btnApproveDCList").show();

            $toolbarEl.find("#btnPendingDCList,#btnEditDCList").hide();
            $toolbarEl.find("#btnGPPendingList,#btnGPProposedList,#btnGPApproveList,#btnGPCKPendingList,#btnGPCKApproveList").hide();
        }
        else if (isSampleDelChallanPage) {
            $toolbarEl.find("#btnPendingDCList,#btnEditDCList,#btnProposedDCList,#btnApproveDCList").show();
            $toolbarEl.find("#divPendingButtons").fadeIn();
            toggleActiveToolbarBtn(this, $toolbarEl);
            toggleActiveToolbarBtn("#btnSampleList", $toolbarEl);
            toggleActiveToolbarBtn("#btnPendingDCList", $toolbarEl);
            $toolbarEl.find("#btnAddDC").fadeIn();
            status = statusConstants.PENDING;

            cTypeID = 1; //1 = Sample
            vIsBDS = 1;
            initMasterTable();
        }
        else if (isSampleGatepassPage) {
            $toolbarEl.find("#btnSampleList,#btnFreeConceptList").show();
            $toolbarEl.find("#btnGPPendingList,#btnGPProposedList").show();

            $toolbarEl.find("#btnPendingDCList,#btnEditDCList,#btnProposedDCList,#btnApproveDCList").hide();
            $toolbarEl.find("#btnGPApproveList,#btnGPCKPendingList,#btnGPCKApproveList").hide();
        }
        else if (isSampleGatepassApprovePage) {
            $toolbarEl.find("#btnSampleList,#btnFreeConceptList").show();
            $toolbarEl.find("#btnGPPendingList,#btnGPApproveList").show();

            $toolbarEl.find("#btnPendingDCList,#btnEditDCList,#btnProposedDCList,#btnApproveDCList").hide();
            $toolbarEl.find("#btnGPProposedList,#btnGPCKPendingList,#btnGPCKApproveList").hide();
        }
        else if (isSampleGatepassCheckoutPage) {
            $toolbarEl.find("#btnSampleList,#btnFreeConceptList").show();
            $toolbarEl.find("#btnGPCKPendingList,#btnGPCKApproveList").show();

            $toolbarEl.find("#btnPendingDCList,#btnEditDCList,#btnProposedDCList,#btnApproveDCList").hide();
            $toolbarEl.find("#btnGPPendingList,#btnGPProposedList,#btnGPApproveList").hide();
        }
        else {
            $toolbarEl.find("#btnSampleList,#btnFreeConceptList").show();
            $toolbarEl.find("#btnPendingDCList,#btnEditDCList,#btnProposedDCList,#btnApproveDCList").show();

            $toolbarEl.find("#btnGPPendingList,#btnGPProposedList,#btnGPApproveList,#btnGPCKPendingList,#btnGPCKApproveList").hide();
        }

        $toolbarEl.find("#btnSampleList,#btnFreeConceptList").on("click", function (e) {
            e.preventDefault();
            if (isApprovePage) {
                $toolbarEl.find("#btnProposedDCList,#btnApproveDCList").show();

                $toolbarEl.find("#btnPendingDCList,#btnEditDCList").hide();
                $toolbarEl.find("#btnGPPendingList,#btnGPProposedList,#btnGPApproveList,#btnGPCKPendingList,#btnGPCKApproveList").hide();

                $toolbarEl.find("#divPendingButtons").fadeIn();
                toggleActiveToolbarBtn(this, $toolbarEl);
                toggleActiveToolbarBtn("#btnSampleList", $toolbarEl);
                toggleActiveToolbarBtn("#btnProposedDCList", $toolbarEl);
                $toolbarEl.find("#btnAddDC").fadeOut();
                status = statusConstants.AWAITING_PROPOSE;
            }
            else if (isSampleGatepassPage) {
                $toolbarEl.find("#btnGPPendingList,#btnGPProposedList,#btnGPApproveList").show();
                $toolbarEl.find("#btnPendingDCList,#btnEditDCList,#btnProposedDCList,#btnApproveDCList").hide();
                $toolbarEl.find("#btnGPCKPendingList,#btnGPCKApproveList").hide();

                $toolbarEl.find("#divPendingButtons").fadeIn();
                toggleActiveToolbarBtn(this, $toolbarEl);
                toggleActiveToolbarBtn("#btnSampleList", $toolbarEl);
                toggleActiveToolbarBtn("#btnGPPendingList", $toolbarEl);
                $toolbarEl.find("#btnAddDC").fadeOut();
                status = statusConstants.PENDING;
            }
            else if (isSampleGatepassApprovePage) {
                $toolbarEl.find("#btnGPProposedList,#btnGPApproveList").show();
                $toolbarEl.find("#btnPendingDCList,#btnEditDCList,#btnProposedDCList,#btnApproveDCList").hide();
                $toolbarEl.find("#btnGPPendingList,#btnGPCKPendingList,#btnGPCKApproveList").hide();

                $toolbarEl.find("#divPendingButtons").fadeIn();
                toggleActiveToolbarBtn(this, $toolbarEl);
                toggleActiveToolbarBtn("#btnSampleList", $toolbarEl);
                toggleActiveToolbarBtn("#btnProposedDCList", $toolbarEl);
                $toolbarEl.find("#btnAddDC").fadeOut();
                status = statusConstants.AWAITING_PROPOSE;
            }
            else if (isSampleGatepassCheckoutPage) {
                $toolbarEl.find("#btnGPCKPendingList,#btnGPCKApproveList").show();
                $toolbarEl.find("#btnPendingDCList,#btnEditDCList,#btnProposedDCList,#btnApproveDCList").hide();
                $toolbarEl.find("#btnGPPendingList,#btnGPProposedList,#btnGPApproveList").hide();

                $toolbarEl.find("#divPendingButtons").fadeIn();
                toggleActiveToolbarBtn(this, $toolbarEl);
                toggleActiveToolbarBtn("#btnSampleList", $toolbarEl);
                toggleActiveToolbarBtn("#btnGPCKPendingList", $toolbarEl);
                $toolbarEl.find("#btnAddDC").fadeOut();
                status = statusConstants.PENDING;
            }
            else {
                $toolbarEl.find("#btnPendingDCList,#btnEditDCList,#btnProposedDCList,#btnApproveDCList").show();
                $toolbarEl.find("#divPendingButtons").fadeIn();
                toggleActiveToolbarBtn(this, $toolbarEl);
                toggleActiveToolbarBtn("#btnSampleList", $toolbarEl);
                toggleActiveToolbarBtn("#btnPendingDCList", $toolbarEl);
                $toolbarEl.find("#btnAddDC").fadeIn();
                status = statusConstants.PENDING;
            }

            if ($(this).attr("id") == "btnSampleList") {
                cTypeID = 1; //1 = Sample
                vIsBDS = 1;
                initMasterTable();

            } else {
                vIsBDS = 0;
                cTypeID = 0; //0 = Free Concept
                initMasterTable();
            }
        });
        $toolbarEl.find("#btnPendingDCList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeIn();
            initMasterTable();
        });
        $toolbarEl.find("#btnEditDCList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.AWAITING_PROPOSE;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeOut();
            initMasterTable();
        });
        $toolbarEl.find("#btnProposedDCList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeOut();
            initMasterTable();
        });
        $toolbarEl.find("#btnApproveDCList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeOut();
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingDCList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeIn();
            initMasterTable();
        });

        //GP 
        $toolbarEl.find("#btnGPPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeOut();
            initMasterTable();
        });
        $toolbarEl.find("#btnGPProposedList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeOut();
            initMasterTable();
        });
        $toolbarEl.find("#btnGPApproveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeOut();
            initMasterTable();
        });
        //GP Checkout
        $toolbarEl.find("#btnGPCKPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeOut();
            initMasterTable();
        });
        $toolbarEl.find("#btnGPCKApproveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            isEditable = false;
            $toolbarEl.find("#btnAddDC").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnAddDC").on("click", getNewData);

        $formEl.find("#btnAddItemsFabric").on("click", function (e) {
            e.preventDefault();
            var ChildsFabric = masterData.Childs.filter(el => el.SubGroupName == subGroupNames.FABRIC);
            if (ChildsFabric != null && ChildsFabric.length > 0) {
                ChildsFabric.map(x => {
                    x.ConceptDate = formatDateToDefault(x.ConceptDate);
                });
            }
            else {
                return toastr.error("Item not Found");
            }
            var finder = new commonFinder({
                title: "Select Items",
                pageId: pageId,
                data: ChildsFabric, //masterData.Childs,
                fields: "KnittingType,TechnicalName,Composition,Gsm,SubClassName,YarnSubProgram,BookingQty,ColorCode,GroupConceptNo,ConceptDate",
                headerTexts: "Machine Type,Technical Name,Composition,Gsm,Sub Class Name,Yarn Sub Program,Qty,Color Code,Group Concept No,Concept Date",
                isMultiselect: true,
                selectedIds: masterData.Childs[0].FabricItemIDs,
                allowPaging: false,
                primaryKeyColumn: "SFDChildID",
                onMultiselect: function (selectedRecords) {
                    var ChildsItemSegmentsList = $tblChildEl.getCurrentViewRecords();
                    ChildFabricItemList = $tblChildEl.getCurrentViewRecords();
                    //Validation Check
                    var Organic = 0, Fair_Trade = 0, Non_Organic = 0;
                    if (cTypeID == 0) {
                        $.each(ChildsItemSegmentsList, function (j, objChild) {
                            if ((objChild.Composition.includes('GOTS')) || (objChild.Composition.includes('Fair Trade'))) {
                                Fair_Trade = 1;
                                $formEl.find("#Non-Organic").prop("checked", false);
                                $formEl.find("#Organic").prop("checked", false);
                                $formEl.find("#Fair-Trade").prop("checked", true);
                            }
                            else if (objChild.Composition.includes('Organic')) {
                                Organic = 1;
                                $formEl.find("#Non-Organic").prop("checked", false);
                                $formEl.find("#Organic").prop("checked", true);
                                $formEl.find("#Fair-Trade").prop("checked", false);
                            }
                            else {
                                Non_Organic = 1;
                                $formEl.find("#Non-Organic").prop("checked", true);
                                $formEl.find("#Organic").prop("checked", false);
                                $formEl.find("#Fair-Trade").prop("checked", false);
                            }
                        });
                    }
                    else {
                        $.each(ChildsItemSegmentsList, function (j, objChild) {
                            if (objChild.YarnSubProgram && ((objChild.YarnSubProgram.includes('GOTS')) || (objChild.YarnSubProgram.includes('Fair Trade')))) {
                                Fair_Trade = 1;
                                $formEl.find("#Non-Organic").prop("checked", false);
                                $formEl.find("#Organic").prop("checked", false);
                                $formEl.find("#Fair-Trade").prop("checked", true);
                            }
                            else if (objChild.YarnSubProgram && objChild.YarnSubProgram.includes('Organic')) {
                                Organic = 1;
                                $formEl.find("#Non-Organic").prop("checked", false);
                                $formEl.find("#Organic").prop("checked", true);
                                $formEl.find("#Fair-Trade").prop("checked", false);
                            }
                            else {
                                Non_Organic = 1;
                                $formEl.find("#Non-Organic").prop("checked", true);
                                $formEl.find("#Organic").prop("checked", false);
                                $formEl.find("#Fair-Trade").prop("checked", false);
                            }
                        });
                    }

                    var Organic = parseInt(Organic) + parseInt(Fair_Trade) + parseInt(Non_Organic);
                    if (Organic > 1) {
                        return toastr.error("Must be same composition.");
                    }

                    for (var i = 0; i < selectedRecords.length; i++) {
                        var exists = ChildsItemSegmentsList.find(function (el) {
                            return el.ItemMasterID == selectedRecords[i].ItemMasterID &&
                                el.ConceptID == selectedRecords[i].ConceptID &&
                                el.CCColorID == selectedRecords[i].CCColorID
                        });

                        if (!exists) {
                            var oPreProcess = {
                                SFDChildID: selectedRecords[i].SFDChildID,
                                SFDID: 0,
                                ItemMasterID: selectedRecords[i].ItemMasterID,
                                ConceptID: selectedRecords[i].ConceptID,
                                CCColorID: selectedRecords[i].CCColorID,
                                ColorID: selectedRecords[i].ColorID,
                                ColorCode: selectedRecords[i].ColorCode,
                                ColorCode: selectedRecords[i].ColorCode,
                                ColorName: selectedRecords[i].ColorName,
                                SubGroupID: selectedRecords[i].SubGroupID,
                                SubClassName: selectedRecords[i].SubClassName,
                                KnittingTypeID: selectedRecords[i].KnittingTypeID,
                                KnittingType: selectedRecords[i].KnittingType,
                                TechnicalNameId: selectedRecords[i].TechnicalNameId,
                                TechnicalName: selectedRecords[i].TechnicalName,
                                CompositionID: selectedRecords[i].CompositionID,
                                Composition: selectedRecords[i].Composition,
                                GSMID: selectedRecords[i].GSMID,
                                Gsm: selectedRecords[i].Gsm,
                                MachineGauge: selectedRecords[i].MachineGauge,
                                BookingID: selectedRecords[i].BookingID,
                                BookingChildID: selectedRecords[i].BookingChildID,
                                ConsumptionID: selectedRecords[i].ConsumptionID,
                                BookingQty: selectedRecords[i].BookingQty,
                                BookingQtyPcs: selectedRecords[i].BookingQtyPcs,
                                StockQty: selectedRecords[i].StockQty,
                                StockQtyPcs: selectedRecords[i].StockQtyPcs
                            }
                            masterData.Childs[0].FabricItemIDs = selectedRecords.map(function (el) { return el.SFDChildID }).toString();
                            //FabricItemList.push(oPreProcess);
                            ChildFabricItemList.push(oPreProcess);
                        }
                    }
                    //initChildTable(FabricItemList);
                    initChildTable(ChildFabricItemList);
                }
            });
            finder.showModal();
        });

        $formEl.find("#btnAddItemsOthers").on("click", function (e) {
            var ChildsOthers = masterData.Childs.filter(function (el) { return el.SubGroupName != subGroupNames.FABRIC });
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Items",
                pageId: pageId,
                data: ChildsOthers, //masterData.Childs,
                fields: "KnittingType,TechnicalName,Gsm,SubClassName,BookingQty,ColorCode,GroupConceptNo,ConceptDate",
                headerTexts: "Machine Type,Technical Name,Gsm,Sub Class Name,Qty,Color Code,Group Concept No,Concept Date",
                isMultiselect: true,
                selectedIds: masterData.Childs[0].OthersItemIDs,
                allowPaging: false,
                primaryKeyColumn: "SFDChildID",
                onMultiselect: function (selectedRecords) {
                    var ChildsItemSegmentsList = $tblOtherItemEl.getCurrentViewRecords();
                    ChildOthersItemList = $tblOtherItemEl.getCurrentViewRecords();
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var exists = ChildsItemSegmentsList.find(function (el) {
                            return el.ItemMasterID == selectedRecords[i].ItemMasterID &&
                                el.ConceptID == selectedRecords[i].ConceptID &&
                                el.CCColorID == selectedRecords[i].CCColorID
                        });

                        if (!exists) {
                            var oPreProcess = {
                                SFDChildID: selectedRecords[i].SFDChildID,
                                SFDID: 0,
                                ItemMasterID: selectedRecords[i].ItemMasterID,
                                ConceptID: selectedRecords[i].ConceptID,
                                CCColorID: selectedRecords[i].CCColorID,
                                ColorID: selectedRecords[i].ColorID,
                                ColorCode: selectedRecords[i].ColorCode,
                                ColorCode: selectedRecords[i].ColorCode,
                                ColorName: selectedRecords[i].ColorName,
                                SubGroupID: selectedRecords[i].SubGroupID,
                                SubGroupName: selectedRecords[i].SubGroupName,
                                SubClassName: selectedRecords[i].SubClassName,
                                KnittingTypeID: selectedRecords[i].KnittingTypeID,
                                KnittingType: selectedRecords[i].KnittingType,
                                TechnicalNameId: selectedRecords[i].TechnicalNameId,
                                TechnicalName: selectedRecords[i].TechnicalName,
                                CompositionID: selectedRecords[i].CompositionID,
                                Composition: selectedRecords[i].Composition,
                                GSMID: selectedRecords[i].GSMID,
                                Gsm: selectedRecords[i].Gsm,
                                Length: selectedRecords[i].Length,
                                Width: selectedRecords[i].Width,
                                MachineGauge: selectedRecords[i].MachineGauge,
                                BookingID: selectedRecords[i].BookingID,
                                BookingChildID: selectedRecords[i].BookingChildID,
                                ConsumptionID: selectedRecords[i].ConsumptionID,
                                BookingQty: selectedRecords[i].BookingQty,
                                BookingQtyPcs: selectedRecords[i].BookingQtyPcs,
                                StockQty: selectedRecords[i].StockQty,
                                StockQtyPcs: selectedRecords[i].StockQtyPcs
                            }
                            masterData.Childs[0].OthersItemIDs = selectedRecords.map(function (el) { return el.SFDChildID }).toString();
                            ChildOthersItemList.push(oPreProcess);
                        }
                    }
                    initOtherItemTable(ChildOthersItemList);
                }
            });
            finder.showModal();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });
        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            save(true);
        });


        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#SFDID").val();
            axios.post(`/api/sample-delivery-challan/approve/${id}`)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    backToList();
                })
                .catch(showResponseError);
        });

        /*GP*/
        $formEl.find("#btnSaveGP").click(function (e) {
            e.preventDefault();

            var data = formDataToJson($formEl.serializeArray());
            data.VehicleType = $formEl.find("#VehicleTypeID option:selected").text();
            data.VehicleNo = $formEl.find("#VehicleNoID option:selected").text();

            initializeValidation($formEl, validationConstraintsGP);
            if (!isValidForm($formEl, validationConstraintsGP)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);

            //var id = $formEl.find("#SFDID").val();
            axios.post("/api/sample-delivery-challan/savegp", data)
                //axios.post(`/api/sample-delivery-challan/savegp/${id}`)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    backToList();
                })
                .catch(showResponseError);
        });
        $formEl.find("#btnApproveGP").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#SFDID").val();
            axios.post(`/api/sample-delivery-challan/approvegp/${id}`)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    backToList();
                })
                .catch(showResponseError);
        });

        /*GP Checkout*/
        $formEl.find("#btnSaveGPCheckOut").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#SFDID").val();
            axios.post(`/api/sample-delivery-challan/savegp_chkout/${id}`)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnSplit").click(function (e) {
            e.preventDefault();
            split();
        });
        $formEl.find("#btnSaveKProd").click(function (e) {
            e.preventDefault();
            saveKProd();
        });
    });

    function initMasterTable() {
        //console.log($tblMasterEl);

        var commands = [];
        var columns = [];

        if (status == statusConstants.PENDING && isSampleDelChallanPage) {

        }
        else {
            commands = [
                {
                    type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' },
                },
                {
                    type: 'ViewChallan', title: 'View Challan', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' },
                },
                {
                    type: 'ViewGatepass', title: 'View Gatepass', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' },
                }
            ]
        }

        if (status == statusConstants.PENDING && isSampleDelChallanPage) {
            columns = [
                {
                    headerText: 'Action', commands: commands, visible: status == statusConstants.PENDING ? false : true,
                    textAlign: 'Center', width: 100, minWidth: 100, maxWidth: 100
                },
                {
                    field: 'OrderUnitID', visible: false
                },
                {
                    field: 'CompanyID', visible: false
                },
                {
                    field: 'GroupConceptNo', headerText: 'Group Concept No', width: 100
                },
                {
                    field: 'ConcepTypeName', headerText: 'Concept Type', width: 100
                },
                {
                    field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Right', type: 'date',
                    format: _ch_date_format_1, width: 80, textAlign: 'Center', headerTextAlign: 'Center'
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', width: 100
                },
                {
                    field: 'Composition', headerText: 'Composition', width: 100
                },
                {
                    field: 'Gsm', headerText: 'GSM', width: 70, textAlign: 'Center', headerTextAlign: 'Center'
                },
                {
                    field: 'OrderUnit', headerText: 'Order Unit', width: 100
                },
                {
                    field: 'CompanyName', headerText: 'Company', width: 100
                },
                {
                    field: 'Remarks', headerText: 'Remarks', width: 140
                }
            ];
        }
        else {
            columns = [
                {
                    headerText: 'Action', commands: commands,
                    textAlign: 'Center', width: 100, minWidth: 100, maxWidth: 100
                },
                {
                    field: 'ConceptTypeID', visible: false
                },
                {
                    field: 'CompanyID', visible: false
                },
                {
                    field: 'OrderUnitID', visible: false
                },
                {
                    field: 'GroupConceptNo', headerText: 'Group Concept No', width: 100
                },
                {
                    field: 'OrderUnit', headerText: 'Order Unit', width: 100
                },
                {
                    field: 'CompanyName', headerText: 'Company', width: 100//, visible: status != statusConstants.PENDING
                },
                {
                    field: 'DCNo', headerText: 'Challan No', width: 100
                },
                {
                    field: 'DCDate', headerText: 'Challan Date', textAlign: 'Right', type: 'date',
                    format: _ch_date_format_1, width: 70, textAlign: 'Center', headerTextAlign: 'Center'
                },
                {
                    field: 'GPNo', headerText: 'Gatepass No', width: 100
                },
                {
                    field: 'GPDate', headerText: 'Gatepass Date', textAlign: 'Right', type: 'date',
                    format: _ch_date_format_1, width: 100, textAlign: 'Center', headerTextAlign: 'Center'
                },
                {
                    field: 'DeliveryItemHead', headerText: 'Delivery Item Type', width: 100
                },
                {
                    field: 'OrderUnit', headerText: 'Order Unit', width: 100
                },
                {
                    field: 'DeliveryFrom', headerText: 'Delivery From', width: 100
                },
                {
                    field: 'DeliveryPlace', headerText: 'Delivery Place', width: 100
                },
                {
                    field: 'VehicleType', headerText: 'Vehicle Type', width: 100
                },
                {
                    field: 'VehicleNo', headerText: 'Vehicle No', width: 100
                },
                {
                    field: 'DriverName', headerText: 'Driver Name', width: 100
                },
                {
                    field: 'DriverContact', headerText: 'Driver Contact', width: 100
                },
                {
                    field: 'Remarks', headerText: 'Remarks', width: 140
                }
            ];
        }

        if (isSampleDelChallanPage) {

            if (status == statusConstants.PENDING) {
                columns.unshift({ type: 'checkbox', width: 50 });
                selectionType = "Multiple";
            }
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/sample-delivery-challan/list?status=${status}&pageName=${pageName}&cTypeID=${cTypeID}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (isSampleDelChallanPage || isApprovePage) {
            if (isApprovePage) ElementDisplay(2);
            else ElementDisplay(1);
        }
        else if (isSampleGatepassPage) {
            ElementDisplay(3);
        }
        else if (isSampleGatepassApprovePage) {
            ElementDisplay(4);
        }
        else if (isSampleGatepassCheckoutPage) {
            ElementDisplay(5);
        }
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.SFDID, args.rowData.ConceptTypeID, args.rowData.GroupConceptNo);
        }

        else if (args.commandColumn.type == "ViewChallan") {
            window.open(`/reports/InlinePdfView?ReportName=SupplyDC.rdl&ChallanNo=${args.rowData.DCNo}`, '_blank');
        }
        else if (args.commandColumn.type == "ViewGatepass") {
            if (args.rowData.GPNo != '') {
                window.open(`/reports/InlinePdfView?ReportName=SupplyGP.rdl&GPNo=${args.rowData.GPNo}`, '_blank');
            } else {
                return toastr.error("Getpass No not found.");
            }
        }
    }

    function getNewData() {
        if ($tblMasterEl.getSelectedRecords().length == 0) {
            toastr.error("Please select row(s)!");
            return;
        }

        var uniqueAry = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "OrderUnitID");

        if (uniqueAry.length != 1) {
            toastr.error("Selected row(s) order unit should be same!");
            return;
        }
        var uniqueAry = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "CompanyID");

        if (uniqueAry.length != 1) {
            toastr.error("Selected row(s) company name should be same!");
            return;
        }

        var iDs = $tblMasterEl.getSelectedRecords().map(function (el) { return el.GroupConceptNo }).toString();
        var selectedRecords = $tblMasterEl.getSelectedRecords();
        var IsConceptTypeFlag = false;
        $.each(selectedRecords, function (j, obj) {
            if (obj.ConceptTypeID != 1) {
                IsConceptTypeFlag = true;
                return;
            }
        });

        axios.get(`/api/sample-delivery-challan/new?GroupConceptNo=${iDs}`)
            .then(function (response) {
                isEditable = true;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.DCDate = formatDateToDefault(masterData.DCDate);
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                masterData.GroupConceptNo = iDs;
                setFormData($formEl, masterData);
                initChildTable([]);
                initOtherItemTable([]);
                ElementDisplay(1);

                $formEl.find("#CompanyID").val(masterData.CompanyID).trigger('change');
                $formEl.find("#OrderUnitID").val(masterData.OrderUnitID).trigger('change');
                $formEl.find("#CompanyID").prop("disabled", true);
                $formEl.find("#OrderUnitID").prop("disabled", true);
                $formEl.find("#btnSave").fadeIn();
                $formEl.find("#divGPNO").fadeOut();
                $formEl.find("#divGPDate").fadeOut();

                if (!IsConceptTypeFlag) {
                    $formEl.find("#divOtherItem").fadeOut();
                } else {
                    $formEl.find("#divOtherItem").fadeIn();
                }
            })
            .catch(showResponseError);
    }

    function initChildTable(data) {
        if ($tblChildEl) {
            $tblChildEl.destroy();
            $(tblChildId).html("");
        }
        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowRowDragAndDrop: false,
            allowResizing: true,
            autofitColumns: false,
            selectionSettings: { type: 'Multiple' },
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Commands', width: 80, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'SFDChildID', isPrimaryKey: true, visible: false },
                { field: 'ConceptID', visible: false },
                { field: 'BookingChildID', visible: false },
                { field: 'BookingID', visible: false },
                { field: 'ConsumptionID', visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'ItemMasterID', visible: false },
                { field: 'CCColorID', visible: false },
                { field: 'ColorID', visible: false },
                { field: 'TechnicalNameId', visible: false },
                { field: 'KnittingTypeID', visible: false },
                { field: 'ConstructionID', visible: false },
                { field: 'CompositionID', visible: false },
                { field: 'GSMID', visible: false },
                { field: 'FUPartID', visible: false },
                { field: 'IsYD', visible: false },
                { field: 'TechnicalName', headerText: 'Technical Name', allowEditing: false },
                { field: 'Composition', headerText: 'Composition', allowEditing: false },
                { field: 'Gsm', headerText: 'GSM', allowEditing: false },
                { field: 'ColorCode', headerText: 'Color Code', allowEditing: false },
                { field: 'ColorName', headerText: 'Color Name', allowEditing: false },
                {
                    field: 'FormID', width: 200, headerText: 'Form', valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.FormList,
                    displayField: "Form",
                    edit: ej2GridDropDownObj({})
                },
                {
                    field: 'HangerQtyInPcs', headerText: 'Hanger Qty (Pcs)',

                },
                { field: 'BookingQty', headerText: 'Quantity(kg)', allowEditing: false },
                { field: 'StockQty', headerText: 'Stock Qty', allowEditing: false, editType: "numericedit", width: 80, edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
                { field: 'DCQty', headerText: 'Challan Qty', allowEditing: false, allowEditing: false, editType: "numericedit", width: 80, edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
                { field: 'Remarks', headerText: 'Remarks' }
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.SFDChildID = getMaxIdForArray(masterData.Childs, "SFDChildID");
                }
                else if (args.requestType === "delete") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(masterData.Childs, "SFDChildID");
                    var ChildsItemSegmentsList = $tblChildEl.getCurrentViewRecords();
                    masterData.Childs[0].FabricItemIDs = ChildsItemSegmentsList.map(function (el) {
                        if (args.data[0].SFDChildID != el.SFDChildID) {
                            return el.SFDChildID
                        }
                    }).toString();
                }
                else if (args.requestType === "save") {
                    if (args.data.FormID == null || typeof args.data.FormID === "undefined" || args.data.FormID == 0) {
                        toastr.error("Select Form");
                        args.data.editable = true;
                        return;
                    }
                    if (args.data.FormID != 1120) {
                        args.rowData = args.data;
                        args.rowData.HangerQtyInPcs = 0;
                        $tblChildEl.updateRow(args.rowIndex, args.rowData);
                    }
                }
            },
            childGrid: {
                queryString: 'SFDChildID',
                allowResizing: true,
                toolbar: [
                    { text: 'Add Roll', tooltipText: 'Add Roll', prefixIcon: 'e-icons e-add', id: 'addItem' },
                    { text: 'Split Roll', tooltipText: 'Split Roll', prefixIcon: 'e-icons e-copy', id: 'splitRoll' }
                ],
                editSettings: { allowAdding: true, allowEditing: false, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: [
                    {
                        headerText: 'Action', width: 10, commands: [
                            //{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                        ]
                    },
                    { field: 'SFDChildRollID', isPrimaryKey: true, visible: false },
                    { field: 'SFDChildID', visible: false, width: 40 },
                    { field: 'BookingChildID', visible: false, width: 40 },
                    { field: 'BookingID', visible: false, width: 40 },
                    { field: 'ConsumptionID', visible: false, width: 40 },
                    { field: 'SubGroupID', visible: false, width: 40 },
                    { field: 'ItemMasterID', visible: false, width: 40 },
                    { field: 'RollID', visible: false, width: 40 },
                    { field: 'RollNo', headerText: 'Roll No', allowEditing: false, allowResizing: true, width: 40 },
                    //{ field: 'Shade', headerText: 'Shade', width: 100, minWidth: 100, maxWidth: 100 },
                    { field: 'BatchNo', headerText: 'Batch No', allowResizing: true, width: 40 },
                    { field: 'UseBatchNo', visible: false, width: 40 },
                    { field: 'RackID', visible: false, width: 40 },
                    { field: 'WeightSheetNo', visible: false, width: 40 },
                    { field: 'RollQtyKg', width: 40, allowResizing: true, headerText: 'Roll Qty(kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
                    { field: 'RollQtyPcs', width: 120, allowResizing: true, headerText: 'Roll Qty(Pcs)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } }
                ],
                toolbarClick: function (args) {
                    var iDCQty = 0;
                    var data = this.parentDetails.parentRowData;
                    selectedData = data;

                    if (args.item.id === "addItem") {

                        setSubGroupValue(masterData.ChildItems);
                        var ChildItemsFabric = masterData.ChildItems.filter(function (el) {
                            return el.SubGroupName == subGroupNames.FABRIC &&
                                el.ColorID == selectedData.ColorID
                        });
                        if (ChildItemsFabric.length==0) {
                            toastr.error("No Roll Found");
                            return false;
                        }

                        var ChildsFabric = masterData.Childs.filter(function (el) {
                            return el.SubGroupName == subGroupNames.FABRIC &&
                                el.ColorID == selectedData.ColorID
                        });

                        var FabricItemList = new Array();

                        if (ChildItemsFabric.length > 0) {
                            //var rollList = ChildItemsFabric.filter(x => x.TechnicalName == selectedData.TechnicalName);

                            var SFDChildRollIDs = "";
                            var childs = $tblChildEl.getCurrentViewRecords();
                            var childObj = childs[0];
                            if (typeof childObj.ChildItems !== "undefined" && childObj.ChildItems != null) {
                                SFDChildRollIDs = childObj.ChildItems.map(x => x.SFDChildRollID).join(",");
                            }

                            var rollList = ChildItemsFabric;
                            var finder = new commonFinder({
                                title: "Select Items",
                                pageId: pageId,
                                data: rollList, //masterData.Childs,
                                fields: "RollNo,RollQtyKg,RollQtyPcs,BatchNo,GroupConceptNo",
                                headerTexts: "Roll No,Roll Qty(kg),Roll Qty(Pcs),Batch No,Group Concept No",
                                isMultiselect: true,
                                selectedIds: SFDChildRollIDs,
                                allowPaging: false,
                                primaryKeyColumn: "SFDChildRollID",
                                onMultiselect: function (selectedRecords) {
                                    for (var i = 0; i < selectedRecords.length; i++) {
                                        var oPreProcess = {
                                            SFDChildRollID: selectedRecords[i].SFDChildRollID,
                                            SFDChildID: data.SFDChildID,
                                            GroupConceptNo: selectedRecords[i].GroupConceptNo,
                                            ConceptID: selectedRecords[i].ConceptID,
                                            CCColorID: selectedRecords[i].CCColorID,
                                            ColorID: selectedRecords[i].ColorID,
                                            BChildID: selectedRecords[i].BChildID,
                                            BItemReqID: selectedRecords[i].BItemReqID,
                                            BatchID: selectedRecords[i].BatchID,
                                            RollQtyKg: selectedRecords[i].RollQtyKg,
                                            RollQtyPcs: selectedRecords[i].RollQtyPcs,
                                            RollID: selectedRecords[i].RollID,
                                            RollNo: selectedRecords[i].RollNo,
                                            SubGroupID: selectedRecords[i].SubGroupID,
                                            SubGroupName: selectedRecords[i].SubGroupName,
                                            BatchNo: selectedRecords[i].BatchNo
                                        }
                                        FabricItemList.push(oPreProcess);
                                        iDCQty += parseFloat(selectedRecords[i].RollQtyKg);
                                    }
                                    var indexFind = ChildsFabric.findIndex(x => x.SFDChildID == data.SFDChildID);
                                    ChildsFabric[indexFind].ChildItems = FabricItemList;
                                    ChildsFabric[indexFind].DCQty = iDCQty;
                                    var index = $tblChildEl.getRowIndexByPrimaryKey(data.SFDChildID);
                                    $tblChildEl.updateRow(index, ChildsFabric[indexFind]);
                                    $tblChildEl.refreshColumns;
                                }
                            });
                            finder.showModal();
                        }
                    }
                    else if (args.item.id === "splitRoll") {

                        setSubGroupValue(masterData.ChildItems);
                        var ChildItemsFabric = masterData.ChildItems.filter(function (el) {
                            return el.SubGroupName == subGroupNames.FABRIC &&
                                el.ColorID == selectedData.ColorID
                        });
                        initMachineParamTable(ChildItemsFabric);
                        $("#modal-machine").modal('show');

                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "add") {

                    }
                    //else if (args.requestType === "save") {
                    //    var data = this.parentDetails.parentRowData;
                    //}
                },
                load: loadFabricFirstLevelChildGrid
            }
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }

    function loadFabricFirstLevelChildGrid() {
        this.dataSource = this.parentDetails.parentRowData.ChildItems;
    }

    function initOtherItemTable(data) {
        if ($tblOtherItemEl) {
            $tblOtherItemEl.destroy();
            $(tblOtherItemId).html("");
        }
        ej.base.enableRipple(true);
        $tblOtherItemEl = new ej.grids.Grid({
            dataSource: data,
            allowRowDragAndDrop: false,
            allowResizing: true,
            autofitColumns: false,
            selectionSettings: { type: 'Multiple' },
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Commands', width: 80, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'SFDChildID', isPrimaryKey: true, visible: false },
                { field: 'ConceptID', visible: false },
                { field: 'BookingChildID', visible: false },
                { field: 'BookingID', visible: false },
                { field: 'ConsumptionID', visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'ItemMasterID', visible: false },
                { field: 'CCColorID', visible: false },
                { field: 'ColorID', visible: false },
                { field: 'TechnicalNameId', visible: false },
                { field: 'KnittingTypeID', visible: false },
                { field: 'ConstructionID', visible: false },
                { field: 'CompositionID', visible: false },
                { field: 'GSMID', visible: false },
                { field: 'Composition', visible: false },
                { field: 'Gsm', visible: false },
                { field: 'FUPartID', visible: false },
                { field: 'IsYD', visible: false },
                { field: 'SubGroupName', headerText: 'End Use', allowEditing: false },
                { field: 'SubClassName', headerText: 'Machine Type', allowEditing: false },
                { field: 'TechnicalName', headerText: 'Technical Name', allowEditing: false },
                { field: 'Length', headerText: 'Length(CM)', allowEditing: false },
                { field: 'Width', headerText: 'Height(CM)', allowEditing: false },
                { field: 'ColorCode', headerText: 'Color Code', allowEditing: false },
                { field: 'ColorName', headerText: 'Color Name', allowEditing: false },
                {
                    field: 'FormID', width: 200, headerText: 'Form', valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.FormList,
                    displayField: "Form",
                    edit: ej2GridDropDownObj({})
                },
                {
                    field: 'HangerQtyInPcs', headerText: 'Hanger Qty (Pcs)',

                },
                { field: 'BookingQtyPcs', headerText: 'Quantity(Pcs)', allowEditing: false },
                { field: 'StockQtyPcs', headerText: 'Stock Qty(Pcs)', allowEditing: false, editType: "numericedit", width: 80, edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                { field: 'DCQtyPcs', headerText: 'Challan Qty(Pcs)', editType: "numericedit", width: 80, edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                { field: 'Remarks', headerText: 'Remarks' }
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.SFDChildID = getMaxIdForArray(masterData.Childs, "SFDChildID");
                }
                else if (args.requestType === "delete") {
                    var index = $tblOtherItemEl.getRowIndexByPrimaryKey(masterData.Childs, "SFDChildID");
                    var ChildsItemSegmentsList = $tblOtherItemEl.getCurrentViewRecords();
                    masterData.Childs[0].OthersItemIDs = ChildsItemSegmentsList.map(function (el) {
                        if (args.data[0].SFDChildID != el.SFDChildID) {
                            return el.SFDChildID
                        }
                    }).toString();
                }
                else if (args.requestType === "save") {
                    var index = $tblOtherItemEl.getRowIndexByPrimaryKey(args.rowData.SFDChildID);
                    if (args.data.FormID == null || typeof args.data.FormID === "undefined" || args.data.FormID == 0) {
                        toastr.error("Select Form");
                        args.data.editable = true;
                        return;
                    }
                    if (args.data.FormID != 1120) {
                        args.rowData = args.data;
                        args.rowData.HangerQtyInPcs = 0;
                        $tblChildEl.updateRow(args.rowIndex, args.rowData);
                    }
                }
            },
            childGrid: {
                queryString: 'SFDChildID',
                allowResizing: true,
                toolbar: [
                    { text: 'Add Item', tooltipText: 'Add Item', prefixIcon: 'e-icons e-add', id: 'addOthersItem' },
                    { text: 'Split Roll', tooltipText: 'Split Roll', prefixIcon: 'e-icons e-copy', id: 'splitRollOthers' }
                ],
                editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: [
                    {
                        headerText: 'Action', width: 60, commands: [
                            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                        ]
                    },
                    { field: 'SFDChildRollID', isPrimaryKey: true, visible: false },
                    { field: 'BookingChildID', visible: false },
                    { field: 'BookingID', visible: false },
                    { field: 'ConsumptionID', visible: false },
                    { field: 'SubGroupID', visible: false },
                    { field: 'ItemMasterID', visible: false },
                    { field: 'RollID', visible: false },
                    { field: 'RollNo', headerText: 'Roll No', allowEditing: false },
                    //{ field: 'Shade', headerText: 'Shade' },
                    { field: 'BatchNo', headerText: 'Batch No' },
                    { field: 'UseBatchNo', visible: false },
                    { field: 'RackID', visible: false },
                    { field: 'WeightSheetNo', visible: false },
                    { field: 'RollQtyKg', headerText: 'Roll Qty(kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
                    { field: 'RollQtyPcs', headerText: 'Roll Qty(Pcs)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } }
                ],
                toolbarClick: function (args) {
                    var iDCQty = 0;
                    var data = this.parentDetails.parentRowData;
                    selectedData = data;

                    if (args.item.id === "addOthersItem") {
                        var ChildItemsOthers = masterData.ChildItems.filter(function (el) {
                            return el.SubGroupName != subGroupNames.FABRIC &&
                                el.ColorID == selectedData.ColorID
                        });

                        var ChildsOthers = masterData.Childs.filter(function (el) {
                            return el.SubGroupName != subGroupNames.FABRIC &&
                                el.ColorID == selectedData.ColorID
                        });

                        var OthersItemList = new Array();
                        if (ChildItemsOthers.length > 0) {
                            var finder = new commonFinder({
                                title: "Select Items",
                                pageId: pageId,
                                data: ChildItemsOthers, //masterData.Childs,
                                fields: "RollNo,RollQtyKg,RollQtyPcs,BatchNo,GroupConceptNo",
                                headerTexts: "Roll No,Roll Qty(Kg),Roll Qty(Pcs),Batch No,Group Concept No",
                                isMultiselect: true,
                                //selectedIds: masterData.Childs[0].SFDChildIDs,
                                allowPaging: false,
                                primaryKeyColumn: "SFDChildRollID",
                                onMultiselect: function (selectedRecords) {
                                    for (var i = 0; i < selectedRecords.length; i++) {
                                        var oPreProcess = {
                                            SFDChildRollID: selectedRecords[i].SFDChildRollID,
                                            SFDChildID: selectedData.SFDChildID,
                                            GroupConceptNo: selectedRecords[i].GroupConceptNo,
                                            ConceptID: selectedRecords[i].ConceptID,
                                            CCColorID: selectedRecords[i].CCColorID,
                                            ColorID: selectedRecords[i].ColorID,
                                            BChildID: selectedRecords[i].BChildID,
                                            BItemReqID: selectedRecords[i].BItemReqID,
                                            BatchID: selectedRecords[i].BatchID,
                                            RollQtyKg: selectedRecords[i].RollQtyKg,
                                            RollQtyPcs: selectedRecords[i].RollQtyPcs,
                                            RollNo: selectedRecords[i].RollNo,
                                            SubGroupID: selectedRecords[i].SubGroupID,
                                            SubGroupName: selectedRecords[i].SubGroupName,
                                            BatchNo: selectedRecords[i].BatchNo
                                        }
                                        OthersItemList.push(oPreProcess);
                                        iDCQty += parseFloat(selectedRecords[i].RollQtyKg);
                                    }
                                    var indexFind = ChildsOthers.findIndex(x => x.SFDChildID == data.SFDChildID);
                                    ChildsOthers[indexFind].ChildItems = OthersItemList;
                                    ChildsOthers[indexFind].DCQty = iDCQty;
                                    var index = $tblOtherItemEl.getRowIndexByPrimaryKey(data.SFDChildID);
                                    $tblOtherItemEl.updateRow(index, ChildsOthers[indexFind]);
                                    $tblOtherItemEl.refreshColumns;
                                }
                            });
                            finder.showModal();
                        }
                    }
                    else if (args.item.id === "splitRollOthers") {
                        setSubGroupValue(masterData.ChildItems);
                        var ChildItemsFabric = masterData.ChildItems.filter(function (el) {
                            return el.SubGroupName != subGroupNames.FABRIC &&
                                el.ColorID == selectedData.ColorID
                        });
                        initMachineParamTable(ChildItemsFabric);
                        $("#modal-machine").modal('show');

                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "add") {

                    }
                },
                load: loadOthersFirstLevelChildGrid
            }
        });
        $tblOtherItemEl.refreshColumns;
        $tblOtherItemEl.appendTo(tblOtherItemId);
    }
    function setSubGroupValue(childItems) {
        _subGroup = 0;
        if (childItems != null && childItems.length > 0) {
            if (childItems[0].SubGroupName == "Fabric") _subGroup = 1;
            else if (childItems[0].SubGroupName == "Collar") _subGroup = 11;
            else if (childItems[0].SubGroupName == "Cuff") _subGroup = 12;
        }
    }
    function loadOthersFirstLevelChildGrid() {
        this.dataSource = this.parentDetails.parentRowData.ChildItems;
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
        $formEl.find("#ConceptID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function initMachineParamTable(list) {

        tblMachineParamId.bootstrapTable("destroy");
        tblMachineParamId.bootstrapTable({
            editable: true,
            columns: [
                {
                    field: "RollID",
                    title: "Roll ID",
                    align: 'left',
                    visible: false
                },
                {
                    field: "RollNo",
                    title: "Roll No",
                    align: 'left'
                },
                {
                    field: "RollQtyKg",
                    title: "Roll Qty (KG)",
                    align: 'right'
                },
                {
                    field: "RollQtyPcs",
                    title: "Roll Qty (PCS)",
                    visible: _subGroup != 1,
                    align: 'right'
                },
                {
                    field: "BatchNo",
                    title: "Batch No",
                    align: 'left'
                },
                //{
                //    field: "GroupConceptNo",
                //    title: "Group Concept No",
                //    align: 'left',
                //},
                {
                    field: "",
                    title: "Split",
                    filterControl: "input",
                    align: 'center',
                    formatter: function (value, row, index, field) {
                        return '<button type="button" id="btnSplitRoll" class="split">Split</button >';
                    },
                    events: {
                        'click .split': function (e, value, row, index) {
                            e.preventDefault();
                            initRollTable();
                            $tblKProductionE1.data("roll", row);
                            $tblKProductionE1.data("index", index);
                            showRoll(row.RollID);

                            totalQty = row.RollQtyKg;
                        },
                    }
                }
            ],
            data: list,
            onEditableSave: function (field, row, oldValue, $el) {
                //row.ParamValue
            },
        });
    }
    function initRollTable() {

        $tblRollEl.bootstrapTable("destroy");
        $tblRollEl.bootstrapTable({
            showFooter: true,
            columns: [
                {
                    field: "BatchID",
                    title: "BatchID",
                    visible: false,
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "RollNo",
                    title: "Roll No",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "RollQtyKg",
                    title: "Roll Qty (Kg)",
                    visible: _subGroup == 1,
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" step="0.01" pattern="^\d+(?:\.\d{1,2})?$" style="padding-right: 24px;">',
                        validate: function (value) {
                            //if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                            //    return 'Must be a positive integer.';
                            //}
                        }
                    }
                },
                {
                    field: "RollQtyPcs",
                    title: "Roll Qty (Pcs)",
                    visible: _subGroup != 1,
                    cellStyle: function () { return { classes: 'm-w-80' } },
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
                }
            ],
            //onEditableSave: function (value, row, index, field) {
            //    var remainQty = parseFloat($tblKProductionE1.data("roll").RollQty) - parseFloat(row.RollQty);
            //    var rows = $tblRollEl.bootstrapTable('getData');
            //    for (var i = 0; i < rows.length; i++) {
            //        if (row.RollNo != rows[i].RollNo) {
            //            rows[i].RollQty = (parseFloat(remainQty) / parseInt(rows.length - 1)).toFixed(2);
            //        }
            //    }
            //    $tblRollEl.bootstrapTable("load", rows);
            //    $tblRollEl.bootstrapTable('hideLoading');
            //}
        });
    }
    function split() {

        var splitList = [];
        var totalRoll = parseInt($formEl.find("#NoOfSplit").val());
        var roll = $tblKProductionE1.data("roll");
        for (var i = 0; i < totalRoll; i++) {
            var obj = {
                RollID: roll.RollID,
                RollNo: roll.RollNo + '_' + (i + 1),
                BatchID: roll.BatchID,
                BatchNo: roll.BatchNo,
                InActive: 0,
                IsSave: false
            }
            if (_subGroup == 1) {
                obj.RollQtyKg = (parseFloat(roll.RollQtyKg) / totalRoll).toFixed(2);
                obj.RollQtyPcs = 0;
            } else {
                obj.RollQtyPcs = (parseInt(roll.RollQtyPcs) / totalRoll);
                obj.RollQtyKg = (parseFloat(roll.RollQtyKg) / totalRoll).toFixed(2);;
            }
            splitList.push(obj);
        }
        $tblRollEl.bootstrapTable("load", splitList);
        $tblRollEl.bootstrapTable('hideLoading');
        $formEl.find("#btnSaveKProd").fadeIn();
    }

    function showRoll(rollId) {
        if (rollId > 0) {
            var ChildItemsFabric = masterData.ChildItems.filter(x => x.RollID == rollId);
            if (ChildItemsFabric.length > 0) {
                $tblRollEl.bootstrapTable("load", ChildItemsFabric);
                $tblRollEl.bootstrapTable('hideLoading');
                $formEl.find("#btnSaveKProd").fadeIn();
            }
            $formEl.find("#NoOfSplit").val(0);
            $("#modal-child").modal('show');

            var gRollID = $tblKProductionE1.data("roll").RollID;
            axios.get(`/api/batch/get-roll/${gRollID}`)
                .then(function (response) {

                    if (response.data.length > 0) {
                        response.data.map(x => {
                            x.RollQtyKg = x.RollQty;
                        });
                        $tblRollEl.bootstrapTable("load", response.data);
                        $tblRollEl.bootstrapTable('hideLoading');
                        $formEl.find("#btnSaveKProd").fadeOut();
                    } else {
                        $tblRollEl.bootstrapTable("load", [$tblKProductionE1.data("roll")]);
                        $tblRollEl.bootstrapTable('hideLoading');
                        $formEl.find("#btnSaveKProd").fadeIn();
                    }
                    $formEl.find("#NoOfSplit").val(0);
                    $("#modal-child").modal('show');
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });
        }
    }

    function saveKProd() {

        var rows = $tblRollEl.bootstrapTable('getData');
        rows.map(x => {
            x.GRollID = x.RollID;
            x.RollQty = x.RollQtyKg;
        });

        var splitTotalQty = rows.reduce((total, row) => total + parseFloat(row.RollQtyKg), 0);
        if (splitTotalQty > totalQty || splitTotalQty < totalQty) {
            toastr.error(`Total Split Qty of Roll must be equal to ${totalQty}`);
            return false;
        }


        axios.post("/api/batch/save-kProd", rows)
            .then(function (response) {
                $tblRollEl.bootstrapTable("load", response.data.newSplitedList);
                $tblRollEl.bootstrapTable('hideLoading');
                toastr.success("Saved successfully!");
                $('#btnCloseRollSplitted').click();
                var aaa = response.data.totalUpdatedList;
                response.data.newSplitedList.map(x => {
                    var index = masterData.ChildItems.findIndex(y => y.RollID == x.GRollID);
                    if (index < 0) {
                        var hasList = masterData.ChildItems != null && masterData.ChildItems.length > 0 ? true : false;
                        x.RollQtyKg = x.RollQty;
                        x.RollID = x.GRollID;
                        x.BatchID = hasList ? masterData.ChildItems[0].BatchID : 0;
                        x.BatchNo = hasList ? masterData.ChildItems[0].BatchNo : 0;
                        x.SubGroupID = hasList ? masterData.ChildItems[0].SubGroupID : _subGroup;
                        x.SubGroupName = hasList ? masterData.ChildItems[0].SubGroupName : getSubGroupName(x.SubGroupID);
                        x.GroupConceptNo = hasList ? masterData.ChildItems[0].GroupConceptNo : "";
                        masterData.ChildItems.push(x);
                        index = masterData.ChildItems.findIndex(y => y.RollID == x.ParentGRollID);
                        if (index > -1) {
                            masterData.ChildItems.splice(index, 1);
                        }
                    }
                });
                initMachineParamTable(masterData.ChildItems);
                //masterData.KnittingProductions = response.data.totalUpdatedList;
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function getSubGroupName(subGroupID) {
        if (subGroupID == 1) return "Fabric";
        else if (subGroupID == 11) return "Collar";
        else if (subGroupID == 12) return "Cuff";
    }
    function getDetails(id, ConceptTypeID, groupConceptNo) {
        axios.get(`/api/sample-delivery-challan/${id}/${groupConceptNo}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.DCDate = formatDateToDefault(masterData.DCDate);
                masterData.GPDate = formatDateToDefault(masterData.GPDate);
                masterData.GroupConceptNo = masterData.GroupConceptNo;
                setFormData($formEl, masterData);
                $formEl.find("#divGPNO").fadeIn();
                $formEl.find("#divGPDate").fadeIn();
                //Radio Button
                if (masterData.DeliveryItemType == 1) {
                    $("#Non-Organic").prop("checked", true);
                }
                else if (masterData.DeliveryItemType == 2) {
                    $("#Organic").prop("checked", true);
                } else {
                    $("#Fair-Trade").prop("checked", true);
                }
                //Checkbox Button
                if (masterData.IsReDelivery == 1) {
                    $("#IsReDelivery").prop("checked", true);
                }

                var FabricData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.FABRIC });
                initChildTable(FabricData);
                var OtherData = masterData.Childs.filter(function (el) { return el.SubGroupName != subGroupNames.FABRIC });
                initOtherItemTable(OtherData);

                if (ConceptTypeID == 1) {
                    $formEl.find("#divOtherItem").fadeOut();
                } else {
                    $formEl.find("#divOtherItem").fadeIn();
                }
            })
            .catch(showResponseError);
    }

    function ElementDisplay(flag) {
        if (flag == 1)  //Sample Delivery Challan
        {
            if (status == statusConstants.PENDING || status == statusConstants.AWAITING_PROPOSE) {
                $formEl.find("#btnSave").fadeIn();
                $formEl.find("#btnSaveAndSend").fadeIn();
                $formEl.find("#btnApprove").fadeOut();

                //GP
                $formEl.find("#btnSaveGP").fadeOut();
                $formEl.find("#btnApproveGP").fadeOut();

                //GP Checkout
                $formEl.find("#btnSaveGPCheckOut").fadeOut();

                IsElementTrue(false);
            }
            else if (status == statusConstants.PROPOSED || status == statusConstants.APPROVED) {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
                $formEl.find("#btnApprove").fadeOut();
                //GP
                $formEl.find("#btnSaveGP").fadeOut();
                $formEl.find("#btnApproveGP").fadeOut();

                //GP Checkout
                $formEl.find("#btnSaveGPCheckOut").fadeOut();
                IsElementTrue(true);
            }
        }
        else if (flag == 2)  //Sample Delivery Challan Approval
        {
            IsElementTrue(true);
            if (status == statusConstants.PROPOSED) {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
                $formEl.find("#btnApprove").fadeIn();

                //GP
                $formEl.find("#btnSaveGP").fadeOut();
                $formEl.find("#btnApproveGP").fadeOut();

                //GP Checkout
                $formEl.find("#btnSaveGPCheckOut").fadeOut();
            }
            else if (status == statusConstants.APPROVED) {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
                $formEl.find("#btnApprove").fadeOut();

                //GP
                $formEl.find("#btnSaveGP").fadeOut();
                $formEl.find("#btnApproveGP").fadeOut();

                //GP Checkout
                $formEl.find("#btnSaveGPCheckOut").fadeOut();
            }


        }
        else if (flag == 3)  //Sample Gatepass
        {
            IsElementTrue(true);
            if (status == statusConstants.PENDING) {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
                $formEl.find("#btnApprove").fadeOut();

                //GP
                $formEl.find("#btnSaveGP").fadeIn();
                $formEl.find("#btnApproveGP").fadeOut();

                //GP Checkout
                $formEl.find("#btnSaveGPCheckOut").fadeOut();

                $formEl.find("#VehicleTypeID").prop("disabled", false);
                $formEl.find("#VehicleNoID").prop("disabled", false);
                $formEl.find("#DriverName").prop("disabled", false);
                $formEl.find("#DriverContact").prop("disabled", false);
            }
            else if (status == statusConstants.PROPOSED) {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
                $formEl.find("#btnApprove").fadeOut();

                //GP
                $formEl.find("#btnSaveGP").fadeOut();
                $formEl.find("#btnApproveGP").fadeOut();

                //GP Checkout
                $formEl.find("#btnSaveGPCheckOut").fadeOut();
            }
            else if (status == statusConstants.APPROVED) {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
                $formEl.find("#btnApprove").fadeOut();

                //GP
                $formEl.find("#btnSaveGP").fadeOut();
                $formEl.find("#btnApproveGP").fadeOut();

                //GP Checkout
                $formEl.find("#btnSaveGPCheckOut").fadeOut();
            }
        }
        else if (flag == 4)  //Sample Gatepass Approve
        {
            IsElementTrue(true);
            if (status == statusConstants.PROPOSED) {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
                $formEl.find("#btnApprove").fadeOut();

                //GP
                $formEl.find("#btnSaveGP").fadeOut();
                $formEl.find("#btnApproveGP").fadeIn();

                //GP Checkout
                $formEl.find("#btnSaveGPCheckOut").fadeOut();
            }
            else if (status == statusConstants.APPROVED) {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
                $formEl.find("#btnApprove").fadeOut();

                //GP
                $formEl.find("#btnSaveGP").fadeOut();
                $formEl.find("#btnApproveGP").fadeOut();

                //GP Checkout
                $formEl.find("#btnSaveGPCheckOut").fadeOut();
            }
        }
        else if (flag == 5)  //Sample Gatepass Checkout
        {
            IsElementTrue(true);
            if (status == statusConstants.PENDING) {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
                $formEl.find("#btnApprove").fadeOut();

                //GP
                $formEl.find("#btnSaveGP").fadeOut();
                $formEl.find("#btnApproveGP").fadeOut();

                //GP Checkout
                $formEl.find("#btnSaveGPCheckOut").fadeIn();
            }
            else if (status == statusConstants.APPROVED) {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
                $formEl.find("#btnApprove").fadeOut();

                //GP
                $formEl.find("#btnSaveGP").fadeOut();
                $formEl.find("#btnApproveGP").fadeOut();

                //GP Checkout
                $formEl.find("#btnSaveGPCheckOut").fadeOut();
            }
        }
    }

    function IsElementTrue(flag) {
        if (flag) {
            $formEl.find("#Remarks").prop("disabled", true);
            $formEl.find("#CompanyID").prop("disabled", true);
            $formEl.find("#OrderUnitID").prop("disabled", true);
            $formEl.find("#DeliveryFromID").prop("disabled", true);
            $formEl.find("#DeliveryPlaceID").prop("disabled", true);
            $formEl.find("#VehicleTypeID").prop("disabled", true);
            $formEl.find("#VehicleNoID").prop("disabled", true);
            $formEl.find("#DriverName").prop("disabled", true);
            $formEl.find("#DriverContact").prop("disabled", true);
            $formEl.find("#LockNo").prop("disabled", true);
            $formEl.find("#Non-Organic").prop("disabled", true);
            $formEl.find("#Organic").prop("disabled", true);
            $formEl.find("#Fair-Trade").prop("disabled", true);
            $formEl.find("#IsReDelivery").prop("disabled", true);
        }
        else {
            $formEl.find("#Remarks").prop("disabled", false);
            $formEl.find("#CompanyID").prop("disabled", false);
            $formEl.find("#OrderUnitID").prop("disabled", false);
            $formEl.find("#DeliveryFromID").prop("disabled", false);
            $formEl.find("#DeliveryPlaceID").prop("disabled", false);
            $formEl.find("#VehicleTypeID").prop("disabled", false);
            $formEl.find("#VehicleNoID").prop("disabled", false);
            $formEl.find("#DriverName").prop("disabled", false);
            $formEl.find("#DriverContact").prop("disabled", false);
            $formEl.find("#LockNo").prop("disabled", false);
            //$formEl.find("#Non-Organic").prop("disabled", false);
            //$formEl.find("#Organic").prop("disabled", false);
            //$formEl.find("#Fair-Trade").prop("disabled", false);
            //$formEl.find("#IsReDelivery").prop("disabled", false);
        }
    }

    var validationConstraints = {
        CompanyID: {
            presence: true
        },
        OrderUnitID: {
            presence: true
        },
        DeliveryFromID: {
            presence: true
        },
        DeliveryPlaceID: {
            presence: true
        }
        //,
        //VehicleTypeID: {
        //    presence: true
        //}
        //,
        //DeliveryItemType: {
        //    presence: true
        //}
    }
    var validationConstraintsGP = {
        VehicleTypeID: {
            presence: true
        }
        ,
        VehicleNoID: {
            presence: true
        },
        DriverName: {
            presence: true
        },
        DriverContact: {
            presence: true
        }
    }

    function isValidChildForm(data) {
        var isValidItemInfo = false;


        $.each(data, function (i, obj) {
            if (obj.FormID == null || obj.FormID == undefined || obj.FormID <= 0) {
                toastr.error("Form is required.");
                isValidItemInfo = true;
            }
            if (obj.FormID == 1120) {
                allowEditing: true;
                //isValidItemInfo = true;
            }
            if (obj.SubGroupID == 1) {
                if (obj.DCQty == null || obj.DCQty == undefined || obj.DCQty <= 0) {
                    toastr.error("Challan Qty is required.");
                    isValidItemInfo = true;
                }
            }
            if (obj.SubGroupID != 1) {
                if (obj.DCQtyPcs == null || obj.DCQtyPcs == undefined || obj.DCQtyPcs <= 0) {
                    toastr.error("Challan Qty(Pcs) is required.");
                    isValidItemInfo = true;
                }
            }
        });

        //$.each(data, function (i, obj) {
        //    $.each(obj.ChildItems, function (j, objChild) {
        //        if (objChild.Shade == null || objChild.Shade == "") {
        //            toastr.error("Shade is required.");
        //            isValidItemInfo = true;
        //        }
        //    });
        //});

        return isValidItemInfo;
    }

    function save(flag) {
        var DataList = [];
        var DataListMaster = new Array();
        var disabled = $formEl.find(':input:disabled').removeAttr('disabled');
        var data = formDataToJson($formEl.serializeArray());
        disabled.attr('disabled', 'disabled');
        //Fabric
        var fabrics = $tblChildEl.getCurrentViewRecords();
        for (var i = 0; i < fabrics.length; i++) {
            if (fabrics[i].ChildItems) {
                fabrics[i].ChildItems.map(x => {
                    var roll = masterData.ChildItems.find(y => y.RollNo == x.RollNo);
                    if (roll) {
                        x.RollID = roll.RollID;
                    }
                });
            }
            DataList.push(fabrics[i]);
        }
        DataListMaster.push(data);

        //Others
        var others = $tblOtherItemEl.getCurrentViewRecords();
        for (var i = 0; i < others.length; i++) {
            if (others[i].ChildItems) {
                others[i].ChildItems.map(x => {
                    var roll = masterData.ChildItems.find(y => y.RollNo == x.RollNo);
                    if (roll) {
                        x.RollID = roll.RollID;
                    }
                });
            }
            DataList.push(others[i]);
        }
        DataListMaster.push(data);

        data["Childs"] = DataList;

        if (data.Childs.length === 0) return toastr.error("At least 1 items is required.");

        //Checkbox Assign
        data.IsReDelivery;
        if ($formEl.find("#IsReDelivery").is(':checked'))
            data.IsReDelivery = 1;
        else {
            data.IsReDelivery = 0;
        }
        //Get Radio Button value 
        if ($formEl.find("#Non-Organic").is(':checked')) {
            data.DeliveryItemType = 1;
        }
        else if ($formEl.find("#Organic").is(':checked')) {
            data.DeliveryItemType = 2;
        }
        else {
            data.DeliveryItemType = 3;
        }
        data.IsBDS = vIsBDS;
        data.DCSendForApproval = flag;
        data.VehicleType = $formEl.find("#VehicleTypeID option:selected").text();
        data.VehicleNo = $formEl.find("#VehicleNoID option:selected").text();

        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        //Child Item Validation
        if (isValidChildForm(data["Childs"])) return;

        axios.post("/api/sample-delivery-challan/save", data)
            .then(function (response) {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();