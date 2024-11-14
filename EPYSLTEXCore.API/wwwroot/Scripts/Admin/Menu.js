(function () {
    var menuId, pageName;
    var $divTblEl, $divDetailsEl, $tblMasterEl, $tblChildEl, $formEl;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);

        getMenuList();

        
    });

    function getMenuList() {
        var url = `/api/nestable-menu/${constants.APPLICATION_ID}`;
        axios.get(url)
            .then(function (response) {
                //console.log(response);
            })
            .catch(showResponseError)
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMenuList();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getDetails(id) {
        axios.get(`/api/bond-financial-year/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                BondFinancialYear = response.data;
                BondFinancialYear.StartDate = formatDateToDefault(BondFinancialYear.StartDate);
                BondFinancialYear.EndDate = formatDateToDefault(BondFinancialYear.EndDate);
                setFormData($formEl, BondFinancialYear);
                initChildTable();
                $tblChildEl.bootstrapTable("load", BondFinancialYear.BondFinancialYearImportLimits);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["BondFinancialYearImportLimits"] = BondFinancialYear.BondFinancialYearImportLimits;
        axios.post("/api/bond-financial-year/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


})();