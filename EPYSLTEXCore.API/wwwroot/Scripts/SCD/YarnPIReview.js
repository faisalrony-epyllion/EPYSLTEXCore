(function () {
    'use strict'
    var currentChildRowData;
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status,
        isCDAPage = false;

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
        isCDAPage = convertToBoolean($(`#${pageId}`).find("#CDAPage").val());

        status = statusConstants.PENDING;
        initMasterTable();
        //getMasterTableData();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
            //getMasterTableData();
        });

        $toolbarEl.find("#btnAcceptList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;

            initMasterTable();
            //getMasterTableData();
        });

        $toolbarEl.find("#btnRejectList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REJECT;

            initMasterTable();
            //getMasterTableData();
        });

        $toolbarEl.find("#btnRevisionList").on("click", function (e) {
            e.preventDefault();
            resetTableParams();
            status = statusConstants.REVISE;
            initMasterTable();
            toggleActiveToolbarBtn(this, $toolbarEl);
            
        });

        $formEl.find("#btnAccept").click(function (e) {
            e.preventDefault();
            save(this, true);
        });
        $formEl.find("#btnReject").click(function (e) {
            e.preventDefault();
            var rejectreason = $formEl.find("#RejectReason").val();
            if (rejectreason != "") {
                save(this,false);
            }
            else {
                return toastr.error("Please Select Reason");
            }

        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#PIQty").keyup(function (e) {
            e.preventDefault();
            btnHideShow(false, 0, 0, 0, 0);
        });

        $formEl.find("#PIValue").keyup(function (e) {
            e.preventDefault();
            btnHideShow(false, 0, 0, 0, 0);
        });
    });
    function btnHideShow(isSetFromParam, prmPIQty, prmPOQty, prmPIValue, prmPOValue) {
        var PIQty = 0,
            PIValue = 0,
            POQty = 0,
            POValue = 0;
        if (isSetFromParam) {
            PIQty = String(prmPIQty);
            PIValue = String(prmPIValue);
            POQty = String(prmPOQty);
            POValue = String(prmPOValue);
        } else {
            PIQty = $formEl.find("#PIQty").val();
            PIValue = $formEl.find("#PIValue").val();
            POQty = $formEl.find("#TotalQty").val();
            POValue = $formEl.find("#TotalValue").val();
        }

        var valueDiff = parseFloat(PIValue.replace(/,/g, '')) - parseFloat(POValue.replace(/,/g, ''));
        if (valueDiff < 0) valueDiff = valueDiff * (-1);
        valueDiff = parseFloat(valueDiff).toFixed(4);

        $formEl.find("#btnAccept").fadeOut();
        $formEl.find("#btnReject").fadeOut();
        $formEl.find("#divReject").fadeOut();

        PIQty = parseFloat(PIQty);
        PIValue = parseFloat(PIValue);
        POQty = parseFloat(POQty);
        POValue = parseFloat(POValue);

        //if (PIQty <= POQty && (PIValue == POValue || (valueDiff >= 0.00 && valueDiff <= 0.99))) {
        if (PIQty <= POQty && (PIValue <= POValue || (valueDiff >= 0.00 && valueDiff <= 0.99))) {
            $formEl.find("#btnAccept").fadeIn();
            $formEl.find("#btnReject").fadeOut();
            $formEl.find("#divReject").fadeOut();
        }
        else {
            $formEl.find("#btnReject").fadeIn();
            $formEl.find("#divReject").fadeIn();
            $formEl.find("#btnAccept").fadeOut();
        }
    }

    function initMasterTable() {
        var commands = [];
        if (status === statusConstants.PENDING) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } },
                { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ]
        } else {
            commands = [
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ]
        }

        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', width: ch_setActionCommandCellWidth(commands)
            },
            {
                field: 'YPINo', headerText: 'PI No'
            },
            {
                field: 'PIDate', headerText: 'PI Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'PONo', headerText: 'PO No'
            },
            {
                field: 'POAddedByName', headerText: 'PO created by'
            },
            {
                field: 'CompanyName', headerText: 'Company'
            },
            {
                field: 'SupplierName', headerText: 'Supplier'
            },
            {
                field: 'POQty', headerText: 'PO Qty'
            },

            {
                field: 'PIQty', headerText: 'PI Qty'
            },
            {
                field: 'PIValue', headerText: 'PI Value'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yarn-pi-review/list?status=${status}&isCDAPage=${isCDAPage}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == "Edit") {
            resetForm();
            getData(args.rowData.YPIReceiveMasterID, args.rowData.SupplierID, args.rowData.CompanyID);
        }
        if (args.commandColumn.type == "View") {
            if (args.rowData.Acknowledge == 0) {
                toastr.error("Waiting For PI Acknowledge");
                return false;
            }
            else if (args.rowData.Accept == 0) {
                toastr.error("Please Accept the PI From Pending List");
                return false;
            }
            
            resetForm();
            getData(args.rowData.YPIReceiveMasterID, args.rowData.SupplierID, args.rowData.CompanyID);
        }
        else if (args.commandColumn.type == 'Report') {
            var a = document.createElement('a');
            a.href = args.rowData.PIFilePath;
            a.setAttribute('target', '_blank');
            a.click();
        }
        $formEl.find("#btnSave").fadeIn();
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#YPIReceiveMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
        //$formEl.find("#divReject,#btnAccept,#btnReject,#divCreditDays,#divCreditDaysPO,#divCreditDaysMsg").fadeOut();
        $formEl.find("#divCreditDays,#divCreditDaysPO,#divCreditDaysMsg").fadeOut();
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getData(id, supplierId, companyId) {
        axios.get(`/api/yarn-pi-review/getData/${id}/${supplierId}/${companyId}/${isCDAPage}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.PIDate = formatDateToDefault(masterData.PIDate);
                setFormData($formEl, masterData);

                //$formEl.find("#TotalQty").val(masterData.YarnPO.TotalQty);
                //$formEl.find("#TotalValue").val(masterData.YarnPO.TotalValue);

                $formEl.find("#TotalQty").val(masterData.POQty);
                $formEl.find("#TotalValue").val(masterData.POValue);
                $formEl.find("#TypeOfLcPO").val(masterData.YarnPO.TypeOfLC);
                $formEl.find("#ShippingTolerancePO").val(masterData.YarnPO.ShippingTolerance);
                $formEl.find("#CreditDaysPO").val(masterData.YarnPO.CreditDays);
                //Usance

               
                if ($formEl.find("#TypeOfLcPO").val() == "Usance") {
                    $formEl.find("#divCreditDaysPO").fadeIn();
                }


                $formEl.find("#IsCDA").val(isCDAPage);

                //btnHideShow(true, masterData.PIQty, masterData.POQty, masterData.PIValue, masterData.POValue)
                if (masterData.PIQty <= masterData.POQty|| masterData.PIValue <= masterData.POValue) {
                    $formEl.find("#btnAccept").fadeIn();
                    $formEl.find("#btnReject").fadeOut();
                    $formEl.find("#divReject").fadeOut();
                }
                else {
                    $formEl.find("#btnReject").fadeIn();
                    $formEl.find("#divReject").fadeIn();
                    $formEl.find("#btnAccept").fadeOut();

                }
                //$tblChildEl.bootstrapTable("load", masterData.KRolls);
                //$tblChildEl.bootstrapTable('hideLoading');

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(invokedBy, isAccept) {
        debugger;
        //var PIQty = $formEl.find("#PIQty").val();
        //var PIValue = $formEl.find("#PIValue").val();
        //masterData.PIQty = $formEl.find("#PIQty").val();
        //masterData.PIValue = $formEl.find("#PIValue").val();
        var data = masterData;// formDataToJson($formEl.serializeArray()); //data["KRolls"] = masterData.KRolls;
        if (isAccept) {
            data.Accept = true;
            data.Reject = false;
        }
        else {
            data.Reject = true;
            data.RejectReason = $formEl.find("#RejectReason").val();
            data.Accept = false;
        }

        axios.post("/api/yarn-pi-review/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();