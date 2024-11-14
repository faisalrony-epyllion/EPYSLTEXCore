(function () {
    var menuId, pageName, pageId;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);

        resetForm();

        $pageEl.find("#btnClearFilter").click(function () {
            resetForm();
        });
        $pageEl.find("#btnGenerateExcel").click(function () {
            generateExcel();
        });
    });

    function resetForm() {
        $pageEl.find("input[type='date']").val(ch_getCurrentDate());
        $pageEl.find("input[type='text']").val("");
    }
    function generateExcel() {
        ch_generateAndExportExcel(pageId, 3, {
            Param1: $pageEl.find("#FromDate").val(),
            Param2: $pageEl.find("#ToDate").val(),
            Param3: $pageEl.find("#EWO").val(),
            Param4: $pageEl.find("#PhysicalCount").val()
        });
    }
})();