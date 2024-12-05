(function () {
    'use strict'
    var currentChildRowData;
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId,
        TblAvailablePOforPIId, $TblAvailablePOforPIEl;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var _ypiReceiveChildID = 9999;
    var status,
        isEditable = false,
        isCDAAcknowledgePage = false,
        isCDAReceivePage = false,
        isYarnReceivePage = false,
        isAllTabActive = false,
        isPendingTabActive = true,
        isPIReceiveTabActive = false,
        isPIRejectTabActive = false,
        isPIRevisionTabActive = false,
        isPendingAkg = false,
        isAcknowledge = false,
        isYarnPIReceiveAckPage = false,
        isCDAPIReceiveAckPage = false,
        isYarnPIReceivePage = false,
        masterData;

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
        TblAvailablePOforPIId = "#tblAvailablePOforPI" + pageId;
        // Page Load Event 
        isYarnReceivePage = convertToBoolean($(`#${pageId}`).find("#YarnPIReceivePage").val());
        isCDAReceivePage = convertToBoolean($(`#${pageId}`).find("#CDAPIReceivePage").val());
        isYarnPIReceiveAckPage = convertToBoolean($(`#${pageId}`).find("#YarnPIReceiveAckPage").val());
        isYarnPIReceivePage = convertToBoolean($(`#${pageId}`).find("#YarnPIReceivePage").val());
        isCDAPIReceiveAckPage = convertToBoolean($(`#${pageId}`).find("#CDAPIReceiveAckPage").val());

        var today = new Date();
        var datetoday = (today.getMonth() + 1) + '/' + today.getDate() + '/' + today.getFullYear();
        $formEl.find("#PIDate").val(datetoday);

        if (isCDAAcknowledgePage) {
            $toolbarEl.find("#btnPendingAkgList,#btnAcknowledgeList").show();
            $toolbarEl.find("#btnPendingList,#btnReceivedList,#btnRejectList").hide();

            $formEl.find("#btnAcknowledge").show();
            $formEl.find("#btnSave").hide();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAkgList"), $toolbarEl);

            status = statusConstants.AWAITING_PROPOSE;

            isAllTabActive = false;
            isPendingTabActive = false;
            isPIReceiveTabActive = false;
            isPIRejectTabActive = false,
                isPIRevisionTabActive = false,
                isPendingAkg = true;
            isAcknowledge = false;
        }


        else if (isYarnPIReceiveAckPage || isCDAPIReceiveAckPage) {
            //$toolbarEl.find("#btnPendingList,#btnReceivedList,#btnRevisionList").hide();
            $toolbarEl.find("#btnPendingList,#btnReceivedList").hide();
            //isAcknowledge = true;
            $formEl.find("#btnAcknowledge").show();
            $formEl.find("#btnUnAcknowlege").show();
            $formEl.find("#btvRevisionList").show();
            $formEl.find("#btnSave").hide();
            $formEl.find("#btnAddMorePO").hide();
            $formEl.find("#btnAdditionalPIValues").hide();
            $formEl.find("#btnDeductionPIValues").hide();
            $formEl.find("#TransShipmentAllow").fadeIn();
          
            status = statusConstants.AWAITING_PROPOSE;
            isAllTabActive = false;
            isPendingTabActive = false;
            isPIReceiveTabActive = false;
            isPIRejectTabActive = false;
            isPIRevisionTabActive = false;
            isPendingAkg = true;
            isAcknowledge = false;
            initMasterTable();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#fgRejectReason").hide();
        }
        else {
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);
            //$toolbarEl.find("#btnPendingAkgList,#btnAcknowledgeList").hide();
            $toolbarEl.find("#btnPendingAkgList,#btnRevisionList").hide();
            $toolbarEl.find("#btnPendingList,#btnReceivedList,#btnRejectList").show();

            $formEl.find("#btnAcknowledge").hide();
            $formEl.find("#btnSave").show();
            status = statusConstants.PENDING;

            isAllTabActive = false;
            isPendingTabActive = true;
            isPIRejectTabActive = false;
            isPIRevisionTabActive = false;
            isPIReceiveTabActive = false;
            isPendingAkg = false;
            isAcknowledge = false;
        }

        initMasterTable();

        //List Event
        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.PENDING;
            isAllTabActive = false;
            isPendingTabActive = true;
            isPIReceiveTabActive = false;
            isPIRejectTabActive = false;
            isPIRevisionTabActive = false;
            isPendingAkg = false;
            isAcknowledge = false;
            initMasterTable();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#fgRejectReason").hide();
            $formEl.find("#btnAddChild").show();
        });

        $toolbarEl.find("#btnReceivedList").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.COMPLETED;
            isAllTabActive = false;
            isPendingTabActive = false;
            isPIReceiveTabActive = true;
            isPIRejectTabActive = false;
            isPIRevisionTabActive = false;
            isPendingAkg = false;
            isAcknowledge = false;
            initMasterTable();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#fgRejectReason,#btnAddChild").hide();
        });

        $toolbarEl.find("#btnRevisionList").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.REVISE;
            isAllTabActive = false;
            isPendingTabActive = false;
            isPIReceiveTabActive = false;
            isPIRejectTabActive = false;
            isPIRevisionTabActive = true;
            isPendingAkg = false;
            isAcknowledge = false;
            initMasterTable();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#fgRejectReason").hide();
        });

        $toolbarEl.find("#btnRejectList").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.REJECT;
            isAllTabActive = false;
            isPendingTabActive = false;
            isPIReceiveTabActive = false;
            isPIRejectTabActive = true;
            isPIRevisionTabActive = false;
            isPendingAkg = false;
            isAcknowledge = false;
            initMasterTable();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#fgRejectReason,#btnAddChild").show();
        });

        $toolbarEl.find("#btnPendingAkgList").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.AWAITING_PROPOSE;
            isAllTabActive = false;
            isPendingTabActive = false;
            isPIReceiveTabActive = false;
            isPendingAkg = true;
            isAcknowledge = false;
            initMasterTable();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#fgRejectReason").hide();
        });

        $toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.ACKNOWLEDGE;
            isAllTabActive = false;
            isPendingTabActive = false;
            isPIReceiveTabActive = false;
            isPendingAkg = false;
            isAcknowledge = true;
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#fgRejectReason").hide();
            $formEl.find("#btnAddChild").show();
            $formEl.find("#btnUnAcknowlege").hide();            

            initMasterTable();
        });

        //$toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
        //    e.preventDefault();
        //    resetTableParams();
        //    status = statusConstants.ACKNOWLEDGE;
        //    isAllTabActive = false;
        //    isPendingTabActive = false;
        //    isPIReceiveTabActive = false;
        //    isPendingAkg = false;
        //    isAcknowledge = true;
        //    toggleActiveToolbarBtn(this, $toolbarEl);
        //    $formEl.find("#fgRejectReason").hide();

        //    initMasterTable();
        //});

        $toolbarEl.find("#btnRejectReviewList").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = "RejectReview";
            isAllTabActive = false;
            isPendingTabActive = false;
            isPIReceiveTabActive = false;
            isPendingAkg = false;
            isAcknowledge = false;
            initMasterTable();
            toggleActiveToolbarBtn(this, $toolbarEl);
            $formEl.find("#fgRejectReason").show();
        });

        if (isYarnPIReceiveAckPage) {
            $toolbarEl.find("#btnPendingAkgList").click();
        }

        //Button Event
        $formEl.find("#btnAddMorePO").on("click", showAvailableYarnPOForPI);

        $("#btnAddAvailablePOForPI").on("click", function () {
            var iYarnPIReceivePOList = $TblAvailablePOforPIEl.getSelectedRecords();
            var poNo = iYarnPIReceivePOList.map(function (el) { return el.PONo });
            $formEl.find("#PONo").val(poNo);
            //masterData.YarnPIReceivePOList = data;
            //var poNo1 = masterData.YarnPIReceivePOList.map(function (el) { return el.PONo });
            //$formEl.find("#PONo").val(poNo);

            $("#modal-available-po-for-pi").modal("hide");
            getYarnPOItems(iYarnPIReceivePOList);
        });

        $formEl.find("#btnAdditionalPIValues").on("click", function (e) {
            e.preventDefault();

            if (masterData.YarnPIReceiveAdditionalValueList.length >= masterData.AdditionalValueSetupList.length)
                return toastr.info("You have already added all Additional Values Available.");

            var newYarnAdditionalPIValueChildData = {
                AdditionalValueID: 0,
                AdditionalValue: 0
            };
            masterData.YarnPIReceiveAdditionalValueList.push(newYarnAdditionalPIValueChildData);
            $formEl.find("#tblYarnPIReceiveAdditionalValueList").bootstrapTable('load', masterData.YarnPIReceiveAdditionalValueList);
        });

        $formEl.find("#btnDeductionPIValues").on("click", function (e) {
            e.preventDefault();
            if (masterData.YarnPIReceiveDeductionValueList.length >= masterData.DeductionValueSetupList.length)
                return toastr.info("You have already added all Deduction Values Available.");

            var newYarnDeductionPIValueChildData = {
                DeductionValueID: 0,
                DeductionValue: 0
            };
            masterData.YarnPIReceiveDeductionValueList.push(newYarnDeductionPIValueChildData);
            $formEl.find("#tblYarnPIReceiveDeductionValueList").bootstrapTable('load', masterData.YarnPIReceiveDeductionValueList);
        });

        $formEl.find("#btCancel").click(function (e) {
            backToList();
        });
        $formEl.find("#btnAddChild").click(function (e) {
            AvailablePI();
        });


        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            Save(e, false);
        });
        $formEl.find("#btnReviseNew").click(function (e) {
            e.preventDefault();
            Revision(e, true);
        });
        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            Save(e, true);
        });
        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            var ackno = true;
            SaveAcknowledge(ackno);
        });

        $formEl.find("#btnUnAcknowlege").click(function (e) {
            e.preventDefault();
            var ackno = false;
            SaveAcknowledge(ackno);
        });

        //Select Event
        $formEl.find("#PaymentTermsID").on("select2:select", function (e) {
            if (e.params.data.id == "1") {
                showHideLCSection(false);
            }
            else {
                showHideLCSection(true);
            }
        });

        $formEl.find("#TypeOfLCID").on("select2:select", function (e) {
            if (e.params.data.id == "1") {
                $formEl.find("#formGroupCreditDays").fadeOut();
            }
            else {
                $formEl.find("#formGroupCreditDays").fadeIn();
            }
        });
    });

    function initMasterTable() {
        var columns = [
            {
                headerText: '', textAlign: 'Center', width: 75, minWidth: 75, maxWidth: 75, visible: status == statusConstants.PENDING, commands: [
                    { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }]
            },
            {
                headerText: '', textAlign: 'Center', width: 75, minWidth: 75, maxWidth: 75, visible: status == statusConstants.REVISE, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } }]
            },
            {
                headerText: '', textAlign: 'Center', width: 75, minWidth: 75, maxWidth: 75, visible: status == statusConstants.COMPLETED || status == statusConstants.REJECT,
                commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'ViewAttachedFile', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file' } }
                ]
            },
            {
                headerText: '', textAlign: 'Center', width: 75, minWidth: 75, maxWidth: 75, visible: status == statusConstants.AWAITING_PROPOSE, commands: [
                    { type: 'PenAck', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'ViewAttachedFile', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file' } }]
            },
            {
                headerText: '', textAlign: 'Center', width: 75, minWidth: 75, maxWidth: 75, visible: status == statusConstants.ACKNOWLEDGE, commands: [
                    { type: 'AckView', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'ViewAttachedFile', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file' } }]
            },
            {
                headerText: '', textAlign: 'Center', width: 75, minWidth: 75, maxWidth: 75, visible: status == "RejectReview", commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            {
                field: 'Status', headerText: 'Status', visible: isPIReceiveTabActive
            },
            {
                field: 'RevisionNo', headerText: 'Revision No', visible: isPIReceiveTabActive
            },
            {
                field: 'CompanyName', headerText: 'Company', width: 80,
                visible: isPendingTabActive || isAllTabActive || isPIReceiveTabActive || isAcknowledge || isPendingAkg || isPIRejectTabActive || isPIRevisionTabActive
            },
            {
                field: 'Status', headerText: 'Status', visible: isPendingTabActive || isAllTabActive, width: 80
            },
            {
                field: 'PONo', headerText: 'PO No',
                visible: isPendingTabActive || isAllTabActive || isPIReceiveTabActive || isAcknowledge || isPendingAkg
            },
            {
                field: 'PODate', headerText: 'PO Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1,
                visible: isPendingTabActive || isAllTabActive
            },
            {
                field: 'POAddedByName', headerText: 'PO Created By',
                visible: isPendingTabActive || isAllTabActive || isPIReceiveTabActive || isAcknowledge || isPendingAkg
            },
            {
                field: 'YPINo', headerText: 'PI No', visible: isPIReceiveTabActive || isPendingAkg || isAcknowledge || isPendingTabActive || isPIRejectTabActive || isPIRevisionTabActive
            },
            {
                field: 'PIDate', headerText: 'PI Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1,
                visible: isPIReceiveTabActive || isPendingAkg || isAcknowledge
            },
            {
                field: 'SupplierName', headerText: 'Supplier'
            },
            {
                field: 'QuotationRefNo', headerText: 'Ref No', visible: isPendingTabActive || isAllTabActive
            },
            {
                field: 'DeliveryStartDate', headerText: 'Delivery Start', textAlign: 'Center', type: 'date', format: _ch_date_format_1,
                visible: isPendingTabActive || isAllTabActive
            },
            {
                field: 'DeliveryEndDate', headerText: 'Delivery End', textAlign: 'Center', type: 'date', format: _ch_date_format_1,
                visible: isPendingTabActive || isAllTabActive
            },
            {
                field: 'POQty', headerText: 'PO Qty'
            },

            {
                field: 'PIQty', headerText: 'PI Qty'
            },
            {
                field: 'PIValue', headerText: 'PI Value'
            },
            {
                field: 'POIds', headerText: 'POIds', visible: false
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();

        var url = "";
        if (isYarnReceivePage) {
            url = "/api/ypi-receive/list?status=" + status;// + "&" + queryParams;
        }
        else {
            url = "/api/ypi-receive/cda-list?status=" + status;//+ "&" + queryParams;
        }

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: url,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        debugger;
        $formEl.find("#btnSave,#btnAcknowledge").hide();
        if (args.commandColumn.type == 'Add') {
            if (status == statusConstants.PENDING) {
                if (args.rowData.Status == "Revision") {
                    if (args.rowData.Approved == 0) {
                        toastr.error("Waiting For PO Approve");
                        return false;
                    }
                }
                if (args.rowData.Status == 'Revision') {
                    resetForm();
                    //getRevise(args.rowData.YPOMasterID, args.rowData.SupplierID, args.rowData.CompanyID, args.rowData.Status);
                    getRevise(args.rowData.YPIReceiveMasterID, args.rowData.SupplierID, args.rowData.CompanyID, args.rowData.POIds);
                    initNewAttachment($formEl.find("#UploadFile"));
                    goToDetails();

                    $formEl.find("#btnRevise").hide();
                    $formEl.find("#btnSave").show();
                    $formEl.find("#btnReviseNew").hide();

                    $formEl.find("#btnAcknowledge,#btnUnAcknowlege").hide();
                }
                else {

                    resetForm();
                    getNew(args.rowData.YPOMasterID, args.rowData.SupplierID, args.rowData.CompanyID, args.rowData.Status);
                    initNewAttachment($formEl.find("#UploadFile"));
                    goToDetails();

                    $formEl.find("#btnRevise").hide();
                    $formEl.find("#btnSave").show();
                    $formEl.find("#btnAcknowledge,#btnUnAcknowlege").hide();
                    $formEl.find("#btnReviseNew").hide();
                }

            }
            if (status == statusConstants.REVISE) {
                resetForm();
                //getRevise(args.rowData.YPOMasterID, args.rowData.SupplierID, args.rowData.CompanyID, args.rowData.Status);
                getRevise(args.rowData.YPIReceiveMasterID, args.rowData.SupplierID, args.rowData.CompanyID, args.rowData.POIds);
                initNewAttachment($formEl.find("#UploadFile"));
                goToDetails();

                $formEl.find("#btnRevise").hide();
                $formEl.find("#btnSave").hide();
                $formEl.find("#btnReviseNew").show();

                $formEl.find("#btnAcknowledge,#btnUnAcknowlege").hide();
            }

        }
        else if (args.commandColumn.type == 'Edit') {
            resetForm();
            //$formEl.find("#divTermsCondition :input").attr("disabled", true);
            //$formEl.find("#TotalDeductionValue").val(args.rowData.TotalDeductionValue);
            $formEl.find("#YPIReceiveMasterID").val(args.rowData.YPIReceiveMasterID);
            getDetails(args.rowData.YPIReceiveMasterID, args.rowData.SupplierID, args.rowData.CompanyID);
            goToDetails();

            $formEl.find("#btnSave,#btnRevise").hide();
            $formEl.find("#btnAcknowledge,#btnUnAcknowlege").hide();
            $formEl.find("#btnReviseNew").hide();
            if (status == statusConstants.REJECT && menuId == 532) { //Yarn PI Receive = 532
                $formEl.find("#btnRevise").show();
                $formEl.find("#btnReviseNew").hide();
            } else if (menuId != 956) { //Yarn PI Acknowledge = 956
                $formEl.find("#btnSave").show();
                $formEl.find("#btnReviseNew").hide();
            }
            if (status == statusConstants.COMPLETED) {
                $formEl.find("#btnSave").hide();
            }
        }
        else if (args.commandColumn.type == 'PenAck') {

            resetForm();
            //$formEl.find("#divTermsCondition :input").attr("disabled", true);
            $formEl.find("#YPIReceiveMasterID").val(args.rowData.YPIReceiveMasterID);
            getDetails(args.rowData.YPIReceiveMasterID, args.rowData.SupplierID, args.rowData.CompanyID);

            goToDetails();

            $formEl.find("#btnRevise").hide();
            $formEl.find("#btnSave").hide();
            $formEl.find("#btnAcknowledge").show();
            $formEl.find("#btnReviseNew").hide();
        }
        else if (args.commandColumn.type == 'AckView') {

            if (isYarnPIReceiveAckPage) {
                ElementDisable(true);
            }
            resetForm();
            //$formEl.find("#divTermsCondition :input").attr("disabled", true);
            $formEl.find("#YPIReceiveMasterID").val(args.rowData.YPIReceiveMasterID);
            getDetails(args.rowData.YPIReceiveMasterID, args.rowData.SupplierID, args.rowData.CompanyID);
            goToDetails();
            if (isYarnPIReceiveAckPage) {
                $formEl.find("#btnUnAcknowlege").hide();
                $formEl.find("#btnRevise").hide();
            }
            else {

                $formEl.find("#btnUnAcknowlege").hide();
                $formEl.find("#btnRevise").show();
            }
            //$formEl.find("#btnRevise").hide();
            $formEl.find("#btnSave").hide();
            $formEl.find("#btnAcknowledge").hide();
            $formEl.find("#btnReviseNew").hide();
        }
        else if (args.commandColumn.type == 'ViewAttachedFile') {
            if (args.rowData.PIFilePath) {
                window.open(args.rowData.PIFilePath, '_blank').focus();
            }
        }
        //$formEl.find("#btnSave").fadeIn();
    }
    //getRevise
    //id, supplierId, companyId
    //function getRevise(yPOMasterId, supplierId, customerId, status) {
    function getRevise(id, supplierId, companyId, poIds) {
        
        //axios.get(`/api/ypi-receive/${id}/${supplierId}/${companyId}/${isYarnReceivePage}`)
        axios.get(`/api/ypi-receive/revise/${id}/${supplierId}/${companyId}/${isYarnReceivePage}/${poIds}`)
            .then(function (response) {
                masterData = response.data;
                masterData.PIDate = formatDateToDefault(masterData.PIDate);
                setFormData($formEl, masterData);

                var receivedChilds = masterData.Childs.filter(function (row) {
                    return row.YPIReceiveChildID !== 0;
                });

                // added 6/26/2023
                masterData.Childs = masterData.Childs.filter(child => !(child.YPIReceiveChildID === 0 && child.POQty === child.POReceivedQty));
                // added 6/26/2023

                //CE
                //masterData.Childs = masterData.Childs.filter(function (el) { return el.POQty > el.POReceivedQty });
                //CE

                //initChildTable(masterData.Childs);
                initChildTable(receivedChilds);
                initYarnAdditionalPIValuesTable();
                initYarnDeductionPIValuesTable();
                $formEl.find("#tblYarnPIReceiveAdditionalValueList").bootstrapTable("load", masterData.YarnPIReceiveAdditionalValueList);
                $formEl.find("#tblYarnPIReceiveDeductionValueList").bootstrapTable('load', masterData.YarnPIReceiveDeductionValueList);

                calculateTotalYarnPIValue(receivedChilds);

                if (masterData.PaymentTermsID === 2)
                    showHideLCSection(true);
                else
                    showHideLCSection(false);

                if (masterData.PortofLoadingID === 105)
                    showHideSupplierRegionSection(false);
                else
                    showHideSupplierRegionSection(true);

                if (masterData.TypeOfLCID === 2)
                    $formEl.find("#formGroupCreditDays").fadeIn();
                else
                    $formEl.find("#formGroupCreditDays").fadeOut();

                initAttachment(masterData.PIFilePath, masterData.AttachmentPreviewTemplate, $formEl.find("#UploadFile"));

                if (status == statusConstants.REVISE) {
                    //ElementDisable(false);
                } else {
                    //ElementDisable(true);
                }
            })
            .catch(showResponseError);
    }
    function getNew(yPOMasterId, supplierId, customerId, status) {
        var url = "";

        if (isYarnReceivePage)
            url = `/api/ypi-receive/new/${yPOMasterId}/${supplierId}/${customerId}`;
        else
            url = `/api/ypi-receive/new-cda/${yPOMasterId}/${supplierId}/${customerId}`;

        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.PIDate = formatDateToDefault(masterData.PIDate);
                masterData.YarnPIReceivePOList.map(function (el) {
                    if (el.BalancePOQty > 0) {
                        masterData.YPINo = null;
                        masterData.YPIReceiveMasterID = 0;
                    }
                });

                
                //CE-anis-14-Nov-22
                masterData.Childs = masterData.Childs.filter(function (el) { return el.POQty > el.POReceivedQty });
                //CE-end anis-14-Nov-22

                setFormData($formEl, masterData);

                //masterData.YarnPIReceivePOList.push({ YPOMasterID: masterData.YPOMasterID, PONo: masterData.PONo });
                //console.log(masterData.YarnPIReceivePOList);
                //$("#divNewYarnPIReceive").fadeIn();
                //$("#divtblYarnIReceive").fadeOut();
                //$("#divButtonExecutionsYarnPIReceive").fadeIn();

                initYarnAdditionalPIValuesTable();
                initYarnDeductionPIValuesTable();
                initChildTable(masterData.Childs);

                calculateTotalYarnPIValue(masterData.Childs);

                if (masterData.PaymentTermsID === 2)
                    showHideLCSection(true);
                else
                    showHideLCSection(false);

                if (masterData.PortofLoadingID === 105)
                    showHideSupplierRegionSection(false);
                else
                    showHideSupplierRegionSection(true);

                if (masterData.TypeOfLCID === 2)
                    $formEl.find("#formGroupCreditDays").fadeIn();
                else
                    $formEl.find("#formGroupCreditDays").fadeOut();

            })
            .catch(showResponseError);
    }

    function getDetails(id, supplierId, companyId) {
        axios.get(`/api/ypi-receive/${id}/${supplierId}/${companyId}/${isYarnReceivePage}`)
            .then(function (response) {

                masterData = response.data;
                masterData.PIDate = formatDateToDefault(masterData.PIDate);
                setFormData($formEl, masterData);

                //$("#divNewYarnPIReceive").fadeIn();
                //$("#divtblYarnIReceive").fadeOut();
                //$("#divButtonExecutionsYarnPIReceive").fadeIn();

                /*$tblChildEl.bootstrapTable('load', masterData.Childs);*/
                initChildTable(masterData.Childs);
                initYarnAdditionalPIValuesTable();
                initYarnDeductionPIValuesTable();
                $formEl.find("#tblYarnPIReceiveAdditionalValueList").bootstrapTable("load", masterData.YarnPIReceiveAdditionalValueList);
                $formEl.find("#tblYarnPIReceiveDeductionValueList").bootstrapTable('load', masterData.YarnPIReceiveDeductionValueList);


                calculateTotalYarnPIValue(masterData.Childs);

                if (masterData.PaymentTermsID === 2)
                    showHideLCSection(true);
                else
                    showHideLCSection(false);

                if (masterData.PortofLoadingID === 105)
                    showHideSupplierRegionSection(false);
                else
                    showHideSupplierRegionSection(true);

                if (masterData.TypeOfLCID === 2)
                    $formEl.find("#formGroupCreditDays").fadeIn();
                else
                    $formEl.find("#formGroupCreditDays").fadeOut();

                initAttachment(masterData.PIFilePath, masterData.AttachmentPreviewTemplate, $formEl.find("#UploadFile"));

                if (status == statusConstants.PENDING || status == statusConstants.COMPLETED) {
                    //ElementDisable(false);
                } else {
                    //ElementDisable(true);
                }
            })
            .catch(showResponseError);
    }
    function calculateTotalPIValueAndNetPIValue(childs) {

        var totalPIValue = 0;
        childs.map(c => {
            var piValue = isNaN(c.PIValue) ? 0 : c.PIValue;
            totalPIValue += parseFloat(piValue);
        });
        $formEl.find("#TotalPIValue").val(totalPIValue);
        var totalAddValue = $formEl.find("#TotalAddValue").val();
        totalAddValue = isNaN(totalAddValue) ? 0 : parseFloat(totalAddValue);
        var totalDeductionValue = $formEl.find("#TotalDeductionValue").val();
        totalDeductionValue = isNaN(totalDeductionValue) ? 0 : parseFloat(totalDeductionValue);
        var netPIValue = totalPIValue + totalAddValue - totalDeductionValue;
        $formEl.find("#NetPIValue").val(netPIValue);
    }
    function initChildTable(data) {
        
        calculateTotalPIValueAndNetPIValue(data);
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        if (isYarnReceivePage) {
            columns = [
                {
                    headerText: 'Command', width: 100, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                    ]
                },
                { field: 'YPIReceiveChildID', isPrimaryKey: true, visible: false, width: 80 },
                { field: 'Segment1ValueDesc', headerText: 'Composition', allowEditing: false, width: 80 },
                { field: 'Segment2ValueDesc', headerText: 'Yarn Type', allowEditing: false, width: 80 },
                { field: 'Segment3ValueDesc', headerText: 'Process', allowEditing: false, width: 80 },
                { field: 'Segment4ValueDesc', headerText: 'Sub Process', allowEditing: false, width: 80 },
                { field: 'Segment5ValueDesc', headerText: 'Quality Parameter', allowEditing: false, width: 80 },
                {
                    field: 'Segment6ValueDesc', headerText: 'Count', allowEditing: false, width: 80
                },
                { field: 'Segment7ValueDesc', headerText: 'No of Ply', allowEditing: false, width: 80 },
                { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false, width: 80 },
                {
                    field: 'POQty', headerText: 'PO Qty (kg)', editType: "numericedit", width: 80, allowEditing: false,
                    edit: { params: { showSpinButton: false, decimals: 0, format: "N2", validateDecimalOnType: true } }
                },
                {
                    field: 'POReceivedQty', headerText: 'PO Rec Qty(kg)', allowEditing: false, editType: "numericedit", width: 80,
                    edit: { params: { showSpinButton: false, decimals: 0, format: "N2", validateDecimalOnType: true } }
                },
                {
                    field: 'Rate', headerText: 'Rate', allowEditing: false, editType: "numericedit", width: 80,
                    params: { showSpinButton: false, decimals: 0, format: "N2", validateDecimalOnType: true }
                },
                {
                    field: 'POValue', headerText: 'PO Value', allowEditing: false, editType: "numericedit", width: 80,
                    params: { showSpinButton: false, decimals: 0, format: "N2", validateDecimalOnType: true }
                },
                {
                    field: 'PIQty', headerText: 'PI Qty', editType: "numericedit", width: 80,
                    edit: { params: { showSpinButton: false, decimals: 2, format: "N2", validateDecimalOnType: true } }
                },
                {
                    field: 'PIValue', headerText: 'PI Value', allowEditing: false, editType: "numericedit", width: 80,
                    params: { showSpinButton: false, decimals: 0, format: "N2", validateDecimalOnType: true }
                }
            ];
        }
        else {
            columns = [
                { field: 'CDAPRChildID', isPrimaryKey: true, visible: false },
                { field: 'Segment1ValueDesc', headerText: 'Agent Name', allowEditing: false },
                { field: 'Segment2ValueDesc', headerText: 'Item Name', allowEditing: false },
                { field: 'Segment3ValueDesc', headerText: 'Yarn Type', allowEditing: false },
                { field: 'Segment4ValueDesc', headerText: 'Yarn Program', allowEditing: false },
                {
                    field: 'POQty', headerText: 'PO Qty (kg)', allowEditing: false, editType: "numericedit",
                    //edit: { params: { decimals: 0, format: "N2", validateDecimalOnType: true } }
                    params: { decimals: 2, format: "N2", validateDecimalOnType: true }
                },
                {
                    field: 'Rate', headerText: 'Rate', allowEditing: false, editType: "numericedit",
                    params: { decimals: 0, format: "N2", validateDecimalOnType: true }
                },
                {
                    field: 'POValue', headerText: 'PO Value', allowEditing: false, editType: "numericedit",
                    edit: { params: { decimals: 0, format: "N2", validateDecimalOnType: true } }
                },
                {
                    field: 'PIQty', headerText: 'PI Qty', editType: "numericedit",
                    edit: { params: { decimals: 0, format: "N2", validateDecimalOnType: true } }
                },
                {
                    field: 'PIValue', headerText: 'PI Value', allowEditing: false, editType: "numericedit",
                    params: { decimals: 0, format: "N2", validateDecimalOnType: true }
                }
            ];
        }


        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                var currentList = $tblChildEl.dataSource;
                if (args.requestType == 'refresh') {

                }
                if (args.requestType === 'beginEdit') {

                }
                if (args.requestType === "add") {
                    //args.data.CDAPRChildID = getMaxIdForArray(masterData.Childs, "CDAPRChildID");
                }
                else if (args.requestType === "save") {

                    args.data.POQty = args.previousData.POQty;

                    var totalQty = parseInt(args.data.PIQty) + parseInt(args.data.POReceivedQty);
                    var remainQty = parseInt(args.data.POQty) - parseInt(args.data.POReceivedQty);

                    //if (totalQty> args.data.POQty) {
                    if (parseFloat(args.data.PIQty) > parseFloat(args.previousData.POQty)) {
                        toastr.error("PIQty cannot greater than POQty");
                        //args.data.PIQty = remainQty;
                        args.data.PIQty = args.previousData.PIQty;
                        return false;
                    }
                    args.data.PIValue = (parseFloat(args.data.PIQty) * parseFloat(args.data.Rate)).toFixed(2);

                    if (isYarnReceivePage) {
                        //YPIReceiveChildID
                        var indexF = masterData.Childs.findIndex(x => x.YPIReceiveChildID == args.data.YPIReceiveChildID);
                        if (indexF > -1) {
                            masterData.Childs[indexF].POQty = args.data.POQty;
                            masterData.Childs[indexF].PIQty = args.data.PIQty;
                            masterData.Childs[indexF].PIValue = args.data.PIValue;
                        }
                    }
                    else {
                        //CDAPRChildID
                        var indexF = masterData.Childs.findIndex(x => x.CDAPRChildID == args.data.CDAPRChildID);
                        if (indexF > -1) {
                            masterData.Childs[indexF].POQty = args.data.POQty;
                            masterData.Childs[indexF].PIQty = args.data.PIQty;
                            masterData.Childs[indexF].PIValue = args.data.PIValue;
                        }
                    }
                    calculateTotalYarnPIValue(masterData.Childs);
                    
                    
                }
                else if (args.requestType === "edit") {
                    //   
                }
                else if (args.requestType === "delete") {

                    if (args.data[0].YPIReceiveMasterID !== 0) {
                        toastr.error("Already received ! You can not delete");
                        initChildTable(currentList);
                        $tblChildEl.refresh();
                        return false;
                    }
                    else {
                        var childList = DeepClone(masterData.Childs);
                        var indexF = childList.findIndex(x => x.YPIReceiveChildID == args.data[0].YPIReceiveChildID);
                        if (indexF > -1) {
                            childList.splice(indexF, 1);
                            initChildTable(childList);
                        }
                    }
                }
            },

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
                            field: 'POQty',
                            decimals: 2,
                            format: "N2",
                            footerTemplate: '${Sum}'
                        },
                        {
                            type: 'Sum',
                            field: 'POReceivedQty',
                            decimals: 2,
                            format: "N2",
                            footerTemplate: '${Sum}'
                        },

                        {
                            type: 'Sum',
                            field: 'POValue',
                            decimals: 2,
                            format: "N2",
                            footerTemplate: '${Sum}'
                        },
                        {
                            type: 'Sum',
                            field: 'PIQty',
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

            commandClick: childCommandClick,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging:false
        };
        tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
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
    function showAvailableYarnPOForPI() {
        var supplierId = $formEl.find("#SupplierID").val();
        var companyId = $formEl.find("#CompanyID").val();
        var yPIReceiveMasterID = $formEl.find("#YPIReceiveMasterID").val();

        var poMasterIds = masterData.YarnPIReceivePOList.map(
            function (el) {
                return el.YPOMasterID
            });

        var url = "";

        if (isYarnReceivePage) {
            url = `/api/ypi-receive/available-po-for-pi?poMasterIds=${poMasterIds.toString()}&supplierId=${supplierId}&companyId=${companyId}&yPIReceiveMasterID=${yPIReceiveMasterID}`;
        }

        else {
            url = `/api/ypi-receive/available-cda-po-for-pi?poMasterIds=${poMasterIds.toString()}&supplierId=${supplierId}&companyId=${companyId}&yPIReceiveMasterID=${yPIReceiveMasterID}`;
        }

        axios.get(url)
            .then(function (response) {
                initAvailalePOTable(response.data);
                $("#modal-available-po-for-pi").modal("show");
            })
            .catch(showResponseError);
    };

    function initAvailalePOTable(records) {
        if ($TblAvailablePOforPIEl) $TblAvailablePOforPIEl.destroy();
        ej.base.enableRipple(true);
        //Anis
        $TblAvailablePOforPIEl = new ej.grids.Grid({
            editSettings: { showDeleteConfirmDialog: true, allowEditing: true, allowDeleting: true },
            allowResizing: true,
            dataSource: records,
            toolbar: ['Search'],
            searchSettings: { fields: ['PONo', 'CompanyName', 'PODate', 'TotalQty', 'TotalValue', 'QuotationRefNo'], operator: 'contains', ignoreCase: false },
            columns: [
                {
                    field: "IsChecked", type: 'checkbox', width: 60, headerText: 'Select'
                },
                {
                    field: 'PONo', headerText: 'PO No', width: 120, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } }
                },
                {
                    field: 'CompanyName', headerText: 'Company', width: 80, allowEditing: false
                },
                {
                    field: 'PODate', headerText: 'PO Date', width: 80, type: 'date', format: _ch_date_format_1, allowEditing: false
                },
                {
                    field: 'TotalQty', headerText: 'Total Qty (Kg)', width: 80, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } }
                },
                {
                    field: 'TotalValue', headerText: 'Total Value ($)', width: 80, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } }
                },
                //{
                //    field: 'BalancePOQty', headerText: 'Bal. PO Qty(Kg)', width: 80, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } }
                //},
                //{
                //    field: 'BalancePOValue', headerText: 'Bal. PO Value($)', width: 80, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0 } }
                //},
                {
                    field: 'QuotationRefNo', headerText: 'Ref. No', width: 80, allowEditing: false
                }
            ]
        });

        $TblAvailablePOforPIEl.appendTo(TblAvailablePOforPIId);
    }

    function getYarnPOItems(iYarnPIReceivePOList) {
        var poMasterIdList = iYarnPIReceivePOList.map(function (el) {
            return el.YPOMasterID
        });
        var yPIReceiveMasterID = $formEl.find("#YPIReceiveMasterID").val();
        var url = "";

        if (isYarnReceivePage) {
            url = "/api/ypi-receive/yarn-po-items?ypoMasterIds=" + poMasterIdList.toString() + "&yPIReceiveMasterID=" + yPIReceiveMasterID;
        }
        else {
            url = "/api/ypi-receive/cda-po-items?ypoMasterIds=" + poMasterIdList.toString() + "&yPIReceiveMasterID=" + yPIReceiveMasterID;
        }

        axios.get(url)
            .then(function (response) {
                masterData = response.data;
                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function initAttachment(path, type, $el) {
        if (!path) {
            initNewAttachment($el);
            return;
        }

        if (!type) type = "any";

        var preveiwData = [rootPath + path];
        //var previewConfig = [{ type: type, caption: "PI Attachment", key: 1, width: "80px", frameClass: "preview-frame" }];
        var previewConfig = [{ type: type, caption: "PI Attachment", key: 1, width: "80px", frameClass: "preview-frame" }];

        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            initialPreview: preveiwData,
            initialPreviewAsData: true,
            initialPreviewFileType: 'image',
            initialPreviewConfig: previewConfig,
            purifyHtml: true,
            required: true,
            maxFileSize: 4096
        });
    }

    function initNewAttachment($el) {
        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            previewFileType: 'any'
        });
    }

    function initYarnAdditionalPIValuesTable() {
        $formEl.find("#tblYarnPIReceiveAdditionalValueList").bootstrapTable('destroy');
        $formEl.find("#tblYarnPIReceiveAdditionalValueList").bootstrapTable({
            scrolling: true,
            showFooter: true,
            columns: [
                {
                    field: "AdditionalValueID",
                    title: "Additional Value Type",
                    editable: {
                        source: masterData.AdditionalValueSetupList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Additional Names' }
                    }
                },
                {
                    field: "AdditionalValue",
                    title: "Additional Value ($)",
                    align: 'right',
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        validate: function (value) {
                            if (isNaN(value)) return 'Value must be greater than 0.';
                        }
                    },
                    footerFormatter: calculateTotalAdditionalPIValue
                }
            ]
        });
    };

    function initYarnDeductionPIValuesTable() {
        $formEl.find("#tblYarnPIReceiveDeductionValueList").bootstrapTable('destroy');
        $formEl.find("#tblYarnPIReceiveDeductionValueList").bootstrapTable({
            scrolling: true,
            showFooter: true,
            columns: [
                {
                    field: "DeductionValueID",
                    title: "Deduction Value Type",
                    editable: {
                        source: masterData.DeductionValueSetupList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Deduction Names' }
                    }
                },
                {
                    field: "DeductionValue",
                    title: "Deduction Value ($)",
                    align: 'right',
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        validate: function (value) {
                            if (isNaN(value)) return 'Value must be greater than 0.';
                        }
                    },
                    footerFormatter: calculateTotalDeductionPIValue
                }
            ]
        });
    };

    function goToDetails() {
        $divDetailsEl.fadeIn();
        $divTblEl.fadeOut();
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        //initMasterTable();
        $tblMasterEl.refresh();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#YPIReceiveMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function calculateTotalAdditionalPIValue(data) {
        var totalAdditionalValue = 0,
            totalPIValue = 0;
        totalPIValue = parseFloat($formEl.find("#TotalPIValue").val());
        totalAdditionalValue = parseFloat($formEl.find("#TotalAddValue").val());

        totalAdditionalValue = parseFloat(data.reduce((total, singleRecord) => parseFloat(total) + parseFloat(singleRecord.AdditionalValue), 0));

        //$.each(data, function (i, row) {
        //    totalAdditionalValue += isNaN(parseFloat(row.AdditionalValue)) ? 0 : parseFloat(row.AdditionalValue);
        //    totalPIValue += isNaN(parseFloat(row.PIValue)) ? 0 : parseFloat(row.PIValue);
        //});
        

        var totalDeductionValue = 0;
        totalDeductionValue = parseFloat($formEl.find("#TotalDeductionValue").val());

        $formEl.find("#NetPIValue").val(totalPIValue + totalAdditionalValue - totalDeductionValue);

        masterData.totalAdditionalValue = totalAdditionalValue;
        $formEl.find("#TotalAddValue").val(totalAdditionalValue);
        return totalAdditionalValue.toFixed(2);
    }

    function calculateTotalDeductionPIValue(data) {
        var totalDeductionValue = 0;

        totalDeductionValue = parseFloat(data.reduce((total, singleRecord) => parseFloat(total) + parseFloat(singleRecord.DeductionValue), 0));
        //$.each(data, function (i, row) {
        //    totalDeductionValue += isNaN(parseFloat(row.DeductionValue)) ? 0 : parseFloat(row.DeductionValue);
        //});
        $formEl.find("#TotalDeductionValue").val(totalDeductionValue);



        var totalPIValue = 0;
        var totalAdditionalValue = 0;
        totalPIValue = parseFloat($formEl.find("#TotalPIValue").val());
        totalAdditionalValue = parseFloat($formEl.find("#TotalAddValue").val());
        $formEl.find("#NetPIValue").val(totalPIValue + totalAdditionalValue - totalDeductionValue);

        var netPIValue = totalPIValue + totalAdditionalValue - totalDeductionValue;
        $formEl.find("#NetPIValue").val(netPIValue);
        masterData.totalDeductionValue = totalDeductionValue;
        return totalDeductionValue.toFixed(2);
    }

    function calculateTotalYarnPIValue(args) {
        var yarnPOValueN = 0;
        $.each(args, function (i, row) {
            yarnPOValueN += isNaN(parseFloat(row.PIValue)) ? 0 : parseFloat(row.PIValue);
        });
        $formEl.find("#TotalPIValue").val(yarnPOValueN);

        var addValue = $formEl.find("#TotalAddValue").val();
        var deductionValue = $formEl.find("#TotalDeductionValue").val();

        var netPIValue = parseFloat(yarnPOValueN) + parseFloat(addValue) - parseFloat(deductionValue);
        //var netPIValue = $formEl.find("#NetPIValue").val();

        if (netPIValue == 'NaN' || netPIValue == undefined || netPIValue == 0) {
            $formEl.find("#NetPIValue").val(yarnPOValueN);
        }
        else {
            $formEl.find("#NetPIValue").val(netPIValue);
        }

        return yarnPOValueN.toFixed(2);

        //var yarnPOValueN = 0;
        //yarnPOValueN += isNaN(parseFloat(args.data.PIValue)) ? 0 : parseFloat(args.data.PIValue); 
        //return yarnPOValueN.toFixed(2);
    }

    function ElementDisable(disable) {
        //$formEl.find("#YPINo").prop("disabled", disable);
        //$formEl.find("#PIDate").prop("disabled", disable);
        $formEl.find("#IncoTermsID").prop("disabled", disable);
        $formEl.find("#PaymentTermsID").prop("disabled", disable);
        $formEl.find("#TypeOfLCID").prop("disabled", disable);
        $formEl.find("#CreditDays").prop("disabled", disable);
        $formEl.find("#CalculationofTenure").prop("disabled", disable);
        $formEl.find("#Charges").prop("disabled", disable);
        $formEl.find("#CountryOfOriginID").prop("disabled", disable);
        $formEl.find("#ShipmentModeID").prop("disabled", disable);
        $formEl.find("#OfferValidity").prop("disabled", disable);
        $formEl.find("#PortofLoadingID").prop("disabled", disable);
        $formEl.find("#PortofDischargeID").prop("disabled", disable);
        $formEl.find("#ShippingTolerance").prop("disabled", disable);
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

    function isValidChildForm(data) {
        var isValidItemInfo = false;

        $.each(data.Childs, function (i, el) {
            if (el.PIQty == "" || el.PIQty == null || el.PIQty <= 0) {
                toastr.error("PI Qty is required.");
                isValidItemInfo = true;
            }
            else if (el.PIQty > el.POQty) {
                toastr.error("PI Qty must be equal or less than PO Qty");
                isValidItemInfo = true;
            }
        });

        return isValidItemInfo;
    }

    var validationConstraints = {
        YPINo: {
            presence: true
        },
        SupplierName: {
            presence: true
        },
        PIDate: {
            presence: true
        }
    };
    function Save(e, isRevise) {
        e.preventDefault();
       
        var result = false;
        var files = $formEl.find("#UploadFile")[0].files;
        var fileExists = masterData.PIFilePath;
        if (!fileExists) {
            if (!files || files.length == 0) return toastr.error("You must upload PI document.");
        }
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors.");
        else hideValidationErrors($formEl);

        $formEl.find("#IsCDA").val(isCDAReceivePage);


        //masterData.YarnPIReceivePOList.PIQty = $tblChildEl.getCurrentViewRecords().PIQty;
        var poList = masterData.YarnPIReceivePOList;
        masterData.YarnPIReceivePOList = $tblChildEl.getCurrentViewRecords();

        masterData.Childs = $tblChildEl.getCurrentViewRecords();

        masterData.YarnPIReceivePOList.forEach(function (item) {
            if (item.YPOMasterID == 0) {
                item.YPOMasterID = masterData.YPOMasterID;
            }
            //item.YPOMasterID = masterData.YPOMasterID;
            item.YPIReceiveMasterID = masterData.YPIReceiveMasterID;
        });


        if (masterData.YarnPIReceivePOList.some(item => item.PIQty === 0 || item.PIQty === null)) {
            toastr.error("PIQty cannot be zero.");
            return false;
        }
        else if (masterData.YarnPIReceivePOList.some(item => item.PIQty > item.POQty)) {
            toastr.error("PIQty cannot be greater than POQty.");
            return false;
        }

        if (masterData.YarnPIReceiveDeductionValueList.length > 0) {
            if (masterData.totalDeductionValue === 0 || masterData.totalDeductionValue === null || masterData.totalDeductionValue === undefined) {
                toastr.error("Deduction Value can not be zero");
                return false;
            }
        }

        if (masterData.YarnPIReceiveAdditionalValueList.length > 0) {
            if (masterData.totalAdditionalValue === 0 || masterData.totalAdditionalValue === null || masterData.totalAdditionalValue === undefined) {
                toastr.error("Additional Value can not be zero");
                return false;
            }
        }

        var formData = getFormData($formEl);
        formData.append("IsRevise", isRevise);
        formData.append("isFileExist", fileExists);
        formData.append("UploadFile", files[0]);
        formData.append("Childs", JSON.stringify(masterData.Childs));
        formData.append("YarnPIReceivePOList", JSON.stringify(masterData.YarnPIReceivePOList));
        formData.append("YarnPIReceiveAdditionalValueList", JSON.stringify(masterData.YarnPIReceiveAdditionalValueList));
        formData.append("YarnPIReceiveDeductionValueList", JSON.stringify(masterData.YarnPIReceiveDeductionValueList));
        formData.append("YarnPOMasterRevision", masterData.YarnPOMasterRevision);

        //Child Validation check 
        if (isValidChildForm(masterData.Childs)) return;

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }
        axios.post("/api/ypi-receive/save", formData, config)
            .then(function (response) {
                showBootboxAlert(`PI with PI No: ${response.data} received successfully.`);
                backToList();
            })
            .catch(showResponseError);
    }

    function Revision(e, isRevise) {

        e.preventDefault();
        var files = $formEl.find("#UploadFile")[0].files;
        //if (!files || files.length == 0) return toastr.error("You must upload PI document.");
        var fileExists = masterData.PIFilePath;
        if (!fileExists) {
            if (!files || files.length == 0) return toastr.error("You must upload PI document.");
        }
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors.");
        else hideValidationErrors($formEl);

        $formEl.find("#IsCDA").val(isCDAReceivePage);
        var formData = getFormData($formEl);

        //formData.append("RevisionNo", masterData.RevisionNo);
        formData.append("RevisionNo", masterData.RevisionNo);
        formData.append("IsRevise", isRevise);
        formData.append("UploadFile", files[0]);
        formData.append("Childs", JSON.stringify(masterData.Childs));
        formData.append("YarnPIReceivePOList", JSON.stringify(masterData.YarnPIReceivePOList));
        formData.append("YarnPIReceiveAdditionalValueList", JSON.stringify(masterData.YarnPIReceiveAdditionalValueList));
        formData.append("YarnPIReceiveDeductionValueList", JSON.stringify(masterData.YarnPIReceiveDeductionValueList));

        //Child Validation check 
        if (isValidChildForm(masterData.Childs)) return;

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }
        //
        //var formObj = formDataToJson(formData);

        axios.post("/api/ypi-receive/revision", formData, config)
            .then(function (response) {
                showBootboxAlert(`PI with PI No: ${response.data} revised successfully.`);
                backToList();
            })
            .catch(showResponseError);
    }

    function SaveAcknowledge(ackno) {

        var data = formDataToJson($formEl.serializeArray());
        data.Acknowledge = ackno;
        axios.post("/api/ypi-receive/acknowledge", data)
            .then(function () {
                toastr.success("Acknowledge Process Successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }

    function AvailablePI() {
        
        var receivedChilds = masterData.Childs.filter(function (row) {
            return row.YPIReceiveChildID !== 0;
        });
        var currentList = $tblChildEl.dataSource;
        var childIds = [];
        currentList.map(x => {
            childIds.push(x.YPOChildID);
        });
        childIds = childIds.join(",");

        var finder = new commonFinder({
            title: "Select Items",
            pageId: pageId,
            data: masterData.Childs,
            fields: "Segment1ValueDesc,Segment2ValueDesc,Segment3ValueDesc,Segment4ValueDesc,Segment5ValueDesc,Segment6ValueDesc,ShadeCode,POQty",
            headerTexts: "Composition, YarnType,Process, Sub Process,Quality Parameter,Count,Shade Code,POQty",
            isMultiselect: true,
            selectedIds: childIds,
            allowPaging: false,
            primaryKeyColumn: "YPOChildID",
            onMultiselect: function (selectedRecords) {
                selectedRecords.forEach(function (value) {
                    var exists = $tblChildEl.getCurrentViewRecords().find(function (el) { return el.YPOChildID == value.YPOChildID });
                    if (!exists) $tblChildEl.getCurrentViewRecords().unshift(value);
                });
                var isPresent = selectedRecords.some(function (element) {
                    return receivedChilds.includes(element);
                });
                if (!isPresent) {
                    selectedRecords.push(...receivedChilds);
                }
      
                selectedRecords.filter(x => x.YPIReceiveChildID == 0).map(x => {
                    x.YPIReceiveChildID = _ypiReceiveChildID++;
                });

                initChildTable(selectedRecords);
                $tblChildEl.refresh();
            }
        });
        finder.showModal();
    }

    //function calculateTotalYarnPOQty(args) {
    //    //console.log(args);
    //    var yarnPOQty = 0;
    //    yarnPOQty += isNaN(parseFloat(args.data.POQty)) ? 0 : parseFloat(args.data.POQty); 
    //    //$.each(data, function (i, row) {
    //    //    console.log(row);
    //    //    yarnPOQty += isNaN(parseFloat(row.POQty)) ? 0 : parseFloat(row.POQty);
    //    //});

    //    return yarnPOQty.toFixed(2);
    //}

    //function calculateTotalYarnPIQty(data) {
    //    var yarnPOQty = 0;

    //    $.each(data, function (i, row) {
    //        yarnPOQty += isNaN(parseFloat(row.PIQty)) ? 0 : parseFloat(row.PIQty);
    //    });

    //    return yarnPOQty.toFixed(2);
    //} 

    //function calculateTotalYarnQtyAll(data) {
    //    var yarnPOQtyAll = 0;

    //    $.each(data, function (i, row) {
    //        yarnPOQtyAll += isNaN(parseFloat(row.TotalQty)) ? 0 : parseFloat(row.TotalQty);
    //    });

    //    return yarnPOQtyAll.toFixed(2);
    //}

    //function calculateTotalYarnValueAll(data) {
    //    var yarnPOValueAll = 0;

    //    $.each(data, function (i, row) {
    //        yarnPOValueAll += isNaN(parseFloat(row.TotalValue)) ? 0 : parseFloat(row.TotalValue);
    //    });

    //    return yarnPOValueAll.toFixed(2);
    //}

    //function getFormattedActions(row) {
    //    var formattedActions = "";
    //    switch (status) {
    //        case statusConstants.PENDING:
    //            formattedActions =
    //                `<span class="btn-group">
    //                <a class="btn btn-xs btn-primary m-w-30 add" href="javascript:void(0)" title="Add PI">
    //                    <i class="fa fa-plus" aria-hidden="true"></i>
    //                </a>
    //            </span>`;
    //            break
    //        case statusConstants.COMPLETED:
    //        case statusConstants.REJECT:
    //            formattedActions =
    //                `<span class="btn-group">
    //                <a class="btn btn-xs btn-primary m-w-30 edit" href="javascript:void(0)" title="Edit PI">
    //                    <i class="fa fa-edit" aria-hidden="true"></i>
    //                </a>
    //            </span>`;
    //            break;
    //        default:
    //            break;
    //    }
    //    return formattedActions;
    //}

    //function calculatePIReceiveTotalPOValue(data) {
    //    var piReceiveTotalValue = 0;

    //    $.each(data, function (i, row) {
    //        piReceiveTotalValue += isNaN(parseFloat(row.TotalValue)) ? 0 : parseFloat(row.TotalValue);
    //    });

    //    return piReceiveTotalValue.toFixed(2);
    //}

    //function calculatePIReceiveTotalPOQty(data) {
    //    var piReceiveTotalQty = 0;

    //    $.each(data, function (i, row) {
    //        piReceiveTotalQty += isNaN(parseFloat(row.TotalQty)) ? 0 : parseFloat(row.TotalQty);
    //    });

    //    return piReceiveTotalQty.toFixed(2);
    //}

})();