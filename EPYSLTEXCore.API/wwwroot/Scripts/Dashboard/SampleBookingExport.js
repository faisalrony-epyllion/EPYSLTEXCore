var formSampleBookingExport;
var SampleBookingExport;
var filterBy = {};
var Id;
var sbStatus = 4;
var tableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}

$(function () {
    formSampleBookingExport = $("#formSampleBookingExport");
    initPendingSampleBookingExportMasterTable();
    getPendingSampleBookingExportMasterData();  
});

function initPendingSampleBookingExportMasterTable() {
    $("#tblSampleBookingExport").bootstrapTable('destroy');
    $("#tblSampleBookingExport").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,
        toolbar: "#SampleBookingExportToolbar",
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        showFooter: true,
        columns: [            
            {
                field: "BookingNo",
                title: "Booking No",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerName",
                title: "Buyer",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BuyerTeamName",
                title: "Buyer Team",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "MerchandiserName",
                title: "Merchandiser",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "BookingQty",
                title: "Booking Qty",
                filterControl: "input",
                align: 'right',
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //footerFormatter: calculateSampleBookingExportMasterBookingQty
            },
            {
                field: "SpBookingDateStr",
                title: "Booking Date"
                //filterControl: "input"
                //filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "RevisionNo",
                title: "Revision No"
                //filterControl: "input"
                //filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            }
        ],
        onPageChange: function (number, size) {
            var newOffset = (number - 1) * size;
            var newLimit = size;
            if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                return;

            tableParams.offset = newOffset;
            tableParams.limit = newLimit;

            getPendingSampleBookingExportMasterData();
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getPendingSampleBookingExportMasterData();
        },
        onRefresh: function () {
            resetBookingAnalysisTableParams();
            getPendingSampleBookingExportMasterData();
        },
        onColumnSearch: function (columnName, filterValue) {
            if (columnName in filterBy && !filterValue) {
                delete filterBy[columnName];
            }
            else
                filterBy[columnName] = filterValue;

            if (Object.keys(filterBy).length === 0 && filterBy.constructor === Object)
                tableParams.filter = "";
            else
                tableParams.filter = JSON.stringify(filterBy);

            getPendingSampleBookingExportMasterData();
        }
    });
}

function getPendingSampleBookingExportMasterData() {
    var queryParams = $.param(tableParams);
    $('#tblSampleBookingExport').bootstrapTable('showLoading');
    var url = "/SBSApi/SampleBookingExportLists" + "?sbStatus=" + sbStatus + "&" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblSampleBookingExport").bootstrapTable('load', response.data);
            $('#tblSampleBookingExport').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}
