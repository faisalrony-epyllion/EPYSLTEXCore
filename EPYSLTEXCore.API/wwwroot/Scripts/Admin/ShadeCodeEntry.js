(function () {
    'use strict'
    var pageId, $pageEl, $formEl;
    var menuId, tblMasterId, $tblMasterEl;
    var masterData = {};
    var currentRow = {};

    $(function () {

       
        if (!menuId)
            //menuId = localStorage.getItem("menuId");
            menuId = localStorage.getItem("current_common_interface_menuid");
        //if (!pageName)
        //    pageName = localStorage.getItem("pageName");

        //pageId = pageName + "-" + menuId;
        //$pageEl = $(`#${pageId}`);
        //$formEl = $(pageConstants.FORM_ID_PREFIX + pageId);

        tblMasterId = `#tblMaster-${menuId}`;
        $("#btnFilter").click(function (e) {
            debugger;
            //var filterText = $("#filterContactName").val();
            initMasterTable();
        });
        initPageData();
    })
   
    function initMasterTable() {
        debugger;
        var filterText = $("#filterContactName").val();
        if (filterText == "") {
            filterText = null;
        }
        var columns = [
            {
                field: 'YSCID', isPrimaryKey: true, allowEditing: false, visible: false
            },
            {
                headerText: 'Commands', width: 40, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    //{ type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            {
                field: 'ContactID', headerText: 'Contact Name',
                valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.ContactList,
                displayField: "ContactName",
                edit: ej2GridDropDownObj({

                })
            },
            {
                field: 'ShadeCode', headerText: 'Shade Code'
            }
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/shade-code-entrys/filterList/` + filterText,
            columns: columns,
            autofitColumns: false,
            allowSorting: true,
            toolbar: ['Add'],
            editSettings: {
                allowAdding: true,
                allowEditing: true,
                allowDeleting: false,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
            actionBegin: function (args) {
                debugger;
                var text = document.getElementById("ContactID_filterBarcell").value;
                if (args.requestType === "save") {
                    save(args.data);
                }

            }
        });
    }
    
    function initPageData() {
        axios.get("/api/shade-code-entrys/get/new")
            .then(function (response) {
                masterData = response.data;
                initMasterTable();
            })
            .catch(showResponseError);
    }

    function checkValidationNumber(value) {
        if (typeof value === "undefined" || value == null || value == 0) return true;
        return false;
    }
    function checkValidationText(value) {
        if (typeof value === "undefined" || value == null || value == "") return true;
        return false;
    }

    function save(data) {
        if (checkValidationNumber(data.ContactID)) {
            toastr.error("Select Contact.");
            return false;
        }
        if (checkValidationText(data.ShadeCode)) {
            toastr.error("Give ShadeCode.");
            return false;
        }

        axios.post("/api/shade-code-entrys/save", data)
            .then(function () {
                toastr.success("Saved successful.");
                initPageData();
            })
            .catch(showResponseError);
    }
})();