(function () {
    var menuId, pageName;
    var toolbarId, $tblOtherItemEl,tblOtherItemId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, tblChildId, $formEl, tblMasterId, $tblRollEl, $divRollEl, $tblKProductionE1;;
    var status;
    var masterData = null;
    var pageId, pageIdWithHash;
    var _SFDID = 999;
    var _FBAckChildID = 999;
    var _paramType = {
        LabdipFabricDelivery: 0,
        LabdipFabricDeliveryApprove: 1,
        LabdipFabricAck: 2,
    }
    var menuType = 0;
    var cTypeID = 0;
    var vIsBDS = 0;
    var ChildFabricItemList = new Array();
    var ChildOthersItemList = new Array();
    var _subGroup = 1;
    var IsEdit = false;
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
        tblMachineParamId = $("#tblMachineParam" + pageId);
        $tblRollEl = $("#tblRoll" + pageId);
        $divRollEl = $("#divRoll" + pageId);
        $tblKProductionE1 = $("#tblKProduction" + pageId);
        pageIdWithHash = "#" + pageId;
        $toolbarEl.find("#btnAddDC").fadeOut();
        menuType = localStorage.getItem("labDipFabricDeliveryPage");
        menuType = parseInt(menuType);
        $formEl.find("#divGPNO").fadeOut();
        $formEl.find("#divGPDate").fadeOut();
        if (menuType == _paramType.LabdipFabricDelivery) {
            $toolbarEl.find("#btnAddDC").fadeIn();
            toggleActiveToolbarBtn($(pageIdWithHash).find("#btnPendingList"), $toolbarEl);
            status = statusConstants.PENDING;
            $formEl.find("#btnSave").show();
            $formEl.find("#btnSaveAndSendForApproval").show();
            $formEl.find("#btnApprove").hide();
            $formEl.find("#btnReject").hide();
            $formEl.find("#btnAcknowledge").hide();
            initMasterTable();

            $toolbarEl.find("#btnSendForApprovalList,#btnApprovedList,#btnEditList,#btnRejectedList").show();
            $toolbarEl.find("#btnPendingList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.PENDING;
                IsEdit = false;
                $toolbarEl.find("#btnAddDC").fadeIn();
                $formEl.find("#btnSave").show(); 
                $formEl.find("#btnSaveAndSendForApproval").show();
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnReject").hide();
                $formEl.find("#btnAcknowledge").hide();
                $formEl.find("#btnAddItemsFabric").show();
                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnEditList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.EDIT;
                IsEdit = true;
                $toolbarEl.find("#btnAddDC").hide();
                $formEl.find("#btnSave").show();
                $formEl.find("#btnSaveAndSendForApproval").show();
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnReject").hide();
                $formEl.find("#btnAcknowledge").hide(); 
                $formEl.find("#btnAddItemsFabric").hide();
                $formEl.find("#addItem,#splitRoll").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnSendForApprovalList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.PROPOSED_FOR_APPROVAL;
                IsEdit = false;
                $toolbarEl.find("#btnAddDC").fadeOut();
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnSaveAndSendForApproval").hide();
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnReject").hide();
                $formEl.find("#btnAcknowledge").hide();
                $formEl.find("#btnAddItemsFabric").hide(); 
                $(pageIdWithHash).find("#addItem").hide();
                $(pageIdWithHash).find("#splitRoll").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnApprovedList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.APPROVED;
                IsEdit = false;
                $toolbarEl.find("#btnAddDC").fadeOut();
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnSaveAndSendForApproval").hide();
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnReject").hide();
                $formEl.find("#btnAcknowledge").hide();
                $formEl.find("#btnAddItemsFabric").hide();
                $formEl.find("#addItem,#splitRoll").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnRejectedList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.REJECT;
                IsEdit = false;
                $toolbarEl.find("#btnAddDC").fadeOut();
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnSaveAndSendForApproval").hide();
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnReject").hide();
                $formEl.find("#btnAcknowledge").hide();
                $formEl.find("#btnAddItemsFabric").hide();
                $formEl.find("#addItem,#splitRoll").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnAcknowledgedList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.ACKNOWLEDGE;
                IsEdit = false;
                $toolbarEl.find("#btnAddDC").fadeOut();
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnSaveAndSendForApproval").hide();
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnReject").hide();
                $formEl.find("#btnAcknowledge").hide();
                $formEl.find("#btnAddItemsFabric").hide();
                $formEl.find("#addItem,#splitRoll").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $formEl.find("#btnCancel").on("click", backToList);
            $formEl.find("#btnAddItemsFabric").on("click", function (e) {
                e.preventDefault();
                var ChildsFabric = masterData.Childs;
                
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
                    fields: "ConceptNo,KnittingType,TechnicalName,Composition,Gsm,SubClassName,YarnSubProgram,BookingQty,ColorCode,GroupConceptNo,ConceptDate",
                    headerTexts: "Concept No,Machine Type,Technical Name,Composition,Gsm,Sub Class Name,Yarn Sub Program,Required Qty,Color Code,Group Concept No,Concept Date",
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
                        
                        initChildTable(ChildFabricItemList);
                    }
                });
                finder.showModal();
            });
            $formEl.find("#btnSplit").click(function (e) {
                e.preventDefault();
                split();
            });
            $formEl.find("#btnSaveKProd").click(function (e) {
                e.preventDefault();
                saveKProd();
            });
            $toolbarEl.find("#btnAddDC").on("click", getNewData);
        }
        else if (menuType == _paramType.LabdipFabricDeliveryApprove) {
            $toolbarEl.find("#btnAddDC").fadeOut();
            toggleActiveToolbarBtn($(pageIdWithHash).find("#btnPendingList"), $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            $formEl.find("#btnSave").hide();
            $formEl.find("#btnSaveAndSendForApproval").hide();
            $formEl.find("#btnApprove").show();
            $formEl.find("#btnReject").show();
            $formEl.find("#btnAcknowledge").hide();
            $formEl.find("#btnAddItemsFabric").hide();
            initMasterTable();

            $toolbarEl.find("#btnApprovedList,#btnRejectedList").show();
            $toolbarEl.find("#btnPendingList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.PROPOSED_FOR_APPROVAL;
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnSaveAndSendForApproval").hide();
                $formEl.find("#btnApprove").show();
                $formEl.find("#btnReject").show();
                $formEl.find("#btnAcknowledge").hide();
                $formEl.find("#btnAddItemsFabric").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnApprovedList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.APPROVED;
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnSaveAndSendForApproval").hide();
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnReject").hide();
                $formEl.find("#btnAcknowledge").hide();
                $formEl.find("#btnAddItemsFabric").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnRejectedList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.REJECT;
                IsEdit = false;
                $toolbarEl.find("#btnAddDC").fadeOut();
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnSaveAndSendForApproval").hide();
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnReject").hide();
                $formEl.find("#btnAcknowledge").hide();
                $formEl.find("#btnAddItemsFabric").hide();
                $formEl.find("#addItem,#splitRoll").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $toolbarEl.find("#btnAcknowledgedList").on("click", function (e) {
                e.preventDefault();
                status = statusConstants.ACKNOWLEDGE;
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnSaveAndSendForApproval").hide();
                $formEl.find("#btnApprove").hide();
                $formEl.find("#btnReject").hide();
                $formEl.find("#btnAcknowledge").hide();
                $formEl.find("#btnAddItemsFabric").hide();
                toggleActiveToolbarBtn(this, $toolbarEl);
                initMasterTable();
            });
            $formEl.find("#btnCancel").on("click", backToList);
        }
        else if (menuType == _paramType.LabdipFabricAck) 
            {
            $toolbarEl.find("#btnAddDC").fadeOut();
            toggleActiveToolbarBtn($(pageIdWithHash).find("#btnPendingList"), $toolbarEl);
            status = statusConstants.APPROVED;
            $formEl.find("#btnSave").hide();
            $formEl.find("#btnSaveAndSendForApproval").hide();
            $formEl.find("#btnApprove").hide();
            $formEl.find("#btnReject").hide();
            $formEl.find("#btnAcknowledge").show();
            $formEl.find("#btnAddItemsFabric").hide();
            initMasterTable();

            $toolbarEl.find("#btnSendForApprovalList,#btnApprovedList").hide();
                $toolbarEl.find("#btnPendingList").on("click", function (e) {
                    e.preventDefault();
                    status = statusConstants.APPROVED;
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnSaveAndSendForApproval").hide();
                    $formEl.find("#btnApprove").hide();
                    $formEl.find("#btnReject").hide();
                    $formEl.find("#btnAcknowledge").show();
                    $formEl.find("#btnAddItemsFabric").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    initMasterTable();
                });
                $toolbarEl.find("#btnAcknowledgedList").on("click", function (e) {
                    e.preventDefault();
                    status = statusConstants.ACKNOWLEDGE;
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnSaveAndSendForApproval").hide();
                    $formEl.find("#btnApprove").hide();
                    $formEl.find("#btnReject").hide();
                    $formEl.find("#btnAcknowledge").hide();
                    $formEl.find("#btnAddItemsFabric").hide();
                    toggleActiveToolbarBtn(this, $toolbarEl);
                    initMasterTable();
                });
                $formEl.find("#btnCancel").on("click", backToList);
        }




            $formEl.find("#btnSave").click(function (e) {
                e.preventDefault();
                if (IsEdit == false) {
                    save('Save');
                }
                else {
                    save('Edit');
                }
        });
        $formEl.find("#btnSaveAndSendForApproval").click(function (e) {
            e.preventDefault();
            save('SaveAndSendForApproval');
        });
        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            save('Approve');
        });
            $formEl.find("#btnReject").click(function (e) {
                e.preventDefault();
                $(pageIdWithHash).find("#txtRejectReason").val("");
                $(pageIdWithHash).find("#modalRejectReason").modal('show');
            });
        $(pageIdWithHash).find("#btnRejectConfirm").click(function (e) {
            e.preventDefault();
            $(pageIdWithHash).find("#modalRejectReason").modal('hide');
            save('Reject', $(pageIdWithHash).find("#txtRejectReason").val());
        });
        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            save('Acknowledge');
        });
        $formEl.find("#btnCancel").on("click", backToList);
        
    });
    function initMasterTable() {
        
            var columns = [
                {
                    headerText: 'Command', width: 100, textAlign: 'center', visible: (menuType == _paramType.LabdipFabricDelivery && status == statusConstants.EDIT) || (menuType == _paramType.LabdipFabricDeliveryApprove && status == statusConstants.PROPOSED_FOR_APPROVAL) || (menuType == _paramType.LabdipFabricAck && status == statusConstants.APPROVED ), commands: [
                        { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                        { type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
                    ]
                },
                {
                    headerText: 'Command', width: 100, textAlign: 'center', visible: (menuType == _paramType.LabdipFabricDelivery == 1 && (status == statusConstants.PROPOSED_FOR_APPROVAL || status == statusConstants.APPROVED || status == statusConstants.REJECT || status == statusConstants.ACKNOWLEDGE)) || (menuType == _paramType.LabdipFabricDeliveryApprove && (status == statusConstants.APPROVED || status == statusConstants.REJECT  || status == statusConstants.ACKNOWLEDGE)) || (menuType == _paramType.LabdipFabricAck && (status == statusConstants.ACKNOWLEDGE)), commands: [
                        { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                        { type: 'View Attachment', buttonOption: { cssClass: 'e-flat booking_attImage', iconCss: 'fa fa-file-image-o' } }
                    ]
                },
                {
                    field: 'LDFMasterID', headerText: 'LDFMasterID', visible: false
                },
                {
                    field: 'BookingNo', headerText: 'Concept No'
                },
                //{
                //    field: 'SLNo', headerText: 'SL No', visible: false 
                //},
                {
                    field: 'BookingDate', headerText: 'Booking Date', type: 'date', format: _ch_date_format_1, visible: menuType == _paramType.LabdipFabricDelivery && status == statusConstants.PENDING
                },
                {
                    field: 'BookingDate', headerText: 'Delivery Date', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING || menuType != _paramType.LabdipFabricDelivery
                },
                //{
                //    field: 'DateAdded', headerText: 'Delivery Date', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
                //},
                {
                    field: 'DateAdded', headerText: 'Acknowledge Date', type: 'date', format: _ch_date_format_1, visible: menuType == _paramType.LabdipFabricDelivery && status == statusConstants.PENDING
                },
                {
                    field: 'DeliveryNo', headerText: 'Delivery No', visible: status != statusConstants.PENDING || menuType != _paramType.LabdipFabricDelivery
                },
                {
                    field: 'RequiredQty', headerText: 'Required Qty'//, visible: status != statusConstants.PENDING || menuType != _paramType.LabdipFabricDelivery
                },
                {
                    field: 'StockQty', headerText: 'Stock Qty'//, visible: status != statusConstants.PENDING || menuType != _paramType.LabdipFabricDelivery
                },
                {
                    field: 'DeliveredQty', headerText: 'Delivered Qty', visible: status != statusConstants.PENDING || menuType != _paramType.LabdipFabricDelivery
                },
                //{
                //    field: 'CompanyName', headerText: 'Company'
                //},
                {
                    field: 'BuyerName', headerText: 'BuyerName', visible: menuType == _paramType.LabdipFabricDelivery && status == statusConstants.PENDING
                },
                //{
                //    field: 'SupplierName', headerText: 'Supplier Name', visible: IsLabdipFabricDelivery == 1 && status == statusConstants.PENDING
                //},
                //{
                //    field: 'SeasonName', headerText: 'Season Name', visible: IsLabdipFabricDelivery == 1 && status == statusConstants.PENDING
                //},
                //{
                //    field: 'Remarks', headerText: 'Remarks'
                //},
                {
                    field: 'RejectReason', headerText: 'Reject Reason', visible: status == statusConstants.REJECT
                }
        ];
        if (menuType == _paramType.LabdipFabricDelivery) {
            if (status == statusConstants.PENDING) {
                columns.unshift({ type: 'checkbox', width: 50 });
                selectionType = "Multiple";
            }
        }
            if ($tblMasterEl) $tblMasterEl.destroy();

            $tblMasterEl = new initEJ2Grid({
                tableId: tblMasterId,
                allowGrouping: true,
                apiEndPoint: `/api/labdip-fabricdelivery/labdip/list?status=${status}`,
                columns: columns,
                commandClick: handleCommands,
                queryCellInfo: cellModifyForBDSAck
            });
        }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            
            getViewLabDip(args.rowData.LFDMasterID);

        }
        else if (args.commandColumn.type == 'View') {
            _statusText = "";
            $formEl.find(".divUnAcknowledgeReason").hide();

         

                _statusText = args.rowData.StatusText;
            getViewLabDip(args.rowData.LFDMasterID);
            


                if (status == statusConstants.UN_ACKNOWLEDGE) {
                    $formEl.find(".divUnAcknowledgeReason").show();
                }
        

        }

        else if (args.commandColumn.type == 'Booking Report') {
            
                window.open(`/reports/InlinePdfView?ReportName=SampleFabric.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
            
        }

        else if (args.commandColumn.type == 'View Attachment') {
            if (args.rowData.ImagePath == '') {
                toastr.error("No attachment found!!");
            } else {
                var imagePath = constants.GMT_ERP_BASE_PATH + args.rowData.ImagePath;
                window.open(imagePath, "_blank");
            }
        }

    }
    function getViewLabDip(LFDMasterID) {
        var url = `/api/labdip-fabricdelivery/GetDeliveryDetails/${LFDMasterID}`;
        axios.get(url)
            .then(function (response) {

                $divDetailsEl.show();
                $divTblEl.hide();
                masterData = response.data;
                masterData.DeliveredDate = formatDateToDefault(masterData.DeliveredDate);
                //masterData.BookingDate = formatDateToDefault(masterData.BookingDate);
                //masterData.FBookingChild.FBAChildPlannings = response.data.AllChildPlannings;

                if (menuType == _paramType.LabdipFabricAck && status == statusConstants.APPROVED) {
                    for (var i = 0; i < masterData.Childs.length; i++) {
                        for (var j = 0; j < masterData.Childs[i].ChildItems.length; j++) {
                            masterData.Childs[i].ChildItems[j].IsAcknowledge = true;
                        }
                    }
                }
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
                //if (masterData.HasFabric) {
            
                //        masterData.FBookingChild.map(m => {
                //            m = DeepClone(setGreyRelatedSingleField(m));
                //            m.ChildItems = setYarnRelatedFields(m.ChildItems, m);
                //        });
                   
                //    initChildTable([]);
                //    $formEl.find("#divFabricInfo").show();
                //}
                //else $formEl.find("#divFabricInfo").hide();

                //if (masterData.HasCollar) {
                //        masterData.FBookingChildCollor.map(m => {
                //            m = DeepClone(setGreyRelatedSingleField(m));
                //            m.ChildItems = setYarnRelatedFields(m.ChildItems, m);
                //        });
                    
                //    initChildCollar(masterData.FBookingChildCollor);
                //    $formEl.find("#divCollarInfo").show();
                //}
                //else $formEl.find("#divCollarInfo").hide();

                //if (masterData.HasCuff) {
                //        masterData.FBookingChildCuff.map(m => {
                //            m = DeepClone(setGreyRelatedSingleField(m));
                //            m.ChildItems = setYarnRelatedFields(m.ChildItems, m);
                //        });
                //    initChildCuff(masterData.FBookingChildCuff);
                //    $formEl.find("#divCufInfo").show();
                //}
                //else $formEl.find("#divCufInfo").hide();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
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
    function backToList() {
        
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        if (status === statusConstants.NEW) {
            status = statusConstants.ACKNOWLEDGE;
            toggleActiveToolbarBtn("#btnPendingList", $toolbarEl);
        }
        initMasterTable();
    }
    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#SFDID").val(-1111);
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
                    headerText: 'Commands', width: 80, visible: menuType == _paramType.LabdipFabricDelivery && (status == statusConstants.PENDING || status == statusConstants.EDIT), commands: [
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
                //{ field: 'ColorCode', headerText: 'Color Code', allowEditing: false },
                //{ field: 'ColorName', headerText: 'Color Name', allowEditing: false },
                //{
                //    field: 'FormID', width: 200, headerText: 'Form', valueAccessor: ej2GridDisplayFormatter,
                //    dataSource: masterData.FormList,
                //    displayField: "Form",
                //    edit: ej2GridDropDownObj({})
                //},
                {
                    field: 'HangerQtyInPcs', headerText: 'Hanger Qty (Pcs)', visible: false ,

                },
                { field: 'ColorName', headerText: 'Color', allowEditing: false },
                { field: 'BookingQty', headerText: 'Required Quantity(kg)', allowEditing: false },
                { field: 'StockQty', headerText: 'Stock Qty', allowEditing: false, editType: "numericedit", width: 80, edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
                { field: 'DCQty', headerText: 'Challan Qty', visible: false, allowEditing: false, allowEditing: false, editType: "numericedit", width: 80, edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
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
            },//
            childGrid: {
                queryString: 'SFDChildID',
                allowResizing: true,
                toolbar: [
                    { text: 'Add Roll', tooltipText: 'Add Roll', prefixIcon: 'e-icons e-add', id: 'addItem', visible: menuType == _paramType.LabdipFabricDelivery && (status == statusConstants.PENDING || status == statusConstants.EDIT)},
                    { text: 'Split Roll', tooltipText: 'Split Roll', prefixIcon: 'e-icons e-copy', id: 'splitRoll', visible: menuType == _paramType.LabdipFabricDelivery && (status == statusConstants.PENDING || status == statusConstants.EDIT)}
                ],
                editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: [
                    {
                        headerText: 'Action', width: 10, visible: menuType == _paramType.LabdipFabricDelivery && (status == statusConstants.PENDING || status == statusConstants.EDIT), commands: [
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
                    { field: 'BatchNo', headerText: 'Batch No',allowEditing:false, allowResizing: true, width: 40 },
                    { field: 'UseBatchNo', visible: false, width: 40 },
                    { field: 'RackID', visible: false, width: 40 },
                    { field: 'WeightSheetNo', visible: false, width: 40 },
                    { field: 'RollQtyKg', width: 40, allowEditing: false, allowResizing: true, headerText: 'Roll Qty(kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
                    { field: 'RollQtyPcs', width: 120, allowEditing: false, allowResizing: true, headerText: 'Roll Qty(Pcs)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                    {
                        field: 'IsAcknowledge', headerText: 'Acknowledged?', allowEditing: (menuType == _paramType.LabdipFabricAck && (status == statusConstants.APPROVED)), visible: menuType == _paramType.LabdipFabricAck || status == statusConstants.ACKNOWLEDGE, displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', defaultValue: true
                    }
                ],
                toolbarClick: function (args) {
                    var iDCQty = 0;
                    var data = this.parentDetails.parentRowData;
                    selectedData = data;

                    if (args.item.id === "addItem") {
                        
                        setSubGroupValue(masterData.ChildItems);
                        var ChildItemsFabric = masterData.ChildItems.filter(function (el) {
                            return el.ConceptID == selectedData.ConceptID;
                        });
                        if (ChildItemsFabric.length == 0) {
                            toastr.error("No Roll Found");
                            return false;
                        }

                        var ChildsFabric = masterData.Childs;

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
                                fields: "RollNo,RollQtyKg,RollQtyPcs,GroupConceptNo",
                                headerTexts: "Roll No,Roll Qty(kg),Roll Qty(Pcs),Group Concept No",
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
                                    //ChildsFabric[indexFind].ChildItems = FabricItemList;
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
                            return el.ConceptID == selectedData.ConceptID;
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
                { field: 'BookingQtyPcs', headerText: 'Required Quantity(Pcs)', allowEditing: false },
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
                    { field: 'RollQtyPcs', headerText: 'Roll Qty(Pcs)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
                    { field: 'IsAcknowledge', headerText: 'Acknowledged?', allowEditing: (menuType == _paramType.LabdipFabricAck && (status == statusConstants.APPROVED)), visible: (menuType == _paramType.LabdipFabricAck && (status == statusConstants.APPROVED)), displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center'}
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
                            return el.ConceptID == selectedData.ConceptID;
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
    async function initChild(data) {
        
        data = setCalculatedValues(data);
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        var additionalColumns = [];
    
            columns = [
                { field: 'ConsumptionID', isPrimaryKey: true, visible: false },
                { field: 'ConceptTypeID', visible: false },
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
                    field: 'Instruction', headerText: 'Instruction', allowEditing: false
                },
                {
                    field: 'IsFabricReq', headerText: 'Fabric Req?', allowEditing: false, visible: true, displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center'
                },
                {
                    field: 'BookingQty', headerText: 'Booking Qty', width: 85, allowEditing: false
                },
                {
                    field: 'TotalQty', headerText: 'Total Qty', width: 85, allowEditing: false, visible: status == false //statusConstants.COMPLETED
                }
                ,
                {
                    field: 'LFDDeliveredQty', headerText: 'Delivered Qty', width: 85, allowEditing: menuType == _paramType.LabdipFabricDelivery && status == statusConstants.PENDING, cellStyle: function () { return { classes: 'm-w-100' } },
                },
                {
                    field: 'LFDDeliveredQtyInKG', headerText: 'Delivered Qty In KG', width: 85, allowEditing: menuType == _paramType.LabdipFabricDelivery && status == statusConstants.PENDING, cellStyle: function () { return { classes: 'm-w-100' } },
                }
                ,
                {
                    field: 'LFDAckQty', headerText: 'Acknowledged Qty', width: 85, allowEditing: menuType == _paramType.LabdipFabricAck && status == statusConstants.APPROVED, cellStyle: function () { return { classes: 'm-w-100' } }, visible: (menuType == _paramType.LabdipFabricAck && status == statusConstants.APPROVED) || status == statusConstants.ACKNOWLEDGE//statusConstants.COMPLETED
                },
                {
                    field: 'LFDAckQtyInKG', headerText: 'Acknowledged Qty In KG', width: 85, allowEditing: menuType == _paramType.LabdipFabricAck && status == statusConstants.APPROVED, cellStyle: function () { return { classes: 'm-w-100' } }, visible: (menuType == _paramType.LabdipFabricAck && status == statusConstants.APPROVED) || status == statusConstants.ACKNOWLEDGE //statusConstants.COMPLETED
                }
            ];
   

        var additionalColumns = [
            //{
            //    field: 'DeliveredQty', headerText: 'Delivered Qty', width: 85, allowEditing: false, visible: status == statusConstants.APPROVED
            //},
            //{
            //    field: 'DelivereyComplete', headerText: 'Is Delivered?', displayAsCheckBox: true, textAlign: 'Center', visible: status == statusConstants.APPROVED
            //}
        ]
        columns.push.apply(columns, additionalColumns);


        ej.base.enableRipple(true);
     

            $tblChildEl = new ej.grids.Grid({
                dataSource: data,
                allowResizing: true,
                columns: columns,
                commandClick: childCommandClick,
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                recordClick: function (args) {
                    if (args.column && args.column.field == "TotalDays") {                       
                        _oRow = args.rowData;
                        _index = args.rowIndex;
                        _modalFrom = subGroupNames.FABRIC;
                        initCriteriaIDTable(_oRow.CriteriaNames, _oRow.FBAChildPlannings, _oRow.FBAChildPlanningsWithIds, _oRow.BookingChildID);
                        $modalCriteriaEl.modal('show');
                    } else if (args.column && args.column.field == "MachineType") {

                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === "save") {
                        args.data = setArgDataValues(args.data, args.rowData);
                    }
                },
            });

      
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    async function childCommandClick(e) {
        var childData = e.rowData;

    }
    function save(btnstatus, result = "") {
        
        var data = formElToJson($formEl);
        if (result !="") {
            data.RejectReason = result;
            data.IsReject = true;
        }

            data.Childs = $tblChildEl.getCurrentViewRecords();
        //for (var i = 0; i < data.FBookingChild.length; i++) {
        //    var child = data.FBookingChild[i];
        //    if (child.BookingQty != null && child.LFDDeliveredQty > child.BookingQty+5) {
        //        toastr.warning("Delivery qty maximum limit crossed. ");
        //        return false;
        //    }
        //}
        if (typeof data.SFDID === "undefined") data.SFDID = 0;
        data.ConceptStatus = btnstatus;
        axios.post("/api/labdip-fabricdelivery/Save", data)
            .then(function (response) {
                //toastr.success("Saved successfully.");
                showBootboxAlert("Labdip Fabric Delivery No: <b>" + response.data + "</b> saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    function setGreyRelatedSingleField(item) {
        //var pL = 10; //percentage
        //var knitting = 0.5 //percentage
        var others = 9.5 / 100; //percentage

        if (!item.GreyReqQty) {
            item.GreyReqQty = item.BookingQty + (item.BookingQty * others);
        }

        if (typeof item.GreyLeftOverQty === "undefined") item.GreyLeftOverQty = 0;

        if (item.GreyLeftOverQty || item.GreyLeftOverQty == 0) {
            item.GreyProdQty = item.GreyReqQty - item.GreyLeftOverQty;
        } else {
            item.GreyProdQty = 0;
        }

        item.GreyReqQty = parseFloat(item.GreyReqQty).toFixed(2);
        item.GreyLeftOverQty = parseFloat(item.GreyLeftOverQty).toFixed(2);
        item.GreyProdQty = parseFloat(item.GreyProdQty).toFixed(2);

        return item;
    }
    function setYarnRelatedFields(items, parent) {
        items.map(item => {
            item = DeepClone(setYarnRelatedSingleField(item, parent));
        });
        return items;
    }
    function setYarnRelatedSingleField(item, parent) {

        if (!isAdditionBulkBooking()) {
            var netConsumption = (parseFloat(parent.GreyProdQty) * parseFloat(item.Distribution) / 100);
            item.YarnReqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(item.Allowance)) / 100);
        }

        if (typeof item.YarnLeftOverQty === "undefined") item.YarnLeftOverQty = 0;

        if (item.YarnLeftOverQty || item.YarnLeftOverQty == 0) {
            item.YarnBalanceQty = item.YarnReqQty - item.YarnLeftOverQty;
        } else {
            item.YarnBalanceQty = 0;
        }

        item.YarnReqQty = parseFloat(item.YarnReqQty).toFixed(2);
        item.YarnLeftOverQty = parseFloat(item.YarnLeftOverQty).toFixed(2);
        item.YarnBalanceQty = parseFloat(item.YarnBalanceQty).toFixed(2);

        if (isNaN(item.YarnReqQty)) item.YarnReqQty = 0;
        if (isNaN(item.YarnLeftOverQty)) item.YarnLeftOverQty = 0;
        if (isNaN(item.YarnBalanceQty)) item.YarnBalanceQty = 0;

        return item;
    }
    function diplayPlanningCriteriaTime(field, data, column) {
        column.disableHtmlEncode = false;
        return `<a class="btn btn-xs btn-default" href="javascript:void(0)" title="Total Time">
                                     ${data[field] ? data[field] : 0}
                                </a>`;
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
    function setSubGroupValue(childItems) {
        _subGroup = 0;
        if (childItems != null && childItems.length > 0) {
            if (childItems[0].SubGroupName == "Fabric") _subGroup = 1;
            else if (childItems[0].SubGroupName == "Collar") _subGroup = 11;
            else if (childItems[0].SubGroupName == "Cuff") _subGroup = 12;
            else _subGroup = 1;
        }
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
                obj.RollQtyKg = (parseFloat(roll.RollQtyKg) / totalRoll).toFixed(2);
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
        debugger
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
                var ChildItemsFabric = masterData.ChildItems.filter(function (el) {
                    return el.ConceptID == selectedData.ConceptID;
                });
                initMachineParamTable(ChildItemsFabric);
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
    function loadOthersFirstLevelChildGrid() {
        this.dataSource = this.parentDetails.parentRowData.ChildItems;
    }
    function getNewData() {
        if ($tblMasterEl.getSelectedRecords().length == 0) {
            toastr.error("Please select row(s)!");
            return;
        }

        //var uniqueAry = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "OrderUnitID");

        //if (uniqueAry.length != 1) {
        //    toastr.error("Selected row(s) order unit should be same!");
        //    return;
        //}
        //var uniqueAry = distinctArrayByProperty($tblMasterEl.getSelectedRecords(), "CompanyID");

        //if (uniqueAry.length != 1) {
        //    toastr.error("Selected row(s) company name should be same!");
        //    return;
        //}
        
        var iDs = $tblMasterEl.getSelectedRecords().map(function (el) { return el.BookingNo }).toString();
        var selectedRecords = $tblMasterEl.getSelectedRecords();
        var IsConceptTypeFlag = false;
        $.each(selectedRecords, function (j, obj) {
            if (obj.ConceptTypeID != 1) {
                IsConceptTypeFlag = true;
                return;
            }
        });
        //var url = `/api/labdip-fabricdelivery/labDip/${bookingId}/${isRnD}`;
        axios.get(`/api/labdip-fabricdelivery/labDip/${iDs}`)
            .then(function (response) {
                isEditable = true;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.DeliveryNo = "<<--New-->>";
                masterData.DeliveredDate = formatDateToDefault(Date.now());
                //masterData.GPDate = formatDateToDefault(masterData.GPDate);
                
                masterData.GroupConceptNo = iDs;
                setFormData($formEl, masterData);

                //if (masterData.HasFabric) {

                //    masterData.FBookingChild.map(m => {
                //        m = DeepClone(setGreyRelatedSingleField(m));
                //        m.ChildItems = setYarnRelatedFields(m.ChildItems, m);
                //    });
                //}


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
})();