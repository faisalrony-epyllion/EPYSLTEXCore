(function () {
    var menuId, pageName;
    var toolbarId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl;
    var sampleBookingVm;

    var status = 2;
    var filterBy = {};
    var bookingtype;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var childEl;
    var isAllChecked = false;
    var isCustomChecked = false;
    var isCustomCheckedYarnCount = false;
    var isCustomCheckedElastaneYarnCount = false;
    var isEntryByChecked = false;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $pageEl = $(pageConstants.PAGE_ID_PREFIX + pageId);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        initMasterTable();
        getMasterTableData();

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();

            var data = formDataToJson($formEl.serializeArray());
            data.Id = $formEl.find("#Id").val();

            data["Childs"] = sampleBookingVm.Childs;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/api/sample-booking/SampleBookingSave", data, config)
                .then(function () {
                    toastr.success("Your booking saved successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            var data = {};
            data.BAnalysisId = $formEl.find("#Id").val();
            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/api/sample-booking/ApproveSampleBooking", data, config)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnBackToList").on("click", function (e) {
            e.preventDefault();
            backToList();
        });

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            resetSampleBookingTableParams();
            status = statusConstants.PENDING;
            isAllChecked = false;
            isEntryByChecked = false;
            initMasterTable();
            getMasterTableData();

            toggleActiveToolbarBtn(this, $toolbarEl);
        });

        $toolbarEl.find("#btnPartiallyCompletedList").on("click", function (e) {
            e.preventDefault();
            resetSampleBookingTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;
            isAllChecked = false;
            isEntryByChecked = true;
            initMasterTable();
            getMasterTableData();

            $("#btnSave").fadeIn();
            $("#btnProposeBookingAnalysis").fadeIn();
            $("#btnSampleBookingApprovedBookingAnalysis").fadeOut();
            $("#btnBookingAnalysisAcknowledgeBookingAnalysis").fadeOut();
            $("#btnBookingAnalysisRejectBookingAnalysis").fadeOut();

            toggleActiveToolbarBtn(this, $toolbarEl);
        });

        $toolbarEl.find("#btnApprovedList").on("click", function (e) {
            e.preventDefault();
            resetSampleBookingTableParams();
            status = statusConstants.APPROVED;
            isAllChecked = false;
            isEntryByChecked = true;
            initMasterTable();
            getMasterTableData();

            toggleActiveToolbarBtn(this, $toolbarEl);
        });
    });

    function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            showRefresh: true,
            showExport: true,
            showColumns: true,
            toolbar: toolbarId,
            exportTypes: "['csv', 'excel']",
            pagination: true,
            filterControl: true,
            searchOnEnterKey: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 50,
                    visible: !isAllChecked,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-default btn-xs edit" href="javascript:void(0)" title="View Booking">',
                            '<i class="fa fa-eye"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            bookingtype = row.BookingType;
                            if (bookingtype == "1") // Sample Booking (Without EWO)
                            {
                                getDetails(row.Id, row.BookingId);
                                $formEl.find("#divFabricBookingItem").fadeIn();
                            }
                            //if (bookingtype == "2") // Revised Booking
                            //{
                            //    getBookingAnalysisMaster(row.Id, row.BookingId);
                            //}
                        }
                    }
                },
                {
                    field: "BookingNo",
                    title: "Booking No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function (value, row, index, field) {
                        return getSampleBookingMasterTableCellStyle(row.BookingType);
                    }
                },
                {
                    field: "BuyerName",
                    title: "Buyer",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function (value, row, index, field) {
                        return getSampleBookingMasterTableCellStyle(row.BookingType);
                    }
                },
                {
                    field: "BuyerTeamName",
                    title: "Buyer Team",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function (value, row, index, field) {
                        return getSampleBookingMasterTableCellStyle(row.BookingType);
                    }
                },
                {
                    field: "StyleNo",
                    title: "Style No",
                    filterControl: "input",
                    visible: !isAllChecked,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function (value, row, index, field) {
                        return getSampleBookingMasterTableCellStyle(row.BookingType);
                    }
                },
                {
                    field: "MerchandiserName",
                    title: "Merchandiser",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function (value, row, index, field) {
                        return getSampleBookingMasterTableCellStyle(row.BookingType);
                    }
                },
                {
                    field: "BookingDate",
                    title: "Booking Date",
                    filterControl: "input",
                    visible: !isAllChecked,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function (value, row, index, field) {
                        return getSampleBookingMasterTableCellStyle(row.BookingType);
                    }
                },
                {
                    field: "BookingType",
                    title: "BookingType",
                    filterControl: "input",
                    visible: false,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function (value, row, index, field) {
                        return getSampleBookingMasterTableCellStyle(value);
                    }
                },
                {
                    field: "EntryBy",
                    title: "Entry By",
                    filterControl: "input",
                    visible: isEntryByChecked,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function (value, row, index, field) {
                        return getSampleBookingMasterTableCellStyle(value);
                    }
                },
                {
                    field: "DateAdded",
                    title: "Entry Date",
                    filterControl: "input",
                    visible: isEntryByChecked,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    cellStyle: function (value, row, index, field) {
                        return getSampleBookingMasterTableCellStyle(value);
                    }
                }
            ],
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                    return;

                tableParams.offset = newOffset;
                tableParams.limit = newLimit;

                getMasterTableData();
            },
            onSort: function (name, order) {
                tableParams.sort = name;
                tableParams.order = order;
                tableParams.offset = 0;

                getMasterTableData();
            },
            onRefresh: function () {
                resetSampleBookingTableParams();
                getMasterTableData();
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

                getMasterTableData();
            }
        });
    }

    function initChildTable(data) {
        $formEl.find("#TblSampleBookingChild").bootstrapTable('destroy');
        $formEl.find("#TblSampleBookingChild").bootstrapTable({
            detailView: true,
            uniqueId: 'Id',
            columns: [
                {
                    field: "FabricConstruction",
                    title: "Construction",
                    filterControl: "input",
                    width: 100
                },
                {
                    field: "FabricComposition",
                    title: "Composition",
                    filterControl: "input",
                    width: 100
                },
                {
                    field: "ColorName",
                    title: "Fabric Color",
                    filterControl: "input",
                    width: 100
                },
                {
                    field: "TechnicalNameId",
                    title: "Technical Name",
                    editable: {
                        source: sampleBookingVm.FabricTechnicalNameList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Technical Name' }
                    },
                    visible: !isCustomChecked,
                    width: 100
                },
                {
                    field: "ShadeId",
                    title: "Shade Name",
                    editable: {
                        type: 'select2',
                        title: 'Select a Shade',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: sampleBookingVm.ShadeList,
                        select2: { width: 100, placeholder: 'Fabric Shade' }
                    },
                    visible: !isCustomChecked,
                    width: 100
                },
                {
                    field: "FabricGsm",
                    title: "GSM",
                    align: "center",
                    filterControl: "input",
                    width: 100
                },
                {
                    field: "FabricWidth",
                    title: "Width",
                    align: "center",
                    filterControl: "input",
                    width: 100
                },
                //{
                //    field: "SampleReference",
                //    title: "Sample Reference",
                //    align: "center",
                //    filterControl: "input",
                //    width: 100,
                //    editable: {
                //        type: 'text',
                //        inputclass: 'input-sm',
                //        showbuttons: false
                //    }
                //},
                {
                    field: "KnittingType",
                    title: "Knitting Type",
                    align: "left",
                    filterControl: "input",
                    width: 100
                },
                {
                    field: "DyeingType",
                    title: "YD/Solid",
                    align: "left",
                    filterControl: "input",
                    width: 100
                },
                {
                    field: "YarnType",
                    title: "Yarn Type",
                    align: "left",
                    filterControl: "input",
                    width: 100
                },
                {
                    field: "YarnProgram",
                    title: "Yarn Program",
                    align: "left",
                    filterControl: "input",
                    width: 100
                },
                {
                    field: "LabdipNo",
                    title: "Lab Dip No",
                    align: "left",
                    filterControl: "input",
                    width: 100
                },
                {
                    field: "Remarks",
                    title: "Instruction",
                    align: "left",
                    filterControl: "input",
                    width: 100
                },
                {
                    field: "BookingQty",
                    title: "Booking Qty",
                    align: "left",
                    filterControl: "input",
                    width: 100
                },
                {
                    field: "DisplayUnitDesc",
                    title: "Booking UOM",
                    align: "left",
                    filterControl: "input",
                    width: 100
                }
            ],
            data: data,
            onExpandRow: function (index, row, $detail) {
                if (!row.TechnicalNameId) {
                    return bootbox.alert({
                        message: '<span class="text-danger">Your must select a technical name before expand.<span>',
                        size: 'small'
                    });
                }
                if (!row.ShadeId) {
                    return bootbox.alert({
                        message: '<span class="text-danger">Your must select a shade before expand.<span>',
                        size: 'small'
                    });
                }

                var buyerId = $formEl.find("#BuyerId").val();
                isCustomCheckedElastaneYarnCount = row.FabricComposition.match(/Elastane/g);
                var childId = $formEl.find("#EntityState").val() == 4 ? 0 : row.Id;

                getElastaneYarnCountLists(row.ConstructionId, row.FabricGsm);

                populateBookingAnalysisChildYarn(childId, row.BookingChildId, row.ConstructionId, row.FabricGsm, row.FabricWidth, row.ShadeId, row.TechnicalNameId, row.FabricComposition, row.DyeingTypeId, buyerId, row.KnittingTypeId, row.YarnProgram, $detail);
            },
            onEditableSave: function (field, row, oldValue, $el) {
                if (row.ShadeId && !row.SampleReference) {
                    var matchingChilds = sampleBookingVm.Childs.filter(function (el) {
                        return el.ConstructionId === row.ConstructionId && el.CompositionId === row.CompositionId && el.FabricColorId === row.FabricColorId;
                    });

                    if (matchingChilds.length <= 1)
                        return;

                    showBootboxConfirm("Similar Shade?", "Do you want to apply this shade for similar items?", function (yes) {
                        if (yes) {
                            $.each(matchingChilds, function (i, row) {
                                row.ShadeName = sampleBookingVm.ShadeList.find(function (el) {
                                    return el.id == row.ShadeId;
                                }).text;

                                $formEl.find("#TblSampleBookingChild").bootstrapTable('updateByUniqueId', row.Id, row);
                            });
                        }
                    });
                }
            }
        });
    }

    function initBookingAnalysisChildYarn($innerTableEl, bookingChildId, constructionId, fabricGsm, fabricWidth, shadeId, technicalNameId, dyeingtype, buyerId, knittingtype, yProgram, data) {
        $innerTableEl.bootstrapTable({
            showFooter: true,
            data: data,
            uniqueId: 'Id',
            rowStyle: function (row, index) {
                if (row.EntityState == 8)
                    return { classes: 'deleted-row' };

                return "";
            },
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 100,
                    formatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    footerFormatter: function () {
                        return [
                            '<span class="btn-group">',
                            '<button class="btn btn-success btn-xs edit" onclick="return addNewChildRow(event, ' + bookingChildId + ',' + constructionId + ',' + fabricGsm + ',' + fabricWidth + ',' + shadeId + ',' + technicalNameId + ',' + dyeingtype + ',' + buyerId + ',' + knittingtype + ',' + "'" + yProgram + "'" + ')" title="Add">',
                            '<i class="fa fa-plus"></i>',
                            ' Add',
                            '</button>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            this.data[index].EntityState = 8;
                            var $target = $(e.target);
                            $target.closest("tr").addClass('deleted-row');
                        }
                    }
                },
                {
                    field: "Segment1ValueId",
                    title: "Yarn Type",
                    align: "left",
                    width: 80,
                    editable: {
                        source: sampleBookingVm.YarnTypeList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Yarn Type' }
                    }
                },
                {
                    field: "Segment3ValueId",
                    title: "Yarn Composition",
                    width: 100,
                    editable: {
                        type: 'select2',
                        title: 'Select a Yarn Composition',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: sampleBookingVm.YarnCompositionList,
                        select2: { width: 200, placeholder: 'Yarn Composition' }
                    }
                },
                {
                    field: "Segment2ValueId",
                    title: "Yarn Count",
                    formatter: function (value, row, index, field) {
                        var yarnCount = !row.Segment2ValueDesc || row.Segment2ValueDesc == 'undefined' || row.Segment2ValueDesc == 'null' ? "Empty" : row.Segment2ValueDesc;
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + yarnCount + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            if (!row.Segment1ValueId) return toastr.error("Yarn Type is not selected");

                            getYarnCountByYarnType(row, $innerTableEl);
                        },
                    }
                },
                {
                    field: "Segment5ValueId",
                    title: "Yarn Color",
                    width: 80,
                    editable: {
                        type: 'select2',
                        title: 'Select a Color',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: sampleBookingVm.YarnColorList,
                        select2: { width: 100, placeholder: 'Yarn Color' }
                    }
                },
                {
                    field: "YarnSubProgramIds",
                    title: "Yarn Sub Program",
                    editable: {
                        type: 'select2',
                        showbuttons: true,
                        source: sampleBookingVm.YarnSubProgramList,
                        select2: { width: 200, height: 300, multiple: true, placeholder: 'Select Sub Program' }
                    }
                },
                {
                    field: "YarnCategory",
                    title: "Yarn Category",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "YarnSpecification",
                    title: "Yarn Specification",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "Segment4ValueDesc",
                    title: "Shade",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "IsYD",
                    title: "YD",
                    placeholder: 'YD',
                    checkbox: true
                },
                {
                    field: "FinalMachineGauge",
                    title: "Production MG",
                    width: 40,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "FinalMachineDia",
                    title: "Production MD",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "FinalStitchLength",
                    title: "Production SL",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "YarnDistribution",
                    title: "Yarn Distribution (%)",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "NetConsumption",
                    title: "Net Consumption"
                },
                {
                    field: "RequiredYarnQty",
                    title: "Required Qty"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "UOM"
                },
                {
                    field: "YarnStatusId",
                    title: "Yarn Status",
                    width: 80,
                    editable: {
                        source: sampleBookingVm.YarnStatusList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Yarn Status' }
                    }
                },
                {
                    field: "YarnLotNo",
                    title: "Yarn Lot No",
                    width: 100,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "SDR",
                    title: "SDR No",
                    width: 100,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "KnittingDate",
                    title: "Knitting Date",
                    formatter: function (value, row, index, field) {
                        return ['<a href="javascript:void(0)" class="editable-link edit">' + value + '</a>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();

                            $(e.target).datepicker({
                                autoclose: true,
                                todayHighlight: true,
                                //startDate: "0d",
                                todayBtn: true
                            }).on('changeDate', function (e) {
                                try {
                                    row.KnittingDate = moment(e.date).format("MM/DD/YYYY");
                                    $innerTableEl.bootstrapTable('updateByUniqueId', row.Id, row);
                                } catch (e) {
                                    row.ColumnValue = "";
                                    //console.log(e);
                                }
                            });
                        }
                    }
                },
                {
                    field: "YarnSupplierId",
                    title: "Yarn Supplier",
                    width: 200,
                    editable: {
                        source: sampleBookingVm.YarnSupplierList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Yarn Supplier' }
                    }
                },
                {
                    field: "MachineNo",
                    title: "Machine No",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "MachineBrand",
                    title: "Machine Brand",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "BookingQtyKg",
                    title: "Booking Qty (KG)",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "FinalAllowancePer",
                    title: "Production Allowance (%)",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "KnittedQty",
                    title: "Knitted Qty",
                    width: 20,
                    align: "right",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "RemarksOnYarn",
                    title: "Special Instruction",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                }
            ],
            onEditableSave: function (field, row, oldValue, $el) {
                switch (field) {
                    case "Segment1ValueId":
                        row.Segment1ValueDesc = sampleBookingVm.YarnTypeList.find(function (el) { return el.id == row.Segment1ValueId }).text;
                        break;
                    case "Segment3ValueId":
                        row.Segment3ValueDesc = sampleBookingVm.YarnCompositionList.find(function (el) { return el.id == row.Segment3ValueId }).text;
                        break;
                    case "Segment5ValueId":
                        row.Segment5ValueDesc = sampleBookingVm.YarnColorList.find(function (el) { return el.id == row.Segment5ValueId }).text;
                        break;
                    case "YarnProgramId":
                        row.YarnProgram = sampleBookingVm.YarnProgramList.find(function (el) { return el.id == row.YarnProgramId }).text;
                        break;
                    case "YarnSubProgramIds":
                        var selectedYarnSubPrograms = setMultiSelectValueInBootstrapTableEditable(sampleBookingVm.YarnSubProgramList, row.YarnSubProgramIds);
                        row.YarnSubProgramIds = selectedYarnSubPrograms.id;
                        row.YarnSubProgramNames = selectedYarnSubPrograms.text;
                        break;
                    default:
                        break;
                }

                if (row.Segment1ValueId && !row.Segment1ValueDesc)
                    row.Segment1ValueDesc = sampleBookingVm.YarnTypeList.find(function (el) { return el.id == row.Segment1ValueId }).text;
                if (row.Segment3ValueId && !row.Segment3ValueDesc)
                    row.Segment3ValueDesc = sampleBookingVm.YarnCompositionList.find(function (el) { return el.id == row.Segment3ValueId }).text;
                if (row.Segment5ValueId && !row.Segment5ValueDesc)
                    row.Segment5ValueDesc = sampleBookingVm.YarnColorList.find(function (el) { return el.id == row.Segment5ValueId }).text;

                row.YarnCategory = calculateYarnCategory(row);
                row.ProductionYarnCategory = row.YarnCategory;
                if ((row.Segment1ValueId == 625) || (row.Segment1ValueId == 8238)) {
                    row.NoOfThread = "0";
                }

                row.NetConsumption = ((row.BookingQty * row.YarnDistribution) / 100).toFixed(4);
                if (row.FinalAllowancePer == "0")
                    row.RequiredYarnQty = row.NetConsumption;
                else
                    row.RequiredYarnQty = (((row.BookingQty * row.YarnDistribution) / 100) + ((row.FinalAllowancePer * row.NetConsumption) / 100)).toFixed(4);

                $innerTableEl.bootstrapTable('updateByUniqueId', row.Id, row);
            }
        });
    }

    function populateBookingAnalysisChildYarn(childId, bookingChildId, constructionId, fabricGsm, fabricWidth, shadeId, technicalNameId, fabricComposition, dyeingtype, buyerId, knittingtype, yProgram, $detail) {
        var $el = $detail.html('<table id="TblSampleBookingChildYarn-' + bookingChildId + '"></table>').find('table');

        // Set SampleBookingAnalysisChild
        var sampleBookingAnalysisChild = {};
        if (childId > 0) {
            sampleBookingAnalysisChild = sampleBookingVm.Childs.find(function (el) { return el.Id == childId; });
            initBookingAnalysisChildYarn($el, bookingChildId, constructionId, fabricGsm, fabricWidth, shadeId, technicalNameId, dyeingtype, buyerId, knittingtype, yProgram, sampleBookingAnalysisChild.ChildYarns);
        }
        else {
            sampleBookingAnalysisChild = sampleBookingVm.Childs.find(function (el) { return el.BookingChildId == bookingChildId; });

            var url = "";
            if (constructionId == 14 || constructionId == 15 || constructionId == 16 || constructionId == 18 || constructionId == 19 || constructionId == 21) {
                url = "/api/sample-booking/newbookinganalysischildyarn?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&fabricGsm=" + fabricGsm + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + fabricComposition + "&fabTechnicalName=" + technicalNameId + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype;
                isCustomCheckedYarnCount = false;
            }
            else {
                url = "/api/sample-booking/newbookinganalysischildyarnOthers?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&fabricGsm=" + fabricGsm + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + fabricComposition + "&fabTechnicalName=" + technicalNameId + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype;
                isCustomCheckedYarnCount = true;
            }

            // Get SampleBokkingAnalysisChildYarns
            axios.get(url)
                .then(function (response) {
                    $.each(response.data, function (i, v) {
                        sampleBookingAnalysisChild.ChildYarns.push(v);
                    });
                    initBookingAnalysisChildYarn($el, bookingChildId, constructionId, fabricGsm, fabricWidth, shadeId, technicalNameId, dyeingtype, buyerId, knittingtype, yProgram, response.data);
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });
        }
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = "/api/sample-booking/list?gridType=bootstrap-table&status=" + status + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getDetails(id, bookingId) {
        var url = "/api/sample-booking/details/" + id + "/" + bookingId;
        axios.get(url)
            .then(function (response) {
                $divTblEl.fadeOut();
                $divDetailsEl.fadeIn();
                sampleBookingVm = response.data;
                resetForm();
                $formEl.find("#Id").val(sampleBookingVm.Id);
                $formEl.find("#EntityState").val(sampleBookingVm.EntityState);
                var analysisNo = !sampleBookingVm.BAnalysisNo ? sampleBookingVm.BookingNo : sampleBookingVm.BAnalysisNo;
                $formEl.find("#BAnalysisNo").val(analysisNo);
                $formEl.find("#BAnalysisNoOrigin").val(sampleBookingVm.BAnalysisNoOrigin);
                $formEl.find("#Id").val(sampleBookingVm.Id);
                $formEl.find("#PreBAnalysisId").val(sampleBookingVm.Id);
                $formEl.find("#BuyerId").val(sampleBookingVm.BuyerId);
                $formEl.find("#BuyerTeamId").val(sampleBookingVm.BuyerTeamId);
                $formEl.find("#CompanyId").val(sampleBookingVm.CompanyId);
                $formEl.find("#ExportOrderId").val(sampleBookingVm.ExportOrderId);
                $formEl.find("#BookingId").val(sampleBookingVm.BookingId);
                $formEl.find("#SubGroupId").val(sampleBookingVm.SubGroupId);
                $formEl.find("#AdditionalBooking").val(sampleBookingVm.AdditionalBooking);
                $formEl.find("#PreProcessRevNo").val(sampleBookingVm.PreProcessRevNo);
                $formEl.find("#RevisionNo").val(sampleBookingVm.RevisionNo);
                $formEl.find("#YInHouseDate").val(sampleBookingVm.YInHouseDate);
                $formEl.find("#YRequiredDate").val(sampleBookingVm.YRequiredDate);
                $formEl.find("#SpTypeId").val(sampleBookingVm.SpTypeId);
                $formEl.find("#SampleTypeName").val(sampleBookingVm.SampleTypeName);
                $formEl.find("#Remarks").val(sampleBookingVm.Remarks);

                $formEl.find("#lblBookingNo").val(sampleBookingVm.BookingNo);
                $formEl.find("#lblExportOrderNo").val(sampleBookingVm.ExportOrderNo);
                $formEl.find("#lblBuyerName").val(sampleBookingVm.BuyerName);
                $formEl.find("#lblBuyerTeam").val(sampleBookingVm.BuyerTeamName);
                $formEl.find("#lblMerchandiserName").val(sampleBookingVm.MerchandiserName);
                if (sampleBookingVm.BookingDate)
                    $formEl.find("#lblBookingDate").val(moment(sampleBookingVm.BookingDate).format('MM/DD/YYYY'));

                $formEl.find("#OrderSession").val(sampleBookingVm.SessionName);
                $formEl.find("#MerchandisingTeam").val(sampleBookingVm.MerchandisingTeam);
                $formEl.find("#SeasonId").val(sampleBookingVm.SeasonId);
                $formEl.find("#MerchandiserTeamId").val(sampleBookingVm.MerchandiserTeamId);
                $formEl.find("#StyleNo").val(sampleBookingVm.StyleNo);

                if (id > 0) {
                    $formEl.find("#SampleReference").val(sampleBookingVm.SampleReference);
                }

                $formEl.find("#EWPNo").val(sampleBookingVm.EwpNo);

                $formEl.find("#divEditSampleBookingMaster").fadeIn();
                $("#divtblSampleBookingMaster").fadeOut();
                $formEl.find("#divButtonExecutionsSampleBooking").fadeIn();

                initChildTable(sampleBookingVm.Childs);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    window.addNewChildRow = function (e, bookingChildId, constructionId, fabricGsm, fabricWidth, shadeId, technicalNameId, dyeingtype, buyerId, knittingtype, yProgram) {
        childEl = $("#TblSampleBookingChildYarn-" + bookingChildId);
        showBootboxSelect2Dialog("Yarn Composition", "YarnCompositionId", "Select Composition", sampleBookingVm.YarnCompositionList, function (result) {
            if (result) {
                yarnComposition = result.text;

                if (constructionId == 14 || constructionId == 15 || constructionId == 16 || constructionId == 18 || constructionId == 19 || constructionId == 21) {
                    var url = "/api/newbookinganalysischildyarnPopUp?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&fabricGsm=" + fabricGsm
                        + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + yarnComposition + "&fabTechnicalName=" + technicalNameId
                        + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype + "&yarnProgram=" + yProgram;
                }
                else {
                    var url = "/api/newbookinganalysischildyarnOthersPopUp?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&fabricGsm=" + fabricGsm
                        + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + yarnComposition + "&fabTechnicalName=" + technicalNameId
                        + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype + "&yarnProgram=" + yProgram;
                }

                var sampleBookingAnalysisChild = sampleBookingVm.Childs.find(function (el) { return el.BookingChildId == bookingChildId; });
                axios.get(url)
                    .then(function (response) {
                        response.data[0].Id = getMaxIdForArray(sampleBookingVm.Childs, "Id");
                        sampleBookingAnalysisChild.ChildYarns.push(response.data[0]);
                        $(childEl).bootstrapTable('load', sampleBookingAnalysisChild.ChildYarns);
                        isCustomCheckedElastaneYarnCount = yarnComposition.match(/Elastane/g);
                    })
                    .catch(function () {
                        toastr.error(constants.LOAD_ERROR_MESSAGE);
                    })
            }
        });
    }

    function getElastaneYarnCountLists(constructionId, fabricGsm) {
        var url = "/api/selectoption/yarncountlistsElastane/" + constructionId + "/" + fabricGsm;
        axios.get(url)
            .then(function (response) {
                elastaneYarnCountList = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function resetSampleBookingTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
    }

    function getSampleBookingMasterTableCellStyle(value) {
        if (value == "0")
            return { classes: "c-bg-danger" };
        else if (value == "2")
            return { classes: "c-bg-warning" };
        else
            return ""
    }

    function getYarnCountByYarnType(rowData, $tableEl) {
        var yarnTypeId = rowData.Segment1ValueId;
        var url = "/api/selectoption/yarn-count-by-yarn-type/" + yarnTypeId;
        axios.get(url)
            .then(function (response) {
                var yarnCountList = response.data;
                showBootboxSelect2Dialog("Select Yarn Count", "sbYarnCount", "Select Yarn Count", yarnCountList, function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected any Yarn Count.");

                    rowData.Segment2ValueId = result.id;
                    rowData.Segment2ValueDesc = result.text;

                    if (rowData.Segment1ValueId && !rowData.Segment1ValueDesc)
                        rowData.Segment1ValueDesc = sampleBookingVm.YarnTypeList.find(function (el) { return el.id == rowData.Segment1ValueId }).text;
                    if (rowData.Segment3ValueId && !rowData.Segment3ValueDesc)
                        rowData.Segment3ValueDesc = sampleBookingVm.YarnCompositionList.find(function (el) { return el.id == rowData.Segment3ValueId }).text;
                    if (rowData.Segment5ValueId && !rowData.Segment5ValueDesc)
                        rowData.Segment5ValueDesc = sampleBookingVm.YarnColorList.find(function (el) { return el.id == rowData.Segment5ValueId }).text;

                    rowData.YarnCategory = calculateYarnCategory(rowData);
                    rowData.ProductionYarnCategory = rowData.YarnCategory;

                    $tableEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
})();