(function () {
    var menuId, pageName;
    //var toolbarId;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl, tblMasterId,
        tblFabricChildId, tblCollarChildId, tblCuffChildId, tblYarnChildId, TblReasonId, $TblReasonEl,
        $tblChildFabricEl, $tblChildCollarEl, $tblChildCuffEl, $tblChildYarnEl, tblCreateCompositionId, $tblCreateCompositionEl;

    var status;
    var masterData;
    var maxCol = 999, vAdditionalBooking = 0;
    var isYarnBookingPage = false, isAcknowledgePage = false;
    var checkReasonItem;// = new Array();
    var reasonStatus = "Others";
    var fabYarnItem = null;
    var collarYarnItem = null;
    var compositionComponents = [];
    var isBlended = false;
    var isValidationNeed = false;
    //var cuffYarnItem = null;
    //var yarnAllList = [];

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblFabricChildId = "#tblFabric-" + pageId;
        tblCollarChildId = "#tblCollar-" + pageId;
        tblCuffChildId = "#tblCuff-" + pageId;
        tblYarnChildId = "#tblYarn-" + pageId;
        tblCreateCompositionId = `#tblCreateComposition-${pageId}`;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        TblReasonId = "#tblReason" + pageId;

        isYarnBookingPage = convertToBoolean($(`#${pageId}`).find("#YarnBookingPage").val());
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        IsShowElement("Load");
        if (isYarnBookingPage) {
            $toolbarEl.find("#btnPendingList,#btnCompleteList,#btnProposeList,#btnReviseList,#btnExecutedList,#btnAdditionalList,#btnStatusAndReport,#btnRefreshList").show();
            $toolbarEl.find("#btnNewAckList,#btnRevisionAckList,#btnAckList").hide();
            //status = statusConstants.PENDING;
            //toggleActiveToolbarBtn($toolbarEl.find("#btnPendingList"), $toolbarEl);

            status = statusConstants.COMPLETED;
            toggleActiveToolbarBtn($toolbarEl.find("#btnCompleteList"), $toolbarEl);

            $formEl.find("#btnSave,#btnSaveAndSend").show();
            $formEl.find("#btnAcknowledge").hide();
            initMasterTable();
        }
        else if (isAcknowledgePage) {
            $toolbarEl.find("#btnPendingList,#btnCompleteList,#btnProposeList,#btnReviseList,#btnExecutedList,#btnAdditionalList,#btnStatusAndReport,#btnRefreshList").hide();
            $toolbarEl.find("#btnNewAckList,#btnRevisionAckList,#btnAckList").show();
            status = statusConstants.AWAITING_PROPOSE;
            toggleActiveToolbarBtn($toolbarEl.find("#btnNewAckList"), $toolbarEl);
            $formEl.find("#btnSave,#btnSaveAndSend").hide();
            $formEl.find("#btnAcknowledge").show();
            initMasterTable();
        }

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            //IsShowElement("PENDING");
            initMasterTable();
        });

        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            //IsShowElement("COMPLETED");
            reasonStatus = "BookingList";
            initMasterTable();
        });

        $toolbarEl.find("#btnProposeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PROPOSED;
            //IsShowElement("PROPOSED");
            reasonStatus = "ProposeList";
            initMasterTable();
        });

        $toolbarEl.find("#btnReviseList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REVISE;
            //IsShowElement("REVISE");
            reasonStatus = "ReviseList";
            initMasterTable();
        });

        $toolbarEl.find("#btnExecutedList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.EXECUTED;
            //IsShowElement("EXECUTED");
            reasonStatus = "ExecutedList";
            initMasterTable();
        });

        $toolbarEl.find("#btnAdditionalList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ADDITIONAL;
            //IsShowElement("ADDITIONAL");
            reasonStatus = "AdditionalList";
            initMasterTable();
        });

        $toolbarEl.find("#btnStatusAndReport").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            //IsShowElement("REPORT");
            status = statusConstants.REPORT;
            initMasterTable();
        });
        $toolbarEl.find("#btnNewAckList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.AWAITING_PROPOSE;
            IsShowElement("AWAITING_PROPOSE");
            initMasterTable();
        });
        $toolbarEl.find("#btnRevisionAckList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REVISE;
            IsShowElement("REVISE");
            initMasterTable();
        });
        $toolbarEl.find("#btnAckList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            IsShowElement("ACKNOWLEDGE");
            initMasterTable();
        });

        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        //Button List

        $formEl.find("#btnShowSummary").click(function (e) {
            e.preventDefault();

            //$formEl.find("#btnShowSummary").html("Show Summary");
            getAllYarnList();

            //if ($(this).text() == "Show Summary") {
            //    getAllYarnList();
            //    $formEl.find("#btnShowSummary").html("Hide Summary");
            //} else {
            //    initYarnChildTableAsync([]);
            //    $formEl.find("#btnShowSummary").html("Show Summary");
            //}
        });

        //$formEl.find("#btnComposition").click(function (e) {
        //    //e.preventDefault();
        //    initTblCreateComposition();
        //    $pageEl.find(`#modal-new-composition-${pageId}`).modal("show");
        //});

        //$pageEl.find("#btnAddComposition").click(saveComposition);

        //$pageEl.find('input[type=radio][name=Blended]').change(function (e) {
        //    e.preventDefault();
        //    isBlended = convertToBoolean(this.value);
        //    initTblCreateComposition();
        //    return false;
        //});

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            if (isYarnBookingPage) {
                save(false, false, false);
            }
            else {
                save(false, false, true);
            }

        });

        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            //if (parseFloat(args.data.Allowance) > 35 || parseFloat(args.data.Allowance)<=0) {
            //    toastr.error("Allowance must be between 1 and 35");
            //    args.cancel = true;
            //    //return;
            //}

            if ($formEl.find('#AdditionalBooking').is(":checked")) {
                additionalsave(true);
            }
            else if ($formEl.find('#RevisionFlag').is(":checked")) {
                save(false, true, true)
            }
            if (status == statusConstants.REVISE) {
                save(false, true, true)
            }
            else if (status == statusConstants.EXECUTED) {
                save(true, true, true);
            }
            else {
                save(true, false, true);
            }
        });

        $formEl.find("#btnAdditionalSave").click(function (e) {
            e.preventDefault();
            additionalsave(false);
        });

        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            //saveRevise(false);
            save(false, true, true);
        });

        $formEl.find("#btnCancel").click(function (e) {
            e.preventDefault();
            var YBookingNo = $formEl.find("#YBookingNo").val();
            var CancelReasonID = $formEl.find("#CancelReasonID").val();
            axios.post(`/api/yarn-booking/cancelbooking/${YBookingNo}/${CancelReasonID}`)
                .then(function () {
                    toastr.success("Cancel operation successfull.");
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            var YBookingNo = $formEl.find("#YBookingNo").val();
            axios.post(`/api/yarn-booking/acknowledge/${YBookingNo}`)
                .then(function () {
                    toastr.success("Acknowledged operation successfull.");
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnBackToList").on("click", backToList);

        $formEl.find("#btnReason").click(function (e) {
            e.preventDefault();
            showReason();
        });

        $("#btnAddAvailablePOForPI").click(function (e) {
            e.preventDefault();
            checkReasonItem = $TblReasonEl.getSelectedRecords();
            $("#modal-reason").modal("hide");
        });

        $formEl.find('#ContactPersonID').on('select2:select', function (e) {

        });

        $formEl.find('#AdditionalBooking').click(function (e) {
            if ($formEl.find('#AdditionalBooking').is(":checked")) {
                $formEl.find("#divBtnReason").fadeIn();
                $formEl.find("#btnAdditionalSave,#btnSaveAndSend").show();
                $formEl.find("#btnSave,#btnCancel,#btnRevise,#btnAcknowledge").hide();
            }
            else {
                $formEl.find("#divBtnReason").fadeOut();
                $formEl.find("#btnSave,#btnSaveAndSend").show();
                $formEl.find("#btnAdditionalSave,#btnCancel,#btnRevise,#btnAcknowledge").hide();
            }
        });

        $formEl.find('#IsCancel').click(function (e) {
            if ($formEl.find('#IsCancel').is(":checked")) {
                $formEl.find("#divBtnReason").fadeIn();
                $formEl.find("#btnCancel").show();
                $formEl.find("#btnSave,#btnSaveAndSend,#btnAdditionalSave,#btnRevise,#btnAcknowledge").hide();
            }
            else {
                $formEl.find("#divBtnReason").fadeOut();
                $formEl.find("#btnSave,#btnSaveAndSend").show();
                $formEl.find("#btnAdditionalSave,#btnCancel,#btnRevise,#btnAcknowledge").hide();
            }
        });
        $formEl.find('#RevisionFlag').click(function (e) {
            if ($formEl.find('#RevisionFlag').is(":checked")) {
                $formEl.find("#divBtnReason").fadeIn();
                $formEl.find("#btnRevise,#btnSaveAndSend").show();
                $formEl.find("#btnSave,#btnCancel,#btnAdditionalSave,#btnAcknowledge").hide();
            } else {
                $formEl.find("#divBtnReason").fadeOut();
                $formEl.find("#btnSave,#btnSaveAndSend").show();
                $formEl.find("#btnAdditionalSave,#btnCancel,#btnRevise,#btnAcknowledge").hide();
            }
        });
        $formEl.find("#addYarnComposition").on("click", function (e) {
            showAddComposition();
        });
        $pageEl.find("#btnAddComposition").click(saveComposition);
        $pageEl.find('input[type=radio][name=Blended]').change(function (e) {
            e.preventDefault();
            isBlended = convertToBoolean(this.value);
            initTblCreateComposition();
            return false;
        });
    });

    function showAddComposition() {
        initTblCreateComposition();
        $pageEl.find(`#modal-new-composition-${pageId}`).modal("show");
    }

    function initTblCreateComposition() {
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
            {
                field: 'YarnSubProgramNew', headerText: 'Yarn Sub Program New', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.YarnSubProgramNews, field: "YarnSubProgramNew" })
            },
            {
                field: 'Certification', headerText: 'Certification', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.Certifications, field: "Certification" })
            },
            {
                field: 'Fiber', headerText: 'Component', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.FabricComponents, field: "Fiber" })
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
                else if (args.requestType === "save" && args.action === "edit") {
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

    function initMasterTable() {
        var commands;
        if (status === statusConstants.PENDING) {
            commands = [
                { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } },
                //{ type: 'Fabric Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                //{ type: 'View Attachment', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-image-o' } }
            ]
        }
        else if (status === statusConstants.REPORT) {
            commands = [
                //{ type: 'Fabric Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Yarn Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                //{ type: 'View Attachment', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-image-o' } }
            ]
        }
        else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                //{ type: 'Fabric Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'Yarn Booking Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                //{ type: 'View Attachment', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-image-o' } }
            ]
        }
        var columns = [
            {
                headerText: '', commands: commands, width: "100px", textAlign: 'Left'
            },
            {
                field: 'IsCompleteDelivery', headerText: 'IsCompleteDelivery', visible: false
            },
            {
                field: 'BookingNo', headerText: 'Booking No'//, visible: status === statusConstants.PENDING
            },
            {
                field: 'YBookingNo', headerText: 'Yarn Booking No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'YBookingDate', headerText: 'Yarn Booking Date', textAlign: 'center', type: 'date', format: _ch_date_format_1,
                visible: status !== statusConstants.PENDING
            },
            {
                field: 'ExportOrderNo', headerText: 'EWO No/SLNo'
            },
            {
                field: 'BuyerDepartment', headerText: 'Buyer Department'
            },
            {
                field: 'BuyerName', headerText: 'Buyer'
            },
            //{
            //    field: 'CompanyName', headerText: 'Company Name'
            //},
            {
                field: 'MerchandiserName', headerText: 'Merchandiser'
            },
            {
                field: 'YearName', headerText: 'Session', visible: status === statusConstants.PENDING
            },
            {
                field: 'TNADays', headerText: 'TNA'
            },
            {
                field: 'YRequiredDate', headerText: 'Yarn Required Date', textAlign: 'center', type: 'date',
                format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            {
                field: 'RevisionNo', headerText: 'Revision No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'AcknowledgeStatus', headerText: 'Acknowledge Status', visible: status !== statusConstants.PENDING
            },
            {
                field: 'IsCancel', headerText: 'Is Canceled?', displayAsCheckBox: true,
                textAlign: 'Center', visible: status !== statusConstants.PENDING
            },
            {
                field: 'ReferenceNo', headerText: 'Reference No', visible: status !== statusConstants.PENDING
            },
            {
                field: 'ContactPersonName', headerText: 'Contact Person', visible: status !== statusConstants.PENDING
            },
            {
                field: 'Depertment', headerText: 'Designation & Department', visible: status !== statusConstants.PENDING
            },
            {
                field: 'Remarks', headerText: 'Remarks', visible: status !== statusConstants.PENDING
            }
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/yarn-booking/list?status=${status}&PageName=${pageName}`,
            columns: columns,
            autofitColumns: status !== statusConstants.PENDING,
            commandClick: handleCommands,
            queryCellInfo: cellModifyForBDSAck
        });
    }

    function handleCommands(args) {
        //if (args.rowData.IsCompleteDelivery == 1) {
        //    toastr.error("Delivery Completed, please select another one.");
        //}

        if (args.commandColumn.type == 'Add') {
            getNew(args.rowData, statusConstants.PENDING);
        }
        else if (args.commandColumn.type === "Edit") {
            if (isYarnBookingPage) {
                if (status == statusConstants.REVISE) {
                    getNew(args.rowData, statusConstants.REVISE);
                }
                else {
                    getData(args.rowData.YBookingNo, args.rowData.WithoutOB, reasonStatus, args.rowData.YBookingID, args.rowData.BookingNo, args.rowData.ExportOrderNo);
                }
            } else {
                getData(args.rowData.YBookingNo, args.rowData.WithoutOB, reasonStatus, args.rowData.YBookingID, args.rowData.BookingNo, args.rowData.ExportOrderNo);
            }
        }
        else if (args.commandColumn.type === "Yarn Booking Report") {
            window.open(`/reports/InlinePdfView?ReportName=YarnBooking_New.rdl&YBookingNo=${args.rowData.YBookingNo}`, '_blank');
        }
        else if (args.commandColumn.type === "Fabric Booking Report") {
            window.open(`/reports/InlinePdfView?ReportName=BookingInformationFabricMainForPMCN.rdl&BookingNo=${args.rowData.BookingNo}`, '_blank');
        }
        else if (args.commandColumn.type == 'View Attachment') {
            if (args.rowData.ImagePath == '') {
                toastr.error("No attachment found!!");
            }
            else {
                var imagePath = constants.GMT_ERP_BASE_PATH + args.rowData.ImagePath;
                window.open(imagePath, "_blank");
            }
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

    async function initFabricChildTableAsync(data, subGroupName) {
        if ($tblChildFabricEl) $tblChildFabricEl.destroy();
        var columns = [], additionalColumns = [], childColumns = [];
        //YarnBookingChild grid load 
        columns = [
            {
                headerText: 'Commands', visible: vAdditionalBooking == 0 ? false : true, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            }
        ];
        columns.push.apply(columns, await getItemColumnsForDisplayBySubGroupAsync(subGroupName));

        additionalColumns = [
            { field: 'YBChildID', isPrimaryKey: true, visible: false },
            { field: 'YBookingID', visible: false },
            { field: 'ConsumptionID', visible: false },
            { field: 'ItemMasterID', isPrimaryKey: true, visible: false },
            { field: 'YarnTypeID', headerText: 'YarnTypeID', visible: false },
            { field: 'FUPartIDs', headerText: 'FUPartID', visible: false },
            { field: 'YarnBrandID', headerText: 'YarnBrandID', visible: false },
            { field: 'BookingUnitID', headerText: 'BookingUnitID', visible: false },
            { field: 'YarnSubBrandIDs', headerText: 'YarnSubBrandID', visible: false },
            { field: 'Remarks', headerText: 'Instructions', allowEditing: false },
            { field: 'YarnBrand', headerText: 'Yarn Program', allowEditing: false },
            //{ field: 'YarnSubBrandName', headerText: 'Yarn Sub Program', allowEditing: false },
            { field: 'A1Desc', headerText: 'Yarn Type', allowEditing: false },
            { field: 'PartName', headerText: 'Use In', allowEditing: false },
            { field: 'FTechnicalName', headerText: 'Technical Name' },
            { field: 'BookingQty', headerText: 'Booking Qty', allowEditing: vAdditionalBooking == 0 ? false : true },
            { field: 'BookingUOM', headerText: 'UOM', allowEditing: false }
        ];
        columns.push.apply(columns, additionalColumns);

        //YarnBookingChildItem grid load
        childColumns = await getChildColumns(data);
        $tblChildFabricEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            //toolbar: [{ text: 'Add Composition', tooltipText: 'Add Yarn Composition', prefixIcon: 'e-icons e-add', id: 'addYarnComposition' }],
            //handleToolbarClick: function (args) {
            //    
            //    if (args.item.id === "addYarnComposition") showAddComposition();
            //},
            editSettings: { allowEditing: false, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            childGrid: {
                queryString: "YBChildID",
                additionalQueryParams: "BookingID",
                allowResizing: true,
                autofitColumns: false,
                editSettings: {
                    allowEditing: true,
                    allowAdding: false,
                    allowDeleting: false,
                    mode: "Normal",
                    showDeleteConfirmDialog: true
                },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        //getAllYarnList();
                    }
                    else if (args.requestType === "add") {
                        args.data.YBChildItemID = maxCol++; //getMaxIdForArray(masterData.Childs, "YBChildItemID");
                        args.data.YBChildID = this.parentDetails.parentKeyFieldValue;

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


                        //args.data.Distribution = "100";
                        //args.data.Allowance = 0;
                        //var netConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(args.data.Distribution)) / 100;
                        //var reqQty = (parseFloat(this.parentDetails.parentRowData.BookingQty) * (parseFloat(args.data.Distribution) + parseFloat(args.data.Allowance))) / 100;
                        //args.data.BookingQty = netConsumption.toFixed(4);
                        //args.data.RequiredQty = reqQty.toFixed(2);

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
                        var index = $tblChildFabricEl.getRowIndexByPrimaryKey(args.rowData.YBChildItemID);

                        var NetConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(args.data.Distribution) / 100);
                        var reqQty = parseFloat(NetConsumption) + ((parseFloat(NetConsumption) * parseFloat(args.data.Allowance)) / 100);

                        //args.data.Distribution = args.rowData.Distribution;
                        args.data.YarnSubBrandIDs = args.rowData.YarnSubBrandIDs;
                        args.data.YBChildID = this.parentDetails.parentKeyFieldValue;
                        args.data.BookingQty = NetConsumption.toFixed(4);
                        args.data.RequiredQty = reqQty.toFixed(2);

                        args.rowData.Segment1ValueId = !args.data.Segment1ValueId ? 0 : args.data.Segment1ValueId;
                        args.rowData.Segment2ValueId = !args.data.Segment2ValueId ? 0 : args.data.Segment2ValueId;
                        args.rowData.Segment3ValueId = !args.data.Segment3ValueId ? 0 : args.data.Segment3ValueId;
                        args.rowData.Segment4ValueId = !args.data.Segment4ValueId ? 0 : args.data.Segment4ValueId;
                        args.rowData.Segment5ValueId = !args.data.Segment5ValueId ? 0 : args.data.Segment5ValueId;
                        args.rowData.Segment6ValueId = !args.data.Segment6ValueId ? 0 : args.data.Segment6ValueId;
                        args.rowData.Segment7ValueId = !args.data.Segment7ValueId ? 0 : args.data.Segment7ValueId;
                        args.rowData.Segment8ValueId = !args.data.Segment8ValueId ? 0 : args.data.Segment8ValueId;

                        ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }

                        //if (parseFloat(args.data.Allowance) > 35 || parseFloat(args.data.Allowance)<=0) {
                        //    toastr.error("Allowance must be between 1 and 35");
                        //    args.cancel = true;
                        //    //return;
                        //}
                        //getAllYarnList();
                    }
                    else if (args.requestType === "delete") {
                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        //getAllYarnList();
                    }
                },
                load: loadFLChildGridFabric
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }
            ],
            contextMenuClick: function (args) {
                //
                if (args.item.id === 'copy') {
                    fabYarnItem = objectCopy(args.rowInfo.rowData.ChildItems);
                    //
                    if (fabYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (fabYarnItem == null) {
                        toastr.error("Please copy first!!");
                        return;
                    } else if (fabYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < fabYarnItem.length; i++) {
                            fabYarnItem[i].YBChildItemID = maxCol++;
                            fabYarnItem[i].YBChildID = args.rowInfo.rowData.YBChildID;
                            var netConsumption = (parseFloat(args.rowInfo.rowData.BookingQty) * parseFloat(fabYarnItem[i].Distribution) / 100);
                            var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(fabYarnItem[i].Allowance)) / 100);
                            fabYarnItem[i].BookingQty = netConsumption.toFixed(4);
                            fabYarnItem[i].RequiredQty = reqQty.toFixed(2);
                            args.rowInfo.rowData.ChildItems.push(JSON.parse(JSON.stringify(fabYarnItem[i])));
                        }
                        $tblChildFabricEl.refresh();
                    }
                }
            }
        });
        $tblChildFabricEl.refreshColumns;
        $tblChildFabricEl.appendTo(tblFabricChildId);
    }
    function loadFLChildGridFabric() {
        this.dataSource = this.parentDetails.parentRowData.ChildItems;
    }
    function resizeColumns(childColumns) {
        var cAry = ["Segment1ValueId", "ShadeCode", "YarnSubBrandName", "Remarks", "Specification", "YDItem", "YD", "Distribution", "BookingQty", "Allowance", "RequiredQty", "DisplayUnitDesc"];
        cAry.map(c => {
            var indexF = childColumns.findIndex(x => x.field == c);
            var widthValue = 80;
            if (c == "Segment1ValueId") widthValue = 180;
            if (indexF > -1) childColumns[indexF].width = widthValue;
        });
        return childColumns;
    }
    //function showAddComposition() {
    //    initTblCreateComposition();
    //    $pageEl.find(`#modal-new-composition-${pageId}`).modal("show");
    //}
    //function initTblCreateComposition() {
    //    var compositionComponents = [];
    //    var columns = [
    //        {
    //            field: 'Id', isPrimaryKey: true, visible: false
    //        },
    //        {
    //            headerText: '', width: 100, commands: [
    //                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
    //                { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
    //                { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
    //                { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
    //        },
    //        {
    //            field: 'Percent', headerText: 'Percent(%)', width: 120, editType: "numericedit", params: { decimals: 0, format: "N", min: 1, validateDecimalOnType: true }, allowEditing: isBlended
    //        }
    //        ,
    //        {
    //            field: 'YarnSubProgramNew', headerText: 'Yarn Sub Program New', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.YarnSubProgramNews, field: "YarnSubProgramNew" })
    //        },
    //        {
    //            field: 'Certification', headerText: 'Certification', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.Certifications, field: "Certification" })
    //        },
    //        {
    //            field: 'Fiber', headerText: 'Component', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.FabricComponents, field: "Fiber" })
    //        }
    //    ];

    //    var gridOptions = {
    //        tableId: tblCreateCompositionId,
    //        data: compositionComponents,
    //        columns: columns,
    //        actionBegin: function (args) {
    //            if (args.requestType === "add") {
    //                if (isBlended) {
    //                    if (compositionComponents.length === 5) {
    //                        toastr.info("You can only add 5 components.");
    //                        args.cancel = true;
    //                        return;
    //                    }
    //                }
    //                else {
    //                    if (compositionComponents.length === 1) {
    //                        toastr.info("You can only add 1 component.");
    //                        args.cancel = true;
    //                        return;
    //                    }
    //                    else args.data.Percent = 100;
    //                }

    //                args.data.Id = getMaxIdForArray(compositionComponents, "Id");
    //            }
    //            else if (args.requestType === "save" && args.action === "edit") {
    //                if (!args.data.Fiber) {
    //                    toastr.warning("Fabric component is required.");
    //                    args.cancel = true;
    //                    return;
    //                }
    //                else if (!args.data.Percent || args.data.Percent <= 0 || args.data.Percent > 100) {
    //                    toastr.warning("Composition percent must be greater than 0 and less than or equal 100.");
    //                    args.cancel = true;
    //                    return;
    //                }
    //            }
    //        },
    //        autofitColumns: false,
    //        showDefaultToolbar: false,
    //        allowFiltering: false,
    //        allowPaging: false,
    //        toolbar: ['Add'],
    //        editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
    //    };
    //    if ($tblCreateCompositionEl) $tblCreateCompositionEl.destroy();
    //    $tblCreateCompositionEl = new initEJ2Grid(gridOptions);
    //}
    //function saveComposition() {
    //    var totalPercent = sumOfArrayItem(compositionComponents, "Percent");
    //    if (totalPercent != 100) return toastr.error("Sum of compostion percent must be 100");
    //    compositionComponents.reverse();

    //    var composition = "";
    //    compositionComponents = _.sortBy(compositionComponents, "Percent").reverse();
    //    compositionComponents.forEach(function (component) {
    //        composition += composition ? ` ${component.Percent}%` : `${component.Percent}%`;
    //        if (component.YarnSubProgramNew) {
    //            if (component.YarnSubProgramNew != 'N/A') {
    //                composition += ` ${component.YarnSubProgramNew}`;
    //            }
    //        }
    //        if (component.Certification) composition += ` ${component.Certification}`;
    //        composition += ` ${component.Fiber}`;
    //    });
    //    var data = {
    //        SegmentValue: composition
    //    };

    //    axios.post("/api/rnd-free-concept-mr/save-yarn-composition", data)
    //        .then(function () {
    //            toastr.success("Composition added successfully.");
    //            initTblCreateComposition();
    //            //$pageEl.find(`#modal-new-composition-${pageId}`).modal("hide");
    //            //masterData.CompositionList.unshift({ id: response.data.Id, text: response.data.SegmentValue });
    //            //initChildTable(masterData.Childs);
    //        })
    //        .catch(showResponseError)
    //}
    async function initCollarChildTableAsync(data, subGroupName) {
        if ($tblChildCollarEl) $tblChildCollarEl.destroy();

        var columns = [], addCollarColumns = [], additionalColumns = [], childColumns = [];

        //YarnBookingChild grid load 
        columns = [
            {
                headerText: 'Commands', visible: vAdditionalBooking == 0 ? false : true, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            }
        ];
        addCollarColumns = [
            { field: 'YBChildID', isPrimaryKey: true, visible: false },
            { field: 'YBChildGroupID', visible: false },
            { field: 'ItemMasterID', visible: false },
            { field: 'Segment1ValueDesc', headerText: 'Collar Description', allowEditing: false },
            { field: 'Segment2ValueDesc', headerText: 'Collar Type', allowEditing: false },
            { field: 'Segment5ValueDesc', headerText: 'Body Color', allowEditing: false },
            { field: 'Segment6ValueDesc', headerText: 'Body Measurement', allowEditing: false },
            { field: 'YarnType', headerText: 'Yarn Type', allowEditing: false },
            { field: 'YarnBrand', headerText: 'Yarn Program', allowEditing: false },
            //{ field: 'YarnSubBrandName', headerText: 'Yarn Sub Program', allowEditing: false },
            { field: 'Remarks', headerText: 'Instruction', allowEditing: false },
            { field: 'BookingQty', headerText: 'Booking Qty', params: { decimals: 0, format: "N2" }, allowEditing: vAdditionalBooking == 0 ? false : true },
            { field: 'BookingUOM', headerText: 'UOM', allowEditing: false }
        ];
        columns.push.apply(columns, addCollarColumns);

        //YarnBookingChildItem grid load
        childColumns = await getChildColumns(data);


        $tblChildCollarEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            childGrid: {
                queryString: "YBChildID",
                additionalQueryParams: "BookingID",
                allowResizing: true,
                autofitColumns: false,
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything..");
                            args.cancel = true;
                        }
                        //getAllYarnList();
                    }
                    else if (args.requestType === "add") {
                        args.data.YBChildItemID = maxCol++; //getMaxIdForArray(masterData.Childs, "YBChildItemID");
                        args.data.YBChildID = this.parentDetails.parentKeyFieldValue;

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

                        //args.data.Distribution = "100";
                        //args.data.Allowance = 0;
                        //var netConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(args.data.Distribution)) / 100;
                        //var reqQty = (parseFloat(this.parentDetails.parentRowData.BookingQty) * (parseFloat(args.data.Distribution) + parseFloat(args.data.Allowance))) / 100;
                        //args.data.BookingQty = netConsumption.toFixed(4);
                        //args.data.RequiredQty = reqQty.toFixed(2);

                        args.data.DisplayUnitDesc = "Kg";
                        args.data.SubGroupId = 11;

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

                        var index = $tblChildCollarEl.getRowIndexByPrimaryKey(args.rowData.YBChildItemID);

                        var NetConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(args.data.Distribution) / 100);
                        var reqQty = parseFloat(NetConsumption) + ((parseFloat(NetConsumption) * parseFloat(args.data.Allowance)) / 100);

                        args.data.YarnSubBrandIDs = args.rowData.YarnSubBrandIDs;
                        args.data.YBChildID = this.parentDetails.parentKeyFieldValue;
                        args.data.BookingQty = NetConsumption.toFixed(4);
                        args.data.RequiredQty = reqQty.toFixed(2);

                        args.rowData.Segment1ValueId = !args.data.Segment1ValueId ? 0 : args.data.Segment1ValueId;
                        args.rowData.Segment2ValueId = !args.data.Segment2ValueId ? 0 : args.data.Segment2ValueId;
                        args.rowData.Segment3ValueId = !args.data.Segment3ValueId ? 0 : args.data.Segment3ValueId;
                        args.rowData.Segment4ValueId = !args.data.Segment4ValueId ? 0 : args.data.Segment4ValueId;
                        args.rowData.Segment5ValueId = !args.data.Segment5ValueId ? 0 : args.data.Segment5ValueId;
                        args.rowData.Segment6ValueId = !args.data.Segment6ValueId ? 0 : args.data.Segment6ValueId;
                        args.rowData.Segment7ValueId = !args.data.Segment7ValueId ? 0 : args.data.Segment7ValueId;
                        args.rowData.Segment8ValueId = !args.data.Segment8ValueId ? 0 : args.data.Segment8ValueId;

                        ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;


                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }
                        //getAllYarnList();
                    }
                    else if (args.requestType === "delete") {
                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        //getAllYarnList();
                    }
                },
                load: loadFLChildGridCollar
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }
            ],
            contextMenuClick: function (args) {
                //
                if (args.item.id === 'copy') {
                    collarYarnItem = objectCopy(args.rowInfo.rowData.ChildItemsGroup);
                    //
                    if (collarYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (collarYarnItem == null) {
                        toastr.error("Please copy first!!");
                        return;
                    } else if (collarYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < collarYarnItem.length; i++) {
                            collarYarnItem[i].YBChildItemID = maxCol++;
                            collarYarnItem[i].YBChildID = args.rowInfo.rowData.YBChildID;
                            var netConsumption = (parseFloat(args.rowInfo.rowData.BookingQty) * parseFloat(collarYarnItem[i].Distribution) / 100);
                            var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(collarYarnItem[i].Allowance)) / 100);
                            collarYarnItem[i].BookingQty = netConsumption.toFixed(4);
                            collarYarnItem[i].RequiredQty = reqQty.toFixed(2);
                            args.rowInfo.rowData.ChildItemsGroup.push(JSON.parse(JSON.stringify(collarYarnItem[i])));
                        }
                        $tblChildCollarEl.refresh();
                    }
                }
            }
        });
        $tblChildCollarEl.refreshColumns;
        $tblChildCollarEl.appendTo(tblCollarChildId);
    }

    function loadFLChildGridCollar() {
        this.dataSource = this.parentDetails.parentRowData.ChildItemsGroup;
        //console.log('Collar Item');
        //console.log(this.parentDetails.parentRowData.ChildItemsGroup)
    }

    async function getChildColumns(data) {
        childColumns = await getYarnItemColumnsAsync(data, false);
        childColumns.unshift({ field: 'YBChildItemID', isPrimaryKey: true, visible: false });
        additionalColumns = [
            { field: 'YBChildGroupID', visible: false },
            { field: 'SubGroupId', visible: false },
            {
                field: 'ShadeCode',
                headerText: 'Shade Code',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: data[0].YarnShadeBooks,
                displayField: "ShadeCode",
                allowEditing: false,
                edit: ej2GridDropDownObj({
                })
            },
            { field: 'YarnCategory', headerText: 'Yarn Category', allowEditing: false, width: 350 },
            {
                field: 'YarnSubBrandName', headerText: 'Yarn Sub Program', allowEditing: false, edit: ej2GridMultipleDropDownObj({
                    dataSource: masterData.YarnSubBrandList,
                    displayField: "YarnSubBrandIDs",
                    valueFieldName: "YarnSubBrandIDs",
                    onChange: function (selectedData, currentRowData) {
                        currentChildRowData = currentRowData;
                    }
                })
            },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: false },
            { field: 'Specification', headerText: 'Yarn Specification', allowEditing: false },
            { field: 'YDItem', headerText: 'YD Item?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', allowEditing: false },
            { field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', allowEditing: false },
            { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false },
            { field: 'BatchNo', headerText: 'Batch No', allowEditing: false },
            { field: 'YarnLotNo', headerText: 'Lot No', allowEditing: false },
            {
                field: 'SpinnerId',
                headerText: 'Spinner',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.Spinners,
                displayField: "SpinnerName",
                allowEditing: false,
                edit: ej2GridDropDownObj({
                })
            },
            { field: 'StitchLength', headerText: 'Stitch Length', allowEditing: false, editType: "numericedit", params: { decimals: 0, format: "N", min: 0, validateDecimalOnType: true } },
            { field: 'Distribution', headerText: 'Yarn Distribution (%)', allowEditing: false, editType: "numericedit", params: { decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } },
            { field: 'BookingQty', headerText: 'Net Consumption', allowEditing: false, params: { decimals: 0, format: "N2" } },
            { field: 'Allowance', headerText: 'Allowance (%)', allowEditing: true, editType: "numericedit", params: { decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } },
            { field: 'RequiredQty', headerText: 'RequiredQty', allowEditing: false, params: { decimals: 0, format: "N2" } },
            { field: 'DisplayUnitDesc', headerText: 'UOM', allowEditing: false }
        ];
        childColumns.push.apply(childColumns, additionalColumns);
        childColumns = resizeColumns(childColumns);
        ej.base.enableRipple(true);

        return childColumns;
    }

    async function initCuffChildTableAsync(data, subGroupName) {
        if ($tblChildCuffEl) $tblChildCuffEl.destroy();

        var columns = [], addCuffColumns = [], additionalColumns = [], childColumns = [];

        //YarnBookingChild grid load 
        columns = [
            {
                headerText: '', visible: vAdditionalBooking == 0 ? false : true, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            }
        ];
        addCuffColumns = [
            { field: 'YBChildID', isPrimaryKey: true, visible: false },
            { field: 'YBChildGroupID', visible: false },
            { field: 'ItemMasterID', visible: false },
            { field: 'Segment1ValueDesc', headerText: 'Cuff/Hem Description', allowEditing: false },
            { field: 'Segment2ValueDesc', headerText: 'Cuff/Hem Type', allowEditing: false },
            { field: 'Segment5ValueDesc', headerText: 'Body Color', allowEditing: false },
            { field: 'Segment6ValueDesc', headerText: 'Body Measurement(cm)', allowEditing: false },
            { field: 'YarnType', headerText: 'Yarn Type', allowEditing: false },
            { field: 'YarnBrand', headerText: 'Yarn Program', allowEditing: false },
            //{ field: 'YarnSubBrandName', headerText: 'Yarn Sub Program', allowEditing: false },
            { field: 'Remarks', headerText: 'Instruction', allowEditing: false },
            { field: 'BookingQty', headerText: 'Booking Qty', params: { decimals: 0, format: "N2" }, allowEditing: vAdditionalBooking == 0 ? false : true },
            { field: 'BookingUOM', headerText: 'UOM', allowEditing: false }
        ];
        columns.push.apply(columns, addCuffColumns);

        //YarnBookingChildItem grid load
        childColumns = await getChildColumns(data);

        $tblChildCuffEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
            childGrid: {
                queryString: "YBChildID",
                additionalQueryParams: "BookingID",
                allowResizing: true,
                autofitColumns: false,
                editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        //getAllYarnList();
                    }
                    else if (args.requestType === "add") {
                        args.data.YBChildItemID = maxCol++; //getMaxIdForArray(masterData.Childs, "YBChildItemID");
                        args.data.YBChildID = this.parentDetails.parentKeyFieldValue;

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

                        //args.data.Distribution = "100";
                        //args.data.Allowance = 0;
                        //var netConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(args.data.Distribution)) / 100;
                        //var reqQty = (parseFloat(this.parentDetails.parentRowData.BookingQty) * (parseFloat(args.data.Distribution) + parseFloat(args.data.Allowance))) / 100;
                        //args.data.BookingQty = netConsumption.toFixed(4);
                        //args.data.RequiredQty = reqQty.toFixed(2);


                        args.data.DisplayUnitDesc = "Kg";
                        args.data.SubGroupId = 12;

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
                        
                        var index = $tblChildCuffEl.getRowIndexByPrimaryKey(args.rowData.YBChildItemID);

                        var NetConsumption = (parseFloat(this.parentDetails.parentRowData.BookingQty) * parseFloat(args.data.Distribution) / 100);
                        var reqQty = parseFloat(NetConsumption) + ((parseFloat(NetConsumption) * parseFloat(args.data.Allowance)) / 100);

                        args.data.YarnSubBrandIDs = args.rowData.YarnSubBrandIDs;
                        args.data.YBChildID = this.parentDetails.parentKeyFieldValue;
                        args.data.BookingQty = NetConsumption.toFixed(4);
                        args.data.RequiredQty = reqQty.toFixed(2);

                        args.rowData.Segment1ValueId = !args.data.Segment1ValueId ? 0 : args.data.Segment1ValueId;
                        args.rowData.Segment2ValueId = !args.data.Segment2ValueId ? 0 : args.data.Segment2ValueId;
                        args.rowData.Segment3ValueId = !args.data.Segment3ValueId ? 0 : args.data.Segment3ValueId;
                        args.rowData.Segment4ValueId = !args.data.Segment4ValueId ? 0 : args.data.Segment4ValueId;
                        args.rowData.Segment5ValueId = !args.data.Segment5ValueId ? 0 : args.data.Segment5ValueId;
                        args.rowData.Segment6ValueId = !args.data.Segment6ValueId ? 0 : args.data.Segment6ValueId;
                        args.rowData.Segment7ValueId = !args.data.Segment7ValueId ? 0 : args.data.Segment7ValueId;
                        args.rowData.Segment8ValueId = !args.data.Segment8ValueId ? 0 : args.data.Segment8ValueId;

                        ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }
                        //getAllYarnList();
                    }
                    else if (args.requestType === "delete") {
                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        //getAllYarnList();
                    }
                },
                load: loadFLChildGridCuff
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }
            ],
            contextMenuClick: function (args) {
                //
                if (args.item.id === 'copy') {
                    //cuffYarnItem = objectCopy(args.rowInfo.rowData.ChildItemsGroup);
                    collarYarnItem = objectCopy(args.rowInfo.rowData.ChildItemsGroup);
                    //
                    if (collarYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (collarYarnItem == null) {
                        toastr.error("Please copy first!!");
                        return;
                    } else if (collarYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < collarYarnItem.length; i++) {
                            collarYarnItem[i].YBChildItemID = maxCol++;
                            collarYarnItem[i].YBChildID = args.rowInfo.rowData.YBChildID;
                            var netConsumption = (parseFloat(args.rowInfo.rowData.BookingQty) * parseFloat(collarYarnItem[i].Distribution) / 100);
                            var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(collarYarnItem[i].Allowance)) / 100);
                            collarYarnItem[i].BookingQty = netConsumption.toFixed(4);
                            collarYarnItem[i].RequiredQty = reqQty.toFixed(2);
                            args.rowInfo.rowData.ChildItemsGroup.push(JSON.parse(JSON.stringify(collarYarnItem[i])));
                        }
                        $tblChildCuffEl.refresh();
                    }
                }
            }
        });
        $tblChildCuffEl.refreshColumns;
        $tblChildCuffEl.appendTo(tblCuffChildId);
    }

    function loadFLChildGridCuff() {
        //alert(10)
        //debugger
        //console.log(this.parentDetails.parentRowData.ChildItemsGroup);
        this.dataSource = this.parentDetails.parentRowData.ChildItemsGroup;
        //console.log('Cuff Item');
        //console.log(this.parentDetails.parentRowData.ChildItemsGroup)
    }
    //220723-Anis
    async function initYarnChildTableAsync(data) {
        if ($tblChildYarnEl) $tblChildYarnEl.destroy();

        var columns = [], addYarnColumns = [], additionalColumns = [], childColumns = [];

        columns = await getYarnItemColumnsAsync(data, true);
        columns.shift();
        addYarnColumns = [
            { field: 'YBChildID', isPrimaryKey: true, visible: false },
            { field: 'YBChildGroupID', visible: false },
            { field: 'ItemMasterID', visible: false },
            { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false },
            { field: 'Specification', headerText: 'Yarn Specification', allowEditing: false },
            { field: 'RequiredQty', headerText: 'Required Qty', params: { decimals: 0, format: "N2" }, allowEditing: vAdditionalBooking == 0 ? false : true },
            { field: 'DisplayUnitDesc', headerText: 'UOM', allowEditing: false },
            {
                field: 'YarnSubBrandName', headerText: 'Yarn Sub Program', edit: ej2GridMultipleDropDownObj({
                    dataSource: masterData.YarnSubBrandList,
                    displayField: "YarnSubBrandIDs",
                    valueFieldName: "YarnSubBrandIDs",
                    onChange: function (selectedData, currentRowData) {
                        currentChildRowData = currentRowData;
                    }
                })
            },
            { field: 'YD', headerText: 'YD', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: false }

            //{ field: 'YarnCategory', headerText: 'Yarn Category', allowEditing: false },
            //{ field: '', headerText: 'Yarn Color', allowEditing: false },
        ];
        columns.push.apply(columns, addYarnColumns);
        columns = resizeColumns(columns);
        ej.base.enableRipple(true);
        $tblChildYarnEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                  
                }
                else if (args.requestType === "save") {
                  
                    ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                    //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                    //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                    //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                    //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                    //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                    //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                    //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;
                  
                }
            },
            editSettings: { allowEditing: false, allowAdding: true, allowDeleting: false, mode: "Normal", showDeleteConfirmDialog: true },
        });
        $tblChildYarnEl.refreshColumns;
        $tblChildYarnEl.appendTo(tblYarnChildId);
    }
    //220723-anis-End


    function initReasonTable(records) {
        if ($TblReasonEl) $TblReasonEl.destroy();
        ej.base.enableRipple(true);
        $TblReasonEl = new ej.grids.Grid({
            editSettings: { showDeleteConfirmDialog: true, allowEditing: true, allowDeleting: true },
            allowResizing: true,
            dataSource: records,
            columns: [
                { field: 'YBKReasonID', isPrimaryKey: true, visible: false },
                {
                    field: 'YBookingID', visible: false
                },
                { field: 'ReasonID', visible: false },
                {
                    field: "Selected", type: 'checkbox', displayAsCheckBox: true, editType: "booleanedit", width: 60, headerText: 'Select'
                },
                {
                    field: 'ReasonName', headerText: 'Reason', width: 100, allowEditing: false
                },
                {
                    field: 'Remarks', headerText: 'Remarks', allowEditing: false
                }
            ]
        });
        $TblReasonEl.appendTo(TblReasonId);
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        $tblMasterEl.refresh();

        $formEl.find(tblFabricChildId).html("");
        $formEl.find(tblCollarChildId).html("");
        $formEl.find(tblCuffChildId).html("");
        IsShowElement("Load");
        initYarnChildTableAsync([]);
        //$formEl.find("#divYarnInfo").hide();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#YBookingID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(rowData, status) {
        var vYBookingNo = "", vYBookingID = 0;
        if (status == statusConstants.PENDING) {
            vYBookingNo = 0;
            vYBookingID = 0;
        } else {
            vYBookingNo = rowData.YBookingNo;
            vYBookingID = rowData.YBookingID;
        }

        var url = `/api/yarn-booking/new/${rowData.WithoutOB}/${rowData.BookingNo}/${vYBookingNo}/${reasonStatus}/${vYBookingID}/${status}`;
        axios.get(url)
            .then(function (response) {
                masterData = response.data;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData.HasFabric = response.data.HasFabric;
                masterData.HasCollar = response.data.HasCollar;
                masterData.HasCuff = response.data.HasCuff;
                masterData.Childs = response.data.Childs;
                masterData.FiberPartList = response.data.FiberPartList;
                masterData.RequiredDate = formatDateToDefault(masterData.RequiredDate);
                masterData.YBookingDate = formatDateToDefault(masterData.YBookingDate);
                masterData.YRequiredDate = formatDateToDefault(masterData.YRequiredDate ?? masterData.YBookingDate);
                masterData.RevisionNo = response.data.RevisionNo;

                //Set Filter Parameter for details data load
                masterData.F_isSample = rowData.WithoutOB;
                masterData.F_BookingNo = rowData.BookingNo;
                masterData.F_YBookingNo = vYBookingNo;
                masterData.F_ReasonStatus = reasonStatus;
                masterData.F_YBookingID = vYBookingID;
                masterData.F_Status = status;

                setFormData($formEl, masterData);
                initReasonTable(masterData.yarnBookingReason);
                /*console.log(masterData.yarnBookingReason);*/
                $("#ExportOrderNo").text(masterData.ExportOrderNo);

                if (masterData.HasFabric) {
                    /*var FabricData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.FABRIC });*/
                    var FabricData = masterData.ChildsGroup.filter(function (el) { return el.SubGroupName == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }

                    initFabricChildTableAsync(FabricData, subGroupNames.FABRIC);
                    $formEl.find("#divFabricInfo").show();
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                }

                if (masterData.HasCollar) {
                    /*var CollarData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.COLLAR });*/
                    var CollarData = masterData.ChildsGroup.filter(function (el) { return el.SubGroupName == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCollarChildTableAsync(CollarData, subGroupNames.COLLAR);

                    //Separated YarnSubBrandID
                    //var objYarn = new Array();
                    //$.each(CollarData, function (i, obj) {
                    //    $.each(obj.ChildItemsGroup, function (j, objChild) {
                    //        objChild.YarnSubBrandIDs = objChild.YarnSubBrandID.split(',');
                    //    });
                    //});

                    $formEl.find("#divCollarInfo").show();
                }
                else {
                    $formEl.find("#divCollarInfo").hide();
                }

                if (masterData.HasCuff) {
                    /*var CuffData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.CUFF });*/
                    var CuffData = masterData.ChildsGroup.filter(function (el) { return el.SubGroupName == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    //Separated YarnSubBrandID
                    //var objYarn = new Array();
                    //$.each(CuffData, function (i, obj) {
                    //    $.each(obj.ChildItemsGroup, function (j, objChild) {  
                    //        objChild.YarnSubBrandIDs = objChild.YarnSubBrandID.split(','); 
                    //    });
                    //}); 
                    initCuffChildTableAsync(CuffData, subGroupNames.CUFF)
                    $formEl.find("#divCufInfo").show();
                    $formEl.find("#divYarnInfo").show();

                    //console.log(objYarn);
                }
                else {
                    $formEl.find("#divCufInfo").hide();
                    $formEl.find("#divYarnInfo").show();
                }

                //Status wise element show
                if (status == statusConstants.PENDING) {
                    IsShowElement("PENDING");
                }
                else if (status == statusConstants.COMPLETED) {
                    IsShowElement("COMPLETED");
                }
                else if (status == statusConstants.PROPOSED) {
                    IsShowElement("PROPOSED");
                }
                else if (status == statusConstants.REVISE) {
                    IsShowElement("REVISE");
                } else if (status == statusConstants.EXECUTED) {
                    IsShowElement("EXECUTED");
                }
                else if (status == statusConstants.ADDITIONAL) {
                    IsShowElement("ADDITIONAL");
                }
                else if (status == statusConstants.ADDITIONAL) {
                    IsShowElement("REPORT");
                }
                initYarnChildTableAsync([]);
            })
            .catch(showResponseError);
    }

    function getData(yBookingNo, WithoutOB, reasonStatus, yBookingID, bookingNo, exportOrderNo) {
        axios.get(`/api/yarn-booking/${yBookingNo}/${WithoutOB}/${reasonStatus}/${yBookingID}/${bookingNo}/${exportOrderNo}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();


                masterData = response.data;
                masterData.YBookingDate = formatDateToDefault(masterData.YBookingDate);
                masterData.YRequiredDate = formatDateToDefault(masterData.YRequiredDate);
                masterData.RequiredDate = formatDateToDefault(masterData.RequiredDate);
                setFormData($formEl, masterData);

                masterData.HasFabric = response.data.HasFabric;
                masterData.HasCollar = response.data.HasCollar;
                masterData.HasCuff = response.data.HasCuff;
                //console.log(masterData.yarnBookingReason);
                initReasonTable(masterData.yarnBookingReason);
                vAdditionalBooking = masterData.AdditionalBooking;

                //Set Filter Parameter for details data load
                masterData.F_isSample = WithoutOB;
                masterData.F_BookingNo = bookingNo;
                masterData.F_YBookingNo = yBookingNo;
                masterData.F_ReasonStatus = reasonStatus;
                masterData.F_YBookingID = yBookingID;
                masterData.F_Status = status;

                if (status == statusConstants.ADDITIONAL) {
                    if (vAdditionalBooking == 0) {
                        vAdditionalBooking = 1;
                    }
                }
                if (masterData.HasFabric) {
                    //var FabricData = masterData.ChildsGroup.filter(function (el) { return el.SubGroupName == subGroupNames.FABRIC });
                    var FabricData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }

                    initFabricChildTableAsync(FabricData, subGroupNames.FABRIC);
                    $formEl.find("#divFabricInfo").show();
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                }

                if (masterData.HasCollar) {
                    //var CollarData = masterData.ChildsGroup.filter(function (el) { return el.SubGroupName == subGroupNames.COLLAR });
                    var CollarData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCollarChildTableAsync(CollarData, subGroupNames.COLLAR);
                    $formEl.find("#divCollarInfo").show();
                }
                else {
                    $formEl.find("#divCollarInfo").hide();
                }

                if (masterData.HasCuff) {
                    //var CuffData = masterData.ChildsGroup.filter(function (el) { return el.SubGroupName == subGroupNames.CUFF });
                    var CuffData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCuffChildTableAsync(CuffData, subGroupNames.CUFF)
                    $formEl.find("#divCufInfo").show();
                    //initYarnChildTableAsync(CuffData, subGroupNames.CUFF)
                    //$formEl.find("#divYarnInfo").show();
                }
                else {
                    $formEl.find("#divCufInfo").hide();
                    //$formEl.find("#divYarnInfo").hide();
                }

                //Status wise element show 
                if (status == statusConstants.PENDING) {
                    IsShowElement("PENDING");
                }
                else if (status == statusConstants.COMPLETED) {
                    IsShowElement("COMPLETED");
                }
                else if (status == statusConstants.PROPOSED) {
                    IsShowElement("PROPOSED");
                }
                else if (status == statusConstants.REVISE) {
                    IsShowElement("REVISE");
                } else if (status == statusConstants.EXECUTED) {
                    IsShowElement("EXECUTED");
                }
                else if (status == statusConstants.ADDITIONAL) {
                    IsShowElement("ADDITIONAL");
                }
                else if (status == statusConstants.ADDITIONAL) {
                    IsShowElement("REPORT");
                }

                initYarnChildTableAsync([]);
            })
            .catch(showResponseError);
    }

    function showReason() {
        var yBookingID = $formEl.find("#YBookingID").val();
        var url = `/api/yarn-booking/reason?yBookingID=${yBookingID}&reasonStatus=${reasonStatus}`;
        axios.get(url)
            .then(function (response) {
                initReasonTable(response.data);
                $("#modal-reason").modal("show");
            })
            .catch(showResponseError);
    };

    function isValidChildForm(data) {
        var isValidItemInfo = false;
        //Fabric technical name check
        //$.each(data, function (i, obj) {
        //    $.each(obj.Childs, function (j, objChild) {
        //        if (objChild.SubGroupId == 1) {
        //            if (objChild.FTechnicalName == null || objChild.FTechnicalName == "") {
        //                toastr.error("Must have fabric technical name");
        //                isValidItemInfo = true;
        //            }
        //        }
        //    });
        //});

        //Distribution check
        var dis = 0, ciFlag = 0;
        $.each(data, function (i, obj) {

            $.each(obj.Childs, function (j, objChild) {
                $.each(objChild.ChildItems, function (k, objCItems) {
                    if (objChild.SubGroupId == objCItems.SubGroupId && objChild.YBChildID == objCItems.YBChildID) {
                        dis = parseFloat(dis) + parseFloat(objCItems.Distribution);
                        ciFlag = 1;
                    }
                });

                if (parseFloat(dis) != 100 && parseFloat(ciFlag) == 1) {
                    toastr.error("Yarn Distribution must be 100%");
                    isValidItemInfo = true;
                    dis = 0;
                    ciFlag = 0;
                }
                dis = 0;
                ciFlag = 0;
            });
        });

        for (var iData = 0; iData < data.length; iData++) {
            var parent = data[iData];
            for (var iChild = 0; iChild < parent.Childs.length; iChild++) {
                var child = parent.Childs[iChild];
                if (child.ChildItems.length === 0 && child.SubGroupId == 1) {
                    toastr.error("At least 1 Yarn items is required.");
                    isValidItemInfo = true;
                    break;
                } else if (child.ChildItemsGroup.length === 0 && child.SubGroupId != 1) {
                    toastr.error("At least 1 Yarn items is required.");
                    isValidItemInfo = true;
                    break;
                }

                for (var iChildItem = 0; iChildItem < child.ChildItems.length; iChildItem++) {
                    var childItem = child.ChildItems[iChildItem];
                    if (childItem.SubGroupId == 1 && (childItem.Allowance > 35 || childItem.Allowance <= 0)) {
                        toastr.error("Allowance must be 1 to 35 ");
                        isValidItemInfo = true;
                        break;
                    }
                }
                if (isValidItemInfo) break;
            }
            if (isValidItemInfo) break;
        }


        /*
        $.each(data, function (i, obj) {
            $.each(obj.Childs, function (j, objChild) {
                $.each(objChild.ChildItems, function (k, objCItems) {
                    console.log(objCItems);
                    if (objCItems.SubGroupId == 1) {
                        var Allowance = 0, allowanceFlag = 0;
                        Allowance = parseFloat(objCItems.Allowance);
                        console.log(objCItems.Allowance);
                        if (Allowance > 35 || Allowance <= 0) {
                            toastr.error("Allowance must be 1 to 35 ");
                            isValidItemInfo = true;
                        }
                    }
                });
    
            });
        });
        //Child Items Validation Check 
        $.each(data, function (i, obj) {
            $.each(obj.Childs, function (j, objChild) {
                if (objChild.ChildItems.length === 0 && objChild.SubGroupId == 1) {
                    toastr.error("At least 1 Yarn items is required.");
                    isValidItemInfo = true;
                } else if (objChild.ChildItemsGroup.length === 0 && objChild.SubGroupId != 1) {
                    toastr.error("At least 1 Yarn items is required.");
                    isValidItemInfo = true;
                }
            });
        });
        */

        return isValidItemInfo;
    }

    var validationConstraints = {
        //ChallanNo: {
        //    presence: true
        //},
        //LReceiveNo: {
        //    presence: true
        //},
        //LReceiveDate: {
        //    presence: true
        //},
        //TransportTypeID: {
        //    presence: true
        //},
        //LoanProviderID: {
        //    presence: true
        //},
        //TransportMode: {
        //    presence: true
        //}
    }

    function getFabricWasteage(wasteageGridList, gsm, qty) {
        var wastage = wasteageGridList.find(function (el) {
            return el.GSMFrom <= gsm && gsm <= el.GSMTo && el.BookingQtyFrom <= qty && qty <= el.BookingQtyTo && el.IsFabric == 1;
        });
        return wastage;
    }

    function getOtherWasteage(wasteageGridList, qty) {
        var wastage = wasteageGridList.find(function (el) {
            return el.BookingQtyFrom <= qty && qty <= el.BookingQtyTo && el.IsFabric == 0;
        });
        return wastage;
    }

    function save(isPropose, isRevise, isValidationNeed) {

        var model = generateModelParam(isPropose, isRevise, isValidationNeed);
        if (model != null && model.length > 0) {
            axios.post("/api/yarn-booking/save", model)
                .then(function () {
                    var message = isRevise ? "Revised" : "Saved";
                    toastr.success("Successfully " + message + ".");
                    backToList();
                })
                .catch(showResponseError);
        }
    }

    function generateModelParam(isPropose, isRevise, isValidationNeed) {
        var isBookingListEditItem = (status != statusConstants.PENDING) ? true : false;
        var DataList = [];
        var DataListMaster = new Array();
        var data = formDataToJson($formEl.serializeArray());
        var contactPersonID = $formEl.find("#ContactPerson").val();
        if (isValidationNeed) {
            if (contactPersonID == null || contactPersonID == 0) {
                toastr.error("Select contact person.");
                return null;
            }
        }
        if (masterData.HasFabric) {
            let fabrics = $tblChildFabricEl.getCurrentViewRecords();
            for (let i = 0; i < fabrics.length; i++) {
                fabrics[i].SubGroupID = 1;
                for (let j = 0; j < fabrics[i].ChildItems.length; j++) {
                    if (fabrics[i].ChildItems[j].Segment1ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment2ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment3ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment4ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment5ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment6ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment7ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment8ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment8ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment5ValueId == 60416 && (fabrics[i].ChildItems[j].ShadeCode == null || fabrics[i].ChildItems[j].ShadeCode == "")) {
                        toastr.error("Select shade code for color melange"); //Color Melange = 60416
                        return null;
                    }
                }

                var wastage = getFabricWasteage(masterData.FabricWastageGridList, parseInt(fabrics[i].Segment4ValueDesc), fabrics[i].BookingQty);
                if (typeof wastage !== "undefined") {
                    fabrics[i].ExcessPercentage = wastage.ExcessPercentage;
                    fabrics[i].ExcessQty = wastage.FixedQty ? wastage.ExcessQty : ((fabrics[i].BookingQty * wastage.ExcessPercentage) / 100);
                    fabrics[i].ExcessQtyInKG = 0;
                    fabrics[i].TotalQty = fabrics[i].BookingQty + fabrics[i].ExcessQty;
                    fabrics[i].TotalQtyInKG = 0;
                }
                DataList.push(fabrics[i]);
            }

            if (isBookingListEditItem) {
                var masterObj = masterData.YarnBookings.find(x => x.SubGroupID == 1);
                if (masterObj) {
                    masterObj.Childs = DataList.filter(x => x.SubGroupID == 1);
                    DataListMaster.push(masterObj);
                }
            } else {
                DataListMaster.push(data);
            }
        }
        if (masterData.HasCollar) {
            let collars = $tblChildCollarEl.getCurrentViewRecords();
            for (let i = 0; i < collars.length; i++) {
                collars[i].SubGroupID = 11;
                for (let j = 0; j < collars[i].ChildItems.length; j++) {
                    if (collars[i].ChildItems[j].Segment1ValueId == undefined) {
                        collars[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment2ValueId == undefined) {
                        collars[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment3ValueId == undefined) {
                        collars[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment4ValueId == undefined) {
                        collars[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment5ValueId == undefined) {
                        collars[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment6ValueId == undefined) {
                        collars[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment7ValueId == undefined) {
                        collars[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment8ValueId == undefined) {
                        collars[i].ChildItems[j].Segment8ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment5ValueId == 60416 && (collars[i].ChildItems[j].ShadeCode == null || collars[i].ChildItems[j].ShadeCode == "")) {
                        toastr.error("Select shade code for color melange"); //Color Melange = 60416
                        return null;
                    }
                }

                var wastage = getOtherWasteage(masterData.FabricWastageGridList, collars[i].BookingQty);
                if (typeof wastage !== "undefined") {
                    collars[i].ExcessPercentage = wastage.ExcessPercentage;
                    collars[i].ExcessQty = wastage.ExcessQty;
                    collars[i].ExcessQtyInKG = 0;
                    collars[i].TotalQty = collars[i].BookingQty + collars[i].ExcessQty;
                    collars[i].TotalQtyInKG = 0;
                }
                DataList.push(collars[i]);
            }
            if (isBookingListEditItem) {
                var masterObj = masterData.YarnBookings.find(x => x.SubGroupID == 11);
                if (masterObj) {
                    masterObj.Childs = DataList.filter(x => x.SubGroupID == 11);
                    DataListMaster.push(masterObj);
                }
            } else {
                DataListMaster.push(data);
            }
        }
        if (masterData.HasCuff) {
            let cuffs = $tblChildCuffEl.getCurrentViewRecords();
            for (let i = 0; i < cuffs.length; i++) {
                cuffs[i].SubGroupID = 12;
                for (let j = 0; j < cuffs[i].ChildItems.length; j++) {
                    if (cuffs[i].ChildItems[j].Segment1ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment2ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment3ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment4ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment5ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment6ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment7ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment8ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment8ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment5ValueId == 60416 && (cuffs[i].ChildItems[j].ShadeCode == null || cuffs[i].ChildItems[j].ShadeCode == "")) {
                        toastr.error("Select shade code for color melange"); //Color Melange = 60416
                        return null;
                    }
                }
                var wastage = getOtherWasteage(masterData.FabricWastageGridList, cuffs[i].BookingQty);
                if (typeof wastage !== "undefined") {
                    cuffs[i].ExcessPercentage = wastage.ExcessPercentage;
                    cuffs[i].ExcessQty = wastage.ExcessQty;
                    cuffs[i].ExcessQtyInKG = 0;
                    cuffs[i].TotalQty = cuffs[i].BookingQty + cuffs[i].ExcessQty;
                    cuffs[i].TotalQtyInKG = 0;
                }
                DataList.push(cuffs[i]);
            }
            if (isBookingListEditItem) {
                var masterObj = masterData.YarnBookings.find(x => x.SubGroupID == 12);
                if (masterObj) {
                    masterObj.Childs = DataList.filter(x => x.SubGroupID == 12);
                    DataListMaster.push(masterObj);
                }
            } else {
                DataListMaster.push(data);
            }
        }

        data.Propose = isPropose;
        data.IsRevice = isRevise;


        if (!isBookingListEditItem) {
            data["Childs"] = DataList;
        }

        //data["ChildsGroup"] = DataList;
        if (isValidationNeed) {
            if (isBookingListEditItem) {
                var hasError = false;
                for (var i = 0; i < DataListMaster.length; i++) {
                    if (DataListMaster[i].Childs.length == 0) {
                        toastr.error("At least 1 Yarn items is required.");
                        hasError = true;
                        return null;
                    }
                    if (hasError) break;
                }
            }
            else if (data.Childs.length === 0) {
                toastr.error("At least 1 Yarn items is required.");
                return null;
            };
        }

        if (isValidationNeed) {
            if (isPropose && isValidChildForm(DataListMaster)) return null;
            else if (isRevise && isValidChildForm(DataListMaster)) return null;
        }

        if (isValidationNeed) {
            initializeValidation($formEl, validationConstraints);
        }


        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (DataListMaster.length > 0) {
            DataListMaster[0].F_isSample = masterData.F_isSample;
            DataListMaster[0].F_BookingNo = masterData.F_BookingNo;
            DataListMaster[0].F_YBookingNo = masterData.F_YBookingNo;
            DataListMaster[0].F_ReasonStatus = masterData.F_ReasonStatus;
            DataListMaster[0].F_YBookingID = masterData.F_YBookingID;
            DataListMaster[0].F_Status = masterData.F_Status;
        }
        DataListMaster.map(x => {
            x.WithoutOB = masterData.F_isSample ? true : false;
        });


        //var model = {
        //    models: DataListMaster,
        //    ContactPersonID: contactPersonID,
        //    Remarks: $formEl.find("#Remarks").val(),
        //    IsModified: (status == statusConstants.PENDING) ? false : true,
        //    Propose: isPropose,
        //    IsRevice: isRevise,
        //    IsSample: masterData.F_isSample
        //}

        model = DataListMaster;
        model.map(x => {
            x.ContactPersonID = contactPersonID;
            x.contactPerson = contactPersonID; //contactPerson is int field of master table not ContactPersonID
            x.Remarks = $formEl.find("#Remarks").val();
            x.IsModified = (status == statusConstants.PENDING) ? false : true;
            x.Propose = isPropose;
            x.IsRevice = isRevise;
            x.IsSample = masterData.F_isSample
        });
        //model.ContactPersonID = contactPersonID;
        //model.Remarks = $formEl.find("#Remarks").val();
        //model.IsModified = (status == statusConstants.PENDING) ? false : true;
        //
        //model.Propose = isPropose;
        //model.IsRevice = isRevise;
        //model.IsSample = masterData.F_isSample;

        return model; //Without Error Return Model
    }

    function additionalsave(flag) {
        var DataList = [];
        var DataListMaster = new Array();
        var hasError = false;

        var data = formDataToJson($formEl.serializeArray());

        if (masterData.HasFabric && !hasError) {
            let fabrics = $tblChildFabricEl.getCurrentViewRecords();

            for (let i = 0; i < fabrics.length; i++) {
                for (let j = 0; j < fabrics[i].ChildItems.length; j++) {
                    if (fabrics[i].ChildItems[j].Segment1ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment2ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment3ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment4ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment5ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment6ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment7ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment8ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment8ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment5ValueId == 60416 && (fabrics[i].ChildItems[j].ShadeCode == null || fabrics[i].ChildItems[j].ShadeCode == "")) {
                        toastr.error("Select shade code for color melange"); //Color Melange = 60416
                        hasError = true;
                        break;
                    }
                }
                if (hasError) break;
                var wastage = getFabricWasteage(masterData.FabricWastageGridList, parseInt(fabrics[i].Segment4ValueDesc), fabrics[i].BookingQty);
                if (typeof wastage !== "undefined") {
                    fabrics[i].ExcessPercentage = wastage.ExcessPercentage;
                    fabrics[i].ExcessQty = wastage.FixedQty ? wastage.ExcessQty : ((fabrics[i].BookingQty * wastage.ExcessPercentage) / 100);
                    fabrics[i].ExcessQtyInKG = 0;
                    fabrics[i].TotalQty = fabrics[i].BookingQty + fabrics[i].ExcessQty;
                    fabrics[i].TotalQtyInKG = 0;
                }
                DataList.push(fabrics[i]);
            }

            DataListMaster.push(data);
        }

        if (masterData.HasCollar && !hasError) {
            let collars = $tblChildCollarEl.getCurrentViewRecords();

            for (let i = 0; i < collars.length; i++) {
                for (let j = 0; j < collars[i].ChildItems.length; j++) {
                    if (collars[i].ChildItems[j].Segment1ValueId == undefined) {
                        collars[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment2ValueId == undefined) {
                        collars[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment3ValueId == undefined) {
                        collars[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment4ValueId == undefined) {
                        collars[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment5ValueId == undefined) {
                        collars[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment6ValueId == undefined) {
                        collars[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment7ValueId == undefined) {
                        collars[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment8ValueId == undefined) {
                        collars[i].ChildItems[j].Segment8ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment5ValueId == 60416 && (collars[i].ChildItems[j].ShadeCode == null || collars[i].ChildItems[j].ShadeCode == "")) {
                        toastr.error("Select shade code for color melange"); //Color Melange = 60416
                        hasError = true;
                        break;
                    }
                }
                if (hasError) break;
                var wastage = getOtherWasteage(masterData.FabricWastageGridList, collars[i].BookingQty);
                if (typeof wastage !== "undefined") {
                    collars[i].ExcessPercentage = wastage.ExcessPercentage;
                    collars[i].ExcessQty = wastage.ExcessQty;
                    collars[i].ExcessQtyInKG = 0;
                    collars[i].TotalQty = collars[i].BookingQty + collars[i].ExcessQty;
                    collars[i].TotalQtyInKG = 0;
                }
                DataList.push(collars[i]);
            }

            DataListMaster.push(data);
        }

        if (masterData.HasCuff && !hasError) {
            let cuffs = $tblChildCuffEl.getCurrentViewRecords();

            for (let i = 0; i < cuffs.length; i++) {
                for (let j = 0; j < cuffs[i].ChildItems.length; j++) {
                    if (cuffs[i].ChildItems[j].Segment1ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment2ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment3ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment4ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment5ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment6ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment7ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment8ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment8ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment5ValueId == 60416 && (cuffs[i].ChildItems[j].ShadeCode == null || cuffs[i].ChildItems[j].ShadeCode == "")) {
                        toastr.error("Select shade code for color melange"); //Color Melange = 60416
                        hasError = true;
                        break;
                    }
                }
                if (hasError) break;
                var wastage = getOtherWasteage(masterData.FabricWastageGridList, cuffs[i].BookingQty);
                if (typeof wastage !== "undefined") {
                    cuffs[i].ExcessPercentage = wastage.ExcessPercentage;
                    cuffs[i].ExcessQty = wastage.ExcessQty;
                    cuffs[i].ExcessQtyInKG = 0;
                    cuffs[i].TotalQty = cuffs[i].BookingQty + cuffs[i].ExcessQty;
                    cuffs[i].TotalQtyInKG = 0;
                }
                DataList.push(cuffs[i]);
            }
            DataListMaster.push(data);
        }
        if (hasError) return false;

        data.Propose = flag;
        data["Childs"] = DataList;
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");

        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (isValidChildForm(DataListMaster)) return;
        //Reason List
        data["yarnBookingReason"] = checkReasonItem;

        //var model = {
        //    models: DataListMaster,
        //    IsModified: (status == statusConstants.PENDING) ? false : true
        //}

        //New Code
        model = DataListMaster;
        model.IsModified = (status == statusConstants.PENDING) ? false : true;

        axios.post("/api/yarn-booking/additionalsave", model)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function saveRevise(flag) {
        var DataList = [];
        var DataListMaster = new Array();

        var data = formDataToJson($formEl.serializeArray());
        if (masterData.HasFabric) {
            let fabrics = $tblChildFabricEl.getCurrentViewRecords();

            for (let i = 0; i < fabrics.length; i++) {
                for (let j = 0; j < fabrics[i].ChildItems.length; j++) {
                    if (fabrics[i].ChildItems[j].Segment1ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment2ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment3ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment4ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment5ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment6ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment7ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (fabrics[i].ChildItems[j].Segment8ValueId == undefined) {
                        fabrics[i].ChildItems[j].Segment8ValueId = 0;
                    }
                }

                var wastage = getFabricWasteage(masterData.FabricWastageGridList, parseInt(fabrics[i].Segment4ValueDesc), fabrics[i].BookingQty);
                if (typeof wastage !== "undefined") {
                    fabrics[i].ExcessPercentage = wastage.ExcessPercentage;
                    fabrics[i].ExcessQty = wastage.FixedQty ? wastage.ExcessQty : ((fabrics[i].BookingQty * wastage.ExcessPercentage) / 100);
                    fabrics[i].ExcessQtyInKG = 0;
                    fabrics[i].TotalQty = fabrics[i].BookingQty + fabrics[i].ExcessQty;
                    fabrics[i].TotalQtyInKG = 0;
                }
                DataList.push(fabrics[i]);
            }

            DataListMaster.push(data);
        }

        if (masterData.HasCollar) {
            let collars = $tblChildCollarEl.getCurrentViewRecords();

            for (let i = 0; i < collars.length; i++) {
                for (let j = 0; j < collars[i].ChildItems.length; j++) {
                    if (collars[i].ChildItems[j].Segment1ValueId == undefined) {
                        collars[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment2ValueId == undefined) {
                        collars[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment3ValueId == undefined) {
                        collars[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment4ValueId == undefined) {
                        collars[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment5ValueId == undefined) {
                        collars[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment6ValueId == undefined) {
                        collars[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment7ValueId == undefined) {
                        collars[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (collars[i].ChildItems[j].Segment8ValueId == undefined) {
                        collars[i].ChildItems[j].Segment8ValueId = 0;
                    }
                }

                var wastage = getOtherWasteage(masterData.FabricWastageGridList, collars[i].BookingQty);
                if (typeof wastage !== "undefined") {
                    collars[i].ExcessPercentage = wastage.ExcessPercentage;
                    collars[i].ExcessQty = wastage.ExcessQty;
                    collars[i].ExcessQtyInKG = 0;
                    collars[i].TotalQty = collars[i].BookingQty + collars[i].ExcessQty;
                    collars[i].TotalQtyInKG = 0;
                }
                DataList.push(collars[i]);
            }

            DataListMaster.push(data);
        }

        if (masterData.HasCuff) {
            let cuffs = $tblChildCuffEl.getCurrentViewRecords();

            for (let i = 0; i < cuffs.length; i++) {
                for (let j = 0; j < cuffs[i].ChildItems.length; j++) {
                    if (cuffs[i].ChildItems[j].Segment1ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment1ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment2ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment2ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment3ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment3ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment4ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment4ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment5ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment5ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment6ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment6ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment7ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment7ValueId = 0;
                    }
                    if (cuffs[i].ChildItems[j].Segment8ValueId == undefined) {
                        cuffs[i].ChildItems[j].Segment8ValueId = 0;
                    }
                }

                var wastage = getOtherWasteage(masterData.FabricWastageGridList, cuffs[i].BookingQty);
                if (typeof wastage !== "undefined") {
                    cuffs[i].ExcessPercentage = wastage.ExcessPercentage;
                    cuffs[i].ExcessQty = wastage.ExcessQty;
                    cuffs[i].ExcessQtyInKG = 0;
                    cuffs[i].TotalQty = cuffs[i].BookingQty + cuffs[i].ExcessQty;
                    cuffs[i].TotalQtyInKG = 0;
                }
                DataList.push(cuffs[i]);
            }

            DataListMaster.push(data);
        }

        data.Propose = flag;
        data["Childs"] = DataList;
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");

        //Reason List
        data["yarnBookingReason"] = checkReasonItem;

        //console.log(data);

        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        if (isValidChildForm(DataListMaster)) return;

        //var model = {
        //    models: DataListMaster,
        //    IsModified: (status == statusConstants.PENDING) ? false : true
        //}

        model = DataListMaster;
        model.IsModified = (status == statusConstants.PENDING) ? false : true;


        axios.post("/api/yarn-booking/revisesave", model)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }

    function IsShowElement(flag) {
        if (isYarnBookingPage) {
            if (flag == "Load") {
                //Element disable in load options
                $formEl.find("#divCancelFlag").fadeOut();
                $formEl.find("#IsCancel").prop("checked", false);
                $formEl.find("#IsCancel").prop("disabled", false);

                $formEl.find("#divAdditionalFlag").fadeOut();
                $formEl.find("#AdditionalBooking").prop("checked", false);

                $formEl.find("#divRevisionFlag").fadeOut();
                $formEl.find("#RevisionFlag").prop("checked", false);

                $formEl.find("#divCancelReason").fadeOut();
                $formEl.find("#CancelReasonID").prop("disabled", false);
                $formEl.find("#divBtnReason").fadeOut();

                $formEl.find("#divRevisionNo").fadeOut();
                //Button
                $formEl.find("#btnSave").fadeIn();
                $formEl.find("#btnSaveAndSend").fadeIn();
                $formEl.find("#btnAdditionalSave").fadeOut();
                $formEl.find("#btnRevise").fadeOut();
                $formEl.find("#btnAcknowledge").fadeOut();
                $formEl.find("#btnCancel").fadeOut();

                //Master Field
                $formEl.find("#ContactPerson").prop("disabled", false);
                $formEl.find("#YBookingDate").prop("disabled", false);
                $formEl.find("#YRequiredDate").prop("disabled", false);
                $formEl.find("#Remarks").prop("disabled", false);
            }
            else if (flag == "PENDING") {
                $formEl.find("#btnSave").fadeIn();
                $formEl.find("#btnSaveAndSend").fadeIn();
            }

            else if (flag == "COMPLETED") {
                //If Cancel data load
                if (masterData.AdditionalBooking != 0) {
                    //Check Box
                    $formEl.find("#divCancelFlag").fadeIn();
                    $formEl.find("#IsCancel").prop("checked", false);
                    $formEl.find("#IsCancel").prop("disabled", false);

                    //Combobox
                    $formEl.find("#divCancelReason").fadeIn();
                    $formEl.find("#CancelReasonID").prop("disabled", false);

                    //Button
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnSaveAndSend").fadeIn();
                }
                if (masterData.IsCancel == 1) {
                    //Check Box
                    $formEl.find("#divCancelFlag").fadeIn();
                    $formEl.find("#IsCancel").prop("checked", true);
                    $formEl.find("#IsCancel").prop("disabled", true);

                    //Combobox
                    $formEl.find("#divCancelReason").fadeIn();
                    $formEl.find("#CancelReasonID").prop("disabled", true);

                    //Button
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnSaveAndSend").fadeOut();
                }
            }

            else if (flag == "PROPOSED") {
                $formEl.find("#divRevisionFlag").fadeIn();
                $formEl.find("#RevisionFlag").prop("checked", false);

                $formEl.find("#btnSave,#btnSaveAndSend").fadeOut();

                if (masterData.IsCancel == 1) {
                    $formEl.find("#divRevisionFlag").fadeOut();
                    //Check Box
                    $formEl.find("#divCancelFlag").fadeIn();
                    $formEl.find("#IsCancel").prop("checked", true);
                    $formEl.find("#IsCancel").prop("disabled", true);

                    //Combobox
                    $formEl.find("#divCancelReason").fadeIn();
                    $formEl.find("#CancelReasonID").prop("disabled", true);

                    //Button
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnSaveAndSend").fadeOut();
                }
            }

            else if (flag == "REVISE") {
                $formEl.find("#divRevisionFlag").fadeIn();
                $formEl.find("#RevisionFlag").prop("checked", true);
                $formEl.find("#RevisionFlag").prop("disabled", true);

                $formEl.find("#divBtnReason").fadeIn();
                $formEl.find("#btnReason").fadeIn();

                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeIn();
                $formEl.find("#btnRevise").fadeIn();
                $formEl.find("#divRevisionNo").fadeIn();
            }

            else if (flag == "EXECUTED") {
                $formEl.find("#divRevisionFlag").fadeIn();
                $formEl.find("#RevisionFlag").prop("checked", false);
                $formEl.find("#RevisionFlag").prop("disabled", false);

                $formEl.find("#divBtnReason").fadeIn();
                $formEl.find("#btnReason").fadeIn();

                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeIn();
                $formEl.find("#btnRevise").fadeIn();

                if (vAdditionalBooking > 0) {
                    $formEl.find("#divCancelFlag").fadeIn();
                    $formEl.find("#IsCancel").prop("checked", false);

                    $formEl.find("#divCancelReason").fadeIn();
                    $formEl.find("#CancelReasonID").prop("disabled", false);
                }

                if (masterData.IsCancel == 1) {
                    $formEl.find("#divRevisionFlag").fadeOut();
                    //Check Box
                    $formEl.find("#divCancelFlag").fadeIn();
                    $formEl.find("#IsCancel").prop("checked", true);
                    $formEl.find("#IsCancel").prop("disabled", true);

                    //Combobox
                    $formEl.find("#divCancelReason").fadeIn();
                    $formEl.find("#CancelReasonID").prop("disabled", true);

                    //Button
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnSaveAndSend").fadeOut();
                    $formEl.find("#btnRevise").fadeOut();
                    $formEl.find("#divBtnReason").fadeOut();
                    $formEl.find("#divCancelReason").fadeIn();
                }
            }

            else if (flag == "ADDITIONAL") {
                $formEl.find("#divAdditionalFlag").fadeIn();
                $formEl.find("#AdditionalBooking").prop("checked", true);
                $formEl.find("#AdditionalBooking").prop("disabled", true);

                $formEl.find("#divBtnReason").fadeIn();
                $formEl.find("#btnReason").fadeIn();

                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeIn();
                $formEl.find("#btnAdditionalSave").fadeIn();

                if (vAdditionalBooking == 0) {
                    vAdditionalBooking = 1;
                }
            }

            else if (flag == "REPORT") {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
            }
        }

        if (isAcknowledgePage) {
            if (flag == "Load") {
                //Element disable in load options
                $formEl.find("#divCancelFlag").fadeOut();
                $formEl.find("#IsCancel").prop("checked", false);

                $formEl.find("#divAdditionalFlag").fadeOut();
                $formEl.find("#AdditionalBooking").prop("checked", false);

                $formEl.find("#divRevisionFlag").fadeOut();
                $formEl.find("#RevisionFlag").prop("checked", false);

                $formEl.find("#divCancelReason").fadeOut();
                $formEl.find("#CancelReasonID").prop("disabled", false);
                $formEl.find("#divBtnReason").fadeOut();

                //Button
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveAndSend").fadeOut();
                $formEl.find("#btnAdditionalSave").fadeOut();
                $formEl.find("#btnRevise").fadeOut();
                $formEl.find("#btnAcknowledge").fadeIn();
                $formEl.find("#btnCancel").fadeOut();

                //Master Field
                $formEl.find("#ContactPerson").prop("disabled", true);
                $formEl.find("#YBookingDate").prop("disabled", true);
                $formEl.find("#YRequiredDate").prop("disabled", true);
                $formEl.find("#Remarks").prop("disabled", true);
            }
            if (flag == "AWAITING_PROPOSE") {
                $formEl.find("#btnAcknowledge").fadeIn();
            }
            if (flag == "REVISE") {
                $formEl.find("#btnAcknowledge").fadeIn();
            }
            if (flag == "ACKNOWLEDGE") {
                $formEl.find("#btnAcknowledge").fadeOut();
            }
        }

    }

    function groupByKey(array, key) {
        return array
            .reduce((hash, obj) => {
                if (obj[key] === undefined) return hash;
                return Object.assign(hash, { [obj[key]]: (hash[obj[key]] || []).concat(obj) })
            }, {})
    }

    function getAllYarnList() {

        //initYarnChildTableAsync([]);
        var yarnallList = [];
        var finalyarnList = [];

        if ($tblChildFabricEl) {
            let fabrics = $tblChildFabricEl.getCurrentViewRecords();
            if (fabrics) {
                for (let i = 0; i < fabrics.length; i++) {
                    for (let j = 0; j < fabrics[i].ChildItems.length; j++) {
                        yarnallList.push(fabrics[i].ChildItems[j]);
                    }
                }
            }
        }
        if ($tblChildCollarEl) {
            let collars = $tblChildCollarEl.getCurrentViewRecords();
            if (collars) {
                for (let i = 0; i < collars.length; i++) {
                    for (let j = 0; j < collars[i].ChildItemsGroup.length; j++) {
                        yarnallList.push(collars[i].ChildItemsGroup[j]);
                    }
                }
            }
        }

        if ($tblChildCuffEl) {
            let cuffs = $tblChildCuffEl.getCurrentViewRecords();
            if (cuffs) {
                for (let i = 0; i < cuffs.length; i++) {
                    for (let j = 0; j < cuffs[i].ChildItemsGroup.length; j++) {
                        yarnallList.push(cuffs[i].ChildItemsGroup[j]);
                    }
                }
            }
        }

        //const data = [{ Phase: "Phase 1", Step: "Step 1", Task: "Task 1", Value: "5" }, { Phase: "Phase 1", Step: "Step 1", Task: "Task 2", Value: "10" }, { Phase: "Phase 1", Step: "Step 2", Task: "Task 1", Value: "15" }, { Phase: "Phase 1", Step: "Step 2", Task: "Task 2", Value: "20" }, { Phase: "Phase 2", Step: "Step 1", Task: "Task 1", Value: "25" }, { Phase: "Phase 2", Step: "Step 1", Task: "Task 2", Value: "30" }, { Phase: "Phase 2", Step: "Step 2", Task: "Task 1", Value: "35" }, { Phase: "Phase 2", Step: "Step 2", Task: "Task 2", Value: "40" }],

        //09-Sept-2022 comments for special request
        //groups = ['Segment1ValueId', 'Segment2ValueId', 'Segment3ValueId', 'Segment4ValueId', 'Segment5ValueId', 'Segment6ValueId', 'Segment7ValueId'],
        //    finalyarnList = Object.values(yarnallList.reduce((r, o) => {
        //        const key = groups.map(k => o[k]).join('|');
        //        r[key] ??= { ...o, RequiredQty: 0 };
        //        //r[key].Task += (r[key].Task && ', ') + o.Task;
        //        r[key].RequiredQty += +o.RequiredQty
        //        return r;
        //    }, {}));

        yarnallList.map(item => {
            item.RequiredQty = parseFloat(item.RequiredQty).toFixed(3);
            finalyarnList.push(item)
        });
        var yarnReqTotal = 0;
        if (finalyarnList.length > 0) {
            finalyarnList.map(x => {
                yarnReqTotal += parseFloat(x.RequiredQty);
            });
            var tObj = {
                Specification: "Grand Total : ",
                RequiredQty: yarnReqTotal.toFixed(3)
            };
            finalyarnList.push(tObj);
        }
        initYarnChildTableAsync(finalyarnList);
        $formEl.find("#divYarnInfo").show();
    }

})();