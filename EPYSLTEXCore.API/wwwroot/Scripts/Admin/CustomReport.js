(function () {
    var menuId;
    var _tblCR = $("#tblCR");
    var _tblHeadCR = $("#tblHeadCR");
    var _tblBodyCR = $("#tblBodyCR");

    var _defaultThCSS = "color: #ffffff; background-color: #1163B6; text-align: center; height: 40px; vertical-align: middle;";
    var _defaultTdCSS = "text-align: center; height: 40px; vertical-align: middle;";

    var _maxPropCount = 0;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        initEvents();
    });

    function initEvents() {
        
        $("#btnGenerateExcel").click(function () {
            $("#tblCR").table2excel();
        });
        $("#btnLoadAllReports").click(function () {
            getAllQueries();
            //$('#myModal').modal('hide');
        });
        $("#btnReportLoad").click(function () {
            var sql = $("#txtSQL").val();
            sql = replaceInvalidChar(sql);

            var isSave = $('#chkSave').is(":checked");
            var txtHeaders = replaceInvalidChar($("#txtHeaders").val());
            var txtReportTitles = replaceInvalidChar($("#txtReportTitles").val());
            var txtReportName = replaceInvalidChar($("#txtReportName").val());

            axios.get(`/api/dynamic-reporting-query/get-list-string/${sql}/${isSave}/${txtHeaders}/${txtReportTitles}/${txtReportName}`)
                .then(function (response) {
                    var dObj = response.data;

                    if (dObj.Message.length > 0) {
                        toastr.error(dObj.Message);
                        return false;
                    }

                    dObj.TableHeaders = $("#txtHeaders").val();
                    resetTable();
                    generateHeader(dObj.TableHeaders);
                    generateBody(dObj.DataList);
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });


            //var queryId = 1;
            //axios.get(`/api/dynamic-reporting-query/get-list/${queryId}`)
            //    .then(function (response) {
            //        var dObj = response.data;
            //        resetTable();
            //        generateHeader(dObj.TableHeaders);
            //        generateBody(dObj.MaxPropCount, dObj.DataList);
            //    })
            //    .catch(function (err) {
            //        toastr.error(err.response.data.Message);
            //    });
        });
    }
    function generateHeader(tableHeaders) {
        tableHeaders = tableHeaders.split(',');

        if (tableHeaders.length > 0 && tableHeaders[0] != "") {
            _maxPropCount = tableHeaders.length;
        }

        if (tableHeaders.length == 0 || tableHeaders[0] == "") {
            tableHeaders = [];
            var propCount = 1;
            while (propCount <= _maxPropCount) {
                tableHeaders.push("Prop" + propCount);
                propCount++;
            }
        }

        var propCount = 1;
        var row = `<tr>`;
        tableHeaders.map(th => {
            if (propCount <= _maxPropCount) {
                th = $.trim(th);
                row += `<th style='` + _defaultThCSS + `'>` + th + `</th>`;
                propCount++;
            }
        });
        row += `</tr>`;
        _tblHeadCR.append(row);
    }
    function generateBody(dataList) {
        if (_maxPropCount == 0 || dataList.length == 0) return false;

        dataList.map(dl => {
            var row = `<tr>`;
            var cellCount = 1;
            while (cellCount <= _maxPropCount) {
                row += `<td style='` + _defaultTdCSS + `'>` + dl["Prop" + cellCount] + `</td>`;
                cellCount++;
            }
            row += `</tr>`;
            _tblBodyCR.append(row);
        });
    }
    function generateSingleRowTblHeader(header) {

    }
    function resetTable() {
        _tblHeadCR.find("tr").remove();
        _tblBodyCR.find("tr").remove();

        $('#chkSave').attr('checked', false);
    }
    function getAllQueries() {
        axios.get(`/api/dynamic-reporting-query/get-all-queries/`)
            .then(function (response) {
                var dObj = response.data;

                if (dObj.Message.length > 0) {
                    toastr.error(dObj.Message);
                    return false;
                }

                $('#myModal').modal('show');

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

})();