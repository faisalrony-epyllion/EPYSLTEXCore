(function () {
    'use strict'
    var menuId, pageName;
    var toolbarId, pageId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl;
    var filterData = {};
    var isCompletePage = false;

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
        isCompletePage = convertToBoolean($(`#${pageId}`).find("#CompletePage").val());

        initMasterTable();
    });

    function initMasterTable() {
        var columns = [
            {
                field: 'KnittingMachineID', isPrimaryKey: true, visible: false
            },
            {
                headerText: '', width: 100, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            },
            {
                field: 'MachineNo', headerText: 'Machine No', allowEditing: false
            },
            {
                field: 'SerialNo', headerText: 'Serial No', allowEditing: false
            },
            {
                field: 'UnitName', headerText: 'Unit Name', allowEditing: false
            },
            {
                field: 'Nature', headerText: 'Machine Nature', allowEditing: false
            },
            {
                field: 'MachineTypeName', headerText: 'Machine Type', allowEditing: false
            },
            {
                field: 'MachineSubClassName', headerText: 'Machine SubClass', allowEditing: false
            },
            {
                field: 'Brand', headerText: 'Machine Brand', allowEditing: false
            },
            {
                field: 'Origin', headerText: 'Origin', allowEditing: false
            },
            {
                field: 'Dia', headerText: 'Machine Dia', allowEditing: false
            },
            {
                field: 'GG', headerText: 'Machine GG', visible: !isCompletePage, edit: ej2GridDropDownObjByfilter({
                    apiEndPoint: "/api/gauge-by-subclass-brand-dia",
                    filterBy: "MachineSubClassID,BrandID,Dia",
                    displayField: "text"
                })
            },
            {
                field: 'GG', headerText: 'Machine GG', allowEditing: false, visible: isCompletePage
            },
            { field: 'IsComplete', headerText: 'Complete?', displayAsCheckBox: true, editType: "booleanedit", visible: isCompletePage, textAlign: 'Center' },
            {
                field: 'NextServicingDate', headerText: 'Next Servicing Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
            },
            {
                field: 'LastServicingDate', headerText: 'Last Servicing Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
            },
            {
                field: 'Capacity', headerText: 'Capacity/Day (Kg)', allowEditing: false
            },
            {
                field: 'Feeder', headerText: 'Feeder', allowEditing: false
            },
            {
                field: 'AvgRPM', headerText: 'Avg RPM', allowEditing: false
            },
            {
                field: 'Head', headerText: 'Head', allowEditing: false
            },
            {
                field: 'TwoToneCapacity', headerText: 'Two Tone Capacity', allowEditing: false
            },
            {
                field: 'SolidCapacity', headerText: 'Solid Capacity', allowEditing: false
            },
            {
                field: 'JacquredCapacity', headerText: 'Jacqured Capacity', allowEditing: false
            },
            {
                field: 'ManufacturerDate', headerText: 'Manufacturer Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
            },
            {
                field: 'ErectionDate', headerText: 'Erection Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
            },
            {
                field: 'Remarks', headerText: 'Remarks', allowEditing: false
            }
        ];

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: "/api/knitting-machine_list",
            columns: columns,
            autofitColumns: true,
            allowSorting: true,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            allowGrouping: true,
            showColumnChooser: true,
            allowExcelExport: true,
            allowPdfExport: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser', 'ExcelExport'],
            handleToolbarClick: toolbarClickExcelMaster
        });
    }
    function toolbarClickExcelMaster(args) {
        if (args['item'].id.indexOf('_excelexport') >= 0) {
            $tblMasterEl.excelExport();
        } else if (args['item'].id.indexOf('_pdfexport') >= 0) {
            var exportProperties = {
                bAllowHorizontalOverflow: false
            };
            $tblMasterEl.pdfExport(exportProperties);
        }
    }
})();