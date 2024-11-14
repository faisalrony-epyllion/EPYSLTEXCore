(function () {
    var menuId, pageName, pageId;
    var toolbarId;
    var $divTblEl, $divDetailsEl, tblMasterId, $tblMasterEl, $formEl;
    var masterData;
    var $subGroupEl;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $subGroupEl = $("#SubGroupID");

        initSubGroup();

        $subGroupEl.on("select2:select select2:unselect", function (e) {
            if (e.params._type === "select") {
                getTableStructure(e.params.data.id);
                $("#UploadFile").prop("disabled", false);
            }
            else {
                $("#UploadFile").val(null).trigger("change").prop("disabled", true);
                $tblMasterEl.destroy();
            }            
        });

        $("#UploadFile").change(getPreview);

        $("#btnSave").click(uploadItems);

        $("#btnClear").click(clearData);
    });

    function initMasterTable(columnData) {
        var columns = [];
        $.each(columnData, function (i, column) {
            columns.push({ field: column.SegmentValueDescName, headerText: column.SegmentDisplayName, width: 120 })
        })

        ej.base.enableRipple(true);
        if($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new ej.grids.Grid({
            allowExcelExport: true,
            allowPdfExport: true,
            toolbar: ['ExcelExport', 'PdfExport', 'CsvExport'],
            allowResizing: true,
            allowFiltering: true,
            actionComplete: handleGridEvents,
            commandClick: handleCommands,
            allowPaging: true,
            pageSettings: { pageCount: 5, currentPage: 1, pageSize: 10, pageSizes: true },
            columns: columns,
        });

        $tblMasterEl.appendTo(tblMasterId);

        $tblMasterEl.toolbarClick = function (args) {
            if (args.item.id.includes('pdfexport')) {
                $tblMasterEl.pdfExport();
            }
            if (args.item.id.includes('excelexport')) {
                $tblMasterEl.excelExport();
            }
            if (args.item.id.includes('csvexport')) {
                $tblMasterEl.csvExport();
            }
        };        
    }

    function clearData() {
        $subGroupEl.val(null).trigger("change");
        $("#UploadFile").val(null).trigger("change").prop("disabled", true);
        $tblMasterEl.destroy();
    }

    function getDetails(id) {
        axios.get(`/api/bond-financial-year/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.BondFinancialYearImportLimits);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function uploadItems() {
        masterData.CreateNewSegmentValueThatNotFound = convertToBoolean($("#CreateNewSegmentValueThatNotFound").val());
        masterData.SubGroupID = $subGroupEl.val();

        axios.post("/api/items/upload", masterData)
            .then(function () {
                toastr.success("Items updated successfully!");
            })
            .catch(showResponseError);
    }

    // #region grid events
    function handleGridEvents(args) {
        //console.log(args);
        switch (args.requestType) {
            case "filtering":
                break;
            case "paging":
                filterBy = {};
            default:
                break;
        }
    }

    function handleCommands(args) {
        //console.log(args);
        getDetails(args.rowData.Id);
    }
    // #endregion

    function initSubGroup() {
        axios.get("/api/selectoption/item-sub-groups")
            .then(function (response) {
                initSelect2($subGroupEl, response.data, true, "Select a Sub Group");
            })
            .catch(showResponseError)
    }

    function getTableStructure(subGroupId) {
        axios.get(`/api/items/table-structure/${subGroupId}`)
            .then(function (response) {
                initMasterTable(response.data);
            })
            .catch(showResponseError)
    }

    function getPreview(e) {
        var file = e.target.files[0];
        var formData = new FormData();
        formData.append("SubGroupID", $subGroupEl.val());
        formData.append("UploadFile", file);

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }

        axios.post("/api/items/preview", formData, config)
            .then(function (response) {
                masterData = {};
                masterData["Items"] = response.data;
                $tblMasterEl.dataSource = masterData.Items;
                $tblMasterEl.pageSettings.totalRecordsCount = masterData.Items.length;
                $tblMasterEl.refresh();
            })
            .catch(showResponseError)
    }

})();