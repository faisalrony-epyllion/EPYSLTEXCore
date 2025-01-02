(function () {
    var currentChildRowData;
    var menuId, pageName, menuParam;
    var toolbarId, pageId, pageIdWithHash;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildItemId;
    var tblCreateCompositionId, $tblCreateCompositionEl;
    var status, id, buyerTeamIDs;

    var isPendingPYBPage = false;
    var isApprovePage = false;
    var isAcknowledgePage = false;
    var isPendingMnMPage = false;
    var isApproveMnMPage = false;
    var isAcknowledgeMnMPage = false;

    var isEditable = false;
    var isAcknowledge = false;
    var isMarketingFlag = false;
    var status = statusConstants.ADDITIONAL;
    var masterData, maxCol = 999;
    var sum = 0;
    var isBlended = false;
    var compositionComponents = [];
    var _segments = [];

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");

        pageId = pageName + "-" + menuId;
        pageIdWithHash = "#" + pageId;
        $pageEl = $(`#${pageId}`);
        //console.log(pageId);
        //$pageEl = $(pageConstants.PAGE_ID_PREFIX + pageId);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildItemId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblCreateCompositionId = `#tblCreateComposition-${pageId}`;
        
        isAcknowledgePage = false;
        isApprovePage = false;
        isPendingMnMPage = false;
        isApproveMnMPage = false;
        isAcknowledgeMnMPage = false;

        if (menuParam === "Ack") {
            isAcknowledgePage = true;
        }
        else if (menuParam === "A") {
            isApprovePage = true;
        }
        else if (menuParam === "PMM") {
            isPendingMnMPage = true;
        }
        else if (menuParam === "AMM") {
            isApproveMnMPage = true;
        }
        else if (menuParam === "AckMM") {
            isAcknowledgeMnMPage = true;
        }


        //isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        //isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        //isPendingMnMPage = convertToBoolean($(`#${pageId}`).find("#PendingMnMPage").val());
        //isApproveMnMPage = convertToBoolean($(`#${pageId}`).find("#ApproveMnMPage").val());
        //isAcknowledgeMnMPage = convertToBoolean($(`#${pageId}`).find("#AcknowledgeMnMPage").val());




        if (!isAcknowledgePage &&
            !isApprovePage &&
            !isPendingMnMPage &&
            !isApproveMnMPage &&
            !isAcknowledgeMnMPage) {
            isPendingPYBPage = true;
        }
        else {
            isPendingPYBPage = false;
        }
        
        $formEl.find("#divRevisionNo").fadeOut();
        $formEl.find("#divRevisionDate").fadeOut();
        $formEl.find("#divRevisionReason").fadeOut();

        $toolbarEl.find("#btnNew,#btnPendingList,#btnSendList,#btnPendingApprovalList,#btnApprovedList,#btnRejectdList,#btnPendingAcknowledgeList,#btnAcknowledgeList,#btnUnAcknowledgeList,#btnAllList").hide();
        $formEl.find("#btnSave,#btnRevise,#btnReviseAndSendToApproval,#btnApproveYPR,#btnRejectYPR,#btnAkgYPR,#btnMnMAck,#btnUnAkgYPR,#btnSendYPR").fadeOut();

        $formEl.find("#addYarnComposition").on("click", function (e) {
            showAddComposition();
        });

        $toolbarEl.find("#btnNew").on("click", function (e) {
            $formEl.find("#btnRevise,#btnReviseAndSendToApproval").fadeOut();
            status = statusConstants.NEW;
            showHideControls(true);
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            getNew();
        });

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ADDITIONAL;
            $toolbarEl.find("#divAddPRForMR").fadeIn();
            initMasterTable();
        });

        $toolbarEl.find("#btnAllList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ALL_STATUS;
            $toolbarEl.find("#divAddPRForMR").fadeIn();
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingApprovalList").on("click", function (e) {
            e.preventDefault();
            
            //$formEl.find("#btnRevise").fadeOut();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED;
            isEditable = isApprovePage ? false : true;
            initMasterTable();
        });

        $toolbarEl.find("#btnApprovedList").on("click", function (e) {
            e.preventDefault();
            //$formEl.find("#btnRevise").fadeOut();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PARTIALLY_COMPLETED;
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnRejectdList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REJECT;
            isEditable = false;
            initMasterTable();
        });

        $toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnUnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.UN_ACKNOWLEDGE;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnSendList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED;
            $toolbarEl.find("#divAddPRForMR").fadeOut();
            initMasterTable();
        });

        $formEl.find("#DepartmentID").change(function () {
            
            var selectedDep = $formEl.find("#DepartmentID option:selected").text();
            var isMer = selectedDep.toUpperCase().includes("MERCHANDISING");
            if (isMer) {
                $formEl.find(".divCopyFromExistingBooking").show();
            }
            else {
                $formEl.find(".divCopyFromExistingBooking").hide();
            }
        });

        $formEl.find("#btnCopyFromExistingBooking").on("click", function (e) {
            var selectedDep = $formEl.find("#DepartmentID option:selected").text();
            var isMer = selectedDep.toUpperCase().includes("MERCHANDISING");

            if (!isMer) {
                toastr.error("Only merchandiser has this permission.");
                return false;
            }

            var aryBuyer = $formEl.find("#BuyerIDs").val();
            if (aryBuyer.length == 0) {
                toastr.error("Select buyer(s).");
                return false;
            }

            var buyerIds = aryBuyer.map(function (el) { return el }).toString();

            var aryBuyerTeam = $formEl.find("#BuyerTeamIDs").val();
            var buyerTeamIDs = aryBuyerTeam.map(function (el) { return el }).toString();
            if (buyerTeamIDs.length == 0) buyerTeamIDs = "-";

            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Item(s)",
                pageId: pageId,
                height: 320,
                modalSize: "modal-lg",
                apiEndPoint: `/api/mr-bds/get-complete-mr-childs/${buyerIds}/${buyerTeamIDs}`,
                fields: "BookingNo,Segment1ValueDesc,Segment2ValueDesc,Segment3ValueDesc,Segment4ValueDesc,Segment5ValueDesc,Segment6ValueDesc,ShadeCode,DisplayUnitDesc,BookingQty,ReqCone,Remarks",
                headerTexts: "Booking No,Composition,Yarn Type,Manufacturing Process,Sub Process,Quality Parameter,Count,Shade Code,Unit,Qty,Req Cone,Remarks",
                isMultiselect: true,
                primaryKeyColumn: "FCMRChildID",
                onMultiselect: function (selectedRecords) {
                    if (selectedRecords.length > 0) {
                        var count = 0;
                        selectedRecords.map(x => {
                            count++;
                            x["QTY"] = x.BookingQty;
                            x["PYBBookingChildID"] = count;
                            x["PYBItemChildDetails"] = [];
                        });
                        $tblChildEl.dataSource = selectedRecords;
                    }
                }
            });
            finder.showModal();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });
        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            revise(false, false);
        });
        $formEl.find("#btnReviseAndSendToApproval").click(function (e) {
            e.preventDefault();
            revise(true, true);
        });

        $formEl.find("#btnApproveYPR").click(function (e) {
            e.preventDefault();
            approve(true);
        });

        $formEl.find("#btnRejectYPR").click(function (e) {
            e.preventDefault();
            bootbox.prompt("Enter your reject reason:", function (result) {
                if (!result) {
                    return toastr.error("Reject reason is required.");
                }
                var id = $formEl.find("#PYBookingID").val();
                var reason = result;
                axios.post(`/api/projection-yarn-booking/reject/${id}/${reason}`)
                    .then(function () {
                        toastr.success("Rejected successfully.");
                        backToList();
                    })
                    .catch(showResponseError);
            });
        });

        $formEl.find("#btnAkgYPR").click(function (e) {
            e.preventDefault();
            acknowledge(masterData);
        });

        $formEl.find("#btnUnAkgYPR").click(function (e) {
            e.preventDefault();
            showBootboxPrompt("Unacknowledge", "Enter your unacknowledge reason:", function (result) {
                if (result) {
                    unacknowledgePR(masterData.PYBookingID, result);
                }
            });
        });

        $formEl.find("#btnSendYPR").click(function (e) {
            e.preventDefault();
            save(true);
        });

        $formEl.find("#btnMnMAck").click(function (e) {
            e.preventDefault();
            AcknowledgeMnM();
        });
        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#addYarnComposition").on("click", function (e) {
            showAddComposition();
        });
        $pageEl.find("#btnAddComposition").click(saveComposition);
        $pageEl.find('input[type=radio][name=BlendedPYB]').change(function (e) {
            e.preventDefault();
            isBlended = convertToBoolean(this.value);
            initTblCreateComposition();
            return false;
        });

        if (isAcknowledgePage) {
            $toolbarEl.find("#btnPendingAcknowledgeList,#btnAcknowledgeList,#btnUnAcknowledgeList").show();
            $toolbarEl.find("#btnNew,#btnPendingList,#btnSendList,#btnApprovedList,#btnRejectdList,#btnPendingApprovalList,#btnAllList").hide();

            $formEl.find("#btnAkgYPR,#btnUnAkgYPR").fadeIn();
            $formEl.find("#btnSave,#btnRejectYPR,#btnApproveYPR,#btnRevise,#btnReviseAndSendToApproval,#btnSendYPR,#btnMnMAck").fadeOut();

            status = statusConstants.PARTIALLY_COMPLETED;
            //toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAcknowledgeList"), $toolbarEl);
            $toolbarEl.find("#btnPendingAcknowledgeList").click();
            isEditable = false;
            isMarketingFlag = false;
        }
        else if (isApprovePage) {
            $toolbarEl.find("#btnNew,#btnPendingList,#btnSendList,#btnPendingAcknowledgeList,#btnAcknowledgeList,#btnUnAcknowledgeList,#btnAllList").hide();
            $toolbarEl.find("#btnApprovedList,#btnRejectdList,#btnPendingApprovalList").show();

            $formEl.find("#btnApproveYPR,#btnRejectYPR").fadeIn();
            $formEl.find("#btnSave,#btnRevise,#btnReviseAndSendToApproval,#btnAkgYPR,#btnMnMAck,#btnUnAkgYPR,#btnSendYPR").fadeOut();

            status = statusConstants.PROPOSED;
            //toggleActiveToolbarBtn($toolbarEl.find("#btnPendingApprovalList"), $toolbarEl);
            $toolbarEl.find("#btnPendingApprovalList").click();
            isEditable = false;
            isMarketingFlag = false;
        }
        else if (isPendingMnMPage) {
            $toolbarEl.find("#btnNew,#btnPendingList,#btnSendList,#btnApprovedList,#btnRejectdList,#btnAcknowledgeList,#btnUnAcknowledgeList,#btnAllList").show();
            $toolbarEl.find("#btnPendingApprovalList,#btnPendingAcknowledgeList").hide();

            $formEl.find("#btnSave,#btnSendYPR").fadeIn();
            $formEl.find("#btnRevise,#btnReviseAndSendToApproval,#btnApproveYPR,#btnRejectYPR,#btnAkgYPR,#btnMnMAck,#btnUnAkgYPR").fadeOut();

            //status = statusConstants.PROPOSED;
            status = statusConstants.ADDITIONAL;
            //toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);
            $toolbarEl.find("#btnPendingList").click();
            isEditable = true;
            isMarketingFlag = true;
        }
        else if (isApproveMnMPage) {
            $toolbarEl.find("#btnNew,#btnPendingList,#btnSendList,#btnPendingAcknowledgeList,#btnAcknowledgeList,#btnUnAcknowledgeList,#btnAllList").hide();
            $toolbarEl.find("#btnApprovedList,#btnRejectdList,#btnPendingApprovalList").show();

            $formEl.find("#btnApproveYPR,#btnRejectYPR").fadeIn();
            $formEl.find("#btnSave,#btnRevise,#btnReviseAndSendToApproval,#btnAkgYPR,#btnMnMAck,#btnUnAkgYPR,#btnSendYPR").fadeOut();

            status = statusConstants.PROPOSED;
            //toggleActiveToolbarBtn($toolbarEl.find("#btnPendingApprovalList"), $toolbarEl);
            $toolbarEl.find("#btnPendingApprovalList").click();
            isEditable = false;
            isMarketingFlag = false;
        }
        else if (isAcknowledgeMnMPage) {
            $toolbarEl.find("#btnPendingAcknowledgeList,#btnAcknowledgeList,#btnUnAcknowledgeList").show();
            $toolbarEl.find("#btnNew,#btnPendingList,#btnSendList,#btnApprovedList,#btnRejectdList,#btnPendingApprovalList,#btnAllList").hide();

            $formEl.find("#btnMnMAck,#btnUnAkgYPR").fadeIn();
            $formEl.find("#btnSave,#btnRejectYPR,#btnApproveYPR,#btnRevise,#btnReviseAndSendToApproval,#btnSendYPR,#btnAkgYPR").fadeOut();

            status = statusConstants.PARTIALLY_COMPLETED;
            //toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAcknowledgeList"), $toolbarEl);
            $toolbarEl.find("#btnPendingAcknowledgeList").click();
            isEditable = false;
            isMarketingFlag = false;
        }
        else {
            $toolbarEl.find("#btnNew,#btnPendingList,#btnSendList,#btnApprovedList,#btnRejectdList,#btnAcknowledgeList,#btnUnAcknowledgeList,#btnAllList").show();
            $toolbarEl.find("#btnPendingApprovalList,#btnPendingAcknowledgeList").hide();

            $formEl.find("#btnSave,#btnSendYPR").fadeIn();
            $formEl.find("#btnRevise,#btnReviseAndSendToApproval,#btnApproveYPR,#btnRejectYPR,#btnAkgYPR,#btnMnMAck,#btnUnAkgYPR").fadeOut();

            //status = statusConstants.PROPOSED;
            status = statusConstants.ADDITIONAL;
            $toolbarEl.find("#btnPendingList").click();
            isEditable = true;
            isMarketingFlag = false;
        }

        initMasterTable();

        getSegments();
    });

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

    function copyButtonHideShow() {
        
        $formEl.find(".divCopyFromExistingBooking").hide();

        var selectedDep = $formEl.find("#DepartmentID option:selected").text();
        var isMer = selectedDep.toUpperCase().includes("MERCHANDISING");

        if (isMer) {
            $formEl.find(".divCopyFromExistingBooking").show();
        }
    }
    function initMasterTable() {
        var commands = [];
        if (isApprovePage || isApproveMnMPage) {
            if (status == statusConstants.APPROVED) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } },
                    { type: 'Email', title: 'Send Email', buttonOption: { cssClass: 'e-flat', iconCss: 'far fa-envelope' } }
                ]
            } else if (status == statusConstants.REJECT) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }//,
                    //{ type: 'Revise', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
                ]
            } else {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Approve', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                    { type: 'Reject', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-ban' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }
                ]
            }
        }
        else if (isAcknowledgePage || isAcknowledgeMnMPage) {
            if (status == statusConstants.ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }//,
                    //{ type: 'Revise', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-recycle' } }
                ]
            }
            else {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    /*{ type: 'Acknowledge', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },*/
                    { type: 'UnAcknowledge', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-times' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }
                ]
            }
        }
        else {
            if (status == statusConstants.APPROVED || status == statusConstants.REJECT || status == statusConstants.ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE) {
                commands = [
                    { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } },
                    { type: 'Revise', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-recycle' } }
                ]
            }
            else if (status == statusConstants.PROPOSED) {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } },
                    { type: 'Revise', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-recycle' } }
                ]
            }
            else {
                commands = [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }
                    //{ type: 'ApprovePYB', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                    //{ type: 'RejectPYB', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-ban' } }
                ]
            }
        }

        var columns = [];
        if (status == statusConstants.REVISE || status == statusConstants.ADDITIONAL) {
            commands = [
                { type: 'ViewPYB', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }
                //{ type: 'ApprovePYB', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check' } },
                //{ type: 'RejectPYB', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-ban' } }
            ]
            columns = [
                {
                    headerText: '', width: 70, commands: commands, visible: status == statusConstants.ADDITIONAL
                },
                {
                    field: 'isMarketingFlag', headerText: 'isMarketingFlag', visible: false
                },
                {
                    field: 'PYBookingNo', headerText: 'Booking No'
                },
                {
                    field: 'PYBookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
                },
                //{
                //    field: 'Buyer', headerText: 'Buyer'
                //},
                {
                    field: 'BookingByName', headerText: 'Booking By Name'
                },
                //{
                //    field: 'RequiredByName', headerText: 'Required By Name'
                //},
                {
                    field: 'DepertmentDescription', headerText: 'From Depertment'
                }

            ];
        }

        else if (status == statusConstants.APPROVED) {
            columns = [
                {
                    headerText: '', textAlign: 'Center', commands: commands
                },
                {
                    field: 'isMarketingFlag', headerText: 'isMarketingFlag', visible: false
                },
                {
                    field: 'PYBookingNo', headerText: 'Booking No'
                },
                {
                    field: 'PYBookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'Buyer', headerText: 'Buyer'
                },
                {
                    field: 'BookingByName', headerText: 'Booking By Name'
                },
                {
                    field: 'RequiredByName', headerText: 'Required By Name'
                },
                {
                    field: 'DepertmentDescription', headerText: 'From Depertment'
                }
            ];
        }
        else if (status == statusConstants.REJECT) {
            columns = [
                {
                    headerText: '', textAlign: 'Center', commands: commands
                },
                {
                    field: 'isMarketingFlag', headerText: 'isMarketingFlag', visible: false
                },
                {
                    field: 'PYBookingNo', headerText: 'Booking No'
                },
                {
                    field: 'PYBookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'Buyer', headerText: 'Buyer'
                },
                {
                    field: 'BookingByName', headerText: 'Booking By Name'
                },
                {
                    field: 'RequiredByName', headerText: 'Required By Name'
                },
                {
                    field: 'DepertmentDescription', headerText: 'From Depertment'
                }
            ];
        }
        else if (status == statusConstants.ALL_STATUS) {
            columns = [
                {
                    headerText: '', textAlign: 'Center', commands: commands
                },
                {
                    field: 'isMarketingFlag', headerText: 'isMarketingFlag', visible: false
                },
                {
                    field: 'PYBookingNo', headerText: 'Booking No'
                },
                {
                    field: 'PYBookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'Buyer', headerText: 'Buyer'
                },
                {
                    field: 'BookingByName', headerText: 'Booking By Name'
                },
                {
                    field: 'RequiredByName', headerText: 'Required By Name'
                },
                {
                    field: 'DepertmentDescription', headerText: 'From Depertment'
                },
                {
                    field: 'Status', headerText: 'Status'
                }
            ];
        }
        else if (status == statusConstants.ACKNOWLEDGE || status == statusConstants.UN_ACKNOWLEDGE) {
            columns = [
                {
                    headerText: 'Actions', textAlign: 'Center', commands: commands
                },
                {
                    field: 'isMarketingFlag', headerText: 'isMarketingFlag', visible: false
                },
                {
                    field: 'PYBookingNo', headerText: 'Booking No'
                },
                {
                    field: 'PYBookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'Buyer', headerText: 'Buyer'
                },
                {
                    field: 'BookingByName', headerText: 'Booking By Name'
                },
                {
                    field: 'RequiredByName', headerText: 'Required By Name'
                },
                {
                    field: 'DepertmentDescription', headerText: 'From Depertment'
                }
            ];
        }
        else {
            columns = [
                {
                    headerText: 'Actions', textAlign: 'Center', commands: commands
                },
                {
                    field: 'isMarketingFlag', headerText: 'isMarketingFlag', visible: false
                },
                {
                    field: 'PYBookingNo', headerText: 'Booking No'
                },
                {
                    field: 'PYBookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'Buyer', headerText: 'Buyer'
                },
                {
                    field: 'BookingByName', headerText: 'Booking By Name'
                },
                {
                    field: 'RequiredByName', headerText: 'Required By Name'
                },
                {
                    field: 'DepertmentDescription', headerText: 'From Depertment'
                }
            ];
        }

        var isApproveOrAcknowledge = (isAcknowledgePage || isApprovePage || isAcknowledgeMnMPage || isApproveMnMPage);

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/projection-yarn-booking/list?status=${status}&isApproveOrAcknowledge=${isApproveOrAcknowledge}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        $formEl.find("#btnSave,#btnRevise,#btnReviseAndSendToApproval,#btnApproveYPR,#btnRejectYPR,#btnAkgYPR,#btnMnMAck,#btnUnAkgYPR,#btnSendYPR").fadeOut();
        $formEl.find("#divRevisionReason").fadeOut();

        if (args.commandColumn.type == 'ViewPYB') {
            getDetails(args.rowData.PYBookingID);
            $formEl.find("#btnSave,#btnNewItem,#btnSendYPR").fadeIn();
            showHideControls(true);
        }
        else if (args.commandColumn.type == 'Edit') {

            if (status == statusConstants.PROPOSED || status == statusConstants.ALL_STATUS || status == statusConstants.NEW) {
                showHideControls(false);
            }
            else {
                showHideControls(true);
            }
            getDetails(args.rowData.PYBookingID);
            /* $formEl.find("#btnNewItem").fadeIn(); */
        }
        else if (args.commandColumn.type == 'View') {
            showHideControls(false);

            getDetails(args.rowData.PYBookingID);

            if (status == statusConstants.PROPOSED) {
                $formEl.find("#btnApproveYPR,#btnRejectYPR").fadeIn();
            }
            else if (status == statusConstants.PARTIALLY_COMPLETED && !(args.rowData.isMarketingFlag)) {
                $formEl.find("#btnAkgYPR,#btnUnAkgYPR").fadeIn();
            }
            else if (status == statusConstants.PARTIALLY_COMPLETED && (args.rowData.isMarketingFlag)) {
                $formEl.find("#btnMnMAck,#btnUnAkgYPR").fadeIn();
            }
        }
        else if (args.commandColumn.type == 'Email') {
            showBootboxConfirm("Send Mail.", "Are you sure to re-send mail?", function (yes) {
                if (yes) {
                    sendMail(args.rowData.PYBookingID);
                }
            });
        }
        else if (args.commandColumn.type == 'Approve') {
            showHideControls(true);
            approvePR(args.rowData);
        }
        else if (args.commandColumn.type == 'Reject') {
            showHideControls(true);
            showBootboxPrompt("Reject Projection Booking", "Enter your reject reason:", function (result) {
                if (result) {
                    rejectPR(args.rowData.PYBookingID, result);
                }
            });
        }
        else if (args.commandColumn.type == 'Acknowledge') {
            showHideControls(true);
            if (args.rowData.isMarketingFlag) {
                AcknowledgeMnM();
            } else {
                acknowledge(args.rowData);
            }
        }
        else if (args.commandColumn.type == 'UnAcknowledge') {
            showHideControls(true);
            showBootboxPrompt("Unacknowledge", "Enter your unacknowledge reason:", function (result) {
                if (result) {
                    unacknowledgePR(args.rowData.PYBookingID, result);
                }
            });
        }
        else if (args.commandColumn.type == 'ViewPYB') {
            axios.get(`/api/projection-yarn-booking/new-mr/${args.rowData.PYBookingID}`)
                .then(function (response) {
                    isEditable = true;
                    status = statusConstants.ADDITIONAL;
                    $divDetailsEl.fadeIn();
                    $divTblEl.fadeOut();
                    $formEl.find("#divRejectReason").hide();
                    $formEl.find("#divUnAcknowledgeReason").hide();
                    if (status == statusConstants.ADDITIONAL)
                        $formEl.find("#btnSave").fadeOut();
                    else
                        $formEl.find("#btnSave").fadeIn();

                    masterData = response.data;
                    $formEl.find(".divReject").hide();
                    masterData.PYBookingDate = formatDateToDefault(masterData.PYBookingDate);

                    setFormData($formEl, masterData);
                    $formEl.find("#TriggerPointID").val('1252').trigger("change"); //Projection Based
                    $formEl.find("#IsRNDPR").prop('checked', true);
                    $formEl.find("#btnRevise,#btnReviseAndSendToApproval").fadeOut();

                    initChildTable(masterData);
                })
                .catch(showResponseError);
        } else if (args.commandColumn.type == 'ApprovePYB') {
            showHideControls(true);
            acknowledgeMR(args.rowData);
        } else if (args.commandColumn.type == 'RejectPYB') {
            showHideControls(true);
            showBootboxPrompt("Reject", "Enter your reject reason:", function (result) {
                if (result) {
                    rejectMR(args.rowData.PYBookingID, result);
                }
            });
        }
        else if (args.commandColumn.type == 'Report') {
            window.open(`/reports/InlinePdfView?ReportName=YarnProjectionBooking.rdl&PYBookingNo=${args.rowData.PYBookingNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'Revise') {
            $formEl.find("#btnRevise,#btnReviseAndSendToApproval").fadeIn();

            showHideControls(false);
            showBootboxConfirm("Revise", "Are you sure you want to revise this record?", function (yes) {
                if (yes) getDetails(args.rowData.PYBookingID);
            })
        }
    }

    async function initChildTable(data) {
        
        if ($tblChildEl) $tblChildEl.destroy();

        var columns = [];
        columns.push(...await getYarnItemColumnsWithSearchDDLAsync(ch_getCountRelatedList(data, 3), isEditable));

        var additionalColumns = [
            { field: 'PYBBookingChildID', isPrimaryKey: true, visible: false },
            {
                field: 'ShadeCode', headerText: 'Shade Code', width: 100,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: data.YarnShadeBooks,
                displayField: "ShadeCode",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'DayValidDurationId', headerText: 'Yarn Sourcing Mode', width: 120,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.DayValidDurations,
                displayField: "text",
                edit: ej2GridDropDownObj({
                })
            },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false, columnValues: 'Kg', width: 40, textAlign: 'Center', headerTextAlign: 'Center' },
            { field: 'QTY', headerText: 'Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100, textAlign: 'Right', headerTextAlign: 'Center' },
            { field: 'ReqCone', headerText: 'Req Cone(Pcs)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100, textAlign: 'Right', headerTextAlign: 'Center' },
            /*{ field: 'PPrice', headerText: 'Agreed Price', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0, format: "N2"  } }, width: 100, textAlign: 'Right', headerTextAlign: 'Center' },*/
            { field: 'PPrice', headerText: 'Agreed Price', width: 100, textAlign: 'Right', headerTextAlign: 'Center', params: { showSpinButton: false, decimals: 0, format: "N2" } },
            { field: 'Remarks', headerText: 'Remarks', width: 100 }

        ];
        columns.push.apply(columns, additionalColumns);

        //child Columns
        var childDetailscolumns = [
            {
                field: 'Commands', headerText: '', width: 20, textAlign: 'Center', commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            { field: 'PYBBookingChildDetailsID', isPrimaryKey: true, visible: false },
            { field: 'PYBBookingChildID', visible: false },
            { field: 'PYBookingID', visible: false },
            { field: 'BookingDate', headerText: 'Booking Date', width: 100, type: 'date', format: _ch_date_format_1, editType: 'datepickeredit', width: 40, textAlign: 'Center' }, //valueAccessor: ej2GridDateFormatter,
            { field: 'DetailsQTY', headerText: 'Quantity', width: 100, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 40, textAlign: 'Center', headerTextAlign: 'Center' }
        ];

        $tblChildEl = new ej.grids.Grid({
            dataSource: data.ProjectionYarnBookingItemChilds,
            allowResizing: true,
            toolbar: ['Add'],
            editSettings: {
                allowEditing: true,
                allowAdding: true,
                allowDeleting: true,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.PYBBookingChildID = getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "PYBBookingChildID");
                    args.data.DisplayUnitDesc = 'Kg';
                    args.data.PYBItemChildDetails = [];
                }
                else if (args.requestType === "save") {
                    if (_segments) {
                        args.data = setYarnSegDesc(args.data);
                    }
                }
                else if (args.requestType === "delete") {
                    if (args.data[0].IsReceived) {
                        toastr.error("Already yarn received. Delete not possible");
                        args.cancel = true;
                    }
                }
            },
            childGrid: {
                queryString: 'PYBBookingChildID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: {
                    allowEditing: true,
                    allowAdding: true,
                    allowDeleting: true,
                    mode: "Normal",
                    showDeleteConfirmDialog: true
                },
                columns: childDetailscolumns,
                actionBegin: function (args) {
                    
                    if (args.requestType === "add") {
                        var totalDis = 0, remainDis = 0, maxDate;

                        var dayValidDurationId = getDefaultValueWhenInvalidN(this.parentDetails.parentRowData.DayValidDurationId);

                        if (dayValidDurationId == 0 && masterData.IsCheckDVD) {
                            toastr.error("Select Yarn Sourcing Mode.");
                            return false;
                        }
                        var isDateValidObj = ch_IsDateValid_DayValidDuration(new Date(), dayValidDurationId, masterData.DayValidDurations);
                        if (!isDateValidObj.IsValid) {
                            args.data.BookingDate = isDateValidObj.MinDate;
                            args.rowData.BookingDate = isDateValidObj.MinDate;
                        }

                        if (typeof this.dataSource !== "undefined") {
                            this.dataSource.forEach(l => {
                                totalDis += l.DetailsQTY;
                                if (!maxDate) {
                                    maxDate = l.BookingDate;
                                } else if (maxDate < l.BookingDate) {
                                    maxDate = l.BookingDate;
                                }
                            });
                        }
                        if (this.parentDetails.parentRowData.QTY == null) this.parentDetails.parentRowData.QTY = 0;
                        if (totalDis < parseFloat(this.parentDetails.parentRowData.QTY)) {
                            remainDis = parseFloat(this.parentDetails.parentRowData.QTY) - totalDis;
                        }
                        else {
                            toastr.error("Distribution can not more then 100% !!");
                            args.cancel = true;
                            return;
                        }
                        args.data.PYBBookingChildDetailsID = maxCol++;
                        args.data.PYBBookingChildID = this.parentDetails.parentKeyFieldValue;
                        args.data.PYBookingID = this.parentDetails.parentRowData.PYBookingID;
                        if (maxDate) {
                            var tempDate = new Date(maxDate);
                            var nextMonth = tempDate.getMonth() + 1;
                            tempDate.setMonth(nextMonth);
                            args.data.BookingDate = tempDate;
                            maxDate = tempDate;
                        }
                        args.data.DetailsQTY = remainDis.toFixed(4);
                    }
                    else if (args.requestType === "save") {
                        var dayValidDurationId = getDefaultValueWhenInvalidN(this.parentDetails.parentRowData.DayValidDurationId);

                        if (dayValidDurationId == 0 && masterData.IsCheckDVD) {
                            toastr.error("Select Yarn Sourcing Mode.");
                            return false;
                        }
                        var isDateValidObj = ch_IsDateValid_DayValidDuration(args.data.BookingDate, dayValidDurationId, masterData.DayValidDurations);
                        if (!isDateValidObj.IsValid) {

                            toastr.error(`Minimum date for this sourcing mode is ${ch_customDateFormat(isDateValidObj.MinDate)}`);

                            args.data.BookingDate = isDateValidObj.MinDate;
                            args.rowData.BookingDate = isDateValidObj.MinDate;
                        }

                        var parentId = this.parentDetails.parentRowData.PYBBookingChildID;
                        var indexF = masterData.ProjectionYarnBookingItemChilds.findIndex(x => x.PYBBookingChildID == parentId);
                        if (indexF > -1) {
                            var childIndexF = masterData.ProjectionYarnBookingItemChilds[indexF]
                                .PYBItemChildDetails
                                .findIndex(x => x.PYBBookingChildDetailsID == args.rowData.PYBBookingChildDetailsID);
                            if (childIndexF > -1) {
                                masterData.ProjectionYarnBookingItemChilds[indexF]
                                    .PYBItemChildDetails[childIndexF] = args.data;
                            }
                        }
                    }
                },
                load: loadChildItemDetailsGrid
            }
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildItemId);
    }

    function loadChildItemDetailsGrid() {
        this.dataSource = this.parentDetails.parentRowData.PYBItemChildDetails;
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        if (status === statusConstants.NEW) {
            status = statusConstants.ADDITIONAL;
            //toggleActiveToolbarBtn("#btnPendingList", $toolbarEl);
            $toolbarEl.find("#btnPendingList").click();
        }
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#PYBookingID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew() {
        axios.get("/api/projection-yarn-booking/new")
            .then(function (response) {
                status = statusConstants.NEW;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                
                masterData = response.data;
                $formEl.find(".divReject").hide();
                masterData.PYBookingDate = formatDateToDefault(masterData.PYBookingDate);
                masterData.FabricBookingStartMonth = formatDateToDefault(masterData.FabricBookingStartMonth);
                masterData.FabricBookingEndMonth = formatDateToDefault(masterData.FabricBookingEndMonth);
                if (masterData.IsSuperUser) {
                    $formEl.find("#DepartmentID").prop("disabled", false);
                    $formEl.find("#BookingByID").prop("disabled", false);
                }
                else {
                    $formEl.find("#DepartmentID").prop("disabled", true);
                    $formEl.find("#BookingByID").prop("disabled", true);
                }
                setFormData($formEl, masterData);
                copyButtonHideShow();
                initChildTable(masterData);
                $formEl.find("#btnSave").fadeIn();
                $formEl.find("#btnSendYPR").fadeIn();

                $formEl.find("#BuyerIDs").on("select2:select", function (e) {
                    var ary = $formEl.find("#BuyerIDs").val();
                    id = ary.map(function (el) { return el }).toString();
                    getBuyerTeamFromBuyer(id);
                });
                $formEl.find("#BuyerTeamIDs").on("select2:select", function (e) {
                    var ary = $formEl.find("#BuyerTeamIDs").val();
                    buyerTeamIDs = ary.map(function (el) { return el }).toString();
                });
            })
            .catch(showResponseError);
    }

    function getBuyerTeamFromBuyer(buyerId) {
        var vUrl = `/api/projection-yarn-booking/GetBuyerTeamFromBuyerServiceWO/${buyerId}`;
        axios.get(vUrl)
            .then(function (response) {
                if ($formEl.find("#BuyerTeamIDs").hasClass("select2-hidden-accessible")) {
                    $formEl.find("#BuyerTeamIDs").empty();
                };

                initSelect2($formEl.find("#BuyerTeamIDs"), response.data.BuyerTeamList);
                if (masterData.BuyerTeamIDs != undefined) {
                    $formEl.find("#BuyerTeamIDs").val(masterData.BuyerTeamIDs).trigger("change");
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }
    function getDetails(PYBookingID) {
        axios.get(`/api/projection-yarn-booking/${PYBookingID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;

                if (masterData.IsReject) $formEl.find(".divReject").show();
                if (masterData.IsSuperUser) {
                    $formEl.find("#DepartmentID").prop("disabled", false);
                    $formEl.find("#BookingByID").prop("disabled", false);
                }
                else {
                    $formEl.find("#DepartmentID").prop("disabled", true);
                    $formEl.find("#BookingByID").prop("disabled", true);
                }
                var bID = Array();
                bID = masterData.BuyerIDsList.split(/[ ,]+/);
                if (masterData.BuyerIDsList)
                    getBuyerTeamFromBuyer(masterData.BuyerIDsList);

                masterData.BuyerIDs = bID;

                var bTeamID = Array();
                bTeamID = masterData.BuyerTeamIDsList.split(/[ ,]+/);
                masterData.BuyerTeamIDs = bTeamID;
                id = masterData.BuyerIDsList;
                buyerTeamIDs = masterData.BuyerTeamIDsList;

                $formEl.find("#BuyerIDs").on("select2:select", function (e) {
                    var ary = $formEl.find("#BuyerIDs").val();
                    id = ary.map(function (el) { return el }).toString();
                    getBuyerTeamFromBuyer(id);
                });

                $formEl.find("#BuyerTeamIDs").on("select2:select", function (e) {
                    var ary = $formEl.find("#BuyerTeamIDs").val();
                    buyerTeamIDs = ary.map(function (el) { return el }).toString();
                });

                masterData.FabricBookingStartMonth = formatDateToDefault(masterData.FabricBookingStartMonth);
                masterData.FabricBookingEndMonth = formatDateToDefault(masterData.FabricBookingEndMonth);
                masterData.PYBookingDate = formatDateToDefault(masterData.PYBookingDate);
                masterData.RevisionDate = formatDateToDefault(masterData.RevisionDate);

                setFormData($formEl, masterData);
                copyButtonHideShow();

                initChildTable(masterData);

                if (masterData.RevisionNo > 0) {
                    $formEl.find("#divRevisionNo").fadeIn();
                    $formEl.find("#divRevisionDate").fadeIn();
                } else {
                    $formEl.find("#divRevisionNo").fadeOut();
                    $formEl.find("#divRevisionDate").fadeOut();
                }
            })
            .catch(showResponseError);
    }

    function approve(isApproved = false) {
        var data = formDataToJson($formEl.serializeArray());
        data.IsApprove = isApproved;
        axios.post("/api/projection-yarn-booking/approve", data)
            .then(function () {
                toastr.success("Approved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function reject(isRejected = false) {
        var data = formDataToJson($formEl.serializeArray());
        data.IsReject = isRejected;
        data.IsAcknowledge = isAcknowledge;
        axios.post("/api/projection-yarn-booking/approve", data)
            .then(function () {
                toastr.success("Rejected successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function sendtoapprove(isSendToApprover = false) {
        var data = formDataToJson($formEl.serializeArray());
        data.SendToApprover = isSendToApprover;
        axios.post("/api/projection-yarn-booking/approve", data)
            .then(function () {
                toastr.success("Send to approver successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function approvePR(data) {
        data.IsApprove = true;
        axios.post("/api/projection-yarn-booking/approve", data)
            .then(function () {
                toastr.success("Approved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function rejectPR(id, reason) {
        var url = `/api/projection-yarn-booking/reject/${id}/${reason}`;
        axios.post(url)
            .then(function () {
                toastr.success("Rejected successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function unacknowledgePR(id, reason) {
        var url = `/api/projection-yarn-booking/unacknowledge/${id}/${reason}`;
        axios.post(url)
            .then(function () {
                toastr.success("Unacknowledged successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function isInvalidValue(value) {
        if (typeof value === "undefined" || value == null || value == 0) return true;
        return false;
    }
    function save(SendToApprover) {
        
        $formEl.find("#BuyerIDsList").val($formEl.find("#BuyerIDs").val().map(function (el) {
            return el
        }).toString());
        $formEl.find("#BuyerTeamIDsList").val($formEl.find("#BuyerTeamIDs").val().map(function (el) {
            return el
        }).toString());
        var data = formDataToJson($formEl.serializeArray());

        //if (data.DepartmentID == null || typeof data.DepartmentID == 'undefined') {
        //    data.DepartmentID = masterData.DepartmentID;
        //}
        //if (data.SeasonID == null || typeof data.SeasonID == 'undefined') {
        //    data.SeasonID = masterData.SeasonID;
        //}
        //if (data.FinancialYearID == null || typeof data.FinancialYearID == 'undefined') {
        //    data.FinancialYearID = masterData.FinancialYearID;
        //}
        data.BuyerIDsList = getDefaultValueWhenInvalidS(data.BuyerIDsList);
        data.BuyerTeamIDsList = getDefaultValueWhenInvalidS(data.BuyerTeamIDsList);
        data.BuyerTeamIDsList = (data.BuyerTeamIDsList.length && data.BuyerTeamIDsList[0] == ',') ? data.BuyerTeamIDsList.slice(1) : data.BuyerTeamIDsList;

        if (data.BuyerIDsList.length == 0 && data.BuyerTeamIDsList.length > 0) {
            return toastr.error("Without buyer, buyer team is invalid !");
        }

        if (isPendingMnMPage || isApproveMnMPage || isAcknowledgeMnMPage) {
            if (data.BuyerIDsList.length == 0) {
                return toastr.error("Select buyer !");
            }
            if (data.BuyerTeamIDsList.length == 0) {
                return toastr.error("Select buyer team !");
            }
        }

        var pYBookingChild = $tblChildEl.getCurrentViewRecords();
        if (pYBookingChild.length <= 0) {
            return toastr.error("Please Select at least one item!");
        }
        var hasError = false,
            shadeCodeInvalidRowIndexs = [];

        for (var j = 0; j < pYBookingChild.length; j++) {
            var currentRow = j + 1;
            if (isInvalidValue(pYBookingChild[j].Segment1ValueId)) {
                hasError = true;
                toastr.error(`Select composition at row ${currentRow}`);
                break;
            }
            if (isInvalidValue(pYBookingChild[j].Segment2ValueId)) {
                hasError = true;
                toastr.error(`Select yarn type at row ${currentRow}`);
                break;
            }
            else if (isInvalidValue(pYBookingChild[j].Segment3ValueId)) {
                hasError = true;
                toastr.error(`Select manufacturing process at row ${currentRow}`);
                break;
            }
            else if (isInvalidValue(pYBookingChild[j].Segment4ValueId)) {
                hasError = true;
                toastr.error(`Select sub process at row ${currentRow}`);
                break;
            }
            else if (isInvalidValue(pYBookingChild[j].Segment5ValueId)) {
                hasError = true;
                toastr.error(`Select quality parameter at row ${currentRow}`);
                break;
            }
            else if (isInvalidValue(pYBookingChild[j].Segment6ValueId)) {
                hasError = true;
                toastr.error(`Select count at row ${currentRow}`);
                break;
            }
            pYBookingChild[j] = setYarnSegDesc(pYBookingChild[j]);

            var dayValidDurationId = getDefaultValueWhenInvalidN(pYBookingChild[j].DayValidDurationId);

            if (dayValidDurationId == 0 && masterData.IsCheckDVD) {
                toastr.error(`Select Yarn Sourcing Mode (At row ${j + 1})`);
                hasError = true;
                break;
            }

            if ((pYBookingChild[j].Segment5ValueDesc.toLowerCase() == "melange" || pYBookingChild[j].Segment5ValueDesc.toLowerCase() == "color melange") && (pYBookingChild[j].ShadeCode == null || pYBookingChild[j].ShadeCode == "")) {
                toastr.error(`Select shade code for color melange (row ${currentRow})`);
                hasError = true;
                break;
            }

            if (SendToApprover && pYBookingChild[j].PYBItemChildDetails.length == 0) {
                hasError = true;
                toastr.error(`Set minimum one booking date for every single item information (row ${currentRow}).`);
                break;
            }

            var totalQty = 0;
            for (var i = 0; i < pYBookingChild[j].PYBItemChildDetails.length; i++) {
                
                //const bookingDate = pYBookingChild[j].PYBItemChildDetails[i].BookingDate;
                //pYBookingChild[j].PYBItemChildDetails[i].BookingDate = new Date(bookingDate).toISOString().split('Z')[0];
                totalQty += parseInt(pYBookingChild[j].PYBItemChildDetails[i].DetailsQTY);
                //pYBookingChild[j].PYBItemChildDetails[i].BookingDate = new Date(pYBookingChild[j].PYBItemChildDetails[i].BookingDate).toDateString();


                var dayValidDurationId = getDefaultValueWhenInvalidN(pYBookingChild[j].DayValidDurationId);
                var isDateValidObj = ch_IsDateValid_DayValidDuration(pYBookingChild[j].PYBItemChildDetails[i].BookingDate, dayValidDurationId, masterData.DayValidDurations);
                if (!isDateValidObj.IsValid && masterData.IsCheckDVD) {

                    toastr.error(`Minimum date for this sourcing mode (Yarn Row at ${j + 1}, Booking Date Row at ${i + 1}) is ${ch_customDateFormat(isDateValidObj.MinDate)}`);
                    hasError = true;
                    break;

                }
            }

            if (hasError) break;
            
            if (totalQty != pYBookingChild[j].QTY) {
                hasError = true;
                toastr.error(`Sum of booking date qty (${totalQty}) must be ${pYBookingChild[j].QTY} (row ${currentRow})`);
                break;
            }
        }
        if (hasError) return false;

        if (isPendingMnMPage) {
            data.isMarketingFlag = isMarketingFlag;
        } else {
            data.isMarketingFlag = isMarketingFlag;
        }
        
        data.ProjectionYarnBookingItemChilds = pYBookingChild;
        data.BookingByID = $formEl.find("#BookingByID").val();
        data.DepartmentID = $formEl.find("#DepartmentID").val();
        data.SeasonID = $formEl.find("#SeasonID").val();
        data.FinancialYearID = $formEl.find("#FinancialYearID").val();
        //Data send to controller
        data.SendToApprover = SendToApprover;

        var hasError = false;
        if (data.ProjectionYarnBookingItemChilds) {
            for (var iChild = 0; iChild < data.ProjectionYarnBookingItemChilds.length; iChild++) {
                if (data.ProjectionYarnBookingItemChilds[iChild].Segment5ValueId) {
                    if (data.ProjectionYarnBookingItemChilds[iChild].Segment5ValueId == 60416 && (data.ProjectionYarnBookingItemChilds[iChild].ShadeCode == null || data.ProjectionYarnBookingItemChilds[iChild].ShadeCode == "")) {
                        toastr.error("Select shade code for color melange"); //Color Melange = 60416
                        hasError = true;
                        break;
                    }
                }
            }
        }
        if (hasError) return false;

        //Validation
        
        if (isPendingMnMPage) {
            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);
        }
   
        if (isValidChildForm(data)) return;

        axios.post("/api/projection-yarn-booking/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
                childDels.length = 0;
            })
            .catch(showResponseError);
    }

    function revise(isApprove, isSendToApprover) {
        $formEl.find("#BuyerIDsList").val($formEl.find("#BuyerIDs").val().map(function (el) {
            return el
        }).toString());
        $formEl.find("#BuyerTeamIDsList").val($formEl.find("#BuyerTeamIDs").val().map(function (el) {
            return el
        }).toString());

        var data = formDataToJson($formEl.serializeArray());

        data.BuyerIDsList = getDefaultValueWhenInvalidS(data.BuyerIDsList);
        data.BuyerTeamIDsList = getDefaultValueWhenInvalidS(data.BuyerTeamIDsList);
        data.BuyerTeamIDsList = (data.BuyerTeamIDsList.length && data.BuyerTeamIDsList[0] == ',') ? data.BuyerTeamIDsList.slice(1) : data.BuyerTeamIDsList;

        if (data.BuyerIDsList.length == 0 && data.BuyerTeamIDsList.length > 0) {
            return toastr.error("Without buyer, buyer team is invalid !");
        }

        if (isPendingMnMPage || isApproveMnMPage || isAcknowledgeMnMPage) {
            if (data.BuyerIDsList.length == 0) {
                return toastr.error("Select buyer !");
            }
            if (data.BuyerTeamIDsList.length == 0) {
                return toastr.error("Select buyer team !");
            }
        }

        var pYBookingChild = $tblChildEl.getCurrentViewRecords();

        if (pYBookingChild.length <= 0) {
            return toastr.error("Please Select at least one item!");
        }


        var hasError = false,
            shadeCodeInvalidRowIndexs = [];

        for (var j = 0; j < pYBookingChild.length; j++) {
            var currentRow = j + 1;
            if (isInvalidValue(pYBookingChild[j].Segment1ValueId)) {
                hasError = true;
                toastr.error(`Select composition at row ${currentRow}`);
                break;
            }
            if (isInvalidValue(pYBookingChild[j].Segment2ValueId)) {
                hasError = true;
                toastr.error(`Select yarn type at row ${currentRow}`);
                break;
            }
            else if (isInvalidValue(pYBookingChild[j].Segment3ValueId)) {
                hasError = true;
                toastr.error(`Select manufacturing process at row ${currentRow}`);
                break;
            }
            else if (isInvalidValue(pYBookingChild[j].Segment4ValueId)) {
                hasError = true;
                toastr.error(`Select sub process at row ${currentRow}`);
                break;
            }
            else if (isInvalidValue(pYBookingChild[j].Segment5ValueId)) {
                hasError = true;
                toastr.error(`Select quality parameter at row ${currentRow}`);
                break;
            }
            else if (isInvalidValue(pYBookingChild[j].Segment6ValueId)) {
                hasError = true;
                toastr.error(`Select count at row ${currentRow}`);
                break;
            }
            pYBookingChild[j] = setYarnSegDesc(pYBookingChild[j]);

            var dayValidDurationId = getDefaultValueWhenInvalidN(pYBookingChild[j].DayValidDurationId);

            if (dayValidDurationId == 0 && masterData.IsCheckDVD) {
                toastr.error(`Select Yarn Sourcing Mode (At row ${j + 1})`);
                hasError = true;
                break;
            }

            if ((pYBookingChild[j].Segment5ValueDesc.toLowerCase() == "melange" || pYBookingChild[j].Segment5ValueDesc.toLowerCase() == "color melange") && (pYBookingChild[j].ShadeCode == null || pYBookingChild[j].ShadeCode == "")) {
                toastr.error(`Select shade code for color melange (row ${currentRow})`);
                hasError = true;
                break;
            }

            if (isSendToApprover && pYBookingChild[j].PYBItemChildDetails.length == 0) {
                hasError = true;
                toastr.error(`Set minimum one booking date for every single item information at row ${currentRow}`);
                break;
            }

            var totalQty = 0;
            for (var i = 0; i < pYBookingChild[j].PYBItemChildDetails.length; i++) {
                totalQty += parseInt(pYBookingChild[j].PYBItemChildDetails[i].DetailsQTY);
                pYBookingChild[j].PYBItemChildDetails[i].BookingDate = new Date(pYBookingChild[j].PYBItemChildDetails[i].BookingDate).toDateString();


                var dayValidDurationId = getDefaultValueWhenInvalidN(pYBookingChild[j].DayValidDurationId);
                var isDateValidObj = ch_IsDateValid_DayValidDuration(pYBookingChild[j].PYBItemChildDetails[i].BookingDate, dayValidDurationId, masterData.DayValidDurations);
                if (!isDateValidObj.IsValid && masterData.IsCheckDVD) {
                    toastr.error(`Minimum date for this sourcing mode (Yarn Row at ${j + 1}, Booking Date Row at ${i + 1}) is ${ch_customDateFormat(isDateValidObj.MinDate)}`);
                    hasError = true;
                    break;
                }
            }

            if (hasError) break;

            if (totalQty != pYBookingChild[j].QTY) {
                hasError = true;
                toastr.error(`Sum of booking date qty (${totalQty}) must be ${pYBookingChild[j].QTY} (row ${currentRow})`);
                break;
            }
        }
        if (hasError) return false;

        if (shadeCodeInvalidRowIndexs.length > 0) {
            if (!confirm(`Do you want to save without shade code at row (${shadeCodeInvalidRowIndexs.join(',')})?`)) {
                return false;
            }
        }

        data["ProjectionYarnBookingItemChilds"] = pYBookingChild;
        data["BookingByID"] = $formEl.find("#BookingByID").val();
        data["DepartmentID"] = $formEl.find("#DepartmentID").val();
        data["SeasonID"] = $formEl.find("#SeasonID").val();
        data["FinancialYearID"] = $formEl.find("#FinancialYearID").val();

        var hasError = false;
        if (data.ProjectionYarnBookingItemChilds) {
            for (var iChild = 0; iChild < data.ProjectionYarnBookingItemChilds.length; iChild++) {
                if (data.ProjectionYarnBookingItemChilds[iChild].Segment5ValueId) {
                    if (data.ProjectionYarnBookingItemChilds[iChild].Segment5ValueId == 60416 && (data.ProjectionYarnBookingItemChilds[iChild].ShadeCode == null || data.ProjectionYarnBookingItemChilds[iChild].ShadeCode == "")) {
                        toastr.error("Select shade code for color melange"); //Color Melange = 60416
                        hasError = true;
                        break;
                    }
                }
            }
        }
        if (hasError) return false;

        if (isPendingMnMPage) {
            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);
        }

        if (isValidChildForm(data)) return;

        //Data send to controller
        data.SendToApprover = isSendToApprover;
        data.RevisionStatus = "Revision";
        data.IsApprove = isApprove;

        if (isPendingMnMPage) {
            data.isMarketingFlag = isMarketingFlag;
        } else {
            data.isMarketingFlag = isMarketingFlag;
        }

        axios.post("/api/projection-yarn-booking/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
                childDels.length = 0;
            })
            .catch(showResponseError);
    }

    function AcknowledgeMnM() {
        $formEl.find("#BuyerIDsList").val($formEl.find("#BuyerIDs").val().map(function (el) {
            return el
        }).toString());
        $formEl.find("#BuyerTeamIDsList").val($formEl.find("#BuyerTeamIDs").val().map(function (el) {
            return el
        }).toString());
        var data = formDataToJson($formEl.serializeArray());

        data.BuyerIDsList = getDefaultValueWhenInvalidS(data.BuyerIDsList);
        data.BuyerTeamIDsList = getDefaultValueWhenInvalidS(data.BuyerTeamIDsList);
        data.BuyerTeamIDsList = (data.BuyerTeamIDsList.length && data.BuyerTeamIDsList[0] == ',') ? data.BuyerTeamIDsList.slice(1) : data.BuyerTeamIDsList;

        if (data.BuyerIDsList.length == 0 && data.BuyerTeamIDsList.length > 0) {
            return toastr.error("Without buyer, buyer team is invalid !");
        }

        if (isPendingMnMPage || isApproveMnMPage || isAcknowledgeMnMPage) {
            if (data.BuyerIDsList.length == 0) {
                return toastr.error("Select buyer !");
            }
            if (data.BuyerTeamIDsList.length == 0) {
                return toastr.error("Select buyer team !");
            }
        }

        var pYBookingChild = $tblChildEl.getCurrentViewRecords();

        var hasError = false;
        for (var j = 0; j < pYBookingChild.length; j++) {

            var dayValidDurationId = getDefaultValueWhenInvalidN(pYBookingChild[j].DayValidDurationId);

            if (dayValidDurationId == 0 && masterData.IsCheckDVD) {
                toastr.error(`Select Yarn Sourcing Mode (At row ${j + 1})`);
                hasError = true;
                break;
            }

            for (var i = 0; i < pYBookingChild[j].PYBItemChildDetails.length; i++) {
                pYBookingChild[j].PYBItemChildDetails[i].BookingDate = new Date(pYBookingChild[j].PYBItemChildDetails[i].BookingDate).toDateString();


                var dayValidDurationId = getDefaultValueWhenInvalidN(pYBookingChild[j].DayValidDurationId);
                var isDateValidObj = ch_IsDateValid_DayValidDuration(pYBookingChild[j].PYBItemChildDetails[i].BookingDate, dayValidDurationId, masterData.DayValidDurations);
                if (!isDateValidObj.IsValid && masterData.IsCheckDVD) {
                    toastr.error(`Minimum date for this sourcing mode (Yarn Row at ${j + 1}, Booking Date Row at ${i + 1}) is ${ch_customDateFormat(isDateValidObj.MinDate)}`);
                    hasError = true;
                    break;
                }
            }
        }
        if (hasError) return false;

        data.ProjectionYarnBookingItemChilds = pYBookingChild;
        data.BookingByID = $formEl.find("#BookingByID").val();
        data.DepartmentID = $formEl.find("#DepartmentID").val();
        data.SeasonID = $formEl.find("#SeasonID").val();
        data.FinancialYearID = $formEl.find("#FinancialYearID").val();

        if (data.ProjectionYarnBookingItemChilds) {
            for (var iChild = 0; iChild < data.ProjectionYarnBookingItemChilds.length; iChild++) {
                data.ProjectionYarnBookingItemChilds[iChild] = setYarnSegDesc(data.ProjectionYarnBookingItemChilds[iChild]);

                if (data.ProjectionYarnBookingItemChilds[iChild].Segment5ValueId) {
                    if (data.ProjectionYarnBookingItemChilds[iChild].Segment5ValueId == 60416 && (data.ProjectionYarnBookingItemChilds[iChild].ShadeCode == null || data.ProjectionYarnBookingItemChilds[iChild].ShadeCode == "")) {
                        toastr.error("Select shade code for color melange"); //Color Melange = 60416
                        hasError = true;
                        break;
                    }
                }
            }
        }
        if (hasError) return false;

        if (isPendingMnMPage) {
            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);
        }

        if (isValidChildForm(data)) return;

        //Data send to controller
        data.SendToApprover = true;

        axios.post("/api/projection-yarn-booking/acknowledgemnm", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
                childDels.length = 0;
            })
            .catch(showResponseError);
    }

    function acknowledge(data) {

        if (data.ProjectionYarnBookingItemChilds) {
            for (var iChild = 0; iChild < data.ProjectionYarnBookingItemChilds.length; iChild++) {
                data.ProjectionYarnBookingItemChilds[iChild] = setYarnSegDesc(data.ProjectionYarnBookingItemChilds[iChild]);
            }
        }
        axios.post("/api/projection-yarn-booking/acknowledge", data)
            .then(function () {
                toastr.success("Acknowledged successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function isValidChildForm(data) {
        var isValidItemInfo = false;

        $.each(data["ProjectionYarnBookingItemChilds"], function (i, el) {
            if (el.QTY == "" || el.QTY == null || el.QTY <= 0) {
                toastr.error("Qty is required.");
                isValidItemInfo = true;
            }
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        CompanyID: {
            presence: true
        },
        DepartmentID: {
            presence: true
        },
        SeasonID: {
            presence: true
        },
        FinancialYearID: {
            presence: true
        }
    }

    async function getSegments() {
        _segments = await axios.get(getYarnItemsApiUrl([]));
        _segments = _segments.data;
    }

    function showHideControls(Flag) {
        if (Flag) {
            $formEl.find("#PYBookingDate").prop("disabled", false);
            $formEl.find("#BookingByID").prop("disabled", false);
            $formEl.find("#CompanyID").prop("disabled", false);
            $formEl.find("#BuyerID").prop("disabled", false);
            $formEl.find("#Remarks").prop("disabled", false);
            $formEl.find("#SeasonID").prop("disabled", false);
            $formEl.find("#FinancialYearID").prop("disabled", false);
        }
        else {
            $formEl.find("#PYBookingDate").prop("disabled", true);
            $formEl.find("#BookingByID").prop("disabled", true);
            $formEl.find("#CompanyID").prop("disabled", true);
            $formEl.find("#BuyerID").prop("disabled", true);
            $formEl.find("#Remarks").prop("disabled", true);
            $formEl.find("#DepartmentID").prop("disabled", true);
            $formEl.find("#SeasonID").prop("disabled", true);
            $formEl.find("#FinancialYearID").prop("disabled", true);
        }
    }
    //Composition related function
    function initTblCreateComposition() {
        var YarnSubProgramNewsFilteredList = masterData.YarnSubProgramNews;
        var CertificationsFilteredList = masterData.Certifications;
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
                            //dataSource: YarnSubProgramNewsFilteredList,
                            dataSource: [],
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
                            //dataSource: CertificationsFilteredList,
                            dataSource: [],
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
        var blendTypeNames = [];
        var programTypeNames = [];
        //compositionComponents = _.sortBy(compositionComponents, "Percent").reverse();
        compositionComponents = compositionComponents.sort((a, b) => b.Percent - a.Percent);
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

            console.log(compositionComponents);
            component.FiberTypeName = getDefaultValueWhenInvalidS(component.FiberTypeName);
            if (component.FiberTypeName.length > 0) {
                blendTypeNames.push(component.FiberTypeName);
            }
            component.ProgramTypeName = getDefaultValueWhenInvalidS(component.ProgramTypeName);
            if (component.ProgramTypeName.length > 0) {
                programTypeNames.push(component.ProgramTypeName);
            }

        });

        blendTypeNames = [...new Set(blendTypeNames)];
        var blendTypeName = blendTypeNames.join(" + ");

        programTypeNames = [...new Set(programTypeNames)];
        var programTypeName = "Conventional";
        var indexF = programTypeNames.findIndex(x => x == "Sustainable");
        if (indexF > -1) {
            programTypeName = "Sustainable";
        }

        //var data = {
        //    SegmentValue: composition
        //};
        var data = {
            
             SegmentValue: composition,
            BlendTypeName: "blendTypeName",
            ProgramTypeName: programTypeName
        }

        axios.post("/api/rnd-free-concept-mr/save-yarn-composition", data)
            .then(function () {
                $pageEl.find(`#modal-new-composition-${pageId}`).modal("hide");
                toastr.success("Composition added successfully.");
                //masterData.CompositionList.unshift({ id: response.data.Id, text: response.data.SegmentValue });
                // initChildTable(masterData.Childs);
            })
            .catch(error => {
                if (error.response) {
                    toastr.error(error.response.data);
                } else {
                    toastr.error('Error message:', error.response.data.Message);
                }
                args.cancel = true;
            });
            //.catch(showResponseError)
    }
    function showAddComposition() {
        initTblCreateComposition();
        $pageEl.find(`#modal-new-composition-${pageId}`).modal("show");
    }
    function sendMail(pyBookingID) {
        var url = `/api/projection-yarn-booking/sendMail/${pyBookingID}`;
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
    function setYarnSegDesc(obj) {
        for (var indexSeg = 1; indexSeg <= 6; indexSeg++) {
            var segIdProp = "Segment" + indexSeg + "ValueId";
            var segDescProp = "Segment" + indexSeg + "ValueDesc";
            var listName = "Segment" + indexSeg + "ValueList";

            if (obj[segIdProp] > 0 && (typeof obj[segDescProp] !== "undefined" || obj[segDescProp] != "")) {
                var objSeg = _segments[listName].find(s => s.id == obj[segIdProp]);
                if (objSeg) {
                    obj[segDescProp] = objSeg.text;
                }
            }
        }
        obj.YarnCategory = GetYarnShortForm(obj.Segment1ValueDesc,
            obj.Segment2ValueDesc,
            obj.Segment3ValueDesc,
            obj.Segment4ValueDesc,
            obj.Segment5ValueDesc,
            obj.Segment6ValueDesc,
            obj.ShadeCode);

        return obj;
    }
})();