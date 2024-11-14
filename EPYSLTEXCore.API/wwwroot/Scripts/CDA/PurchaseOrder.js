(function () {
    'use strict'
    var masterData = {}, currentChildRowData;
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var status;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var prMasterId, companyId;

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
         
        if (pageName == "CDAPOApproval") {
            status = statusConstants.PROPOSED; // Peding for approval
            initMasterTable();
            $toolbarEl.find("#btnYPONew").hide();
            $toolbarEl.find("#btnPendingList").hide();
            $toolbarEl.find("#btnAwaitingProposeList").hide();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingApprovalList"), $toolbarEl);
            $toolbarEl.find("#btnAddCDAPR").fadeOut();
        } else {
            status = statusConstants.PENDING;
            initMasterTable();
            $toolbarEl.find("#btnYPONew").show();
            $toolbarEl.find("#btnPendingList").show();
            $toolbarEl.find("#btnAwaitingProposeList").show();
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);
            $toolbarEl.find("#btnAddCDAPR").fadeIn();
        }

        $toolbarEl.find("#btnYPONew").click(btnNewClick);

        $toolbarEl.find("#btnPendingList").click(btnPendingListClick);

        $toolbarEl.find("#btnAwaitingProposeList").click(btnAwaitingProposeListClick);

        $toolbarEl.find("#btnPendingApprovalList").click(btnPendingApprovalListClick);

        $toolbarEl.find("#btnApprovedList").click(btnApprovedListClick);

        $toolbarEl.find("#btnRejectList").click(btnRejectListClick);

        $toolbarEl.find("#btnAllList").click(btnAllListClick);

        if ($formEl.find("#IsRevision").is(':checked')) {
            $formEl.find("#IsCancel").prop("checked", false);
        }

        $formEl.find("#SupplierID").on("select2:select", function (e) {
            var url = `/api/cdapo/get-supplier-info/${e.params.data.id}`;
            axios.get(url)
                .then(function (response) {
                    
                    response.data.CDAPRMasterID = masterData.CDAPRMasterID;
                    response.data.PRDate = masterData.PRDate;
                    response.data.PRNO = masterData.PRNO;
                    response.data.Remarks = masterData.Remarks;
                    response.data.CompanyID = masterData.CompanyID;
                    response.data.CompanyName = masterData.CompanyName;
                    response.data.CDAPOChilds = masterData.CDAPOChilds;
                    response.data.SupplierList = masterData.SupplierList;
                    response.data.BuyerList = masterData.BuyerList;
                    response.data.YarnPOChildOrders = masterData.YarnPOChildOrders;
                    response.data.POForList = masterData.POForList;
                    response.data.CurrencyID = masterData.CurrencyID;
                    response.data.CurrencyCode = masterData.CurrencyCode;
                    response.data.SubGroupID = masterData.SubGroupID;
                    response.data.ReImbursementCurrencyID = masterData.ReImbursementCurrencyID;
                    response.data.ReImbursmentCurrency = masterData.ReImbursmentCurrency;
                    $("#SupplierID").val(e.params.data.id);
                    masterData = response.data;
                    masterData.PODate = formatDateToDefault(masterData.PODate);
                    masterData.DeliveryStartDate = formatDateToDefault(masterData.DeliveryStartDate);
                    masterData.DeliveryEndDate = formatDateToDefault(masterData.DeliveryEndDate);
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

                })
                .catch(showResponseError)
        });

        $formEl.find("#PaymentTermsID").on("select2:select", function (e) {
            if (e.params.data.id == "1") showHideLCSection(false);
            else showHideLCSection(true);
        });

        $formEl.find("#TypeOfLCID").on("select2:select", function (e) {
            if (e.params.data.id == "1") $formEl.find("#formGroupCreditDays").fadeOut();
            else $formEl.find("#formGroupCreditDays").fadeIn();
        });

        $formEl.find('#DeliveryStartDate').datepicker()
            .on('changeDate', function (ev) {
                
                //startDate = new Date(ev.date.getFullYear(), ev.date.getMonth(), ev.date.getDate(), 0, 0, 0);
                if (new Date($formEl.find('#PODate').val()) != null && new Date($formEl.find('#PODate').val()) != 'undefined') {
                    if (new Date($formEl.find('#DeliveryStartDate').val()) < new Date($formEl.find('#PODate').val())) {
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
            $("#modal-child-Yarn-TNA").modal('show');
        });

        $formEl.find("#btnEditCancelYarnPO").click(function (e) {
            e.preventDefault();
            backToList();
            //toggleActiveToolbarBtn($formEl.find("#btnPendingList"), $toolbarEl);
        });

        $formEl.find("#btnSaveYPO").click(function (e) {
            e.preventDefault();
            saveYPO();
        });

        $formEl.find("#btnSaveAndProposeYPO").click(function (e) {
            e.preventDefault();
            saveYPO(true);
        });

        $formEl.find("#btnApproveYPO").click(function (e) {
            e.preventDefault(); 
            var url = "/api/cdapo/approve-ypo/" + $formEl.find("#CDAPOMasterID").val();
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
                        CDAPOMasterID: $formEl.find("#CDAPOMasterID").val(),
                        UnapproveReason: result
                    };

                    axios.post("/api/cdapo/reject-ypo", data)
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

        $formEl.find("#btnAddChild").on("click", function (e) {
            e.preventDefault();
            var childIDs = $tblChildEl.getCurrentViewRecords().map(function (el) { return el.PRChildID }).toString();
            var finder = new commonFinder({
                title: "Select",
                pageId: pageId,
                height: 320,
                modalSize: "modal-lg",
                apiEndPoint: `/api/cdapo/pr-child-list?status=${status}&childIDs=${childIDs}&companyId=${masterData.CompanyID}`,
                fields: "PRNo,Segment1ValueDesc,Segment2ValueDesc",
                headerTexts: "PR No,Agent,Item",
                isMultiselect: true,
                primaryKeyColumn: "PRChildID",
                onMultiselect: function (selectedRecords) {
                    selectedRecords.forEach(function (value) {
                        var exists = $tblChildEl.getCurrentViewRecords().find(function (el) { return el.PRChildID == value.PRChildID });
                        if (!exists) $tblChildEl.getCurrentViewRecords().unshift(value);
                    });
                    initChildTable($tblChildEl.getCurrentViewRecords());
                    $tblChildEl.refresh();
                }
            });

            finder.showModal();
        });

        $toolbarEl.find("#btnAddCDAPR").on("click", addPOFromPR);
    });

    function btnNewClick(e) {
        e.preventDefault();
        toggleActiveToolbarBtn(this, $toolbarEl);
        showSupplierSelection();
        $toolbarEl.find("#btnAddCDAPR").fadeOut();
    }

    function btnPendingListClick(e) {
        e.preventDefault();
        resetTableParams();
        status = statusConstants.PENDING;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddCDAPR").fadeIn();
    }

    function btnAwaitingProposeListClick(e) {
        e.preventDefault();
        resetTableParams();
        status = statusConstants.AWAITING_PROPOSE;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddCDAPR").fadeOut();
    }

    function btnPendingApprovalListClick(e) {
        e.preventDefault();
        resetTableParams();
        status = statusConstants.PROPOSED;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddCDAPR").fadeOut();
    }

    function btnApprovedListClick(e) {
        e.preventDefault();
        resetTableParams();
        status = statusConstants.APPROVED;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut(); //fadeIn();
        $toolbarEl.find("#btnAddCDAPR").fadeOut();
    }

    function btnRejectListClick(e) {
        e.preventDefault();
        resetTableParams();
        status = statusConstants.UN_APPROVE;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddCDAPR").fadeOut();
    }

    function btnAllListClick(e) {
        e.preventDefault();
        resetTableParams();
        status = statusConstants.ALL;
        initMasterTable();
        toggleActiveToolbarBtn(this, $toolbarEl);
        $formEl.find("#RevisionArea").fadeOut();
        $toolbarEl.find("#btnAddCDAPR").fadeOut();
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#CDAPOMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();

        showHideLCSection(false);
        showHideSupplierRegionSection(false);
        //$tblChildEl.bootstrapTable('destroy');
        //$formEl.find("#tblYarnPOforExportOrders").bootstrapTable('load', masterData.YarnPOForOrders);
        //$formEl.find("#formGroupPoForDetails").remove();
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
                if (pageName == "CDAPOApproval") {
                    commands = [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
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
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
                break;
            case statusConstants.ALL:
                commands = [
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
                break;
            case statusConstants.UN_APPROVE:
                if (pageName == "CDAPOApproval") {
                    commands = [
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                } else {
                    commands = [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
                }
                break;
            default:
                break;
        }

        var columns = [
            {
                headerText: '', visible: status != statusConstants.PENDING, commands: commands, textAlign: 'Center', width: 75, minWidth:75, maxWidth:75
            },
            {
                field: 'CDAPOChildID', visible: false
            },
            {
                field: 'PRNO', headerText: 'PR No', visible: status == statusConstants.PENDING
            },
            {
                field: 'PRDate', headerText: 'PR Date', textAlign: 'Center', type: 'date',
                format: _ch_date_format_1, visible: status == statusConstants.PENDING
            },
            {
                field: 'PRRequiredDate', headerText: 'PR Req. Date', textAlign: 'Center', type: 'date',
                format: _ch_date_format_1, visible: status == statusConstants.PENDING
            },
            {
                field: 'PRByUser', headerText: 'PR By', visible: status == statusConstants.PENDING
            },
            {
                field: 'PONo', headerText: 'PO No', visible: status != statusConstants.PENDING
            },
            {
                field: 'PODate', headerText: 'PO Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'CompanyName', headerText: 'Company'
            },
            {
                field: 'SupplierName', headerText: 'Supplier', visible: status != statusConstants.PENDING
            },
            {
                field: 'QuotationRefNo', headerText: 'Ref No', visible: status != statusConstants.PENDING
            },
            {
                field: 'DeliveryStartDate', headerText: 'Delivery Start', textAlign: 'Center', type: 'date',
                format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'DeliveryEndDate', headerText: 'Delivery End', textAlign: 'Center', type: 'date',
                format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'POStatus', headerText: 'Status', visible: status != statusConstants.PENDING
            },
            {
                field: 'InHouseDate', headerText: 'In-House Date', textAlign: 'Center', type: 'date',
                format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'Segment1ValueDesc', headerText: 'Chemicals Agent/Dyes Group',
                visible: status == statusConstants.PENDING  
            },
            {
                field: 'Segment2ValueDesc', headerText: 'Chemicals Form/Dyes Item', visible: status == statusConstants.PENDING
            },
            {
                field: 'Segment3ValueDesc', headerText: 'Chemicals Group', visible: status == statusConstants.PENDING
            },
            {
                field: 'Segment4ValueDesc', headerText: 'Chemicals Item Name', visible: status == statusConstants.PENDING
            },
            {
                field: 'ReqQty', headerText: 'Req Qty', visible: status == statusConstants.PENDING
            },
        ];

        if (status == statusConstants.PENDING) {
            columns.unshift({ type: 'checkbox', width: 50 });
            var selectionType = "Multiple";
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: status != statusConstants.PENDING,
            apiEndPoint: `/api/cdapo/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) { 
        if (args.commandColumn.type == 'Add') {
            //addPOFromPR(args.rowData);
            //$formEl.find("#divBasicInfo :input").attr("disabled", false);
            //$formEl.find("#divChildInfo :input").attr("disabled", false);
            //$formEl.find("#divTermsCondition :input").attr("disabled", false);
            //$formEl.find("#divInhouseInfo :input").attr("disabled", false);
        } else if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData);
            if (status == statusConstants.PROPOSED || status == statusConstants.APPROVED || status == statusConstants.UN_APPROVE || status == statusConstants.ALL) {
                //$formEl.find("#divBasicInfo :input").attr("disabled", true);
                //$formEl.find("#divChildInfo :input").attr("disabled", true);
                //$formEl.find("#divTermsCondition :input").attr("disabled", true);
                //$formEl.find("#divInhouseInfo :input").attr("disabled", true);
            }
            else {
                //$formEl.find("#divBasicInfo :input").attr("disabled", false);
                //$formEl.find("#divChildInfo :input").attr("disabled", false);
                //$formEl.find("#divTermsCondition :input").attr("disabled", false);
                //$formEl.find("#divInhouseInfo :input").attr("disabled", false);
            }
        } else if (args.commandColumn.type == 'Propose') {
            proposePO(args.rowData);
        } else if (args.commandColumn.type == 'Approve') {
            approvePO(args.rowData);
        } else if (args.commandColumn.type == 'Reject') {
            rejectPO(args.rowData);
        } else if (args.commandColumn.type == 'Report') {
            var a = document.createElement('a');
            a.href = "/reports/InlinePdfView?ReportId=990&PONo=" + args.rowData.PONo;
            a.setAttribute('target', '_blank');
            a.click();
        }
    }

    function addPOFromPR(row) {  
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
            return el.CDAPRMasterID
        }).toString();

        var cDAPRChildIDs = $tblMasterEl.getSelectedRecords().map(function (el) {
            return el.CDAPOChildID
        }).toString(); 
        
        companyId = uniqueAry[0].CompanyID;

        $divTblEl.fadeOut();
        $divDetailsEl.fadeIn();

        $formEl.find("#btnSaveYPO").fadeIn();
        $formEl.find("#btnSaveAndProposeYPO").fadeIn();
        $formEl.find("#btnApproveYPO").fadeOut();
        $formEl.find("#btnRejectYPO").fadeOut();
        $formEl.find("#SupplierTNA").fadeIn();
        $formEl.find("#RevisionArea").fadeOut();

        HoldOn.open({
            theme: "sk-circle"
        });
        
        var url = `/api/cdapo/new/${prMasterIDs}/${cDAPRChildIDs}/${companyId}`;
        axios.get(url)
            .then(function (response) {
                masterData = response.data;
                masterData.PODate = formatDateToDefault(masterData.PODate);
                masterData.DeliveryStartDate = formatDateToDefault(masterData.DeliveryStartDate);
                masterData.DeliveryEndDate = formatDateToDefault(masterData.DeliveryEndDate);
                masterData.QuotationRefDate = formatDateToDefault(masterData.QuotationRefDate);
                masterData.InHouseDate = formatDateToDefault(masterData.InHouseDate);

                setFormData($formEl, masterData);

                companyId = masterData.CompanyID;

                if (masterData.PaymentTermsID === 2) showHideLCSection(true);
                else showHideLCSection(false);

                if (masterData.PortofLoadingID === 105) showHideSupplierRegionSection(false);
                else showHideSupplierRegionSection(true);

                if (masterData.TypeOfLCID === 2) $formEl.find("#formGroupCreditDays").fadeIn();
                else $formEl.find("#formGroupCreditDays").fadeOut();

                $("#PODateCurrent").text(masterData.DeliveryStartDate);
                $pageEl.find("#SFToPLDate").text(formatDateToDefault(masterData.SFToPLDate));
                $pageEl.find("#SFToPLDays").text(masterData.SFToPLDays);
                $pageEl.find("#PLToPDDate").text(formatDateToDefault(masterData.PLToPDDate));
                $pageEl.find("#PLToPDDays").text(masterData.PLToPDDays);
                $pageEl.find("#PDToCFDate").text(formatDateToDefault(masterData.PDToCFDate));
                $pageEl.find("#PDToCFDays").text(masterData.PDToCFDays);
                $pageEl.find("#InHouseDays").text(masterData.InHouseDays);

                initChildTable(masterData.Childs);

                HoldOn.close();
            })
            .catch(showResponseError)
    }

    async function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();

        var columns = [
            {
                headerText: 'Command', width: 100, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }
                ]
            },
            { field: 'PRChildID', isPrimaryKey: true, visible: false },
            { field: 'PRNo', headerText: 'PR No', allowEditing: false },
        ];
        if (masterData.SubGroupID == 100) {
            columns.push.apply(columns, [
                { field: 'Segment1ValueDesc', headerText: 'Dyes Group', allowEditing: false },
                { field: 'Segment2ValueDesc', headerText: 'Dyes Item Name', allowEditing: false }
            ]);
        } else {
            columns.push.apply(columns, [
                { field: 'Segment1ValueDesc', headerText: 'Chemicals Agent', allowEditing: false },
                { field: 'Segment2ValueDesc', headerText: 'Chemicals Form', allowEditing: false },
                { field: 'Segment3ValueDesc', headerText: 'Chemicals Group', allowEditing: false },
                { field: 'Segment4ValueDesc', headerText: 'Chemicals Item Name', allowEditing: false }
            ]);
        }

        var additionalColumns = [ 
            { field: 'QuotationRefNo', headerText: 'Quotation Ref. No'}, 
            {
                field: 'QuotationRefDate', headerText: 'Quotation Ref. Date', type: 'date', format: _ch_date_format_1,
                editType: 'datepickeredit', width: 40, textAlign: 'Center'
            },
            { field: 'ReqQty', headerText: 'PR Qty', allowEditing: false },
            { field: 'POQty', headerText: 'PO Qty' },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },
            { field: 'Rate', headerText: 'Rate' },
            { field: 'POValue', headerText: 'Total Value', allowEditing: false },
            { field: 'HSCode', headerText: 'HS Code' },
            { field: 'POFor', headerText: 'Purchase For', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks' }
        ];
        columns.push.apply(columns, additionalColumns);

        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "save") { 
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.PRChildID);
                    args.data.POValue = (args.data.POQty * args.data.Rate).toFixed(2); 
                    masterData.Childs[index] = args.data;
                }
                //else if (args.requestType === "add") {
                //}
            },
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false,
            //toolbar: [{ text: 'Add Items', tooltipText: 'Add PR Childs', prefixIcon: 'e-icons e-add', id: 'tblAddItem' }],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
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
                $formEl.find("#btnSaveYPO").fadeIn();
                $formEl.find("#btnSaveAndProposeYPO").fadeIn();
                break;
            case statusConstants.PROPOSED:
                if (pageName == "CDAPOApproval") {
                    $formEl.find("#btnSaveYPO").fadeOut();
                    $formEl.find("#btnSaveAndProposeYPO").fadeOut();
                    $formEl.find("#btnApproveYPO").fadeIn();
                    $formEl.find("#btnRejectYPO").fadeIn();
                }
                else {
                    $formEl.find("#btnApproveYPO").fadeOut();
                    $formEl.find("#btnRejectYPO").fadeOut();
                    $formEl.find("#btnSaveYPO").fadeOut();
                    $formEl.find("#btnSaveAndProposeYPO").fadeOut();
                }
                break;
            case statusConstants.UN_APPROVE:
            case statusConstants.APPROVED:
                $formEl.find("#btnApproveYPO").fadeOut();
                $formEl.find("#btnRejectYPO").fadeOut();
                $formEl.find("#btnSaveYPO").fadeOut();
                $formEl.find("#btnSaveAndProposeYPO").fadeOut();
                break;
            default:
                break;
        }

        var url = `/api/cdapo/${row.CDAPOMasterID}/${row.SupplierID}`;
        axios.get(url)
            .then(function (response) {
                $divTblEl.fadeOut();
                $divDetailsEl.fadeIn();
                $formEl.find("#SupplierTNA").fadeIn();

                masterData = response.data;
                masterData.PODate = formatDateToDefault(masterData.PODate);
                masterData.DeliveryStartDate = formatDateToDefault(masterData.DeliveryStartDate);
                masterData.DeliveryEndDate = formatDateToDefault(masterData.DeliveryEndDate);
                masterData.QuotationRefDate = formatDateToDefault(masterData.QuotationRefDate);
                masterData.InHouseDate = formatDateToDefault(masterData.InHouseDate);
                setFormData($formEl, masterData);

                if (masterData.PaymentTermsID === 2) showHideLCSection(true);
                else showHideLCSection(false);

                if (masterData.PortofLoadingID === 105) showHideSupplierRegionSection(false);
                else showHideSupplierRegionSection(true);

                if (masterData.TypeOfLCID === 2) $formEl.find("#formGroupCreditDays").fadeIn();
                else $formEl.find("#formGroupCreditDays").fadeOut();

                $("#PODateCurrent").text(masterData.DeliveryStartDate);
                $pageEl.find("#SFToPLDate").text(formatDateToDefault(masterData.SFToPLDate));
                $pageEl.find("#SFToPLDays").text(masterData.SFToPLDays);
                $pageEl.find("#PLToPDDate").text(formatDateToDefault(masterData.PLToPDDate));
                $pageEl.find("#PLToPDDays").text(masterData.PLToPDDays);
                $pageEl.find("#PDToCFDate").text(formatDateToDefault(masterData.PDToCFDate));
                $pageEl.find("#PDToCFDays").text(masterData.PDToCFDays);
                $pageEl.find("#InHouseDays").text(masterData.InHouseDays);

                initChildTable(masterData.Childs);

                HoldOn.close();
            })
            .catch(showResponseError)
    }

    function proposePO(row) {
        showBootboxConfirm("Propose Yarn PO", "Are you sure you want to propose this PO?", function (yes) {
            if (yes) {
                var url = "/api/cdapo/propose-ypo/" + row.CDAPOMasterID;
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
        //console.log(row);
        showBootboxConfirm("Approve Yarn PO", "Are you sure you want to approve this PO?", function (yes) {
            if (yes) {
                var url = "/api/cdapo/approve-ypo/" + row.CDAPOMasterID;
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
                    CDAPOMasterID: row.CDAPOMasterID,
                    UnapproveReason: result
                };

                axios.post("/api/cdapo/reject-ypo", data)
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

    function saveYPO(isPropose = false) {
        $formEl.find(':checkbox').each(function () {
            this.value = this.checked;
        });

        var data = formDataToJson($formEl.serializeArray());
        if (!data.CompanyID) data.CompanyID = companyId;
        data["Childs"] = $tblChildEl.getCurrentViewRecords();
        data.Proposed = isPropose ? true : false;

        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl); 
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");       

        //Child Validation check 
        if (isValidChildForm(data)) return;

        axios.post("/api/cdapo/save", data)
            .then(function (response) {
                showBootboxAlert("Yarn PO No: <b>" + response.data + "</b> saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function isValidChildForm(data) {
        var isValidItemInfo = false;  
        if ($formEl.find("#PaymentTermsID").val() == "2") { 
            if ($formEl.find("#TypeOfLCID").val() == "" || $formEl.find("#TypeOfLCID").val() == null) {
                toastr.error("Type of L/C is required.");
                isValidItemInfo = true;
            }
            if ($formEl.find("#CalculationOfTenure").val() == "" || $formEl.find("#CalculationOfTenure").val() == null) {
                toastr.error("Calc. of Tenure is required.");
                isValidItemInfo = true;
            }
        } 

        $.each(data["Childs"], function (i, el) {
            if (el.POQty == "" || el.POQty == null || el.POQty <= 0) {
                toastr.error("PO Qty is required.");
                isValidItemInfo = true;
            }
            if (el.POQty > el.ReqQty) {
                toastr.error("PO qty must be equal or less than PR qty");
                isValidItemInfo = true;
            }
        });
        return isValidItemInfo;
    }

    var validationConstraints = {
        SupplierID: {
            presence: true
        },
        IncoTermsID: {
            presence: true
        },
        PaymentTermsID: {
            presence: true
        }
    }
})();