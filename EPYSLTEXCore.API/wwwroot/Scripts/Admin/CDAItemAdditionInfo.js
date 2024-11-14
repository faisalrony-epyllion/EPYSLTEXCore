(function () {
    'use strict'
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var status;
    var masterData;
    var subGroupId, subGroupName;
    var isEditable = true;

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
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING;
        getSubGroup();
        

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            //getDetails();
        });

        $toolbarEl.find("#btnEditList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.EDIT;
            //getDetails();
        });

        $("#btnNew").on("click", getDetails);
    }); 

    function getSubGroup() {  
        axios.get(`/api/cdaitemadd/newsubgroup`)
            .then(function (response) {
                $divDetailsEl.fadeOut();
                $divTblEl.fadeIn();
                masterData = response.data;

                var item = "";
                $.each(masterData.SubGroupList, function (j, obj) { 
                    item += '<option value="' + obj.id + '">' + obj.text + '</option>'; 
                });
                $('#SubGroupID').html(item); 
            })
            .catch(showResponseError);
    }

    function getDetails() { 
        var subGroupId = $("#SubGroupID").val(); 
        if (subGroupId == "100") {
            subGroupName = "Dyes_Group";
        } else {
            subGroupName = "Chemicals_Group";
        }

        axios.get(`/api/cdaitemadd/list?status=${status}&subGroupName=${subGroupName}`)
            .then(function (response) {
                $divDetailsEl.fadeOut();
                $divTblEl.fadeIn();
                masterData = response.data;
                setFormData($formEl, masterData);
                initMasterTable(masterData.Childs);
            })
            .catch(showResponseError);
    }

    function initMasterTable(data) {
        alert(masterData.SubGroupName);
        console.log(data);
        var columns = [], SubGroupColumns = [];

        var columns =
            [
                {
                    field: 'ItemMasterID', isPrimaryKey: true, visible: false
                },
                {
                    headerText: 'Commands', commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                }
            ];
        if (masterData.SubGroupName === subGroupNames.DYES) {
            SubGroupColumns = [
                {
                    field: 'Segment1ValueId', headerText: 'Primary Group', allowEditing: isEditable, required: true,
                    width: 350, valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.Segment1ValueList,
                    displayField: "Segment1ValueDesc", edit: ej2GridDropDownObj({
                        width: 350
                    })
                },
                {
                    field: 'Segment2ValueId', headerText: 'Item Name', allowEditing: isEditable, width: 120,
                    valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.Segment2ValueList,
                    displayField: "Segment2ValueDesc", edit: ej2GridDropDownObj({
                    })
                } 
            ]; 
        }
        else {
            SubGroupColumns = [
                {
                    field: 'Segment1ValueId', headerText: 'Primary Group', allowEditing: isEditable, required: true,
                    width: 350, valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.Segment1ValueList,
                    displayField: "Segment1ValueDesc", edit: ej2GridDropDownObj({
                        width: 350
                    })
                },
                {
                    field: 'Segment2ValueId', headerText: 'Agent', allowEditing: isEditable, width: 120,
                    valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.Segment2ValueList,
                    displayField: "Segment2ValueDesc", edit: ej2GridDropDownObj({
                    })
                },
                {
                    field: 'Segment3ValueId', headerText: 'Item Name', allowEditing: isEditable,
                    width: 100, valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.Segment3ValueList,
                    displayField: "Segment3ValueDesc", edit: ej2GridDropDownObj({
                    })
                },
                {
                    field: 'Segment4ValueId', headerText: 'Form', allowEditing: isEditable, width: 100,
                    valueAccessor: ej2GridDisplayFormatter,
                    dataSource: masterData.Segment4ValueList,
                    displayField: "Segment4ValueDesc", edit: ej2GridDropDownObj({
                    })
                }
            ];
        }

        columns.push.apply(columns, SubGroupColumns);

        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            columns: columns,
            data: data,
            autofitColumns: true,
            allowSorting: true,
            editSettings: {
                allowAdding: true, allowEditing: true, allowDeleting: true,
                mode: "Normal", showDeleteConfirmDialog: true
            },
            allowGrouping: true,
            showColumnChooser: true,
            allowExcelExport: true,
            allowPdfExport: true,
            showDefaultToolbar: false,
            actionBegin: function (args) {
                if (args.requestType === "save") {
                    save(args.data);
                }
            },
        });

        $('.e-groupdroparea').css({ 'display': 'none' });
    }

    function save(yarnAllocation) {
        var data = yarnAllocation;
        axios.post("/api/cdaitemadd/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                getDetails();
            })
            .catch(showResponseError);
    }

})();