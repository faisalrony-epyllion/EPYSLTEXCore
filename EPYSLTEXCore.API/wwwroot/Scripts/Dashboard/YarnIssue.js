 
(function () {
    var menuId,
        pageName;
    var toolbarId,
        pageId;
    var status = statusConstants.PROPOSED;
    var $divTblEl,
        $divDetailsEl,
        $toolbarEl,
        $tblMasterEl,
        $tblChildEl,
        $formEl,
        tblMasterId,
        tblChildId;

    var masterData;
    var isIssueReqPage = false,
        isApprovePage = false,
        isAcknowledgePage = false,
        status = statusConstants.PROPOSED;

    var formYarnIssue;
    var yarnIssueMaster;
    var yrStatus = 1;
    var IssueChilds = [];
    var isAllChecked = false;

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


        formYarnIssue = $("#formYarnIssue");

        $("#btnPendingIssue").css('background', '#008080');
        $("#btnPendingIssue").css('color', '#FFFFFF');

        initPendingYarnIssueMasterTable(); 

        $("#btnPendingIssue").on("click", function (e) {
            e.preventDefault();
            yrStatus = 1;
            isAllChecked = false; 
            initPendingYarnIssueMasterTable(); 
            $("#btnPendingIssue").css('background', '#008080');
            $("#btnPendingIssue").css('color', '#FFFFFF');

            $("#btnPendingIssueforApproval").css('background', '#FFFFFF');
            $("#btnPendingIssueforApproval").css('color', '#000000');

            $("#btnIssueLists").css('background', '#FFFFFF');
            $("#btnIssueLists").css('color', '#000000');
        });

        $("#btnPendingIssueforApproval").on("click", function (e) {
            e.preventDefault();
            yrStatus = 2;
            isAllChecked = true; 
            initPendingYarnIssueMasterTable(); 

            $("#btnPendingIssueforApproval").css('background', '#008080');
            $("#btnPendingIssueforApproval").css('color', '#FFFFFF');

            $("#btnPendingIssue").css('background', '#FFFFFF');
            $("#btnPendingIssue").css('color', '#000000');

            $("#btnIssueLists").css('background', '#FFFFFF');
            $("#btnIssueLists").css('color', '#000000');
        });

        $("#btnIssueLists").on("click", function (e) {
            e.preventDefault();
            yrStatus = 3;
            isAllChecked = true; 
            initPendingYarnIssueMasterTable(); 

            $("#btnIssueLists").css('background', '#008080');
            $("#btnIssueLists").css('color', '#FFFFFF');

            $("#btnPendingIssue").css('background', '#FFFFFF');
            $("#btnPendingIssue").css('color', '#000000');

            $("#btnPendingIssueforApproval").css('background', '#FFFFFF');
            $("#btnPendingIssueforApproval").css('color', '#000000');
        });

        $("#btnIssueUnApproved").on("click", function (e) {
            e.preventDefault();
            yrStatus = 3; 
            initPendingYarnIssueMasterTable(); 
        });

        $("#btnAddOrders").on("click", function (e) {
            e.preventDefault();
            getExportOrdersFromBuyerCompany();
            $("#modal-child").modal('show');
        });

        $("#btnAddIssueBy").on("click", function (e) {
            e.preventDefault();
            $("#modal-child-").modal('show');
            getIssueByUsers();
        });

        $("#btnSaveYIssue").click(function (e) {
            e.preventDefault();
            var data = formDataToJson(formYarnIssue.serializeArray());
            data["IssueChilds"] = yarnIssueMaster.IssueChilds;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/yissueApi/yarnIssueSave", data, config)
                .then(function () {
                    toastr.success("Your Issue saved successfully." + " Issue No: " + $("#IssueNo2").val());
                    YarnIssuebackToList();
                })
                .catch(showResponseError);
        });

        $("#btnYIssueEditCancel").on("click", function (e) {
            e.preventDefault();
            YarnIssuebackToList();
        });

        $("#btnApprovedYIssue").click(function (e) {
            e.preventDefault();

            var data = { IssueID: $("#IssueID").val() };

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/yissueApi/YarnIssueApprovedLists", data, config)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    $("#divNewYarnIssue").fadeOut();
                    $("#divtblYarnIssue").fadeIn();
                    $("#divYarnIssueButtonExecutions").fadeOut();
                    getPendingYarnIssueMasterData();
                })
                .catch(showResponseError);
        });

        $("#btnUnApprovedYReq").click(function (e) {
            e.preventDefault();

            bootbox.prompt("Are you sure you want to Unapproved this?", function (result) {
                if (!result) {
                    return toastr.error("Unapproved reason is required.");
                }

                var data = { IssueID: $("#IssueID").val() };
                data.UnapproveReason = result;

                var config = { headers: { 'Content-Type': 'application/json' } };
                axios.post("/yrApi/UnapproveYarnIssuelist", data, config)
                    .then(function () {
                        toastr.success(constants.APPROVE_SUCCESSFULLY);
                        $("#divNewYarnIssue").fadeOut();
                        $("#divtblYarnIssue").fadeIn();
                        $("#divYarnIssueButtonExecutions").fadeOut();
                        getPendingYarnIssueMasterData();
                    })
                    .catch(showResponseError);
            });
        });
    });

    function initPendingYarnIssueMasterTable() {
        var columns = []; 
        columns = [
            {
                headerText: 'Commands', width: 80, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            {
                field: 'ReqNo', headerText: 'Requisition No', visible: !isAllChecked
            },
            {
                field: 'ReqDateStr', headerText: 'Requisition Date', type: 'date', format: _ch_date_format_1, visible: !isAllChecked
            },
            {
                field: 'IssueNo', headerText: 'Issue No', visible: isAllChecked
            },
            {
                field: 'IssueDateStr', headerText: 'Issue Date', type: 'date', format: _ch_date_format_1, visible: isAllChecked
            },
            {
                field: 'ExportOrderNo', headerText: 'Export Order No'
            },
            {
                field: 'RequisitionBy', headerText: 'Requisition By'
            } 
        ]; 

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/yissueApi/YarnIssueLists?yrStatus=${yrStatus}`,
            columns: columns,
            autofitColumns: false,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {

        if (yrStatus ==1) { 
            $("#divNewYarnIssue").fadeIn();
            $("#divtblYarnIssue").fadeOut();
            $("#divYarnIssueButtonExecutions").fadeIn();
            $("#btnApprovedYReq").fadeOut();
            $("#btnSaveYIssue").fadeIn();
            $("#btnUnApprovedYReq").fadeIn();
            getYarnIssueNew(row.IssueID);
        }
        else if (yrStatus == 2) {
            $("#IssueID").val(row.IssueID);
            $("#divNewYarnIssue").fadeIn();
            $("#divtblYarnIssue").fadeOut();
            $("#divYarnIssueButtonExecutions").fadeIn();
            $("#btnApprovedYIssue").fadeIn();
            $("#btnSaveYIssue").fadeIn();
            $("#btnUnApprovedYReq").fadeOut();
            getYarnIssueEdit(row.IssueID);
        }
        else {
            $("#IssueID").val(row.IssueID);
            $("#divNewYarnIssue").fadeIn();
            $("#divtblYarnIssue").fadeOut();
            $("#divYarnIssueButtonExecutions").fadeIn();
            $("#btnApprovedYIssue").fadeOut();
            $("#btnSaveYIssue").fadeOut();
            getYarnIssueEdit(row.IssueID);
        }
    }

   
    function getYarnIssueNew(reqId) {
        url = "/yissueApi/getYarnIssueNew/" + reqId;

        axios.get(url)
            .then(function (response) {
                yarnIssueMaster = response.data; 
                $("#IssueNo2").val(response.data.IssueNo);
                $("#ReqID").val(response.data.ReqID);
                $("#ReqNo").val(response.data.ReqNo);
                $("#ReqDate").val(response.data.ReqDateStr);
                $("#ExportOrderID").val(response.data.ExportOrderID);
                $("#ExportOrderNo").val(response.data.ExportOrderNo);
                $("#ReqBy").val(response.data.ReqBy);
                $("#RequisitionBy").val(response.data.RequisitionBy); 
                $("#LocationID").select2({ 'data': response.data.LocationList });

                initTblYarnIssueChildItems();

                $("#tblyarnIssueChilds").bootstrapTable('load', response.data.IssueChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getYarnIssueEdit(id) {
        url = "/yissueApi/getYarnIssueEdit/" + id;

        axios.get(url)
            .then(function (response) {
                yarnIssueMaster = response.data;
                $("#IssueID").val(response.data.IssueID);
                $("#ReqID").val(response.data.ReqID);
                $("#ReqNo").val(response.data.ReqNo);
                $("#ReqDate").val(response.data.ReqDateStr);
                $("#ExportOrderID").val(response.data.ExportOrderID);
                $("#ExportOrderNo").val(response.data.ExportOrderNo);
                $("#ReqBy").val(response.data.ReqBy);
                $("#RequisitionBy").val(response.data.RequisitionBy);

                $("#IssueNo").val(response.data.IssueNo);
                $("#IssueDate").val(response.data.IssueDateStr);

                $("#LocationID").select2({ 'data': response.data.LocationList });
                $("#LocationID").val(response.data.LocationID).trigger("change");

                initTblYarnIssueChildItems();

                $("#tblyarnIssueChilds").bootstrapTable('load', response.data.IssueChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    async function initTblYarnIssueChildItems(data) {
        isEditable = true;
        if ($tblChildEl) $tblChildEl.destroy(); 
        var columns = [
            { field: 'ChildID', isPrimaryKey: true, visible: false },
            { field: 'YarnType', headerText: 'Yarn Type' },
            { field: 'YarnComposition', headerText: 'Yarn Composition' },
            { field: 'YarnCount', headerText: 'Yarn Count' },
            { field: 'YarnColor', headerText: 'Yarn Color' },
            { field: 'YarnShade', headerText: 'Shade' },
            { field: 'DisplayUnitDesc', headerText: 'Unit' },
            { field: 'StockQty', headerText: 'Stock Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'ReqQty', headerText: 'Requisition Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'IssueQty', headerText: 'Issue Qty', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'IssueChildRemarks', headerText: 'Remarks' }, 
            { field: 'Remarks', headerText: 'Remarks' }
        ]; 

        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.ChildID = getMaxIdForArray(masterData.IssueChilds, "ChildID");
                    args.data.ItemMasterID = getMaxIdForArray(masterData.IssueChilds, "ItemMasterID");
                    args.data.DisplayUnitDesc = "Kg";
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.ChildID); 
                    masterData.Childs[index] = args.data;
                }
            },
            //commandClick: childCommandClick,
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };
        tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        $tblChildEl = new initEJ2Grid(tableOptions);
    }

    function YarnIssuebackToList() {
        $("#divNewYarnIssue").fadeOut();
        $("#divtblYarnIssue").fadeIn();
        $("#divYarnIssueButtonExecutions").fadeOut();
        getPendingYarnIssueMasterData();
    }

//function resetTableParams() {
//    tableParams.offset = 0;
//    tableParams.limit = 10;
//    tableParams.filter = '';
//    tableParams.sort = '';
//    tableParams.order = '';
//}

})();