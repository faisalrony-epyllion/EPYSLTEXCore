(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var isBlended = false;
    var $divTblEl, pageId, $pageEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId,
        $formEl, $tblOtherItemEl, tblOtherItemId, $tblCuffItemEl, tblCuffItemId, tblCreateCompositionId, $tblCreateCompositionEl;
    var status = statusConstants.PENDING;
    var masterData, currentChildRowData = true, maxColYarn = 999, maxColCollar = 999, maxColCuff = 999;
    var isFabric = 0, isCollar = 0, isCuff = 0;
    var isBDS = 1;
    var _segments = [];
    var validationConstraints = {
    };
    var fabYarnItem = null;
    var collarYarnItem = null;
    //var cuffYarnItem = null;
    var copyYarnItem = null;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");


        if (menuParam == "MRBDS") isBDS = 1;
        else if (menuParam == "MRPB" || menuParam == "MRPBAck") isBDS = 3;

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblColorChildId = "#tblColorChild" + pageId;
        tblOtherItemId = "#tblOtherItem" + pageId;
        tblCuffItemId = "#tblCuffItem" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        tblCreateCompositionId = `#tblCreateComposition-${pageId}`;

        $toolbarEl.find("#btnAckList").fadeOut();

        if (menuParam == "MRPBAck") {
            $toolbarEl.find("#btnList").fadeOut();
            $toolbarEl.find("#btnDraftList").fadeOut();
            $toolbarEl.find("#btnRevisionList").fadeOut();
            $toolbarEl.find("#btnAckList").fadeIn();
            $formEl.find("#btnAcknowledgeAutoPR").fadeIn();
        }

        initMasterTable();
        $formEl.find("#addYarnComposition").on("click", function (e) {
            showAddComposition();
        });
        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });
        $toolbarEl.find("#btnDraftList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PARTIALLY_COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();

            toggleActiveToolbarBtn(this, $toolbarEl);
            if (menuParam == "MRPBAck") {
                status = statusConstants.PROPOSED_FOR_ACKNOWLEDGE;
            }
            else {
                status = statusConstants.COMPLETED;
            }
            //status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnAckList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            $formEl.find("#btnAcknowledgeAutoPR").fadeOut();
            initMasterTable();
        });

        //$toolbarEl.find("#btnRevisionAckList").on("click", function (e) {
        //    e.preventDefault();
        //    toggleActiveToolbarBtn(this, $toolbarEl);
        //    status = statusConstants.REVISE_FOR_ACKNOWLEDGE;
        //    $formEl.find("#btnAcknowledgeAutoPR").fadeOut();
        //    initMasterTable();
        //});



        $toolbarEl.find("#btnRevisionList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REVISE;
            initMasterTable();
        });
        $pageEl.find('input[type=radio][name=Blended]').change(function (e) {
            e.preventDefault();
            isBlended = convertToBoolean(this.value);
            initTblCreateComposition();
            return false;
        });
        $pageEl.find("#btnAddComposition").click(saveComposition);
        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });

        $formEl.find("#btnSaveComplete").click(function (e) {
            e.preventDefault();
            save(true);
        });

        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            revise(true);
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            approve(this);
        });

        $formEl.find("#btnAcknowledgeAutoPR").click(function (e) {
            e.preventDefault();
            acknowledgeforautopr(this);
        });

        $formEl.find("#btnCancel").on("click", backToListWithoutFilter);

        if (menuParam == "MRPBAck") {
            $toolbarEl.find("#btnCompleteList").click();
            $toolbarEl.find("#btnCompleteList span").text("Pending for Acknowledge");
            $toolbarEl.find("#btnCompleteList i").removeClass('fa-list').addClass('fa-hourglass');
        }

        getSegments();
    });

    function acknowledgeforautopr() {
        //var YBookingNo = $formEl.find("#YBookingNo").val();
        //var url = `/api/mr-bds/remove-from-reject/${masterData.FCMRMasterID}`;
        //masterData.OtherItems[0].BuyerName
        console.log(masterData);
        axios.post(`/api/mr-bds/acknowledgeAutoPR/${masterData.OtherItems[0].ConceptNo}`)
            .then(function () {
                toastr.success("Acknowledge operation successfull.");
                backToList();
            })
            .catch(showResponseError);
    }


    async function getSegments() {
        _segments = await axios.get(getYarnItemsApiUrl([]));
        _segments = _segments.data;
    }

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

    function initMasterTable() {
        var commands = [];
        if (status == statusConstants.PENDING) {
            commands = [
                { type: 'New', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
            ]
        } else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } }
            ]
        }

        var columns = [
            {
                headerText: 'Actions', commands: commands, width: 10
            },
            //{
            //    field: 'Status', headerText: 'Status', width: 10, visible: status == statusConstants.PENDING
            //},
            {
                field: 'BookingNo', headerText: 'Booking No', width: 20
            },
            {
                field: 'YBookingNo', headerText: 'YBooking No', width: 20, visible: false
            },
            {
                field: 'BookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 20
            },
            {
                field: 'AcknowledgeDate', headerText: 'Acknowledge Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 20, visible: status == statusConstants.PENDING
            },
            {
                field: 'BuyerName', headerText: 'Buyer', width: 20
            },
            {
                field: 'BuyerDepartment', headerText: 'Buyer Department', width: 20
            },
            {
                field: 'CompanyName', headerText: 'CompanyName', width: 20
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            //allowGrouping: true,
            apiEndPoint: `/api/mr-bds/list?status=${status}&isBDS=${isBDS}`,
            columns: columns,
            allowSorting: true,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'New') {
            if (args.rowData.Status == 'New') {
                getNew(args.rowData.FBAckID);
                $formEl.find("#btnRevise").fadeOut();
                $formEl.find("#btnSave,#btnSaveComplete").fadeIn();
            }
            else {
                getRevision(args.rowData.FBAckID, args.rowData.ConceptNo);
            }
            $formEl.find("#btnAcknowledge,#btnAcknowledgeAutoPR").fadeOut();
        } else if (args.commandColumn.type == 'Edit' && status == statusConstants.REVISE) {
            if (args.rowData) {
                getRevision(args.rowData.FBAckID, args.rowData.ConceptNo);
                $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge,#btnAcknowledgeAutoPR").fadeOut();
                $formEl.find("#btnRevise").fadeIn();
            }
        } else if (args.commandColumn.type == 'Edit' && status == statusConstants.COMPLETED) {
            if (args.rowData) {
                getRevisionForCompleteList(args.rowData.ConceptNo);
                $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge,#btnRevise,#btnAcknowledgeAutoPR").fadeOut();
                if (menuParam == "MRPBAck") {
                    $formEl.find("#btnAcknowledgeAutoPR").fadeIn();
                }
                else {
                    $formEl.find("#btnRevise").fadeIn();
                }

            }
        }
        else if (args.commandColumn.type == 'Edit' && status == statusConstants.PROPOSED_FOR_ACKNOWLEDGE) {
            if (args.rowData) {
                getRevisionForCompleteList(args.rowData.ConceptNo);
                //getAcknowledgeForList(args.rowData.YBookingNo);
                $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge").fadeOut();
                $formEl.find("#btnRevise").fadeOut();
                $formEl.find("#btnAcknowledgeAutoPR").fadeIn();
            }
        }
        else if (args.commandColumn.type == 'Edit') {
            if (args.rowData) {
                getDetails(args.rowData.ConceptNo);
                $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge,#btnAcknowledgeAutoPR").fadeOut();
                $formEl.find("#btnRevise").fadeOut();
                if (status == statusConstants.PARTIALLY_COMPLETED) $formEl.find("#btnSave,#btnSaveComplete").fadeIn();
            }
        }
    }

    async function initYarnChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();

        //Child Grid Record
        var childColumns = await getYarnItemColumnsAsync(data, currentChildRowData);
        //if (menuParam == "MRPBAck") {
        //    childColumns.unshift({ field: 'YBChildID', isPrimaryKey: true, visible: false });
        //}
        //else {
        //    childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        //}
        childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        var additionalColumns = [
            {
                field: 'ShadeCode',
                headerText: 'Shade Code',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: data[0].YarnShadeBooks,
                displayField: "ShadeCode",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'YDProductionMasterID', headerText: 'YDProductionMasterID', visible: false
            },
            { field: 'YDItem', headerText: 'YD Item?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            { field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            {
                field: 'Distribution', headerText: 'Yarn Distribution (%)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 2, min: 1 } }, width: 100
                /*edit: { params: { showSpinButton: false, decimals: 0, format: "N2", min: 1, validateDecimalOnType: true } }, width: 100*/
            },
            {
                field: 'BookingQty', headerText: 'Net Consumption', allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } }, width: 100
            },
            {
                field: 'Allowance', headerText: 'Allowance (%)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
            },
            {
                field: 'ReqQty', headerText: 'Req Qty(KG )', editType: "numericedit", allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100
            },
            { field: 'ReqCone', headerText: 'Req Cone(PCS)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100 },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            /* { field: 'StockQty', headerText: 'Stock Qty(KG)', allowEditing: false }*/
        ];
        childColumns.push.apply(childColumns, additionalColumns);
        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            // toolbar: ['Add'],
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser'],
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    //var maxId = Math..apply(Math, data.map(function (el) { return el["Distribution"]; }));
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
                    var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(remainDis) / 100);
                    var reqQty = netConsumption;
                    args.data.Distribution = remainDis;
                    args.data.BookingQty = netConsumption.toFixed(4);
                    args.data.Allowance = 0.00;
                    args.data.ReqQty = reqQty.toFixed(2);

                    args.data.FCMRChildID = maxColYarn++;
                    args.data.FCMRMasterID = this.parentDetails.parentKeyFieldValue;
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.FCMRChildID);
                    var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(args.data.Distribution) / 100);
                    var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(args.data.Allowance)) / 100);
                    args.data.BookingQty = netConsumption.toFixed(4);
                    args.data.ReqQty = reqQty.toFixed(2);
                }
            },
            columns: [
                //Master Grid Record
                //if(menuParam == "MRPBAck") {
                //    this.dataSource = this.parentDetails.parentRowData.ChildItems;
                //}
                { field: 'FCMRMasterID', isPrimaryKey: true, visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'Construction', headerText: 'Construction', width: 100, allowEditing: false, visible: false },
                { field: 'Composition', headerText: 'Composition', width: 100, allowEditing: false },
                { field: 'Color', headerText: 'Color', width: 100, allowEditing: false },
                { field: 'GSM', headerText: 'GSM', width: 40, allowEditing: false },
                { field: 'DyeingType', headerText: 'Dyeing Type', width: 70, allowEditing: false },
                { field: 'KnittingType', headerText: 'Knitting Type', width: 100, allowEditing: false, visible: false },

                { field: 'YarnProgram', headerText: 'Yarn Program', width: 80, allowEditing: false },
                { field: 'YarnSubProgram', headerText: 'Yarn Sub Program', width: 100, allowEditing: false },

                { field: 'MachineType', headerText: 'Machine Type', width: 100, allowEditing: false, visible: false },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 100, allowEditing: false },
                { field: 'IsSubContact', headerText: 'Sub-Contact?', textAlign: 'Center', width: 70, allowEditing: false, editType: "booleanedit", displayAsCheckBox: true, visible: false },
                { field: 'DeliveryDate', headerText: 'Delivery Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false, width: 100, visible: false },
                { field: 'FabricWidth', headerText: 'Fabric Width', width: 100, allowEditing: false, visible: false },
                { field: 'YarnType', headerText: 'Yarn Type', width: 70, allowEditing: false },
                { field: 'ReferenceNo', headerText: 'Reference No', width: 100, allowEditing: false, visible: false },
                { field: 'ColorReferenceNo', headerText: 'ColorReference No', width: 100, allowEditing: false, visible: false },
                { field: 'LengthYds', headerText: 'Length (Yds)', width: 100, allowEditing: false, visible: false },
                { field: 'LengthInch', headerText: 'Length (Inch)', width: 100, allowEditing: false, visible: false },
                { field: 'Instruction', headerText: 'Instruction', width: 100, allowEditing: false, visible: false },
                { field: 'LabDipNo', headerText: 'Lab Dip No', width: 100, allowEditing: false, visible: false },
                { field: 'ForBDSStyleNo', headerText: 'Style No', width: 100, allowEditing: false, visible: false },
                { field: 'Qty', headerText: 'Booking Qty', width: 70, allowEditing: false, visible: false },
                { field: 'TotalQty', headerText: 'Req. Knitting Qty', width: 70, allowEditing: false }
                //{ field: 'ConsumptionQty', headerText: 'Consumption Qty', width: 100, allowEditing: false }
            ],

            childGrid: {
                //queryString: menuParam == "MRPBAck" ? 'YBChildID' : 'FCMRMasterID',
                queryString: 'FCMRMasterID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                    }
                    else if (args.requestType === "add") {
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
                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(remainDis) / 100);
                        var reqQty = netConsumption;
                        args.data.Distribution = remainDis;
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.Allowance = 0.00;
                        args.data.ReqQty = reqQty.toFixed(2);

                        args.data.FCMRChildID = maxColYarn++;
                        args.data.FCMRMasterID = this.parentDetails.parentKeyFieldValue;
                        args.data.ReqCone = 1;
                    }
                    else if (args.requestType === "save") {
                        //Check YDItem when YD field check
                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }

                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(args.data.Distribution) / 100);
                        var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(args.data.Allowance)) / 100);
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.ReqQty = reqQty.toFixed(2);
                        args.data = getFreeConceptMRChild(args.data);

                        ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                        $tblChildEl.updateRow(args.rowIndex, args.data);
                    }
                    else if (args.requestType === "delete") {
                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        //var index = $tblChildEl.getRowIndexByPrimaryKey(args.data[0].FCMRChildID);
                        //if (index > -1) {
                        //    args.data.EntityState = 8;
                        //    masterData.Childs[index] = args.data;
                        //}
                    }
                },
                load: loadFirstLChildYarnGrid
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    fabYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    if (fabYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (fabYarnItem == null || fabYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < fabYarnItem.length; i++) {
                            //var parentRowData = $tblChildEl.getRowByIndex($tblChildEl.getRowIndexByPrimaryKey(args.rowInfo.rowData.FCMRMasterID));
                            var copiedItem = objectCopy(fabYarnItem[i]);
                            copiedItem.FCMRChildID = maxColYarn++;
                            copiedItem.FCMRMasterID = args.rowInfo.rowData.FCMRMasterID;
                            var netConsumption = (parseFloat(args.rowInfo.rowData.TotalQty) * parseFloat(copiedItem.Distribution) / 100);
                            var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(copiedItem.Allowance)) / 100);
                            copiedItem.BookingQty = netConsumption.toFixed(4);
                            copiedItem.ReqQty = reqQty.toFixed(2);
                            //$tblChildEl.addRecord(copiedItem, 'Child', args.rowInfo.rowIndex);
                            args.rowInfo.rowData.Childs.push(copiedItem);
                        }
                        $tblChildEl.refresh();
                    }
                }
            }
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }

    function getSegmentValueDesc(segNo, argsObj) {
        var propValue = argsObj["Segment" + segNo + "ValueId"];
        if (typeof propValue === "undefined" || propValue == null) return "";
        var segValueList = _segments["Segment" + segNo + "ValueList"];
        if (typeof segValueList !== "undefined" && segValueList != null && parseInt(propValue) > 0) {
            var seg = segValueList.find(x => x.id == propValue);
            if (typeof seg !== "undefined" && seg != null) return seg.text;
        }
        return "";
    }

    function getFreeConceptMRChild(argsObj) {
        var oFreeConceptMRChild = {
            FCMRChildID: argsObj.FCMRChildID,
            FCMRMasterID: argsObj.FCMRMasterID,
            YarnCategory: argsObj.YarnCategory,
            YDItem: argsObj.YDItem,
            YDItem: argsObj.YDItem,
            YD: argsObj.YD,
            ReqQty: parseFloat(argsObj.ReqQty).toFixed(2),
            ReqCone: argsObj.ReqCone,
            Remarks: argsObj.Remarks,
            //IsPR: true,
            IsPR: argsObj.IsPR,
            ShadeCode: argsObj.ShadeCode,
            Distribution: argsObj.Distribution,
            BookingQty: argsObj.BookingQty,
            Allowance: argsObj.Allowance,

            Segment1ValueId: argsObj.Segment1ValueId,
            Segment2ValueId: argsObj.Segment2ValueId,
            Segment3ValueId: argsObj.Segment3ValueId,
            Segment4ValueId: argsObj.Segment4ValueId,
            Segment5ValueId: argsObj.Segment5ValueId,
            Segment6ValueId: argsObj.Segment6ValueId,
            Segment7ValueId: argsObj.Segment7ValueId
        };

        if (_segments != null && typeof _segments !== "undefined") {
            for (var i = 1; i <= 7; i++) {
                oFreeConceptMRChild["Segment" + i + "ValueDesc"] = getSegmentValueDesc(i, argsObj);
            }
        }
        return oFreeConceptMRChild;
    }

    function loadFirstLChildYarnGrid() {
        //
        this.dataSource = this.parentDetails.parentRowData.Childs;
        //if (menuParam == "MRPBAck") {
        //    this.dataSource = this.parentDetails.parentRowData.ChildItems;
        //}
        //else {
        //    this.dataSource = this.parentDetails.parentRowData.Childs;
        //}
    }

    async function initOtherItemTable(data) {
        if ($tblOtherItemEl) $tblOtherItemEl.destroy();

        //Child Grid Record
        var childColumns = await getYarnItemColumnsAsync(data, currentChildRowData);
        childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        //if (menuParam == "MRPBAck") {
        //    childColumns.unshift({ field: 'YBChildID', isPrimaryKey: true, visible: false });
        //}
        //else {
        //    childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        //}
        var additionalColumns = [
            {
                field: 'ShadeCode',
                headerText: 'Shade Code',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: data[0].YarnShadeBooks,
                displayField: "ShadeCode",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'YDProductionMasterID', headerText: 'YDProductionMasterID', width: 20, visible: false
            },
            { field: 'YDItem', headerText: 'YD Item?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            { field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            {
                field: 'Distribution', headerText: 'Yarn Distribution (%)', editType: "numericedit",
                /*edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100*/
                edit: { params: { showSpinButton: false, decimals: 2, min: 1 } }, width: 100
            },
            {
                field: 'BookingQty', headerText: 'Net Consumption', allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } }, width: 100
            },
            {
                field: 'Allowance', headerText: 'Allowance (%)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
            },
            {
                field: 'ReqQty', headerText: 'Req Qty(KG )', editType: "numericedit", allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100
            },
            {
                field: 'ReqCone', headerText: 'Req Cone(PCS)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100
            },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 }
        ];
        childColumns.push.apply(childColumns, additionalColumns);

        ej.base.enableRipple(true);
        $tblOtherItemEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser'],
            columns: [
                //Master Grid Record
                { field: 'FCMRMasterID', isPrimaryKey: true, visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'Construction', headerText: 'Construction', width: 20, allowEditing: false },
                { field: 'Composition', headerText: 'Composition', width: 20, allowEditing: false },
                { field: 'Color', headerText: 'Body Color', width: 20, allowEditing: false },
                { field: 'Length', headerText: 'Length', width: 20, allowEditing: false },
                { field: 'Width', headerText: 'Width', width: 20, allowEditing: false },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 20, allowEditing: false },
                { field: 'Qty', headerText: 'Booking Qty (Pcs)', width: 30, allowEditing: false, visible: false },
                { field: 'QtyInKG', headerText: 'Booking Qty (Kg)', width: 30, allowEditing: false, visible: false },
                { field: 'TotalQty', headerText: 'Req. Knitting Qty (Pcs)', width: 30, allowEditing: false },
                { field: 'TotalQtyInKG', headerText: 'Req. Knitting Qty (Kg)', width: 30, allowEditing: false }
            ],
            childGrid: {
                queryString: 'FCMRMasterID',
                //queryString: menuParam == "MRPBAck" ? 'YBChildID' : 'FCMRMasterID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                    }
                    else if (args.requestType === "add") {
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
                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG) * parseFloat(remainDis) / 100);
                        var reqQty = netConsumption;
                        args.data.Distribution = remainDis;
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.Allowance = 0.00;
                        args.data.ReqQty = reqQty.toFixed(2);
                        args.data.FCMRChildID = maxColCollar++; //getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "FCMRChildID");

                        args.data.FCMRMasterID = this.parentDetails.parentKeyFieldValue;
                        args.data.ReqCone = 1;
                    }
                    else if (args.requestType === "save") {
                        //Check YDItem when YD field check
                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }

                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG) * parseFloat(args.data.Distribution) / 100);
                        var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(args.data.Allowance)) / 100);
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.ReqQty = reqQty.toFixed(2);
                        args.data = getFreeConceptMRChild(args.data);

                        ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                        $tblOtherItemEl.updateRow(args.rowIndex, args.data);
                    }
                    else if (args.requestType === "delete") {
                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        //var index = $tblOtherItemEl.getRowIndexByPrimaryKey(args.data[0].FCMRChildID);
                        //if (index > -1) {
                        //    args.data.EntityState = 8;
                        //    masterData.Childs[index] = args.data;
                        //}
                    }
                },
                load: loadFirstLChildCollarGrid
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    collarYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    if (collarYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (collarYarnItem == null || collarYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < collarYarnItem.length; i++) {
                            var copiedItem = objectCopy(collarYarnItem[i]);
                            copiedItem.FCMRChildID = maxColYarn++;
                            copiedItem.FCMRMasterID = args.rowInfo.rowData.FCMRMasterID;
                            var netConsumption = (parseFloat(args.rowInfo.rowData.TotalQtyInKG) * parseFloat(copiedItem.Distribution) / 100);
                            var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(copiedItem.Allowance)) / 100);
                            copiedItem.BookingQty = netConsumption.toFixed(4);
                            copiedItem.ReqQty = reqQty.toFixed(2);
                            args.rowInfo.rowData.Childs.push(copiedItem);
                        }
                        $tblOtherItemEl.refresh();
                    }
                }
            }
        });
        $tblOtherItemEl.refreshColumns;
        $tblOtherItemEl.appendTo(tblOtherItemId);
    }

    function loadFirstLChildCollarGrid() {
        this.dataSource = this.parentDetails.parentRowData.Childs;
        //if (menuParam == "MRPBAck") {
        //    this.dataSource = this.parentDetails.parentRowData.ChildItems;
        //}
        //else {
        //    this.dataSource = this.parentDetails.parentRowData.Childs;
        //}
    }

    async function initCuffItemTable(data) {

        if ($tblCuffItemEl) $tblCuffItemEl.destroy();

        //Child Grid Record
        var childColumns = await getYarnItemColumnsAsync(data, currentChildRowData);
        childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        //if (menuParam == "MRPBAck") {
        //    childColumns.unshift({ field: 'YBChildID', isPrimaryKey: true, visible: false });
        //}
        //else {
        //    childColumns.unshift({ field: 'FCMRChildID', isPrimaryKey: true, visible: false });
        //}
        var additionalColumns = [
            {
                field: 'ShadeCode',
                headerText: 'Shade Code',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: data[0].YarnShadeBooks,
                displayField: "ShadeCode",
                edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'YDProductionMasterID', headerText: 'YDProductionMasterID', visible: false
            },
            { field: 'YDItem', headerText: 'YD Item?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            { field: 'YD', headerText: 'Go for YD?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            {
                field: 'Distribution', headerText: 'Yarn Distribution (%)', editType: "numericedit",
                /*edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100*/
                edit: { params: { showSpinButton: false, decimals: 2, min: 1 } }, width: 100
            },
            {
                field: 'BookingQty', headerText: 'Net Consumption', allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 0, format: "N2" } }, width: 100
            },
            {
                field: 'Allowance', headerText: 'Allowance (%)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
            },
            {
                field: 'ReqQty', headerText: 'Req Qty(KG )', editType: "numericedit", allowEditing: false,
                edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100
            },
            {
                field: 'ReqCone', headerText: 'Req Cone(PCS)', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 0, min: 1 } }, width: 100
            },
            { field: 'IsPR', headerText: 'Go for PR?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            /*{ field: 'StockQty', headerText: 'Stock Qty(KG)', allowEditing: false }*/
        ];
        childColumns.push.apply(childColumns, additionalColumns);

        ej.base.enableRipple(true);
        $tblCuffItemEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser'],
            columns: [
                //Master Grid Record
                { field: 'FCMRMasterID', isPrimaryKey: true, visible: false },
                { field: 'SubGroupID', visible: false },
                { field: 'Construction', headerText: 'Construction', width: 20, allowEditing: false },
                { field: 'Composition', headerText: 'Composition', width: 20, allowEditing: false },
                { field: 'Color', headerText: 'Body Color', width: 20, allowEditing: false },
                { field: 'Length', headerText: 'Length', width: 20, allowEditing: false },
                { field: 'Width', headerText: 'Width', width: 20, allowEditing: false },
                { field: 'TechnicalName', headerText: 'Technical Name', width: 20, allowEditing: false },
                { field: 'Qty', headerText: 'Booking Qty (Pcs)', width: 30, allowEditing: false, visible: false },
                { field: 'QtyInKG', headerText: 'Booking Qty (Kg)', width: 30, allowEditing: false, visible: false },
                { field: 'TotalQty', headerText: 'Req. Knitting Qty (Pcs)', width: 30, allowEditing: false },
                { field: 'TotalQtyInKG', headerText: 'Req. Knitting Qty (Kg)', width: 30, allowEditing: false }
            ],
            childGrid: {
                queryString: 'FCMRMasterID',
                //queryString: menuParam == "MRPBAck" ? 'YBChildID' : 'FCMRMasterID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                    }
                    else if (args.requestType === "add") {
                        //var maxId = Math.max.apply(Math, data.map(function (el) { return el[filterBy]; }));
                        //var result = data.filter(m => m[column] == search);
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
                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG) * parseFloat(remainDis) / 100);
                        var reqQty = netConsumption;
                        args.data.Distribution = remainDis;
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.Allowance = 0.00;
                        args.data.ReqQty = reqQty.toFixed(2);


                        args.data.FCMRChildID = maxColCuff++; //getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "FCMRChildID");
                        args.data.FCMRMasterID = this.parentDetails.parentKeyFieldValue;

                        //args.data.Distribution = 100.00;
                        //args.data.BookingQty = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG)).toFixed(2);
                        //args.data.Allowance = 0.00;
                        //args.data.ReqQty = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG)).toFixed(2);
                        args.data.ReqCone = 1;
                    }
                    else if (args.requestType === "save") {
                        //Check YDItem when YD field check
                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }

                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQtyInKG) * parseFloat(args.data.Distribution) / 100);
                        var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(args.data.Allowance)) / 100);
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.ReqQty = reqQty.toFixed(2);
                        args.data = getFreeConceptMRChild(args.data);

                        ////args.data.Segment1ValueId = !args.rowData.Segment1ValueId ? 0 : args.rowData.Segment1ValueId;
                        //args.data.Segment2ValueId = !args.rowData.Segment2ValueId ? 0 : args.rowData.Segment2ValueId;
                        //args.data.Segment3ValueId = !args.rowData.Segment3ValueId ? 0 : args.rowData.Segment3ValueId;
                        //args.data.Segment4ValueId = !args.rowData.Segment4ValueId ? 0 : args.rowData.Segment4ValueId;
                        //args.data.Segment5ValueId = !args.rowData.Segment5ValueId ? 0 : args.rowData.Segment5ValueId;
                        //args.data.Segment6ValueId = !args.rowData.Segment6ValueId ? 0 : args.rowData.Segment6ValueId;
                        //args.data.Segment7ValueId = !args.rowData.Segment7ValueId ? 0 : args.rowData.Segment7ValueId;
                        //args.data.Segment8ValueId = !args.rowData.Segment8ValueId ? 0 : args.rowData.Segment8ValueId;

                        $tblCuffItemEl.updateRow(args.rowIndex, args.data);
                    }
                    else if (args.requestType === "delete") {
                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        //var index = $tblCuffItemEl.getRowIndexByPrimaryKey(args.data[0].FCMRChildID);
                        //if (index > -1) {
                        //    args.data.EntityState = 8;
                        //    masterData.Childs[index] = args.data;
                        //}
                    }
                },
                load: loadFirstLChildCuffGrid
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    //cuffYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    collarYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    if (collarYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (collarYarnItem == null || collarYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < collarYarnItem.length; i++) {
                            var copiedItem = objectCopy(collarYarnItem[i]);
                            copiedItem.FCMRChildID = maxColYarn++;
                            copiedItem.FCMRMasterID = args.rowInfo.rowData.FCMRMasterID;
                            var netConsumption = (parseFloat(args.rowInfo.rowData.TotalQtyInKG) * parseFloat(copiedItem.Distribution) / 100);
                            var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(copiedItem.Allowance)) / 100);
                            copiedItem.BookingQty = netConsumption.toFixed(4);
                            copiedItem.ReqQty = reqQty.toFixed(2);
                            args.rowInfo.rowData.Childs.push(copiedItem);
                        }
                        $tblCuffItemEl.refresh();
                    }
                }
            }
        });
        $tblCuffItemEl.refreshColumns;
        $tblCuffItemEl.appendTo(tblCuffItemId);
    }

    function loadFirstLChildCuffGrid() {
        this.dataSource = this.parentDetails.parentRowData.Childs;
        //if (menuParam == "MRPBAck") {
        //    this.dataSource = this.parentDetails.parentRowData.ChildItems;
        //}
        //else {
        //    this.dataSource = this.parentDetails.parentRowData.Childs;
        //}
    }
    function backToListWithoutFilter() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
    }
    function backToList() {
        backToListWithoutFilter();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#FCMRMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(FBAckID) {
        var url = `/api/mr-bds/new/${FBAckID}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.BookingNo = masterData.OtherItems.length > 0 ? masterData.OtherItems[0].BookingNo : "";
                masterData.BookingDate = masterData.OtherItems.length > 0 ? formatDateToDefault(masterData.OtherItems[0].BookingDate) : "";
                setFormData($formEl, masterData);
                isFabric = 0, isCollar = 0, isCuff = 0;
                if (masterData.HasFabric) {
                    var FabricData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initYarnChildTable(FabricData);
                    $formEl.find("#divFabricInfo").show();
                    $formEl.find("#onlyFabric").prop('checked', true);
                    isFabric = 1;
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                    $formEl.find("#onlyFabric").prop('checked', false);
                    isFabric = 0;
                }
                if (masterData.HasCollar) {
                    var CollarData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initOtherItemTable(CollarData);
                    $formEl.find("#divOtherItem").show();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                    isCollar = 1;
                }
                else {
                    $formEl.find("#divOtherItem").hide();
                    $formEl.find("#fabricOtherItem").prop('checked', false);
                    isCollar = 0;
                }
                if (masterData.HasCuff) {
                    var CuffData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCuffItemTable(CuffData);
                    $formEl.find("#divCuffItem").show();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                    isCuff = 1;
                }
                else {
                    $formEl.find("#divCuffItem").hide();
                    $formEl.find("#onlyOtherItem").prop('checked', false);
                    isCuff = 0;
                }

                $formEl.find("#BookingID").val(masterData.BookingID);
                if (masterData.TrialNo == 0) {
                    $("#divRetrial").fadeOut();
                } else {
                    $("#divRetrial").fadeIn();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(grpConceptNo) {
        axios.get(`/api/mr-bds/${grpConceptNo}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                if (masterData.OtherItems.length > 0) {
                    masterData.BookingNo = masterData.OtherItems[0].BookingNo;
                    masterData.BookingDate = formatDateToDefault(masterData.OtherItems[0].BookingDate);
                    masterData.BookingID = masterData.OtherItems[0].BookingID;
                }
                setFormData($formEl, masterData);
                isFabric = 0, isCollar = 0, isCuff = 0;
                if (masterData.HasFabric) {
                    var FabricData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initYarnChildTable(FabricData);
                    $formEl.find("#divFabricInfo").show();
                    $formEl.find("#onlyFabric").prop('checked', true);
                    isFabric = 1;
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                    $formEl.find("#onlyFabric").prop('checked', false);
                    isFabric = 0;
                }
                if (masterData.HasCollar) {
                    var CollarData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initOtherItemTable(CollarData);
                    $formEl.find("#divOtherItem").show();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                    isCollar = 1;
                }
                else {
                    $formEl.find("#divOtherItem").hide();
                    $formEl.find("#fabricOtherItem").prop('checked', false);
                    isCollar = 0;
                }
                if (masterData.HasCuff) {
                    var CuffData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCuffItemTable(CuffData);
                    $formEl.find("#divCuffItem").show();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                    isCuff = 1;
                }
                else {
                    $formEl.find("#divCuffItem").hide();
                    $formEl.find("#onlyOtherItem").prop('checked', false);
                    isCuff = 0;
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getRevision(fbAckId, grpConceptNo) {
        axios.get(`/api/mr-bds/revision/${fbAckId}/${grpConceptNo}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                if (masterData.OtherItems.length > 0) {
                    masterData.BookingNo = masterData.OtherItems[0].BookingNo;
                    masterData.BookingDate = formatDateToDefault(masterData.OtherItems[0].BookingDate);
                    masterData.BookingID = masterData.OtherItems[0].BookingID;
                }
                setFormData($formEl, masterData);
                isFabric = 0, isCollar = 0, isCuff = 0;
                if (masterData.HasFabric) {
                    var FabricData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initYarnChildTable(FabricData);
                    $formEl.find("#divFabricInfo").show();
                    $formEl.find("#onlyFabric").prop('checked', true);
                    isFabric = 1;
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                    $formEl.find("#onlyFabric").prop('checked', false);
                    isFabric = 0;
                }
                if (masterData.HasCollar) {
                    var CollarData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initOtherItemTable(CollarData);
                    $formEl.find("#divOtherItem").show();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                    isCollar = 1;
                }
                else {
                    $formEl.find("#divOtherItem").hide();
                    $formEl.find("#fabricOtherItem").prop('checked', false);
                    isCollar = 0;
                }
                if (masterData.HasCuff) {
                    var CuffData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCuffItemTable(CuffData);
                    $formEl.find("#divCuffItem").show();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                    isCuff = 1;
                }
                else {
                    $formEl.find("#divCuffItem").hide();
                    $formEl.find("#onlyOtherItem").prop('checked', false);
                    isCuff = 0;
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getRevisionForCompleteList(grpConceptNo) {
        axios.get(`/api/mr-bds/revision/${grpConceptNo}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                if (masterData.OtherItems.length > 0) {
                    masterData.BookingNo = masterData.OtherItems[0].BookingNo;
                    masterData.BookingDate = formatDateToDefault(masterData.OtherItems[0].BookingDate);
                    masterData.BookingID = masterData.OtherItems[0].BookingID;
                    masterData.BuyerName = masterData.OtherItems[0].BuyerName;
                }
                setFormData($formEl, masterData);
                isFabric = 0, isCollar = 0, isCuff = 0;
                if (masterData.HasFabric) {
                    var FabricData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initYarnChildTable(FabricData);
                    $formEl.find("#divFabricInfo").show();
                    $formEl.find("#onlyFabric").prop('checked', true);
                    isFabric = 1;
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                    $formEl.find("#onlyFabric").prop('checked', false);
                    isFabric = 0;
                }
                if (masterData.HasCollar) {
                    var CollarData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initOtherItemTable(CollarData);
                    $formEl.find("#divOtherItem").show();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                    isCollar = 1;
                }
                else {
                    $formEl.find("#divOtherItem").hide();
                    $formEl.find("#fabricOtherItem").prop('checked', false);
                    isCollar = 0;
                }
                if (masterData.HasCuff) {
                    var CuffData = masterData.OtherItems.filter(function (el) { return el.ItemSubGroup == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCuffItemTable(CuffData);
                    $formEl.find("#divCuffItem").show();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                    isCuff = 1;
                }
                else {
                    $formEl.find("#divCuffItem").hide();
                    $formEl.find("#onlyOtherItem").prop('checked', false);
                    isCuff = 0;
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getAcknowledgeForList(YBookingNo) {
        //var YBookingNo = 122052200531;
        //axios.get(`/api/mr-bds/pendingacknowledgement/${grpConceptNo}`)
        //axios.get(`/api/yarn-booking/GetAsync/${YBookingNo}`)

        axios.get(`/api/yarn-booking/GetAsyncforAutoPR/${YBookingNo}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                //if (masterData.OtherItems.length > 0) {
                //    masterData.BookingNo = masterData.OtherItems[0].BookingNo;
                //    masterData.BookingDate = formatDateToDefault(masterData.OtherItems[0].BookingDate);
                //    masterData.BookingID = masterData.OtherItems[0].BookingID;
                //}
                masterData.BookingDate = formatDateToDefault(masterData.YBookingDate);
                setFormData($formEl, masterData);
                isFabric = 0, isCollar = 0, isCuff = 0;
                if (masterData.HasFabric) {
                    var FabricData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.FABRIC });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        FabricData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initYarnChildTable(FabricData);
                    $formEl.find("#divFabricInfo").show();
                    $formEl.find("#onlyFabric").prop('checked', true);
                    isFabric = 1;
                }
                else {
                    $formEl.find("#divFabricInfo").hide();
                    $formEl.find("#onlyFabric").prop('checked', false);
                    isFabric = 0;
                }
                if (masterData.HasCollar) {
                    var CollarData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.COLLAR });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CollarData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initOtherItemTable(CollarData);
                    $formEl.find("#divOtherItem").show();
                    $formEl.find("#fabricOtherItem").prop('checked', true);
                    isCollar = 1;
                }
                else {
                    $formEl.find("#divOtherItem").hide();
                    $formEl.find("#fabricOtherItem").prop('checked', false);
                    isCollar = 0;
                }
                if (masterData.HasCuff) {
                    var CuffData = masterData.Childs.filter(function (el) { return el.SubGroupName == subGroupNames.CUFF });
                    if (masterData.YarnShadeBooks != null && masterData.YarnShadeBooks.length > 0) {
                        CuffData[0].YarnShadeBooks = masterData.YarnShadeBooks;
                    }
                    initCuffItemTable(CuffData);
                    $formEl.find("#divCuffItem").show();
                    $formEl.find("#onlyOtherItem").prop('checked', true);
                    isCuff = 1;
                }
                else {
                    $formEl.find("#divCuffItem").hide();
                    $formEl.find("#onlyOtherItem").prop('checked', false);
                    isCuff = 0;
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }


    function isValidChildForm(data) {
        var isValidItemInfo = false;

        //Distribution check
        var dis = 0, ciFlag = 0;
        $.each(data, function (i, obj) {
            $.each(obj.Childs, function (j, objChild) {
                if (obj.FCMRMasterID == objChild.FCMRMasterID) {
                    dis = parseInt(dis) + parseInt(objChild.Distribution);
                    ciFlag = 1;
                }
            });

            if (parseInt(dis) != 100 && parseInt(ciFlag) == 1) {
                //toastr.error("Yarn Distribution must be 100%");
                //isValidItemInfo = true;
                dis = 0;
                ciFlag = 0;
            }
            dis = 0;
            ciFlag = 0;
        });

        //Child Items Validation Check

        for (var i = 0; i < data.length; i++) {
            if (data[i].Childs.length === 0) {
                toastr.error("At least 1 Yarn item is required.");
                isValidItemInfo = true;
                break;
            }
        }
        return isValidItemInfo;
    }

    function save(IsComplete) {
        //
        //Validation
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        //var data = formDataToJson($formEl.serializeArray());

        var mrListMain = [];
        var hasError = false;
        if (isFabric == 1) {
            var mrList = [];
            $tblChildEl.getCurrentViewRecords().map(x => {
                mrList.push(x);
            });
            if (mrList.length > 0) {
                for (var i = 0; i < mrList.length; i++) {
                    if (menuParam == "MRPB") {
                        mrList[i].Childs.map(y => { y.IsPR = true; });
                    }
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Yarn Information).");
                            hasError = true;
                            break;
                        }
                        else {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Yarn Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (isCollar == 1 && !hasError) {
            var mrList = [];
            $tblOtherItemEl.getCurrentViewRecords().map(x => {
                mrList.push(x);
            });

            if (mrList.length > 0 && !hasError) {
                for (var i = 0; i < mrList.length; i++) {
                    if (menuParam == "MRPB") {
                        mrList[i].Childs.map(y => { y.IsPR = true; });
                    }
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Collar Information).");
                            hasError = true;
                            break;
                        }
                        if (childs.length > 0) {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Collar Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (isCuff == 1 && !hasError) {
            var mrList = [];
            $tblCuffItemEl.getCurrentViewRecords().map(x => {
                mrList.push(x);
            });
            if (mrList.length > 0 && !hasError) {
                for (var i = 0; i < mrList.length; i++) {
                    if (menuParam == "MRPB") {
                        mrList[i].Childs.map(y => { y.IsPR = true; });
                    }
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Cuff Information).");
                            hasError = true;
                            break;
                        }
                        if (childs.length > 0) {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Cuff Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (hasError) return false;

        var hasIssue = false;
        var dyeingType = "";
        for (var i = 0; i < mrListMain.length; i++) {
            dyeingType = mrListMain[i].DyeingType;

            if (isYarn(dyeingType)) {
                var childs = mrListMain[i].Childs;
                if (childs.length > 0) {
                    var childYD = childs.find(x => x.YD == true);
                    if (typeof childYD === "undefined" || childYD == null) {
                        hasIssue = true;
                        break;
                    }
                }
            }
        }

        //if (IsComplete) {
        //    if (isValidChildForm(mrListMain)) return;
        //} 

        var data = mrListMain;
        $.each(data, function (i, obj) {
            obj.GroupConceptNo = $formEl.find('#BookingNo').val();
            obj.BookingID = $formEl.find('#BookingID').val();
            obj.IsComplete = IsComplete;
            obj.IsBDS = isBDS;
            obj.Modify = (status === statusConstants.PENDING) ? false : true
        });

        if (hasIssue) {
            showBootboxConfirm("Save Record.", "Do you want to continue without selecting any YD of " + dyeingType + " ?", function (yes) {
                if (yes) {
                    axios.post("/api/mr-bds/save", data)
                        .then(function () {
                            toastr.success("Saved successfully.");
                            backToList();
                        })
                        .catch(function (error) {
                            toastr.error(error.response.data.Message);
                        });
                }
            });
        } else {
            axios.post("/api/mr-bds/save", data)
                .then(function () {
                    toastr.success("Saved successfully.");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }
    }

    function isYarn(dyeingType) {
        if (dyeingType) {
            if (dyeingType.toUpperCase().indexOf('YARN') >= 0 && dyeingType.toUpperCase().indexOf('DYED') >= 0) return true;
        }
        return false;
    }

    function approve() {
        var url = `/api/mr-bds/remove-from-reject/${masterData.FCMRMasterID}`;
        axios.post(url)
            .then(function () {
                toastr.success(constants.ACKNOWLEDGE_SUCCESSFULLY);
                backToList();
            })
            .catch(showResponseError);
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

    function GetNumberValue(value) {
        if (isNaN(value) || typeof value === "undefined" || value == null) return 0;
        return value;
    }

    function revise(IsComplete) {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);
        //var data = formDataToJson($formEl.serializeArray());
        var mrListMain = [];

        var hasError = false;
        if (isFabric == 1) {
            var mrList = [];
            $tblChildEl.getCurrentViewRecords().map(x => {
                mrList.push(x);
            });

            if (mrList.length > 0) {
                for (var i = 0; i < mrList.length; i++) {
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Yarn Information).");
                            hasError = true;
                            break;
                        }
                        if (childs.length > 0) {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Yarn Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (isCollar == 1 && !hasError) {
            var mrList = [];
            $tblOtherItemEl.getCurrentViewRecords().map(x => {
                mrList.push(x);
            });
            if (mrList.length > 0 && !hasError) {
                for (var i = 0; i < mrList.length; i++) {
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Collar Information).");
                            hasError = true;
                            break;
                        }
                        if (childs.length > 0) {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Collar Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (isCuff == 1 && !hasError) {
            var mrList = [];
            $tblCuffItemEl.getCurrentViewRecords().map(x => {
                mrList.push(x);
            });
            if (mrList.length > 0 && !hasError) {
                for (var i = 0; i < mrList.length; i++) {
                    var childs = mrList[i].Childs;
                    if (IsComplete) {
                        if (childs.length == 0) {
                            toastr.error("At least 1 Yarn item is required (Cuff Information).");
                            hasError = true;
                            break;
                        }
                        if (childs.length > 0) {
                            var totalYarnDis = 0;
                            for (var j = 0; j < childs.length; j++) {
                                if (childs[j].Segment5ValueDesc) {
                                    if ((childs[j].Segment5ValueDesc.toLowerCase() == "melange" || childs[j].Segment5ValueDesc.toLowerCase() == "color melange") && (childs[j].ShadeCode == null || childs[j].ShadeCode == "")) {
                                        toastr.error("Select shade code for color melange");
                                        hasError = true;
                                        break;
                                    }
                                }
                                totalYarnDis += GetNumberValue(childs[j].Distribution);
                            }
                            if (hasError) break;
                            if (totalYarnDis != 100) {
                                toastr.error("Sum of Yarn Distribution must be 100 at Cuff Information. Current sum of Yarn Distribution is " + totalYarnDis);
                                hasError = true;
                                break;
                            }
                        }
                    }
                    mrList[i].IsComplete = IsComplete;
                    mrListMain.push(mrList[i]);
                }
            }
        }
        if (hasError) return false;

        var hasIssue = false;
        var dyeingType = "";
        for (var i = 0; i < mrListMain.length; i++) {

            dyeingType = mrListMain[i].DyeingType;
            if (isYarn(dyeingType)) {
                var childs = mrListMain[i].Childs;
                if (childs.length > 0) {
                    var childYD = childs.find(x => x.YD == true);
                    if (typeof childYD === "undefined" || childYD == null) {
                        hasIssue = true;
                        break;
                    }
                }
            }
        }

        //Load all data
        var data = mrListMain;

        if (hasIssue) {
            showBootboxConfirm("Save Record.", "Do you want to continue without selecting any YD of " + dyeingType + " ?", function (yes) {
                if (yes) {

                    $.each(data, function (i, obj) {
                        obj.GroupConceptNo = $formEl.find('#BookingNo').val();
                        obj.BookingID = $formEl.find('#BookingID').val();
                        obj.IsComplete = IsComplete;
                        obj.Modify = (status === statusConstants.PENDING) ? false : true;
                    });

                    axios.post("/api/mr-bds/revise", data)
                        .then(function () {
                            toastr.success("Revised successfully.");
                            backToList();
                        })
                        .catch(function (error) {
                            toastr.error(error.response.data.Message);
                        });
                }
            });
        } else {

            var isOwnRevise = false;
            if (status == statusConstants.COMPLETED) {
                isOwnRevise = true;
            }

            $.each(data, function (i, obj) {
                obj.GroupConceptNo = $formEl.find('#BookingNo').val();
                obj.BookingID = $formEl.find('#BookingID').val();
                obj.IsComplete = IsComplete;
                obj.Modify = (status === statusConstants.PENDING) ? false : true;
                obj.IsOwnRevise = isOwnRevise;
            });

            axios.post("/api/mr-bds/revise", data)
                .then(function () {
                    toastr.success("Revised successfully.");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }
    }
})();