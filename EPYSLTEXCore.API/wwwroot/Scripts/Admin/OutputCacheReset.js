(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl, tblMasterId;
    var filterSetup, filterSetups = []

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        //$tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;

        initMasterTable();


    });
    function initMasterTable() {
        var commands = [
            { type: 'Reset', title: 'Reset', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-refresh' } },
/*            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } }*/
        ];

        columns = [
            { field: 'CacheID', isPrimaryKey: true, visible: false },
            { field: 'CacheKey', headerText: 'Cache Key', width: 35 },
            { field: 'CacheDetails', headerText: 'Cache Details', width: 35 },
            { field: 'ApiName', headerText: 'Api', width: 35 },
            { field: 'ParameterValue', headerText: 'Parameter Value', width: 35 },
            {
                headerText: 'Reset', textAlign: 'Center', commands: commands,
                textAlign: 'Center', width: 30
            },
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            allowFiltering: true,
            editSettings: {
                allowEditing: true,
                allowAdding: false,
                allowDeleting: true,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
            apiEndPoint: `/api/items/get-cache-reset-setup`,
            columns: columns,
            commandClick: handleCommands,

            actionBegin: function (args) {
                
                if (args.requestType === "Save") {
                    /*var data = {
                        SubClassID:args.data.SubClassID,
                        SubClassName:args.data.SubClassName,
                        TypeID:args.data.TypeID,
                        ShortCode:args.data.ShortCode,
                        isModified:true
                    }

                    axios.post("/api/kmachine-subclass/save", data)
                        .then(function () {
                            toastr.success("Saved successfully!");
                            getInitData();
                        })
                        .catch(showResponseError);*/
                }
            },
            actionComplete: function (args) {
            }
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Reset') {
            axios.post(`/api/${args.rowData.ApiName}/${args.rowData.ParameterValue}`)
                .then(function () {
                    toastr.success("Cache reset completed successfully!");
                    getInitData();
                })
                .catch(showResponseError);
        }
    }


})();