(function () {
    var menuId, pageName, pageId;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, tblMasterId, tblChildId, $tblMasterEl, $tblChildEl, $tblItemChildE1, tblItemChildId, $formEl;
    var filterBy = {};
    var status;
    var masterData;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        tblItemChildId = "#tblItemChild" + pageId;
        tblPostChildId = "#tblPostChild" + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING;
        initMasterTable();
        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
        });
    });
    function initMasterTable() {
        var commands = status == statusConstants.PENDING
            ? [{ type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }]
            : [
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                { type: 'Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ];

        var columns = [
            { headerText: 'Actions', commands: commands, width: 15, textAlign: 'Center' },
            { field: 'BatchNo', headerText: 'Batch No', width: 20 },
            { field: 'KnittingType', headerText: 'Machine Type', width: 25 },
            { field: 'ColorName', headerText: 'Color', width: 15 },
            { field: 'FabricGsm', headerText: 'Fabric GSM', width: 15 },
            { field: 'Qty', headerText: 'Qty (Kg)', width: 15 },
            { field: 'QtyPcs', headerText: 'Qty (Pcs)', width: 15 }
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/finish-fabric-stock/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.FFSFormID);
        }
        else { }
    }
    function getDetails(id, flag) {
        axios.get(`/api/rnd-stock/${id}/${flag}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;

                setFormData($formEl, masterData);
                initChildTable(masterData.childs);
            })
            .catch(showResponseError);

    }

})();