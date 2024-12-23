﻿(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl, tblMasterId;
    var spinnerList, spinnerLists = []

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        //$tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;

        getInitData();

        $formEl.find("#btnSave").click(save);

    });
    function initMasterTable() {
        var commands = [
            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
            { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
            { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
            { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
        ];

        columns = [
            {
                headerText: 'Command', textAlign: 'Center', commands: commands,
                textAlign: 'Center', width: 30
            },

            { field: 'YarnPackingID', isPrimaryKey: true, visible: false },
            {
                field: 'SpinnerID',
                headerText: 'Spinner',
                width: 35,
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.SpinnerList,
                displayField: "Spinner",
                edit: ej2GridDropDownObj({
                })
            },
            { field: 'PackNo', headerText: 'Pack No', width: 35 },
            { field: 'Cone', headerText: 'Cone', width: 35 },
            { field: 'NetWeight', headerText: 'Net Weight', width: 35 },
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
            apiEndPoint: `/api/spinner-wise-yarn-packing-hk/list`,
            columns: columns,
            commandClick: handleCommands,

            actionBegin: function (args) {
                if (args.requestType === "save") {
                   
                }
            },
            actionComplete: function (args) {
            }
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Add') {
            //getNew(args.rowData.SalesInvoiceMasterID);
        }
        else if (args.commandColumn.type == 'Edit') {
            //getDetails(args.rowData.SalesInvoiceMasterID);
        }
        else if (args.commandColumn.type == 'Delete') {
            //getDetails(args.rowData.SalesInvoiceMasterID);
        }
    }
    function getInitData() {

        var url = "/api/spinner-wise-yarn-packing-hk/GetMaster";
        axios.get(url)
            .then(function (response) {
                masterData = response.data;

                setFormData($formEl, masterData);
                initMasterTable();
            })
            .catch(showResponseError)
    }


    function save(e) {
        e.preventDefault();
        var data = formElToJson($formEl);
        data.SetupID = 0;

        axios.post("/api/spinner-wise-yarn-packing-hk/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                //getInitData();
                initMasterTable();
            })
            .catch(showResponseError);
    }

})();