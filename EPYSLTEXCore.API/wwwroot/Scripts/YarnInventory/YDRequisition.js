(function () {
    var menuId, pageName;
    var toolbarId, pageId, pageIdWithHash;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var status;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var _paramType = {
        YDReq: 0,
        YDReqAppr: 1
    }
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
        pageIdWithHash = "#" + pageId;
        menuType = localStorage.getItem("YDReqPage");
        menuType = parseInt(menuType);

        if (menuType == _paramType.YDReq) {

            $toolbarEl.find("#btnPending").show();
            $toolbarEl.find("#btnList").show();
            $toolbarEl.find("#btnPendingApprovalList").hide();
            $toolbarEl.find("#btnApprovedList").show();

            toggleActiveToolbarBtn($(pageIdWithHash).find("#btnPending"), $toolbarEl);
            status = statusConstants.PENDING;
            $divDetailsEl.find("#btnSave").show();
            $divDetailsEl.find("#btnApprove").hide();
            $divDetailsEl.find("#btnReject").hide();

            initMasterTable();

            $toolbarEl.find("#btnPending").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                resetTableParams();
                status = statusConstants.PENDING;

                initMasterTable();

                $divDetailsEl.find("#btnSave").show();
                $divDetailsEl.find("#btnApprove").hide();
                $divDetailsEl.find("#btnReject").hide();
            });

            $toolbarEl.find("#btnList").on("click", function (e) {

                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                resetTableParams();
                status = statusConstants.PENDING_CONFIRMATION;

                initMasterTable();

                $divDetailsEl.find("#btnSave").show();
                $divDetailsEl.find("#btnApprove").hide();
                $divDetailsEl.find("#btnReject").hide();
            });
            $toolbarEl.find("#btnApprovedList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                resetTableParams();
                status = statusConstants.APPROVED;

                initMasterTable();

                $divDetailsEl.find("#btnSave").hide();
                $divDetailsEl.find("#btnApprove").hide();
                $divDetailsEl.find("#btnReject").hide();
            });

            $formEl.find("#btnSave").click(function (e) {
                e.preventDefault();
                save(false, false);
            });

        }
        else if (menuType == _paramType.YDReqAppr) {
            $toolbarEl.find("#btnPending").hide();
            $toolbarEl.find("#btnList").hide();
            $toolbarEl.find("#btnPendingApprovalList").show();
            $toolbarEl.find("#btnApprovedList").show();

            toggleActiveToolbarBtn($(pageIdWithHash).find("#btnPendingApprovalList"), $toolbarEl);
            status = statusConstants.PENDING_CONFIRMATION;
            $divDetailsEl.find("#btnSave").hide();
            $divDetailsEl.find("#btnApprove").show();
            $divDetailsEl.find("#btnReject").show();

            initMasterTable();

            $toolbarEl.find("#btnPendingApprovalList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                resetTableParams();
                status = statusConstants.PENDING_CONFIRMATION;

                initMasterTable();

                $divDetailsEl.find("#btnSave").hide();
                $divDetailsEl.find("#btnApprove").show();
                $divDetailsEl.find("#btnReject").show();
            });

            $toolbarEl.find("#btnApprovedList").on("click", function (e) {
                e.preventDefault();
                toggleActiveToolbarBtn(this, $toolbarEl);
                resetTableParams();
                status = statusConstants.APPROVED;

                initMasterTable();

                $divDetailsEl.find("#btnSave").hide();
                $divDetailsEl.find("#btnApprove").hide();
                $divDetailsEl.find("#btnReject").hide();
            });

            $formEl.find("#btnApprove").click(function (e) {
                e.preventDefault();
                save(true, false);
            });
            $formEl.find("#btnReject").click(function (e) {
                e.preventDefault();
                save(false, true);
            });
        }

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function initMasterTable() {
        var columns = [
            {
                headerText: '', width: 100, visible: status == statusConstants.PENDING, commands: [
                    { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
                ]
            },
            {
                headerText: '', width: 100, visible: status == statusConstants.PENDING_CONFIRMATION, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Yarn Dyeing Requisition Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            },
            {
                headerText: '', width: 100, visible: status == statusConstants.APPROVED, commands: [
                    { type: 'View', title: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    { type: 'Yarn Dyeing Requisition Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            },

            {
                field: 'ConceptNo', headerText: 'Concept No', width: 100
            },
            {
                field: 'YDBookingNo', headerText: 'YD Booking No', width: 100
            },
            {
                field: 'YDBookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 100
            },
            {
                field: 'BuyerName', headerText: 'Buyer', width: 100
            },
            {
                field: 'BookingByUser', headerText: 'Booking By', width: 100
            },
            {
                field: 'YDReqNo', headerText: 'Req No', visible: status != statusConstants.PENDING, width: 100
            },
            {
                field: 'YDReqDate', headerText: 'Req Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status == statusConstants.PENDING_CONFIRMATION, width: 100
            },
            {
                field: 'ReqByUser', headerText: 'Req By', visible: status == statusConstants.PENDING_CONFIRMATION, width: 100
            },
            {
                field: 'ReqQty', headerText: 'Booking Qty', width: 100
            },
            {
                field: 'RequestedQty', headerText: 'Req Qty', width: 100
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yd-requisition/list/${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {

        if (status == statusConstants.PENDING) {
            getNew(args.rowData.YDBookingMasterID, args.rowData.IsBDS);
            //$formEl.find("#btnSave").fadeIn();
        }
        else if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.YDReqMasterID);
            //$formEl.find("#btnSave").fadeIn();
        }
        else if (args.commandColumn.type == 'View') {
            getDetails(args.rowData.YDReqMasterID);
            //$formEl.find("#btnSave").fadeIn();
        }
        else if (args.commandColumn.type == 'Yarn Dyeing Requisition Report') {
            /*
            var a = document.createElement('a');
            a.href = "/reports/InlinePdfView?ReportName=Yarn Dyeing Requisition.Rdl=" + args.rowData.ConceptNo;
            a.setAttribute('target', '_blank');
            a.click();
            */
            window.open(`/reports/InlinePdfView?ReportName=YDYarnRequisitionSlip.rdl&RequisitionID=${args.rowData.YDReqMasterID}`, '_blank');
        }

    }

    async function initChildTable(data) {
        isEditable = true;

        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        columns.push(
            {
                headerText: 'Commands', width: 120, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            },
        );
        //columns.push.apply(columns, await getYarnItemColumnsForDisplayOnly());
        columns.push.apply(columns, [{ field: 'YarnCategory', headerText: 'Yarn Description', allowEditing: false }]);
        columns.push.apply(columns, [{ field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false }]);
        //
        var additionalColumns = [
            { field: 'AllocationChildItemID', isPrimaryKey: true, visible: false },
            { field: 'YDReqMasterID', visible: false },
            { field: 'SpinnerID', headerText: 'Spinner', visible: false },
            { field: 'SpinnerName', headerText: 'Spinner', allowEditing: false },
            { field: 'LotNo', headerText: 'Lot No', allowEditing: false },
            { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false },
            //{ field: 'NoOfThread', headerText: 'No of Thread', allowEditing: false },
            { field: 'AllocatedQty', headerText: 'Allocated Qty', visible: masterData.IsBDS == 2, allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
            { field: 'NetYarnReqQty', headerText: 'Yarn Req Qty', allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
            { field: 'PendingQty', headerText: 'Pending Qty', allowEditing: false, editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } }, visible: status == statusConstants.PENDING },
            { field: 'ReqQty', headerText: 'Req Qty(kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 2, min: 0 } } },
            { field: 'ReqCone', headerText: 'Req Cone(pcs)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            //{ field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },
            { field: 'StockQty', headerText: 'Stock Qty', allowEditing: false, visible: masterData.IsBDS != 2 },
            { field: 'Remarks', headerText: 'Remarks' }
        ];
        columns.push.apply(columns, additionalColumns);

        if (typeof masterData.StockTypeList === "undefined" || masterData.StockTypeList == null) {
            masterData.StockTypeList = [];
        }

        var stockTypeCell = "";
        if (menuType != _paramType.YDReqAppr & (status == statusConstants.PENDING_CONFIRMATION || status == statusConstants.PENDING || status == statusConstants.REVISE)) {
            stockTypeCell = {
                field: 'StockTypeId',
                headerText: 'Stock Type',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.StockTypeList,
                displayField: "text",
                allowEditing: true,
                visible: masterData.IsBDS != 2,
                edit: ej2GridDropDownObj({
                })
            };

        }
        else {
            stockTypeCell = {
                field: 'StockTypeName',
                headerText: 'Stock Type',
                allowEditing: false,
                visible: masterData.IsBDS != 2
            }
        }

        var indexS = columns.findIndex(x => x.field == 'ReqCone');
        indexS = indexS + 1;
        columns.splice(indexS, 0, stockTypeCell);

        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            /*showTimeIndicator :false,*/
            actionBegin: function (args) {
                //console.log("requestType");
                //console.log(args.requestType);
                if (args.requestType === "add") {
                    args.data.YDReqMasterID = getMaxIdForArray(masterData.Childs, "YDReqMasterID");
                    args.data.DisplayUnitDesc = "Kg";
                    args.data.BookingQty = 0;
                    args.data.ReqQty = 0;
                }
                else if (args.requestType === "save") {
                    if (args.data.ReqQty > args.data.PendingQty) {
                        toastr.error(`Req Qty ${args.data.ReqQty} cannot be greater than Pending Qty ${args.data.PendingQty}`);
                        args.data.ReqQty = args.data.PendingQty;
                        args.rowData = args.data;
                        return false;
                    }
                    if (masterData.IsBDS != 2) {
                        args.data.StockQty = parseInt(args.data.StockTypeId) == 3 ? parseFloat(args.data.AdvanceStockQty) : parseFloat(args.data.SampleStockQty);
                    }

                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.YDReqMasterID);
                    //args.data.CompanyNames = args.rowData.CompanyNames; 
                    masterData.Childs[index] = args.data;
                }
            },
            //commandClick: childCommandClick,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };

        /*tableOptions["toolbar"] = ['Add'];*/
        tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        $tblChildEl = new initEJ2Grid(tableOptions);
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
        $formEl.find("#YDReqMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }
    function getNew(ydBookingMasterId, isBDS) {
        axios.get(`/api/yd-requisition/new/${ydBookingMasterId}/${isBDS}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                /*copiedRecord = null;*/
                masterData = response.data;
                masterData.YDReqDate = formatDateToDefault(masterData.YDReqDate);
                masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);

                $formEl.find("#btnSave").fadeIn();
            })
            .catch(showResponseError);
    }
    function getDetails(id) {
        var url = `/api/yd-requisition/${id}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.YDReqDate = formatDateToDefault(masterData.YDReqDate);
                masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);
                setFormData($formEl, masterData);

                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }
    function save(isApprove = false, isReject = false) {
        //Data get for save process
        var data = formElToJson($formEl);
        data.Childs = $tblChildEl.getCurrentViewRecords();
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");

        //Child Validation check 
        if (isValidChildForm(data)) return;

        //Data send to controller
        //console.log(data);
        data.BuyerId = 0;

        var hasError = false;
        if (data.IsBDS == 2) {
            for (var i = 0; i < data.Childs.length; i++) {
                if (data.Childs[i].ReqQty > data.Childs[i].AllocatedQty) {
                    toastr.error(`Req Qty ${data.Childs[i].ReqQty} cannot be greater than Allocated Qty ${data.Childs[i].AllocatedQty}`);
                    hasError = true;
                    break;
                }
                if (hasError) break;
            }
            if (hasError) return false;

            var hasError = false;
            for (var i = 0; i < data.Childs.length; i++) {
                var child = data.Childs[i];
                var totalReqQty = getTotalReqQtyOfCurrentItem(data.Childs, child.YBChildItemID);
                var maxReqQty = child.PendingQty; //getMaxAllocatedQty(data.Childs, child.YBChildItemID);
                if (totalReqQty > maxReqQty) {
                    toastr.error(`Total req qty is ${totalReqQty} cannot be greater than maximum req qty is ${maxReqQty}. `);
                    hasError = true;
                    break;
                }
            }
            if (hasError) return false;

        }
        else {
            var hasError = false;
            for (var i = 0; i < data.Childs.length; i++) {
                if (data.Childs[i].ReqQty > data.Childs[i].StockQty) {
                    toastr.error(`Req Qty ${data.Childs[i].ReqQty} cannot be greater than Stock Qty ${data.Childs[i].StockQty}`);
                    hasError = true;
                    break;
                }
                if (hasError) break;
            }
            if (hasError) return false;

            var hasError = false;
            for (var i = 0; i < data.Childs.length; i++) {
                var child = data.Childs[i];
                var totalReqQty = child.ReqQty//getTotalReqQtyOfCurrentItem(data.Childs, child.YBChildItemID);
                var maxReqQty = child.PendingQty; //getMaxAllocatedQty(data.Childs, child.YBChildItemID);
                if (totalReqQty > maxReqQty) {
                    toastr.error(`Total req qty is ${totalReqQty} cannot be greater than maximum req qty is ${maxReqQty}. `);
                    hasError = true;
                    break;
                }
            }
            if (hasError) return false;

            var hasError = false;
            for (var i = 0; i < data.Childs.length; i++) {
                var child = data.Childs[i];
                var totalReqQty = getTotalReqQtyOfCurrentStockSet(data.Childs, child.YarnStockSetId);
                var maxReqQty = child.StockQty; //getMaxAllocatedQty(data.Childs, child.YBChildItemID);
                if (totalReqQty > maxReqQty) {
                    toastr.error(`Total req qty is ${totalReqQty} cannot be greater than stock qty is ${maxReqQty}. `);
                    hasError = true;
                    break;
                }
            }
            if (hasError) return false;

        }
        if (hasError) return false;

        var path = "/api/yd-requisition/save";
        if (isApprove == true) {
            path = "/api/yd-requisition/approve"
        }
        else if (isReject == true) {
            path = "/api/yd-requisition/reject"
        }
        else {
            path = "/api/yd-requisition/save";
        }
        axios.post(path, data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    function isValidChildForm(data) {
        var isValidItemInfo = false;

        $.each(data.Childs, function (i, el) {
            if (el.ReqQty == "" || el.ReqQty == null || el.ReqQty <= 0) {
                toastr.error("Req Qty is required.");
                isValidItemInfo = true;
            }
            else if (el.BookingQty == "" || el.BookingQty == null || el.BookingQty <= 0) {
                toastr.error("Booking Qty is required.");
                isValidItemInfo = true;
            }
        });

        return isValidItemInfo;
    }
    function getTotalReqQtyOfCurrentItem(childs, ybChildItemID) {
        var totalReqQty = 0;
        childs.filter(x => x.YBChildItemID == ybChildItemID).map(x => {
            totalReqQty += getDefaultValueWhenInvalidN_Float(x.ReqQty);
        });
        return totalReqQty;
    }
    function getTotalReqQtyOfCurrentStockSet(childs, yarnStockSetId) {
        var totalReqQty = 0;
        childs.filter(x => x.YarnStockSetId == yarnStockSetId).map(x => {
            totalReqQty += getDefaultValueWhenInvalidN_Float(x.ReqQty);
        });
        return totalReqQty;
    }
    var validationConstraints = {
        YDReqNo: {
            presence: true
        }
    }
})();

