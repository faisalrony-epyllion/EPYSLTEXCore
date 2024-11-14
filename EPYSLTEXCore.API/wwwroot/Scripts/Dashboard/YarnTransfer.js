(function () {
    var menuId,
        pageName;
    var toolbarId,
        pageId;

    var $divTblEl,
        $divDetailsEl,
        $toolbarEl,
        $tblMasterEl,
        $tblChildEl,
        $formEl,
        tblMasterId,
        tblChildId,
        $tblExportOrderListsTo,
        $tblExportOrderListsFrom,
        $tblYarnTransferChildNew;

    var masterData;
    var isIssueReqPage = false,
        isApprovePage = false,
        isAcknowledgePage = false,
        status = statusConstants.PROPOSED;
    var formYarnTransfer;
    var YarnTransferMaster;

    var yTransferStatus = 1;
    var TransferChilds = [];
    var isAllChecked = false;
    var ExportOrderfilterBy = {};
    var ExportOrderfilterByToTransfer = {};


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
        tblChildTwistingId = "#tblChildTwisting" + pageId;
        $tblColorChildEl = $("#tblColorChild" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);


        formYarnTransfer = $("#formYarnTransfer");

        $("#btnPendingTransfer").css('background', '#008080');
        $("#btnPendingTransfer").css('color', '#FFFFFF');

        initPendingYarnTransferMasterTable();
        //getPendingYarnTransferMasterData();
        initExportOrderListsTable();
        initExportOrderListsToTable();

        $("#btnYReqNew").on("click", function (e) {
            e.preventDefault();
            resetForm();

            getNewYarnTransferData();

            $("#divNewYarnTransfer").fadeIn();
            $("#divtblYarnTransfer").fadeOut();
            $("#divYarnTransferButtonExecutions").fadeIn();

            $("#btnApprovedYReq").fadeOut();
            $("#btnUnApprovedYReq").fadeOut();

            $("#btnAddOrders").fadeIn();
            $("#btnAddOrdersTo").fadeIn();

            $("#btnYReqNew").css('background', '#008080');
            $("#btnYReqNew").css('color', '#FFFFFF');

            $("#btnPendingTransfer").css('background', '#FFFFFF');
            $("#btnPendingTransfer").css('color', '#000000');

            $("#btnTransferLists").css('background', '#FFFFFF');
            $("#btnTransferLists").css('color', '#000000');
        });

        $("#btnPendingTransfer").on("click", function (e) {
            e.preventDefault();
            yTransferStatus = 2;
            //resetTableParams();
            initPendingYarnTransferMasterTable();
            //getPendingYarnTransferMasterData();

            $("#btnPendingTransfer").css('background', '#008080');
            $("#btnPendingTransfer").css('color', '#FFFFFF');

            $("#btnTransferLists").css('background', '#FFFFFF');
            $("#btnTransferLists").css('color', '#000000');

            $("#btnYReqNew").css('background', '#FFFFFF');
            $("#btnYReqNew").css('color', '#000000');
        });

        $("#btnTransferLists").on("click", function (e) {
            e.preventDefault();
            yTransferStatus = 3;
            //resetTableParams();
            initPendingYarnTransferMasterTable();
            //getPendingYarnTransferMasterData();

            $("#btnTransferLists").css('background', '#008080');
            $("#btnTransferLists").css('color', '#FFFFFF');

            $("#btnPendingTransfer").css('background', '#FFFFFF');
            $("#btnPendingTransfer").css('color', '#000000');

            $("#btnYReqNew").css('background', '#FFFFFF');
            $("#btnYReqNew").css('color', '#000000');
        });

        $("#btnTransferUnApproved").on("click", function (e) {
            e.preventDefault();
            yTransferStatus = 3;
            //resetTableParams();
            initPendingYarnTransferMasterTable();
            //getPendingYarnTransferMasterData();
        });

        $("#btnAddOrders").on("click", function (e) {
            e.preventDefault();
            getExportOrdersFromBuyerCompany();
            $("#modal-child").modal('show');
        });

        $("#btnAddOrdersTo").on("click", function (e) {
            e.preventDefault();
            getExportOrdersFromBuyerCompanyToTransfer();
            $("#modal-child-").modal('show');
        });

        $("#btnSaveYarnTransfer").click(function (e) {
            e.preventDefault();
            var data = formDataToJson(formYarnTransfer.serializeArray());
            data["TransferChilds"] = TransferChilds;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/yTApi/YarnTransferSave", data, config)
                .then(function () {
                    toastr.success("Your Transfer saved successfully." + " Transfer No: " + $("#TransferNo2").val());
                    YarnTransferbackToList();
                })
                .catch(showResponseError);
        });

        $("#btnYarnTransferEditCancel").on("click", function (e) {
            e.preventDefault();
            YarnTransferbackToList();
        });

        $("#btnApprovedYarnTransfer").click(function (e) {
            e.preventDefault();

            var data = { Id: $("#Id").val() };

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/yTApi/YarnTransferApprovedLists", data, config)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    YarnTransferbackToList();
                    //getPendingYarnTransferMasterData();
                })
                .catch(showResponseError);
        });

        $("#btnUnApprovedYReq").click(function (e) {
            e.preventDefault();

            bootbox.prompt("Are you sure you want to Unapproved this?", function (result) {
                if (!result) {
                    return toastr.error("Unapproved reason is required.");
                }

                var data = { TransferMasterID: $("#TransferMasterID").val() };
                data.UnapproveReason = result;

                var config = { headers: { 'Content-Type': 'application/json' } };
                axios.post("/yrApi/UnapproveYarnTransferlist", data, config)
                    .then(function () {
                        toastr.success(constants.APPROVE_SUCCESSFULLY);
                        $("#divNewYarnTransfer").fadeOut();
                        $("#divtblYarnTransfer").fadeIn();
                        $("#divYarnTransferButtonExecutions").fadeOut();
                        //getPendingYarnTransferMasterData();
                    })
                    .catch(showResponseError);
            });
        });
    });

    function getYarnChildItems(exportOrderId) {
        axios.get("/yTApi/YarnTransfer_YarnChildItems/" + exportOrderId)
            .then(function (response) {
                TransferChilds = response.data;
                initTblYarnTransferChildItems();
                $("#tblYarnTransferChildNew").bootstrapTable('load', response.data);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initPendingYarnTransferMasterTable() {
        var columns = [];
        columns = [
            {
                headerText: 'Commands', width: 80, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            {
                field: 'TransferNo', headerText: 'Transfer No'
            },
            {
                field: 'TransferDateStr', headerText: 'Transfer Date', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'FromBuyerName', headerText: 'From Buyer'
            },
            {
                field: 'FromExportOrderNo', headerText: 'From EWO No'
            },
            {
                field: 'ToBuyerName', headerText: 'To Buyer'
            },

            {
                field: 'ToExportOrderNo', headerText: 'To EWO No'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/yTApi/YarnTransferLists?yTransferStatus=${yTransferStatus}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (yTransferStatus == 1) {
            $("#TransferMasterID").val(row.TransferMasterID);
            getYarnTransferEdit(row.TransferMasterID);
            $("#divNewYarnTransfer").fadeIn();
            $("#divtblYarnTransfer").fadeOut();
            $("#divYarnTransferButtonExecutions").fadeIn();
            $("#btnAddOrders").fadeOut();
            $("#btnAddOrdersTo").fadeOut();
            $("#btnApprovedYarnTransfer").fadeIn();
            $("#btnSaveYarnTransfer").fadeIn();
            $("#btnUnApprovedYarnTransfer").fadeIn();
        }
        else if (yTransferStatus == 2) {
            $("#TransferMasterID").val(row.TransferMasterID);
            getYarnTransferEdit(row.TransferMasterID);
            $("#divNewYarnTransfer").fadeIn();
            $("#divtblYarnTransfer").fadeOut();
            $("#divYarnTransferButtonExecutions").fadeIn();
            $("#btnAddOrders").fadeOut();
            $("#btnAddOrdersTo").fadeOut();
            $("#btnApprovedYarnTransfer").fadeIn();
            $("#btnSaveYarnTransfer").fadeIn();
            $("#btnUnApprovedYarnTransfer").fadeIn();
        }
        else if (yTransferStatus == 3) {
            $("#TransferMasterID").val(row.TransferMasterID);
            getYarnTransferEdit(row.TransferMasterID);
            $("#divNewYarnTransfer").fadeIn();
            $("#divtblYarnTransfer").fadeOut();
            $("#divYarnTransferButtonExecutions").fadeIn();
            $("#btnAddOrders").fadeOut();
            $("#btnAddOrdersTo").fadeOut();
            $("#btnApprovedYarnTransfer").fadeOut();
            $("#btnSaveYarnTransfer").fadeOut();
            $("#btnUnApprovedYarnTransfer").fadeOut();
        }
    }
    function getNewYarnTransferData() {
        url = "/yTApi/NewYarnTransfer/";

        axios.get(url)
            .then(function (response) {
                YarnTransferMaster = response.data;
                $("#TransferNo2").val(response.data.TransferNo);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getYarnTransferEdit(id) {
        url = "/yTApi/getYarnTransferEdit/" + id;

        axios.get(url)
            .then(function (response) {
                YarnTransferMaster = response.data;
                $("#TransferMasterID").val(response.data.TransferMasterID);
                $("#CompanyID").val(response.data.CompanyID);
                $("#TransferNo").val(response.data.TransferNo);
                $("#TransferDate").val(response.data.TransferDateStr);
                $("#FromExportOrderID").val(response.data.FromExportOrderID);
                $("#FromExportOrderNo").val(response.data.FromExportOrderNo);
                $("#FromBuyerID").val(response.data.FromBuyerID);
                $("#FromBuyerName").val(response.data.FromBuyerName);
                $("#ToExportOrderID").val(response.data.ToExportOrderID);
                $("#ToExportOrderNo").val(response.data.ToExportOrderNo);
                $("#ToBuyerID").val(response.data.ToBuyerID);
                $("#ToBuyerName").val(response.data.ToBuyerName);

                initTblYarnTransferChildItems();

                $("#tblYarnTransferChildNew").bootstrapTable('load', response.data.TransferChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initTblYarnTransferChildItems() {
        if ($tblYarnTransferChildNew) $tblYarnTransferChildNew.destroy();
        var columns = [];
        var columns = [
          /*  { field: 'YDReqIssueChildID', isPrimaryKey: true, visible: false },*/
            { field: 'YarnType', headerText: 'Yarn Type' },
            { field: 'YarnComposition', headerText: 'Yarn Composition' },
            { field: 'YarnCount', headerText: 'Yarn Count'},
            { field: 'YarnColor', headerText: 'Yarn Color'},
            { field: 'YarnShade', headerText: 'Shade'}, 
            { field: 'DisplayUnitDesc', headerText: 'Unit'},
            { field: 'StockQty', headerText: 'Stock Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'TransferQty', headerText: 'Transfer Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'Remarks', headerText: 'Remarks' }
        ];
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    //args.data.YDReqIssueChildID = getMaxIdForArray(masterData.Childs, "YDReqIssueChildID");
                    //args.data.ItemMasterID = getMaxIdForArray(masterData.Childs, "ItemMasterID");
                    //args.data.DisplayUnitDesc = "Kg";
                }
                else if (args.requestType === "save") {
                    //var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.YDReqIssueChildID);
                    //args.data.BrandName = args.rowData.BrandName;
                    //masterData.Childs[index] = args.data;
                }
            },
            //commandClick: childCommandClick,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };
        //tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        //$tblChildEl = new initEJ2Grid(tableOptions); 
    }

    function resetForm() {
        formYarnTransfer.trigger("reset");
        formYarnTransfer.find("#TransferMasterID").val(-1111);
        formYarnTransfer.find("#EntityState").val(4);
    }

    function YarnTransferbackToList() {
        $("#divNewYarnTransfer").fadeOut();
        $("#divtblYarnTransfer").fadeIn();
        $("#divYarnTransferButtonExecutions").fadeOut();
        //getPendingYarnTransferMasterData();

        $("#btnYReqNew").css('background', '#FFFFFF');
        $("#btnYReqNew").css('color', '#000000');

        $("#btnPendingTransfer").css('background', '#FFFFFF');
        $("#btnPendingTransfer").css('color', '#000000');

        $("#btnTransferLists").css('background', '#FFFFFF');
        $("#btnTransferLists").css('color', '#000000');
    }

    //function resetTableParams() {
    //    tableParams.offset = 0;
    //    tableParams.limit = 10;
    //    tableParams.filter = '';
    //    tableParams.sort = '';
    //    tableParams.order = '';
    //}

    function getExportOrdersFromBuyerCompany() {
        var queryParams = $.param(ExportOrderTableParams);
        url = "/yTApi/YarnTransfer_YarnPOExportOrderLists" + "?" + queryParams;
        axios.get(url)
            .then(function (response) {
                exportOrderLists = response.data;
                $("#tblExportOrderListsFrom").bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(err.response.data.Message);
            })
    };

    function getExportOrdersFromBuyerCompanyToTransfer() {
        axios.get(`/yTApi/YarnTransfer_YarnPOExportOrderLists`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                copiedRecord = null;
                masterData = response.data;

                exportOrderLists = response.data;

                //setFormData($formEl, masterData);
                //initChildTable(masterData.Childs);
                //$("#tblExportOrderListsTo").bootstrapTable('load', masterData);
                initExportOrderListsToTable(masterData);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });

        //var queryParams = $.param(ExportOrderTableParams);
        //url = "/yTApi/YarnTransfer_YarnPOExportOrderLists" + "?" + queryParams;
        //axios.get(url)
        //    .then(function (response) {
        //        exportOrderLists = response.data;
        //        $("#tblExportOrderListsTo").bootstrapTable('load', response.data);
        //    })
        //    .catch(function () {
        //        toastr.error(err.response.data.Message);
        //    })
    };

    function initExportOrderListsTable() {
        if ($tblExportOrderListsFrom) $tblExportOrderListsFrom.destroy();
        var columns = [];
        var columns = [
            { field: 'YDReqIssueChildID', isPrimaryKey: true, visible: false },
            { field: 'CompanyName', headerText: 'Shade Code' },
            { field: 'NoOfThread', headerText: 'No of Thread' },
            { field: 'ReqQty', headerText: 'Req. Qty', allowEditing: false },
            { field: 'IssueQty', headerText: 'Issue Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'IssueQtyCone', headerText: 'Issue Qty(Cone)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'IssueQtyCarton', headerText: 'IssueQty(Cart)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'Rate', headerText: 'Rate', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 1 } } },
            { field: 'LotNo', headerText: 'Lot No' },
            { field: 'DisplayUnitDesc', headerText: 'Unit', allowEditing: false },
            { field: 'Remarks', headerText: 'Remarks' }
        ];
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            //actionBegin: function (args) {
            //    if (args.requestType === "add") {
            //        args.data.YDReqIssueChildID = getMaxIdForArray(masterData.Childs, "YDReqIssueChildID");
            //        args.data.ItemMasterID = getMaxIdForArray(masterData.Childs, "ItemMasterID");
            //        args.data.DisplayUnitDesc = "Kg";
            //    }
            //    else if (args.requestType === "save") {
            //        var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.YDReqIssueChildID);
            //        args.data.BrandName = args.rowData.BrandName;
            //        masterData.Childs[index] = args.data;
            //    }
            //},
            //commandClick: childCommandClick,
            commandDBClick: commandDBClick_ExportOrderListsTo,
            //onPageChange:
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };
        tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        $tblExportOrderListsTo = new initEJ2Grid(tableOptions);
        
    }

    function initExportOrderListsToTable(data) {
        if ($tblExportOrderListsTo) $tblExportOrderListsTo.destroy();
        var columns = [
            { field: 'CompanyName', headerText: 'Company' },
            { field: 'ExportOrderNo', headerText: 'Export Order No' },
            { field: 'BuyerName', headerText: 'Buyer Name' },
            { field: 'BuyerTeam', headerText: 'Buyer Team' },
            { field: 'StyleNo', headerText: 'Style No' },
            { field: 'ReceiveQty', headerText: 'Receive Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } }
        ];

        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            //actionBegin: function (args) {
            //    if (args.requestType === "add") {
            //        args.data.YDReqIssueChildID = getMaxIdForArray(masterData.Childs, "YDReqIssueChildID");
            //        args.data.ItemMasterID = getMaxIdForArray(masterData.Childs, "ItemMasterID");
            //        args.data.DisplayUnitDesc = "Kg";
            //    }
            //    else if (args.requestType === "save") {
            //        var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.YDReqIssueChildID);
            //        args.data.BrandName = args.rowData.BrandName;
            //        masterData.Childs[index] = args.data;
            //    }
            //},
            //commandClick: childCommandClick,
            commandDBClick: commandDBClick_ExportOrderListsTo,  
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };
        tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        $tblExportOrderListsTo = new initEJ2Grid(tableOptions);
    }
    function commandDBClick_ExportOrderListsTo(args) {
        $("#modal-child-").modal('hide');
        $("#ToExportOrderID").val(row.ExportOrderID);
        $("#ToExportOrderNo").val(row.ExportOrderNo);
        $("#ToBuyerID").val(row.BuyerID);
        $("#ToBuyerName").val(row.BuyerName);
        $("#CompanyID").val(row.CompanyID);
        $("#SupplierID").val(row.SupplierID);
        getYarnChildItems(row.ExportOrderID);
    }
})();