(function () {
    var $table;

    $(function () {
        $table = $("#tblLogData");
        getLogSetups();
        initTable();
    })

    function initTable() {
        $table.bootstrapTable('destroy');
        $table.bootstrapTable({
            sortable: true,
            showRefresh: true,
            showExport: true,
            exportTypes: "['json']",
            pagination: true,
            filterControl: true,
            sidePagination: "client",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            columns: [
                {
                    field: "time",
                    title: "Time",
                    filterControl: "input",
                    sortable: true
                },
                {
                    field: "clientIp",
                    title: "Client IP",
                    filterControl: "input",
                    sortable: true
                },
                {
                    field: "message",
                    title: "Message",
                    filterControl: "input"
                },
                {
                    field: "exception",
                    title: "Exception",
                    filterControl: "input"
                },
                {
                    field: "requestUrl",
                    title: "Request Url",
                    filterControl: "input"
                },
                {
                    field: "user",
                    title: "User",
                    filterControl: "input"
                }
            ],
            onRefresh: function () {
                alert("refreshed");
            },
        });
    }

    function getLogSetups() {
        axios.get("/api/log-viwer-setup")
            .then(function (response) {
                initSelect2($("#ddlLogViewerSetup"), response.data);
                $("#ddlLogViewerSetup").on("select2:select", getLogFiles);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getLogFiles(e) {
        axios.get(`/api/log-files?directory=${e.params.data.desc}`)
            .then(function (response) {
                initSelect2($("#ddlLogFiles"), response.data);
                $("#ddlLogFiles").prop("disabled", false);
                $("#ddlLogFiles").on("select2:select", getLogData);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getLogData(e) {
        axios.get(`/api/log-data?fileName=${e.params.data.id}`)
            .then(function (response) {
                $table.bootstrapTable("load", response.data);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }
})();