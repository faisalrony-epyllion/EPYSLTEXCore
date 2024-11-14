(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, $tblYarnEl, tblMasterId, tblChildId;
    var filterBy = {};
    var status = statusConstants.PENDING;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var KnittingProgram;
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
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        //$tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $tblYarnEl = $("#tblKnittingPlanYarnKC");
        initMasterTable();

        $formEl.find("#btnAddColor_KC").fadeOut();

        $('input[type=radio][name=ChangeFreeConcept]').change(function () {
            if (this.value == 'true') {
                $formEl.find("#btnAddColor_KC").fadeIn();
                $("#divConceptColor_KC").fadeIn();
            } else {
                $formEl.find("#btnAddColor_KC").fadeOut();
                $("#divConceptColor_KC").fadeOut();
            }
        });

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

        $formEl.find("#btnCancel").on("click", backToListWithoutFilter);

        $formEl.find('#ActualTotalNeedle,#ActualGreyLength').on('keyup', function (e) {
            var ActualTotalNeedle = $formEl.find('#ActualTotalNeedle').val();
            var ActualGreyLength = $formEl.find('#ActualGreyLength').val();
            var ActualNeedle = parseFloat(ActualTotalNeedle) / parseFloat(ActualGreyLength);

            if (ActualNeedle == "NaN" || ActualNeedle == "Infinity") {
                ActualNeedle = 0;
            }

            $formEl.find('#ActualNeedle').val(parseFloat(ActualNeedle.toFixed(2)));
        });

        $formEl.find('#ActualTotalNeedle,#ActualGreyLength').on('change', function (e) {
            var ActualTotalNeedle = $formEl.find('#ActualTotalNeedle').val();
            var ActualGreyLength = $formEl.find('#ActualGreyLength').val();
            var ActualNeedle = parseFloat(ActualTotalNeedle) / parseFloat(ActualGreyLength);
            if (ActualNeedle == "NaN" || ActualNeedle == "Infinity") {
                ActualNeedle = 0;
            }
            $formEl.find('#ActualNeedle').val(parseFloat(ActualNeedle.toFixed(2)));
        });

        //ColorWayDesignOk, ActualTotalNeedle, ActualTotalCourse, ActualGreyHeight, ActualGreyLength, ActualNeedle, ActualCPI,
        $formEl.find('#ActualTotalCourse,#ActualGreyHeight').on('keyup', function (e) {
            var ActualTotalCourse = $formEl.find('#ActualTotalCourse').val();
            var ActualGreyHeight = $formEl.find('#ActualGreyHeight').val();
            var ActualCPI = (parseInt(ActualTotalCourse) / parseInt(ActualGreyHeight)) * 2.54;
            if (ActualCPI == "NaN" || ActualCPI == "Infinity") {
                ActualCPI = 0;
            }
            $formEl.find('#ActualCPI').val(parseInt(ActualCPI));
        });

        $formEl.find('#ActualTotalCourse,#ActualGreyHeight').on('change', function (e) {
            var ActualTotalCourse = $formEl.find('#ActualTotalCourse').val();
            var ActualGreyHeight = $formEl.find('#ActualGreyHeight').val();
            var ActualCPI = (parseInt(ActualTotalCourse) / parseInt(ActualGreyHeight)) * 2.54;
            if (ActualCPI == "NaN" || ActualCPI == "Infinity") {
                ActualCPI = 0;
            }
            $formEl.find('#ActualCPI').val(parseInt(ActualCPI));
        });

        $('input[type=radio][name=GrayFabricOK]').change(function () {
            if (this.value == 'true') {
                $formEl.find("#confirmation-status").fadeOut();
            } else {
                $formEl.find("#confirmation-status").fadeIn();
            }
        });

        $formEl.find("#btnAddColor_KC").on("click", function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Color for free concept",
                pageId: pageId,
                height: 350,
                data: KnittingProgram.ColorList,
                fields: "text",
                headerTexts: "Color Name",
                isMultiselect: true,
                primaryKeyColumn: "id",
                onMultiselect: function (selectedRecords) {
                    selectedRecords.forEach(function (value) {
                        var exists = $tblChildEl.getCurrentViewRecords().find(function (el) { return el.ColorId == value.id });
                        if (!exists) $tblChildEl.getCurrentViewRecords().unshift({ ColorId: value.id, ColorName: value.text });
                    });
                    initChildTable($tblChildEl.getCurrentViewRecords());
                    $tblChildEl.refresh();
                }
            });

            finder.showModal();
        });
    });

    function handleCommands(args) {
        if (args.commandColumn.type == 'ViewReport') {
            window.open(`/reports/InlinePdfView?ReportName=KnittingJobCard.rdl&KJobCardNo=${args.rowData.KJobCardNo}`, '_blank');
        }
        if (args.commandColumn.type == 'Edit') {
            if (args.rowData.SubGroupID == 1) {
                $formEl.find("#divColorWayDesignOk").fadeOut();
                $formEl.find("#divActualTotalNeedle").fadeOut();
                $formEl.find("#divActualTotalCourse").fadeOut();
                $formEl.find("#divActualGreyHeight").fadeOut();
                $formEl.find("#divActualGreyLength").fadeOut();
                $formEl.find("#divActualNeedle").fadeOut();
                $formEl.find("#divActualCPI").fadeOut();
                $formEl.find("#divGrayGSM").fadeIn();
                //$formEl.find("#need-yes").prop("disabled", false);
                //$formEl.find("#need-no").prop("disabled", false);
            } else {
                $formEl.find("#divColorWayDesignOk").fadeIn();
                $formEl.find("#divActualTotalNeedle").fadeIn();
                $formEl.find("#divActualTotalCourse").fadeIn();
                $formEl.find("#divActualGreyHeight").fadeIn();
                $formEl.find("#divActualGreyLength").fadeIn();
                $formEl.find("#divActualNeedle").fadeIn();
                $formEl.find("#divActualCPI").fadeIn();
                $formEl.find("#divGrayGSM").fadeOut();
                //$formEl.find("#need-yes").prop("disabled", false);
                //$formEl.find("#need-no").prop("disabled", false);
            }

            getDetails(args.rowData.KPMasterID, args.rowData.IsBDS, args.rowData.ConceptID, args.rowData.GroupConceptNo);
        }
    }

    function initMasterTable() {
        var commands = [
            {
                type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' }
            },
            {
                type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' }
            }
        ];

        var columns = [
            { headerText: '', commands: commands, textAlign: 'Center', width: ch_setActionCommandCellWidth(commands) },
            { field: 'SubGroupID', visible: false },
            { field: 'ReqDeliveryDate', headerText: 'Req. DeliveryDate', textAlign: 'left', type: 'date', format: _ch_date_format_1, visible: status == 2 },
            { field: 'KJobCardNo', headerText: 'KJobCard No' },
            { field: 'DateAdded', headerText: 'Confirmation Date', textAlign: 'left', type: 'date', format: _ch_date_format_1, visible:status==3 },
            { field: 'PlanNo', headerText: 'Program No' },
            { field: 'ConceptNo', headerText: 'Concept No' },
            { field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Right', type: 'date', format: _ch_date_format_1 },
            { field: 'ConceptForName', headerText: 'Concept For', visible:false },
            { field: 'BuyerName', headerText: 'Buyer Name'},
            { field: 'ColorName', headerText: 'Color' },
            { field: 'KnittingType', headerText: 'Knitting Type' },
            { field: 'SubGroupName', headerText: 'Sub Group Name' },
            { field: 'TechnicalName', headerText: 'Technical Name' },
            { field: 'Size', headerText: 'Size' },
            { field: 'Composition', headerText: 'Composition' },
            { field: 'GSM', headerText: 'GSM' },
            { field: 'Qty', headerText: 'QTY' },
            { field: 'PlanQty', headerText: 'Plan Qty' }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            allowFiltering: true,
            apiEndPoint: `/api/knitting-confirmation/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function initChildTable(records) {
        if ($tblChildEl) $tblChildEl.destroy();
        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            allowResizing: true,
            dataSource: records,
            commandClick: chhandleCommands_KC,
            //toolbar: [ 'Edit', 'Delete'],
            editSettings: { allowEditing: true, allowDeleting: true },
            columns: [
                //{
                //    headerText: 'Command', commands: [
                //        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                //        { type: 'Add', title: 'Remove this row', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }
                //    ]
                //},
                {
                    field: 'ContactID', visible: false
                },
                {
                    field: 'ColorId', visible: false
                },
                {
                    field: 'ColorName', headerText: 'Color Name', allowEditing: false
                },
                {
                    field: 'ColorCode', headerText: 'Color Code', editType: 'text'
                },
                {
                    field: 'Remarks', headerText: 'Remarks'
                }
            ]
        });
        $tblChildEl.appendTo(tblChildId);
    }

    function chhandleCommands_KC(args) {
        if (args.commandColumn.type == "Add") {
            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                if (yes) {
                    var i = 0;
                    while (i < $tblChildEl.getCurrentViewRecords().length) {
                        if ($tblChildEl.getCurrentViewRecords()[i].ColorId === args.rowData.ColorId) {
                            $tblChildEl.getCurrentViewRecords().splice(i, 1);
                        } else {
                            ++i;
                        }
                    }
                    initChildTable($tblChildEl.getCurrentViewRecords());
                }
            });
        }
    }

    function initTblYarn(records) {
        $tblYarnEl.bootstrapTable("destroy");
        $tblYarnEl.bootstrapTable({
            uniqueId: 'KPYarnID',
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
                    title: "Spinner",
                    filterControl: "input",
                    align: 'center'
                },
                {
                    field: "YarnPly",
                    title: "Yarn Ply",
                    filterControl: "input",
                    align: 'center'
                },
                //{
                //    field: "YarnPly",
                //    title: "Yarn Ply",
                //    filterControl: "input",
                //    align: 'center',
                //    //visible: (KnittingProgram.SubGroupID == 1), //1 = fabric
                //    editable: {
                //        type: "text",
                //        showbuttons: false,
                //        tpl: '<input type="decimal" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                //        validate: function (value) {
                //            if (!value || !value.trim() || isNaN(parseFloat(value)) || parseFloat(value) <= 0) {
                //                return 'Must be a positive integer.';
                //            }
                //        }
                //    }
                //},
                {
                    field: "StitchLength",
                    title: "Stitch Length",
                    filterControl: "input",
                    align: 'center',
                    visible: (KnittingProgram.SubGroupID == 1) //1 = fabric
                }
            ],
            data: records
        });
    }

    function initTblFabricChild() {
        $formEl.find("#tblFabricInformationKC").bootstrapTable('destroy').bootstrapTable({
            uniqueId: 'ItemMasterID',
            checkboxHeader: false,
            /* detailView: true,*/
            columns: [
                {
                    field: "MCSubClassName",
                    title: "M/C Sub Class"
                },
                {
                    field: "MachineGauge",
                    title: "Machine Gauge"
                },
                {
                    field: "MachineDia",
                    title: "Machine Dia"
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
                    field: "Brand",
                    title: "Brand"
                },
                {
                    field: "KnittingMachineNo",
                    title: "Machine"
                },
                {
                    field: "FUPartName",
                    title: "End Use",
                    visible: (KnittingProgram.SubGroupID != 1), //1 = fabric
                    width: 100
                },
                {
                    field: "KnittingTypeID",
                    title: "Knitting Type",
                    visible: (KnittingProgram.SubGroupID == 1), //1 = fabric
                    align: 'center'
                },
                {
                    field: "Needle",
                    title: "Needle/cm",
                    visible: (KnittingProgram.SubGroupID != 1), //1 = fabric
                    align: 'center'
                },
                {
                    field: "CPI",
                    title: "CPI",
                    visible: (KnittingProgram.SubGroupID != 1), //1 = fabric
                    align: 'center'
                },
                {
                    field: "TotalNeedle",
                    title: "Total Needle",
                    visible: (KnittingProgram.SubGroupID != 1), //1 = fabric
                    align: 'center'
                },
                {
                    field: "TotalCourse",
                    title: "Total Course",
                    visible: (KnittingProgram.SubGroupID != 1), //1 = fabric
                    align: 'center'
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
                },
                {
                    field: "KJobCardNo",
                    title: "Job Card No"
                }
            ]
        });
    }

    function backToListWithoutFilter() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        $formEl.find("#btnAddColor_KC").fadeOut();
    }

    function backToList() {
        backToListWithoutFilter();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#KPMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getDetails(id, isBDS, conceptId, grpConceptNo) {
        axios.get(`/api/knitting-confirmation/${id}/${isBDS}/${conceptId}/${grpConceptNo}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                KnittingProgram = response.data;
                KnittingProgram.ReqDeliveryDate = formatDateToDefault(KnittingProgram.ReqDeliveryDate);
                KnittingProgram.ConceptDate = formatDateToDefault(KnittingProgram.ConceptDate);
                setFormData($formEl, KnittingProgram);
                initChildTable(KnittingProgram.ChildColors);
                $formEl.find("#RemainingQty").val(parseFloat(KnittingProgram.Qty) - parseFloat(KnittingProgram.PlanQty));

                if (KnittingProgram.ConceptForName === conceptType.ColorBased) {
                    $("#divConceptStatusChange_KC").fadeOut();
                    $("#divConceptColor_KC").fadeIn();
                } else {
                    $("#divConceptStatusChange_KC").fadeIn();
                    $("#divConceptColor_KC").fadeOut();
                }

                initTblYarn(KnittingProgram.Yarns);
                $("#divKnittingPlanYarnKC").fadeIn();
                /*  *//*  if (KnittingProgram.SubGroupName === subGroupNames.FABRIC) {*/
                $("#divFabricInformationKC").fadeIn();

                initTblFabricChild();
                $formEl.find("#tblFabricInformationKC").bootstrapTable("load", KnittingProgram.Childs);
                $formEl.find("#tblFabricInformationKC").bootstrapTable('hideLoading');
                //getChildData();
                /*   }*/

                if (KnittingProgram.GrayFabricOK) {
                    $("#gray-ok").prop("checked", true);
                    $formEl.find("#confirmation-status").fadeOut();
                }
                else {
                    $("#gray-not-ok").prop("checked", true);
                    $formEl.find("#confirmation-status").fadeIn();
                }

                if (KnittingProgram.NeedPreFinishingProcess) {
                    $("#need-yes").prop("checked", true);
                    //$formEl.find("#need-process").fadeOut();
                }
                else {
                    $("#need-no").prop("checked", true);
                    //$formEl.find("#need-process").fadeIn();
                }

                if (KnittingProgram.ColorWayDesignOk) {
                    $("#color-ok").prop("checked", true);
                }
                else {
                    $("#color-not-ok").prop("checked", true);
                }

                if (KnittingProgram.SubGroupName === subGroupNames.FABRIC) {
                    $formEl.find("#divGrayWidth,#divComposition,#divGSM").fadeIn();
                } else {
                    $formEl.find("#divGrayWidth,#divComposition,#divGSM").fadeOut();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {

        KnittingProgram.ChildColors = $tblChildEl.getCurrentViewRecords();
        var data = formDataToJson($formEl.serializeArray());
        data.GrayFabricOK = convertToBoolean(data.GrayFabricOK);
        data.NeedPreFinishingProcess = KnittingProgram.NeedPreFinishingProcess;
        data["Childs"] = KnittingProgram.Childs;
        data["ChildColors"] = KnittingProgram.ChildColors;
        //data["Yarns"] = KnittingProgram.Yarns;

        if (data.GrayFabricOK && KnittingProgram.SubGroupName === subGroupNames.FABRIC) {
            if (parseFloat($formEl.find("#GrayGSM").val()) == 0) {
                toastr.error("Please enter Gray GSM!"); return;
            }
            if (parseFloat($formEl.find("#GrayWidth").val()) == 0) {
                toastr.error("Please enter Gray Width!"); return;
            }
        }
        axios.post("/api/knitting-confirmation/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();