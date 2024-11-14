(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, $tblYarnEl, tblMasterId;
    var filterBy = {};
    var status = statusConstants.PENDING;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var isEditable = true;
    var masterData;
    var abc;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $tblYarnEl = $("#tblKnittingPlanYarnKC");
        initMasterTable();

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
        });
        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnRejectList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REJECT;
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });
        $formEl.find("#btnCancel").on("click", backToList);

        $('input[type=radio][name=GrayFabricOK]').change(function () {
            if (this.value == 'true') {
                $formEl.find("#confirmation-status").fadeOut();
            } else {
                $formEl.find("#confirmation-status").fadeIn();
            }
        });

    });

    function handleCommands(args) {
        getDetails(args.rowData.Id);
    }


    function initMasterTable() {
        var commands = [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }];

        var columns = [
            { headerText: 'Actions', commands: commands },

            { field: 'ReqDeliveryDate', headerText: 'Req. DeliveryDate', textAlign: 'Right', type: 'date', format: _ch_date_format_1},
            { field: 'PlanNo', headerText: 'Program No' },
            { field: 'BookingNo', headerText: 'Booking No' },
            { field: 'BookingDate', headerText: 'Booking Date', textAlign: 'Right', type: 'date', format: _ch_date_format_1 },
            //{ field: 'KnittingType', headerText: 'Knitting Type' },
            //{ field: 'TechnicalName', headerText: 'Technical Name' },
            //{ field: 'Composition', headerText: 'Composition' },
            //{ field: 'GSM', headerText: 'GSM' },
            /*{ field: 'Qty', headerText: 'QTY' },*/
            { field: 'PlanQty', headerText: 'Plan Qty' }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/first-roll-check/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            uniqueId: 'Id',
            editable: isEditable,
            columns: [
                {
                    field: "ColorName",
                    title: "Color Name",

                },
                {
                    field: "ColorCode",
                    title: "Color Code",

                }
            ]
        });
    }

    function initTblYarn(records) {
        $tblYarnEl.bootstrapTable("destroy");
        $tblYarnEl.bootstrapTable({
            uniqueId: 'Id',
            editable: true,
            columns: [
                {
                    field: "YarnCount",
                    title: "Yarn Count",
                    filterControl: "input",
                    align: 'center'
                },

                {
                    field: "YarnType",
                    title: "Yarn Type",
                    filterControl: "input",
                    align: 'center'
                },
                {
                    field: "YarnLotNo",
                    title: "Yarn Lot",
                    filterControl: "input",
                    align: 'center'
                },
                {
                    field: "YarnBrand",
                    title: "Yarn Brand",
                    filterControl: "input",
                    align: 'center'
                },
                {
                    field: "StitchLength",
                    title: "Stitch Length",
                    filterControl: "input",
                    align: 'center'
                }
            ],
            data: records
        });
    }

    function initTblFabricChild() {
        $formEl.find("#tblFabricInformationKC").bootstrapTable('destroy').bootstrapTable({
            uniqueId: 'ItemMasterID',
            detailView: true,
            columns: [
                {
                    field: "MCSubClassName",
                    title: "M/C Sub Class"
                },
                {
                    field: "MachineGauge",
                    title: "Machine Gauge",
                    width: 100
                },
                {
                    field: "StartDate",
                    title: "Start Date",
                    filterControl: "input",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "EndDate",
                    title: "End Date",
                    filterControl: "input",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "Uom",
                    title: "Unit",
                    filterControl: "input"
                },
                {
                    field: "BookingQty",
                    title: "Qty",
                    align: 'center'
                }
            ],
            onExpandRow: function (index, row, $detail) {
                
                if (row.MCSubClassID == 0) {
                    toastr.error("Please select M/C Sub Class!");
                    return;
                } else if (row.MachineGauge == 0) {
                    toastr.error("Please select Machine Gauge!");
                    return;
                } else if (row.BookingQty == 0) {
                    toastr.error("Please enter Qty!");
                    return;
                }
                populateJobCardTable(row, $detail);
            },
        });
    }
    function populateJobCardTable(kpChild, $detail) {
        
        if (parseInt(kpChild.MachineGauge) != 0 && parseInt(kpChild.BookingQty) != 0) {
            $el = $detail.html('<table id="TblJobCardTable-' + kpChild.Id + '"></table>').find('table');
            //console.log($el);
            var ind = getIndexFromArray(masterData.Childs, "Id", kpChild.Id)
            initJobCardTable($el, kpChild, ind);
            var cnt = masterData.Childs[ind].KJobCardMasters.length;
            if (cnt > 0)
                $el.bootstrapTable('load', masterData.Childs[ind].KJobCardMasters);
        }
        else {
            toastr.warning("Machine Gauge and Qty should more than 0.!!!");
        }
    }
    function initJobCardTable($jobCardTable, kpChild, ind) {
        $jobCardTable.bootstrapTable({
            showFooter: true,
            uniqueId: 'Id',
            checkboxHeader: false,
            columns: [

                {
                    field: "KJobCardNo",
                    title: "Job Card No",
                    width: 100
                },
                {
                    field: "KJobCardDate",
                    title: "Job Card Date",
                    width: 100,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "Brand",
                    title: "Brand",
                    width: 100
                },
                {
                    field: "IsSubContact",
                    title: "Sub Contact?",
                    width: 80,
                    checkbox: true,
                    showSelectTitle: true
                },
                {
                    field: "Contact",
                    title: "Floor/Sub-Contractor"

                },
                {
                    field: "MachineDia",
                    title: "Machine Dia",
                    width: 100
                },
                {
                    field: "KnittingMachineNo",
                    title: "Machine",
                    width: 120
                },
                {
                    field: "KJobCardQty",
                    title: "Job Card Qty",
                    width: 80

                },
                {
                    field: "Remarks",
                    title: "Remarks",
                    width: 120
                }
            ]
        });
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
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
        axios.get(`/api/first-roll-check/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReqDeliveryDate = formatDateToDefault(masterData.ReqDeliveryDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                setFormData($formEl, masterData);
                initChildTable();
                $tblChildEl.bootstrapTable("load", masterData.ChildColors);
                $tblChildEl.bootstrapTable('hideLoading');

                

                initTblYarn(masterData.Yarns);
                $("#divKnittingPlanYarnKC").fadeIn();
                
                if (masterData.SubGroupName === subGroupNames.FABRIC) {
                    $("#divFabricInformationKC").fadeIn();
                    initTblFabricChild();
                    $formEl.find("#tblFabricInformationKC").bootstrapTable("load", masterData.Childs);
                    $formEl.find("#tblFabricInformationKC").bootstrapTable('hideLoading');
                    //getChildData();
                }

                if (masterData.GrayFabricOK) {
                    $("#gray-ok").prop("checked", true);
                    $formEl.find("#confirmation-status").fadeOut();
                }
                else {
                    $("#gray-not-ok").prop("checked", true);
                    $formEl.find("#confirmation-status").fadeIn();
                }

                if (masterData.NeedPreFinishingProcess) {
                    $("#need-yes").prop("checked", true);
                    $formEl.find("#need-process").fadeOut();
                }
                else {
                    $("#need-no").prop("checked", true);
                    $formEl.find("#need-process").fadeIn();
                }
                if (masterData.ChildColors.length == 0) {
                    $formEl.find("#need-process").fadeOut();
                } else {
                    $formEl.find("#need-process").fadeIn();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = masterData.Childs;
        if ($formEl.find("#GrayFabricOK").is(':checked') == true)
            data.GrayFabricOK = true;
        if ($formEl.find("#NeedPreFinishingProcess").is(':checked') == true)
            data.NeedPreFinishingProcess = true;
        axios.post("/api/first-roll-check/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();