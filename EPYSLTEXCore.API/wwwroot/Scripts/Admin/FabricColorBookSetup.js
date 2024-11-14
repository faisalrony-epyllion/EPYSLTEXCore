var $tblMasterEl;

(function () {
    'use strict'
    var menuId, tblMasterId;
    var masterData = {};

    $(function () {
        menuId = localStorage.getItem("current_common_interface_menuid");
        tblMasterId = `#tblMaster-${menuId}`;

        initPageData();
    })

    function initMasterTable() {
        var columns = [
            {
                field: 'PTNID', isPrimaryKey: true, visible: false
            },
            {
                headerText: 'Commands', width: 100, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            {
                field: 'ColorID', headerText: 'Color Name', valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.ColorList, displayField: "ColorName", edit: ej2GridDropDownObj ({  })
            },
            {
                field: 'ColorSource', headerText: 'Color Source', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: ["TCX", "TPX"], field: "ColorSource" })
            },
            {
                field: 'ColorCode', headerText: 'Color Code'
            },
            {
                field: 'RGBOrHex', headerText: 'RGB or Hex Value', valueAccessor: ej2GridColorFormatter
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: "/api/fabric-color-book-setups",
            columns: columns,
            autofitColumns: false,
            allowSorting: true,
            toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
        });
    }

    function initPageData() {
        axios.get("/api/fabric-color-book-setups/new")
            .then(function (response) {
                masterData = response.data;
                initMasterTable()
            })
            .catch(showResponseError);
    }
})();