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

        initToolbars();
    });

    function initToolbars() {
        $toolbarEl.find("#btnPendingList").click(function () {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });
        $toolbarEl.find("#btnRevisionPendingList").click(function () {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REVISE;
            initMasterTable();
        });
        $toolbarEl.find("#btnPartialAllocationList").click(function () {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PARTIALLY_COMPLETED;
            initMasterTable();
        });
        $toolbarEl.find("#btnCompleteList").click(function () {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            initMasterTable();
        });
        $toolbarEl.find("#btnRejectList").click(function () {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.REJECT;
            initMasterTable();
        });
        $toolbarEl.find("#btnPendingList").click();
    }
    function initMasterTable() {
        var commands = [];
        if (status == statusConstants.PENDING) {
            commands = [
                { type: 'New', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
            ]
        } else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } }
            ]
        }

        var columns = [
            {
                headerText: 'Actions', commands: commands, width: 10
            },
            {
                field: 'BookingNo', headerText: 'Booking No', width: 20
            },
            {
                field: 'YBookingNo', headerText: 'YBooking No', width: 20, visible: false
            },
            {
                field: 'BookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 20
            },
            {
                field: 'AcknowledgeDate', headerText: 'Acknowledge Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 20, visible: status == statusConstants.PENDING
            },
            {
                field: 'BuyerName', headerText: 'Buyer', width: 20
            },
            {
                field: 'BuyerDepartment', headerText: 'Buyer Department', width: 20
            },
            {
                field: 'CompanyName', headerText: 'CompanyName', width: 20
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/mr-bds/list?status=${status}&isBDS=${isBDS}`,
            columns: columns,
            allowSorting: true,
            commandClick: handleCommands
        });
    }
})();