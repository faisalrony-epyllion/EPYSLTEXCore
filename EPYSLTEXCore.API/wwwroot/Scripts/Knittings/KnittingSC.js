(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl,
        tblMasterId, tblChildId, itemdtlTblId, $tblitemdtl;

    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;
    var subGroupName = "";
    var masterData;
    var PopUpCorBList = [];
    var programListIds = [];
    var isKSC = false;
    var isKSCApprove = false;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        //$tblitemdtl = $("#tbldtlitem" + pageId);
        itemdtlTblId = "#tbldtlitem" + pageId;

        isKSC = menuParam == "SC" ? true : false;
        isKSCApprove = menuParam == "SCA" ? true : false;

        $toolbarEl.find("#btnPendingList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
            $toolbarEl.find("#btnCreate").show();
        });
        $toolbarEl.find("#btnDraftList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.DRAFT;
            initMasterTable();
            $toolbarEl.find("#btnCreate").show();
        });
        $toolbarEl.find("#btnRevisionList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REVISE;
            initMasterTable();
            $toolbarEl.find("#btnCreate").hide();
        });
        $toolbarEl.find("#btnPendingForApprovalList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED_FOR_APPROVAL;
            initMasterTable();
            $toolbarEl.find("#btnCreate").hide();
        });
        $toolbarEl.find("#btnApprovedList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            initMasterTable();
            $toolbarEl.find("#btnCreate").hide();
        });
        $toolbarEl.find("#btnRejectList").click(function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REJECT;
            initMasterTable();
            $toolbarEl.find("#btnCreate").hide();
        });
        $toolbarEl.find("#btnAllList").on("click", function (e) {
            e.preventDefault();
            actionBtnHideShow();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ALL;
            $toolbarEl.find("#btnCreate").hide();
            initMasterTable();
        });

        $toolbarEl.find("#btnRefreshList").click(function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        $formEl.find("#btnSave").click(function () {
            save(false, false, false, "");
        });
        $formEl.find("#btnSaveAndSendForApproval").click(function () {
            save(true, false, false, "");
        });
        $formEl.find("#btnAprrove").click(function () {
            save(false, true, false, "");
        });
        $formEl.find("#btnReject").click(function () {
            bootbox.prompt("Enter your Reject reason:", function (result) {
                if (!result) {
                    return toastr.error("Reject reason is required.");
                }
                save(false, false, true, result);
            });
        });

        $toolbarEl.find("#btnCreate").click(function (e) {
            var selectedRecords = $tblMasterEl.getSelectedRecords();
            if (selectedRecords.length > 0) {
                getProgramDataForSave();
            }
        });

        $formEl.find("#btnAddItem").on("click", function (e) {
            e.preventDefault();
            PopUpCorBList = [];
            var subContactID = 0;
            if ($tblChildEl.dataSource) {
                subContactID = $tblChildEl.dataSource[0].SubContactID;
            }
            axios.get(`/api/knitting-sub-contract/CorBListForPopUp/${subContactID}`)
                .then(function (response) {
                    response.data.Childs.map(function (el) {
                        el.YRequiredDate = formatDateToDefault(el.YRequiredDate);
                        return el
                    });
                    PopUpCorBList = response.data.Childs;

                    var finder = new commonFinder({
                        title: "Select New",
                        pageId: pageId,
                        height: 350,
                        data: PopUpCorBList,
                        fields: "CorBookingNo,SubContactName,YRequiredDate,SCQty,ProgramName",
                        headerTexts: "Concept/Booking No,Sub Contractor,Required Date,Quantity,Program Name",
                        isMultiselect: true,
                        primaryKeyColumn: "KPMasterID",
                        //selectedIds: programListIds,
                        /* selectedUniqueIds: selectedUniqueIds,*/
                        onMultiselect: function (selectedRecords) {
                            if (selectedRecords.length > 0) {
                                var newItems = getNewArrayItems($tblChildEl.dataSource, selectedRecords, "KPMasterID");
                            }

                            //var newItems = getNewArrayItems($tblChildEl.dataSource, selectedRecords, "KPMasterID");
                            for (i = 0; i < newItems.length; i++) {
                                if (newItems[0].SubGroupID == 1) {
                                    if ((masterData == undefined || $tblChildEl.dataSource.length == 0) &&
                                        newItems[i].SubContactID == newItems[0].SubContactID
                                        //&&
                                        //newItems[i].KnittingType == newItems[0].KnittingType &&
                                        //newItems[i].TechnicalName == newItems[0].TechnicalName &&
                                        //newItems[i].Composition == newItems[0].Composition &&
                                        //newItems[i].Gsm == newItems[0].Gsm
                                    ) {
                                        $tblChildEl.dataSource.unshift(newItems[i]);
                                    }
                                    else if (newItems[i].SubContactID == $tblChildEl.dataSource[0].SubContactID
                                        //&&
                                        //newItems[i].KnittingType == $tblChildEl.dataSource[0].KnittingType &&
                                        //newItems[i].TechnicalName == $tblChildEl.dataSource[0].TechnicalName &&
                                        //newItems[i].Composition == $tblChildEl.dataSource[0].Composition &&
                                        //newItems[i].Gsm == $tblChildEl.dataSource[0].Gsm
                                    ) {
                                        $tblChildEl.dataSource.unshift(newItems[i]);
                                    }
                                } else {
                                    if ((masterData == undefined || $tblChildEl.dataSource.length == 0) &&
                                        newItems[i].SubContactID == newItems[0].SubContactID
                                        //&&
                                        //newItems[i].KnittingType == newItems[0].KnittingType &&
                                        //newItems[i].TechnicalName == newItems[0].TechnicalName &&
                                        //newItems[i].Length == newItems[0].Length &&
                                        //newItems[i].Width == newItems[0].Width
                                    ) {
                                        $tblChildEl.dataSource.unshift(newItems[i]);
                                    }
                                    else if (newItems[i].SubContactID == $tblChildEl.dataSource[0].SubContactID
                                        //&&
                                        //newItems[i].KnittingType == $tblChildEl.dataSource[0].KnittingType &&
                                        //newItems[i].TechnicalName == $tblChildEl.dataSource[0].TechnicalName &&
                                        //newItems[i].Length == $tblChildEl.dataSource[0].Length &&
                                        //newItems[i].Width == $tblChildEl.dataSource[0].Width
                                    ) {
                                        $tblChildEl.dataSource.unshift(newItems[i]);
                                    }
                                }
                            }

                            if ($tblChildEl.dataSource != []) {
                                var programIds = $tblChildEl.dataSource[0].KPMasterID;
                                var SubContactID = $tblChildEl.dataSource[0].SubContactID;

                                for (i = 1; i < $tblChildEl.dataSource.length; i++) {
                                    if ($tblChildEl.dataSource[0].SubGroupID == 1) {
                                        if ($tblChildEl.dataSource[0].SubContactID == $tblChildEl.dataSource[i].SubContactID
                                            //&&
                                            //$tblChildEl.dataSource[0].KnittingType == $tblChildEl.dataSource[i].KnittingType &&
                                            //$tblChildEl.dataSource[0].TechnicalName == $tblChildEl.dataSource[i].TechnicalName &&
                                            //$tblChildEl.dataSource[0].Composition == $tblChildEl.dataSource[i].Composition &&
                                            //$tblChildEl.dataSource[0].Gsm == $tblChildEl.dataSource[i].Gsm
                                        ) {
                                            programIds = programIds + ',' + $tblChildEl.dataSource[i].KPMasterID;
                                            //SubContactID = $tblChildEl.dataSource[0].SubContactID;
                                        }
                                    } else {
                                        if ($tblChildEl.dataSource[0].SubContactID == $tblChildEl.dataSource[i].SubContactID
                                            //&&
                                            //$tblChildEl.dataSource[0].KnittingType == $tblChildEl.dataSource[i].KnittingType &&
                                            //$tblChildEl.dataSource[0].TechnicalName == $tblChildEl.dataSource[i].TechnicalName &&
                                            //$tblChildEl.dataSource[0].Length == $tblChildEl.dataSource[i].Length &&
                                            //$tblChildEl.dataSource[0].Width == $tblChildEl.dataSource[i].Width
                                        ) {
                                            programIds = programIds + ',' + $tblChildEl.dataSource[i].KPMasterID;
                                            //SubContactID = $tblChildEl.dataSource[0].SubContactID;
                                        }
                                    }
                                }
                            }

                            //programListIds = $tblChildEl.dataSource = selectedRecords.map(function (el) { return el.KPMasterID }).toString();
                            if (selectedRecords.length > 0) {
                                if (IsMatchValidation(selectedRecords) == false) {
                                    return;
                                }
                            }
                            getProgramData(programIds, BookingNo, SubContactID);
                        }
                    });
                    finder.showModal();
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });
        });

        $formEl.find("#btnSubContact").click(function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Machine",
                pageId: pageId,
                apiEndPoint: `/api/knitting-sub-contract/get-all-sub-contracts`,
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "KnittingMachineID",
                fields: "GG,Dia,Brand,Contact,MachineNo,Capacity,IsSubContact",
                headerTexts: "GG,Dia,Brand,Unit,MachineNo,Capacity,SubContact?",
                widths: "30,30,100,70,50,50,50",
                onSelect: function (res) {
                    finder.hideModal();
                    res = res.rowData;
                    $formEl.find("#KnittingMachineID").val(res.KnittingMachineID);
                    $formEl.find("#SubContactID").val(res.KnittingUnitID);
                    $formEl.find("#SubContactName").val(res.Contact);

                    masterData.KnittingMachineID = res.KnittingMachineID;
                    masterData.SubContactID = res.KnittingUnitID;
                    masterData.SubContactName = res.Contact;

                    if (res.KnittingUnitID > 0) {
                        axios.get(`/api/knitting-sub-contract/get-dropdown-list/${res.KnittingUnitID}`)
                            .then(function (response) {
                                var data = response.data;
                                masterData.IncoTermsList = data.IncoTermsList;
                                masterData.PaymentTermsList = data.PaymentTermsList;
                                masterData.TypeOfLCList = data.TypeOfLCList;
                                masterData.entityTypes = data.entityTypes;
                                masterData.ShipmentModeList = data.ShipmentModeList;
                                masterData.CreditDaysList = data.CreditDaysList;
                                masterData.PortofLoadingList = data.PortofLoadingList;
                                masterData.PortofDischargeList = data.PortofDischargeList;
                                masterData.OfferValidityList = data.OfferValidityList;
                                masterData.QualityApprovalProcedureList = data.QualityApprovalProcedureList;
                                masterData.CalculationofTenureList = data.CalculationofTenureList;
                                masterData.ExportOrderList = data.ExportOrderList;

                                masterData.ContactPersonName = $formEl.find("#ContactPersonName").val();
                                masterData.Remarks = $formEl.find("#Remarks").val();

                                setFormData($formEl, masterData);
                            })
                            .catch(function (err) {
                                toastr.error(err.response.data.Message);
                            });
                    }
                }
            });
            finder.showModal();
        });

        $formEl.find("#btnCancel").click(function () {
            backToList();
        });

        $formEl.find("#PaymentTermsID").on("select2:select", function (e) {
            if (e.params.data.id == "1") showHideLCSection(false);
            else showHideLCSection(true);
        });

        $formEl.find("#TypeOfLCID").on("select2:select", function (e) {
            if (e.params.data.id == "1") $formEl.find("#formGroupCreditDays").fadeOut();
            else $formEl.find("#formGroupCreditDays").fadeIn();
        });

        $toolbarEl.find(".btnToolbar").hide();
        if (isKSC) {
            $toolbarEl.find("#btnPendingList,#btnDraftList,#btnRevisionList,#btnPendingForApprovalList,#btnApprovedList,#btnRejectList").show();

            $toolbarEl.find("#btnPendingList").click();
        } else if (isKSCApprove) {
            $toolbarEl.find("#btnPendingForApprovalList,#btnApprovedList,#btnRejectList").show();

            $toolbarEl.find("#btnPendingForApprovalList").click();
        }
    });

    function initMasterTable() {
        var columns = [
            {
                headerText: 'Command', width: 100, visible: status !== statusConstants.PENDING, commands: [
                    { type: 'Edit', title: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            },
            {
                field: 'SubContactID', headerText: 'SubContactID', visible: false
            },
            //{
            //    type: 'checkbox', width: 100, headerText: 'Select', visible: status == statusConstants.PENDING
            //},
            {
                field: 'KSCNo', headerText: 'KSC No', visible: status != statusConstants.PENDING
            },
            {
                field: 'KSCDate', headerText: 'KSC Date', visible: status !== statusConstants.PENDING, type: 'date', format: _ch_date_format_1
            },
            {
                field: 'KSCByUser', headerText: 'KSC By', visible: status != statusConstants.PENDING
            },
            {
                field: 'CorBookingNo', headerText: 'Concept/Booking No'
            },
            {
                field: 'ExportOrderNo', headerText: 'EWO No', width: 80, visible: status == statusConstants.PENDING
            },
            {
                field: 'CompanyName', headerText: 'Unit', width: 60
            },
            {
                field: 'SubContactName', headerText: 'Sub Contractor'
            },
            {
                field: 'ProduceKnittingQty', headerText: 'Knitting Prod Qty'
            },
            {
                field: 'BookingQty', headerText: 'Plan Qty'
            },
            {
                field: 'SCQty', headerText: 'Sub Contract Qty'
            },
            {
                field: 'YRequiredDate', headerText: 'Required Date', visible: status == statusConstants.PENDING, type: 'date', format: _ch_date_format_1
            },
            {
                field: 'SubGroupID', headerText: 'SubGroupID', visible: false// status == statusConstants.PENDING
            },
            {
                field: 'SubGroupName', headerText: 'SubGroup Name', visible: false// status == statusConstants.PENDING
            },
            {
                field: 'KnittingType', headerText: 'Machine Type', visible: status == statusConstants.PENDING
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name', visible: status == statusConstants.PENDING
            },
            {
                field: 'Composition', headerText: 'Composition', visible: status == statusConstants.PENDING
            },
            {
                field: 'Gsm', headerText: 'GSM', visible: status == statusConstants.PENDING
            },
            {
                field: 'Length', headerText: 'Length', visible: status == statusConstants.PENDING
            },
            {
                field: 'Width', headerText: 'Width', visible: status == statusConstants.PENDING
            },
            {
                field: 'Color', headerText: 'Color', visible: status == statusConstants.PENDING
            },
            {
                field: 'MachineGauge', headerText: 'Machine Gauge', visible: status == statusConstants.PENDING
            },
            {
                field: 'MachineDia', headerText: 'Machine Dia', visible: status == statusConstants.PENDING
            },
            {
                field: 'ProgramName', headerText: 'Program', visible: status == statusConstants.PENDING
            }
        ];
        if (status == statusConstants.PENDING) {
            columns.splice(2, 0, {
                type: 'checkbox', width: 100, headerText: 'Select', visible: status == statusConstants.PENDING
            });
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            allowFiltering: true,
            apiEndPoint: `/api/knitting-sub-contract/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.KSCMasterID, args.rowData.SubContactID);
            $formEl.find("#btnAddItem").hide();
        }
        else if (args.commandColumn.type === "Report") {
            window.open(`/reports/InlinePdfView?ReportName=KnittingSubContract.rdl&KSCNo=${args.rowData.KSCNo}`, '_blank');
        }
    }

    function initChildTable(records) {

        if ($tblChildEl) $tblChildEl.destroy();
        ej.base.enableRipple(true);

        var childColumns = [
            { field: 'YBChildItemID', isPrimaryKey: true, visible: false },
            { field: 'YBChildID', visible: false },
            { field: 'AllocationChildItemID', visible: false },
            { field: 'YarnCategory', headerText: 'Yarn Details', allowEditing: false },
            { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false },
            { field: 'YarnLotNo', headerText: 'Yarn Lot', allowEditing: false },
            { field: 'YD', headerText: 'YD', displayAsCheckBox: true, editType: "booleanedit" },
            { field: 'BatchNo', headerText: 'Batch No', allowEditing: true },
            { field: 'Spinner', headerText: 'Spinner', allowEditing: false },
            { field: 'YarnPly', headerText: 'YarnPly', allowEditing: false },
            { field: 'StitchLength', headerText: 'Stitch Length', allowEditing: false },
            { field: 'Distribution', headerText: 'Distribution', allowEditing: true },
            { field: 'AllocatedQty', headerText: 'AllocatedQty', allowEditing: false },
            { field: 'NetYarnReqQty', headerText: 'Net Yarn Req Qty', allowEditing: false }
        ];

        $tblChildEl = new ej.grids.Grid({
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            commandClick: programhandleCommands,
            autofitColumns: true,
            allowResizing: true,
            enableContextMenu: true,
            enableSingleClickEdit: true,
            dataSource: records,
            columns: [
                {
                    headerText: '', width: 80, commands: [
                        {
                            type: 'Edit', title: 'Remove this row',
                            buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' }
                        }]
                },
                {
                    field: 'ConceptID', headerText: 'ConceptID', visible: false, isPrimaryKey: true
                },
                { field: 'ItemMasterID', visible: false, allowEditing: false},
                {
                    field: 'SubGroupID', headerText: 'SubGroupID', visible: false, allowEditing: false
                },
                {
                    field: 'CorBookingNo', width: 140, headerText: 'Concept/Booking No', allowEditing: false
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', allowEditing: false
                },
                {
                    field: 'Composition', headerText: 'Composition', visible: subGroupName == 'Fabric' ? true : false, allowEditing: false
                },
                {
                    field: 'FabricWidth', headerText: 'Fabric Width', allowEditing: false, visible: subGroupName == 'Fabric' ? true : false
                },
                {
                    field: 'Gsm', headerText: 'GSM', visible: subGroupName == 'Fabric' ? true : false, allowEditing: false
                },
                {
                    field: 'Length', headerText: 'Length', visible: subGroupName != 'Fabric' ? true : false, allowEditing: false
                },
                {
                    field: 'Width', headerText: 'Width', visible: subGroupName != 'Fabric' ? true : false, allowEditing: false
                },
                //{
                //    field: 'MachineGauge', headerText: 'Gauge', allowEditing: false, visible: subGroupName == 'Fabric' ? true : false
                //},
                //{
                //    field: 'MachineDia', headerText: 'Dia', allowEditing: false, visible: subGroupName == 'Fabric' ? true : false
                //},
                //{
                //    field: 'StitchLengthAll', headerText: 'StitchLength', allowEditing: false
                //},
                {
                    field: 'SubContactName', headerText: 'Sub Contractor', visible: false, allowEditing: false
                },
                ////{
                ////    field: 'YRequiredDate', headerText: 'Required Date', type: 'date', format: _ch_date_format_1, allowEditing: false
                ////},
                {
                    field: 'SubGroupName', headerText: 'SubGroup Name', visible: true, allowEditing: false
                },
                {
                    field: 'Color', headerText: 'Color', visible: true, allowEditing: false
                },
                {
                    field: 'BookingQty', headerText: 'Plan Qty', allowEditing: false
                },
                //{
                //    field: 'AllocatedQty', headerText: 'Allocated Qty', allowEditing: false
                //},
                {
                    field: 'SCQty', headerText: 'Sub Contract Qty', allowEditing: true
                },
                {
                    field: 'Rate', headerText: 'Rate', editType: 'numericedit', allowEditing: true,
                    textAlign: 'Right', edit: { params: { showSpinButton: false, decimals: 2, min: 0, format: "N2" } }
                },
                {
                    field: 'Cost', headerText: 'Cost', allowEditing: false
                },
                //{
                //    field: 'ProgramName', headerText: 'Program', allowEditing: false
                //},
                //{
                //    field: 'KnittingType', headerText: 'Machine Type', allowEditing: false
                //},

            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {
                    if (args.data.SCQty > args.data.BookingQty) {
                        toastr.error(`Sub Contract Qty ${args.data.SCQty} cannot be greater than Plan Qty ${args.data.BookingQty}`);
                        args.data.SCQty = 0;
                        return false;
                    }

                    args.data.Cost = args.data.SCQty * args.data.Rate;

                    var childItems = args.data.YBChildItems;
                    for (var i = 0; i < childItems.length; i++) {
                        childItems[i].NetYarnReqQty = ((parseFloat(childItems[i].PrimaryNetYarnReqQty) / 100) * ((parseFloat(args.data.SCQty) / parseFloat(args.data.BookingQty)) * 100)).toFixed(2);
                    }
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.data.ConceptID);
                    args.data.YBChildItems = childItems;
                    $tblChildEl.updateRow(index, args.data);
                }
                else if (args.requestType.toLowerCase() === "delete") {


                }
            },
            childGrid: {
                allowResizing: true,
                autofitColumns: false,
                queryString: "YBChildID",
                columns: childColumns,
                editSettings: {
                    allowEditing: true,
                    allowAdding: false,
                    allowDeleting: false,
                    mode: "Normal",
                    showDeleteConfirmDialog: false
                },
                load: loadYarnBookingChildItems
            },
        });
        $tblChildEl.appendTo(tblChildId);
        $tblChildEl.refresh();
    }
    function loadYarnBookingChildItems() {
        this.dataSource = this.parentDetails.parentRowData.YBChildItems;
    }

    function programhandleCommands(args) {
        var j = 0;
        while (j < $tblChildEl.dataSource.length) {
            if ($tblChildEl.dataSource[j].KPMasterID === args.rowData.KPMasterID) {
                $tblChildEl.dataSource.splice(j, 1);
            } else {
                ++j;
            }
        }
        initChildTable($tblChildEl.dataSource);
    }

    //function initChildTableItem(records) {

    //    if ($tblitemdtl) $tblitemdtl.destroy();
    //    ej.base.enableRipple(true);
    //    $tblitemdtl = new ej.grids.Grid({
    //        //editSettings: { showDeleteConfirmDialog: true, allowEditing: false, allowDeleting: false },
    //        allowResizing: true,
    //        dataSource: records,
    //        columns: [

    //            {
    //                field: 'YarnType', width: 140, headerText: 'Yarn Type', allowEditing: false
    //            },
    //            {
    //                field: 'YarnCount', headerText: 'Yarn Count', allowEditing: false
    //            },
    //            {
    //                field: 'YarnLotNo', headerText: 'Yarn Lot No', allowEditing: false
    //            },                
    //            {
    //                field: 'StitchLength', headerText: 'Stitch Length', allowEditing: false
    //            },
    //            {
    //                field: 'ReqQty', headerText: 'Req Qty(KG)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }
    //            },
    //            {
    //                field: 'ReqCone', headerText: 'Req Cone(PCS)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }
    //            }

    //        ]
    //    });
    //    $tblitemdtl.appendTo(itemdtlTblId);
    //}

    function IsMatchValidation(selectedRecords) {
        var isMatch = true;
        var uniqueAry;

        uniqueAry = distinctArrayByProperty(selectedRecords, "SubContactID");
        if (uniqueAry.length != 1) {
            toastr.error("Selected row(s) Sub Contractor should be same!");
            isMatch = false;
        }

        uniqueAry = distinctArrayByProperty(selectedRecords, "CompanyName");
        if (uniqueAry.length != 1) {
            toastr.error("Selected row(s) Unit should be same!");
            isMatch = false;
        }

        return isMatch;
    }

    function getProgramDataForSave() {
        actionBtnHideShow();
        var selectedRecords = $tblMasterEl.getSelectedRecords();

        var subGroupIDs = [];
        selectedRecords.map(x => {
            if (x.SubGroupID == 12) x.SubGroupID = 11;
            var indexF = subGroupIDs.findIndex(c => c == x.SubGroupID);
            if (indexF == -1) {
                subGroupIDs.push(x.SubGroupID);
            }
        });
        if (subGroupIDs.length > 1) {
            toastr.error("Select only fabric or collar & cuff");
            return false;
        }

        if (IsMatchValidation(selectedRecords) == false) {
            return;
        }

        $divDetailsEl.fadeIn();
        $divTblEl.fadeOut();
        $formEl.find("#btnAddItem").show();
        initChildTable([]);

        if (selectedRecords.length > 0) {
            var programIds = selectedRecords[0].KPChildID;
            //var programIds = selectedRecords[0].KPMasterID;

            var subGroupID = selectedRecords[0].SubGroupID;
            var SubContactID = selectedRecords[0].SubContactID;
            var NotMatch = 0;

            if (subGroupID == 1) {
                for (i = 1; i < selectedRecords.length; i++) {
                    if (selectedRecords[0].SubContactID == selectedRecords[i].SubContactID
                        && selectedRecords[0].CompanyName == selectedRecords[i].CompanyName
                    ) {
                        programIds = programIds + ',' + selectedRecords[i].KPChildID;
                    }
                }
            } else {
                for (i = 1; i < selectedRecords.length; i++) {
                    if (selectedRecords[0].SubContactID == selectedRecords[i].SubContactID
                        && selectedRecords[0].CompanyName == selectedRecords[i].CompanyName
                    ) {
                        programIds = programIds + ',' + selectedRecords[i].KPChildID;
                    }
                }
            }
            getProgramData(programIds, SubContactID);
        }
    }
    function getSubContactInfo() {
        var obj = {
            SubContactID: 0,
            SubContactName: ""
        };

        if (masterData.SubContactID > 0) obj.SubContactID = masterData.SubContactID;
        else if (masterData.Childs[0].SubContactID > 0) obj.SubContactID = masterData.Childs[0].SubContactID;

        if (getDefaultValueWhenInvalidS(masterData.SubContactName).length > 0) obj.SubContactName = masterData.SubContactName;
        else if (getDefaultValueWhenInvalidS(masterData.Childs[0].SubContactName).length > 0) obj.SubContactName = masterData.Childs[0].SubContactName;

        return obj;
    }

    function getProgramData(programIds, subContactID) {
        actionBtnHideShow();

        axios.get(`/api/knitting-sub-contract/new/${programIds}/${subContactID}`)
            .then(function (response) {

                masterData = response.data;
                masterData.KSCDate = formatDateToDefault(masterData.KSCDate);
                masterData.DeliveryStartDate = formatDateToDefault(masterData.DeliveryStartDate);
                masterData.DeliveryEndDate = formatDateToDefault(masterData.DeliveryEndDate);

                var scObj = getSubContactInfo();

                masterData.SubContactID = scObj.SubContactID; //masterData.Childs[0].ProgramName == "Bulk" ? 0 : masterData.SubContactID;
                masterData.SubContactName = scObj.SubContactName; // masterData.Childs[0].ProgramName == "Bulk" ? "" : masterData.Childs[0].SubContactName;

                subGroupName = masterData.Childs[0].SubGroupName;
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs);

                if (masterData.PaymentTermsID === 1) showHideLCSection(false);
                else showHideLCSection(true);

                if (masterData.PortofLoadingID === 105) showHideSupplierRegionSection(false);
                else showHideSupplierRegionSection(true);

                if (masterData.TypeOfLCID === 1) $formEl.find("#formGroupCreditDays").fadeOut();
                else $formEl.find("#formGroupCreditDays").fadeIn();
            })
            .catch(showResponseError);
    }

    function getPoupConceptOrBookingData() {
        //axios.get(`/api/knitting-sub-contract/CorBListForPopUp`)
        //var subContactID = 745;
        PopUpCorBList = [];
        axios.get(`/api/knitting-sub-contract/CorBListForPopUp`)
            .then(function (response) {
                PopUpCorBList = response.data.Childs;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
        //getPoupConceptOrBookingData();
        initChildTable([]);
        if ($tblChildEl) $tblChildEl.destroy();
        PopUpCorBList = [];
        $tblChildEl.dataSource = [];

        showHideLCSection(false);
        showHideSupplierRegionSection(false);
    }

    function showHideLCSection(show) {
        if (show) {
            $formEl.find("#formGroupTypeOfLcID").show();
            $formEl.find("#formGroupCalculationofTenure").show();
        }
        else {
            $formEl.find("#formGroupTypeOfLcID").hide();
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

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#KSCMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getDetails(id, subContactID) {
        actionBtnHideShow();
        axios.get(`/api/knitting-sub-contract/${id}/${subContactID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.KSCDate = formatDateToDefault(masterData.KSCDate);
                masterData.DeliveryStartDate = formatDateToDefault(masterData.DeliveryStartDate);
                masterData.DeliveryEndDate = formatDateToDefault(masterData.DeliveryEndDate);

                masterData.SubContactID = masterData.SubContactID;
                masterData.SubContactName = masterData.Childs[0].ProgramName == "Bulk" ? masterData.SubContactName : masterData.Childs[0].SubContactName;

                subGroupName = masterData.Childs[0].SubGroupName;
                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);

                if (masterData.PaymentTermsID === 1) showHideLCSection(false);
                else showHideLCSection(true);

                if (masterData.PortofLoadingID === 105) showHideSupplierRegionSection(false);
                else showHideSupplierRegionSection(true);

                if (masterData.TypeOfLCID === 1) $formEl.find("#formGroupCreditDays").fadeOut();
                else $formEl.find("#formGroupCreditDays").fadeIn();
            })
            .catch(showResponseError);
    }

    function save(isSendForApprove, isApprove, isReject, rejectReason) {
        var data = formDataToJson($formEl.serializeArray());

        if (isReject && rejectReason.length == 0) {
            toastr.error("Give reason for reject.");
            return false;
        }
        
        if (isSendForApprove) {
            if (new Date(data.DeliveryStartDate) > new Date(data.DeliveryEndDate)) {
                toastr.error(`Delivery Start Date ${data.DeliveryStartDate} cannot be less than Delivery End Date ${data.DeliveryEndDate}`);
                return false;
            }
        }

        data.IsSendForApproval = isSendForApprove;
        data.IsApprove = isApprove;
        data.IsReject = isReject;
        data.RejectReason = rejectReason;

        data.Childs = $tblChildEl.getCurrentViewRecords();
        if (data.Childs.length === 0) {
            return toastr.error("At least 1 Yarn items is required.");
        }
        else {
            for (var i = 0; i < data.Childs.length; i++) {
                if (data.Childs[i].SCQty <= 0) {
                    return toastr.error("Sub Contract Qty is required.");
                }
                if (data.Childs[i].Rate <= 0) {
                    return toastr.error("Rate is required.");
                }
            }
        }
        var hasError = false;
        for (var i = 0; i < data.Childs.length; i++) {
            for (var j = 0; j < data.Childs[i].YBChildItems.length; j++) {
                if (data.Childs[i].YBChildItems[j].IsBDS == 2 && data.Childs[i].YBChildItems[j].AllocatedQty == 0) {
                    toastr.error("Allocation pending.");
                    hasError = true;
                    break;
                }
            }
            if (hasError) break;
        }

        if (hasError) return false;

        if ($formEl.find("#TransShipmentAllow").is(':checked'))
            data.TransShipmentAllow = 1;
        else {
            data.TransShipmentAllow = 0;
        }

        axios.post("/api/knitting-sub-contract/save", data)
            .then(function () {
                if (isReject) {
                    toastr.success("Successfully Rejected.");
                } else {
                    toastr.success("Successfully Saved.");
                }
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function actionBtnHideShow() {
        $formEl.find(".btnAction").hide();
        if (status == statusConstants.PENDING || status == statusConstants.DRAFT) $formEl.find("#btnSave,#btnSaveAndSendForApproval").show();
        else if (isKSCApprove && status == statusConstants.PROPOSED_FOR_APPROVAL) $formEl.find("#btnAprrove,#btnReject").show();
    }
})();