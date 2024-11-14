var tableParams = {
    offset: 0,
    limit: 10,
    sort: '',
    order: '',
    filter: ''
}
var filterBy = {};

$(function () {
    
    initFTNDataBankTable();
    getFTNDataBankData();    
});

function initFTNDataBankTable() {
    $("#tblFTNDataBank").bootstrapTable('destroy');
    $("#tblFTNDataBank").bootstrapTable({
        showRefresh: true,
        showExport: true,
        showColumns: true,        
        exportTypes: "['csv', 'excel']",
        pagination: true,
        filterControl: true,
        searchOnEnterKey: true,
        sidePagination: "server",
        pageList: "[10, 25, 50, 100, 500]",
        cache: false,
        columns: [            
            {
                field: "TechnicalName",
                title: "Technical Name",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "ConstructionName",
                title: "Construction",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            },
            {
                field: "CompositionName",
                title: "Composition",
                filterControl: "input",
                filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
            }
        ],
        onPageChange: function (number, size) {
            var newOffset = (number - 1) * size;
            var newLimit = size;
            if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                return;

            tableParams.offset = newOffset;
            tableParams.limit = newLimit;

            getFTNDataBankData();  
        },
        onSort: function (name, order) {
            tableParams.sort = name;
            tableParams.order = order;
            tableParams.offset = 0;

            getFTNDataBankData();
        },
        onRefresh: function () {
            resetTableParams();
            getFTNDataBankData();
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

            getFTNDataBankData();
        }
    });
}

function getFTNDataBankData() {
    var queryParams = $.param(tableParams);
    $('#tblFTNDataBank').bootstrapTable('showLoading');
    var url = "/api/fabricTechnicalNameDataBank?" + queryParams;
    axios.get(url)
        .then(function (response) {
            $("#tblFTNDataBank").bootstrapTable('load', response.data);
            $('#tblFTNDataBank').bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        })
}

function resetTableParams() {
    tableParams.offset = 0;
    tableParams.limit = 10;
    tableParams.filter = '';
    tableParams.sort = '';
    tableParams.order = '';
}