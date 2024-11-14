(function () {
    var menuId, pageName;
    var toolbarId;
    
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId;
    var filterBy = {};
    var status = statusConstants.APPROVED;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

   

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.APPROVED;
            initMasterTable();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACKNOWLEDGE;
            initMasterTable();
        });
        
    });

    function initMasterTable() {
        var dataManager = ej2GetData(`/api/rnd-knitting-production/list?status=${status}`);
        if ($tblMasterEl) $tblMasterEl.destroy();
        ej.base.enableRipple(true);
        debugger;
        status = status;
        $tblMasterEl = new ej.grids.Grid({
            allowExcelExport: true,
            allowPdfExport: true,
            dataSource: dataManager,
            allowResizing: true,
            allowFiltering: true,
            allowPaging: true,
            commandClick: handleCommands,
            pageSettings: { pageCount: 5, currentPage: 1, pageSize: 10, pageSizes: true },
            columns: [
                {
                    headerText: '', width: 80, visible: status == statusConstants.APPROVED, commands: [
                        { type: 'Edit', title: 'Complete QC this', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-check-square' } }]// iconCss: 'fa fa-check-square e-icons' //iconCss: 'e-edit e-icons'
                },
               
                {
                    type: 'Id', visible: false
                },
               
                {
                    field: 'KJobCardNo', headerText: 'Job Card No'
                },
                {
                    field: 'ProductionDate', headerText: 'ProductionDate', type: 'date', format: _ch_date_format_1
                },
                {
                    field: 'GroupConceptNo', headerText: 'Concept No'
                },
                {
                    field: 'ProdQty', headerText: 'Production Qty'
                },
                {
                    field: 'RollNo', headerText: 'Roll No'
                },
                {
                    field: 'Buyer', headerText: 'Buyer'
                },
                {
                    field: 'BuyerTeam', headerText: 'Buyer Team'
                },
                {
                    field: 'RollQty', headerText: 'Roll Qty'
                }
            ],
        });

        $tblMasterEl.appendTo(tblMasterId);
    }
    function handleCommands(args) {
        showBootboxConfirm("Send Record.", "Are you sure to Complete?", function (yes) {
            if (yes) {
                var id = args.rowData.Id;
                axios.post(`/api/rnd-knitting-production/complete/${id}`)
                    .then(function () {
                        toastr.success("Sending successfully.");
                        backToList();
                    })
                    .catch(showResponseError);
            }
        });
        
    }
    function backToList() {
        initMasterTable();
    }
})();