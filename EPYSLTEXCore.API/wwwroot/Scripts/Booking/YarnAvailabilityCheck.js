(function () {
    'use strict'
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var status;
    var masterData;

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
        getDetails();

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            getDetails(); 
        });

        $toolbarEl.find("#btnEditList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.EDIT;
            getDetails(); 
        }); 
    });

    function getDetails() {
        axios.get(`/api/yarn-ava-check/list?status=${status}`)
            .then(function (response) {
                $divDetailsEl.fadeOut();
                $divTblEl.fadeIn();
                masterData = response.data;
                setFormData($formEl, masterData);
                initMasterTable(masterData.yarnAvailabilityCheck);
            })
            .catch(showResponseError);
    }

    function initMasterTable(data) {
        console.log(data);
        var columns = [
            {
                field: 'YACID', isPrimaryKey: true, visible: false
            },
            {
                headerText: 'Commands', commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            },
            {
                field: 'BookingID', headerText: 'BookingID', visible: false
            },
            {
                field: 'ConceptID', headerText: 'ConceptID', visible: false
            }, 
            {
                field: 'BuyerID', headerText: 'BuyerID', visible: false
            },
            {
                field: 'BuyerTeamID', headerText: 'BuyerTeamID', visible: false
            }, 
            {
                field: 'ItemMasterID', headerText: 'ItemMasterID', visible: false
            },
            {
                field: 'CompanyID', headerText: 'CompanyID', visible: false
            },
            {
                field: 'IsBDS', headerText: 'IsBDS', visible: false
            },
            {
                field: 'BookingNo', headerText: 'Booking No/Concept No', allowEditing: false
            },
            {
                field: 'ExportOrderNo', headerText: 'EWO No', allowEditing: false
            },
            {
                field: 'BuyerName', headerText: 'Buyer', allowEditing: false
            },
            {
                field: 'BuyerDepartment', headerText: 'Buyer Team', allowEditing: false
            }, 
            {
                field: 'BookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
            },
            {
                field: 'AcknowledgeDate', headerText: 'Booking Ack. Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
            },
            {
                field: 'YarnConsumption', headerText: 'Yarn Consumption', allowEditing: false
            },
            {
                field: 'YarnType', headerText: 'Yarn Type', allowEditing: false
            },
            {
                field: 'ManufacturingProcess', headerText: 'Manufacturing Process', allowEditing: false
            },
            {
                field: 'SubProcess', headerText: 'Sub Process', allowEditing: false
            },
            {
                field: 'QualityParameter', headerText: 'Quality Parameter', allowEditing: false
            },
            {
                field: 'YarnCount', headerText: 'Yarn Count', allowEditing: false
            }, 
            {
                field: 'Remarks', headerText: 'Remarks', allowEditing: false
            },
            {
                field: 'YarnSpecification', headerText: 'Specification', allowEditing: false
            },
            {
                field: 'TNADays', headerText: 'TNA Days', allowEditing: false
            }, 
            {
                field: 'YRequiredDate', headerText: 'Yarn Required Date',
                textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false
            },
            {
                field: 'BookingQty', headerText: 'Qty(Kg)', allowEditing: false
            },
            {
                field: 'ApproveStockQty', headerText: 'Approve Stock Qty(kg)', allowEditing: false
            },
            {
                field: 'DiagnosticStockQty', headerText: 'Diagonis Stock', allowEditing: false
            },
            {
                field: 'PipelineStockQty', headerText: 'Pipeline Stock Qty', allowEditing: false
            }, 
            {
                field: 'IsPR', headerText: 'GotoPR', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center'
            },
            {
                field: 'PRQty', headerText: 'PR Qty'
            }
        ];

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

    function save(yarnAvailabilityCheck) {
        var data = yarnAvailabilityCheck;
        axios.post("/api/yarn-ava-check/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                getDetails();
            })
            .catch(showResponseError);
    }

})();
 
