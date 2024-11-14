(function () {
    var formSampleBookingMaster;
    var bookingAnalysisMaster;
    var status = 2;
    var filterBy = {};
    var yearnCompositionfilterBy = {};
    var ShadeID2;
    var bookingtype;
    var yComposition;
    var yType;
    var yarnTypeName;
    var yarnColorName;
    var yProgram;
    var ySubProgram;
    var yarnCount;
    var com1V;
    var com2V;
    var Com1V;
    var Com2V;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var yarnCompositionTableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var isPendingItem = false;
    var shadeList = [];
    var yarnColorList = [];
    var yarnCountList = [];
    var yarnCountALLList = [];
    var yarnSubProgramList = [];
    var fabricTechnicalNameList = [];
    var yarnTypeLists = [];
    var yarnYDStatusList = [];
    var processMasterList = [];
    var constructionChildList = [];
    var elastaneYarnCountList = [];
    var yarnSupplierLists = [];
    var yarnStatusLists = [];

    var childEl;
    var childSaveData = [];
    var isAllChecked = false;
    var isCustomChecked = false;
    var isCustomCheckedYarnColor = false;
    var isCustomCheckedYarnCount = false;
    var isCustomCheckedYarnCount2 = false;
    var isCustomCheckedElastaneYarnCount = false;
    var isLaterCustomChecked = false;
    var isFMTDDateChecked = false;
    var bookingQty;
    var bookingChildId;
    var constructionId;
    var fabricGsm;
    var fabricWidth;
    var shadeId;

    var sampleBookingAnalysisChildList = [];

    $(function () {
        formSampleBookingMaster = $("#formSampleBookingMaster");

        $("#btnSampleBookingPending").css('background', '#1B4F72');
        $("#btnSampleBookingPending").css('color', '#FFFFFF');

        initSampleBookingMasterTable();
        getSampleBookingMasterData();
        getFabricShades();
        getYarnColors();
        getFabricTechnicalNames();
        getYarnTypeLists();
        getYarnYDStatus();
        getYarnSubProgramLists();
        getYarnSupplierNames();
        getYarnStatus();
        isFMTDDateChecked = false;

        $("#tblSampleBookingYarnComposition").bootstrapTable({
            pagination: true,
            filterControl: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            columns: [
                {
                    field: "YarnCompositionId",
                    title: "Yarn Composition Id",
                    filterControl: "input",
                    visible: false
                },
                {
                    field: "YarnCompositionName",
                    title: "Yarn Composition",
                    filterControl: "input"
                }
            ],
            onDblClickRow: function (row, $element, field) {
                yComposition = row.YarnCompositionName;

                if (constructionId == 14 || constructionId == 15 || constructionId == 16 || constructionId == 18 || constructionId == 19 || constructionId == 21) {
                    var url = "/api/newbookinganalysischildyarnPopUp?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId
                        + "&fabricGsm=" + fabricGsm + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + row.YarnCompositionName
                        + "&fabTechnicalName=" + TechnicalNameId + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype + "&yarnProgram=" + yProgram;
                }
                else {
                    var url = "/api/newbookinganalysischildyarnOthersPopUp?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId
                        + "&fabricGsm=" + fabricGsm + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + row.YarnCompositionName
                        + "&fabTechnicalName=" + TechnicalNameId + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype + "&yarnProgram=" + yProgram;
                }

                var sampleBookingAnalysisChild = sampleBookingAnalysisChildList.find(function (el) {
                    return el.BookingChildId == bookingChildId;
                });
                axios.get(url)
                    .then(function (response) {
                        response.data[0].Id = getMaxIdForArray(sampleBookingAnalysisChildList, "Id");
                        sampleBookingAnalysisChild.ChildYarns.push(response.data[0]);
                        $(childEl).bootstrapTable('load', sampleBookingAnalysisChild.ChildYarns);
                        if (yComposition.match(/Elastane/g)) {
                            isCustomCheckedElastaneYarnCount = true;
                        }
                        else {
                            isCustomCheckedElastaneYarnCount = false;
                        }
                        $("#SampleBookingModal-Child").modal('hide');
                    })
                    .catch(function () {
                        toastr.error(constants.LOAD_ERROR_MESSAGE);
                    })
            },
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (yarnCompositionTableParams.offset == newOffset && yarnCompositionTableParams.limit == newLimit)
                    return;

                yarnCompositionTableParams.offset = newOffset;
                yarnCompositionTableParams.limit = newLimit;

                getFabricCompositionLists();
            },
            onSort: function (name, order) {
                yarnCompositionTableParams.sort = name;
                yarnCompositionTableParams.order = order;
                yarnCompositionTableParams.offset = 0;

                getFabricCompositionLists();
            },
            onRefresh: function () {
                yarnCompositionTableParams.offset = 0;
                yarnCompositionTableParams.limit = 10;
                yarnCompositionTableParams.sort = '';
                yarnCompositionTableParams.order = '';
                yarnCompositionTableParams.filter = '';

                getFabricCompositionLists();
            },
            onColumnSearch: function (columnName, filterValue) {
                if (columnName in yearnCompositionfilterBy && !filterValue) {
                    delete yearnCompositionfilterBy[columnName];
                }
                else
                    yearnCompositionfilterBy[columnName] = filterValue;

                if (Object.keys(yearnCompositionfilterBy).length === 0 && yearnCompositionfilterBy.constructor === Object)
                    yarnCompositionTableParams.filter = "";
                else
                    yarnCompositionTableParams.filter = JSON.stringify(yearnCompositionfilterBy);

                getFabricCompositionLists();
            }
        });

        $("#tblYarnCountElastane").bootstrapTable({
            cache: false,
            columns: [
                {
                    field: "YarnCountId",
                    title: "Yarn Count Id",
                    filterControl: "input",
                    visible: false
                },
                {
                    field: "FinalYarnCount",
                    title: "Yarn Count",
                    filterControl: "input"
                }
            ],
            onDblClickRow: function (row, $element, field) {
                $element.closest('table').bootstrapTable('updateByUniqueId', row.FinalYarnCount, row);
                $element.bootstrapTable('load', row);
                $("#modal-Elastane-YarnCount").modal('hide');
            }
        });

        $("#btnSaveSampleBooking").click(function (e) {
            e.preventDefault();
            var data = formDataToJson(formSampleBookingMaster.serializeArray());
            data.Id = formSampleBookingMaster.find("#Id").val();
            var hasChildYarnCount = 0;
            $.each(sampleBookingAnalysisChildList, function (i, child) {
                if (child.ChildYarns.length)
                    hasChildYarnCount++;
            });

            data["Childs"] = sampleBookingAnalysisChildList;

            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/SBApi/SampleBookingSave", data, config)
                .then(function () {
                    toastr.success("Your booking saved successfully.");
                    backToListSampleBooking();
                })
                .catch(showResponseError);
        });

        $("#btnApprovedSampleBooking").click(function (e) {
            e.preventDefault();
            var data = {};
            data.BAnalysisId = formSampleBookingMaster.find("#Id").val();
            var config = { headers: { 'Content-Type': 'application/json' } };
            axios.post("/SBApi/ApproveSampleBooking", data, config)
                .then(function () {
                    toastr.success(constants.APPROVE_SUCCESSFULLY);
                    BacktoListSampleBooking();
                })
                .catch(showResponseError);
        });

        $("#btnEditCancelSampleBooking").on("click", function (e) {
            e.preventDefault();
            backToListSampleBooking();
        });

        $("#btnSampleBookingPartiallyCompleted").on("click", function (e) {
            e.preventDefault();
            resetSampleBookingTableParams();
            status = 7;
            isAllChecked = false;
            isFMTDDateChecked = false;
            isLaterCustomChecked = true;
            initSampleBookingMasterTable();
            getSampleBookingMasterData();

            $("#btnSaveSampleBooking").fadeIn();
            $("#btnProposeBookingAnalysis").fadeIn();
            $("#btnSampleBookingApprovedBookingAnalysis").fadeOut();
            $("#btnBookingAnalysisAcknowledgeBookingAnalysis").fadeOut();
            $("#btnBookingAnalysisRejectBookingAnalysis").fadeOut();

            $("#btnSampleBookingPending").css('background', '#FFFFFF');
            $("#btnSampleBookingPending").css('color', '#000000');

            $("#btnSampleBookingPartiallyCompleted").css('background', '#1B4F72');
            $("#btnSampleBookingPartiallyCompleted").css('color', '#FFFFFF');
            $("#btnSampleBookingPartiallyCompleted").css('font-weight', 'bold');

            $("#btnSampleBookingApproved").css('background', '#FFFFFF');
            $("#btnSampleBookingApproved").css('color', '#000000');
        });

        $("#btnSampleBookingPending").on("click", function (e) {
            e.preventDefault();
            resetSampleBookingTableParams();
            status = 2;
            isAllChecked = false;
            isFMTDDateChecked = false;
            isLaterCustomChecked = true;
            initSampleBookingMasterTable();
            getSampleBookingMasterData();

            $("#btnSampleBookingPending").css('background', '#1B4F72');
            $("#btnSampleBookingPending").css('color', '#FFFFFF');
            $("#btnSampleBookingPending").css('font-weight', 'bold');

            $("#btnSampleBookingPartiallyCompleted").css('background', '#FFFFFF');
            $("#btnSampleBookingPartiallyCompleted").css('color', '#000000');

            $("#btnSampleBookingApproved").css('background', '#FFFFFF');
            $("#btnSampleBookingApproved").css('color', '#000000');
        });

        $("#btnSampleBookingApproved").on("click", function (e) {
            e.preventDefault();
            resetSampleBookingTableParams();
            status = 5;
            isAllChecked = false;
            isFMTDDateChecked = false;
            initSampleBookingMasterTable();
            getSampleBookingMasterData();

            $("#btnSampleBookingPending").css('background', '#FFFFFF');
            $("#btnSampleBookingPending").css('color', '#000000');

            $("#btnSampleBookingPartiallyCompleted").css('background', '#FFFFFF');
            $("#btnSampleBookingPartiallyCompleted").css('color', '#000000');

            $("#btnSampleBookingApproved").css('background', '#1B4F72');
            $("#btnSampleBookingApproved").css('color', '#FFFFFF');
            $("#btnSampleBookingApproved").css('font-weight', 'bold');
        });
    });

    function initSampleBookingMasterTable() {
        $("#tblSampleBookingMaster").bootstrapTable('destroy');
        $("#tblSampleBookingMaster").bootstrapTable({
            showRefresh: true,
            showExport: true,
            showColumns: true,
            toolbar: "#SampleBookingToolbar",
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
                            SampleBookingResetForm();

                            formSampleBookingMaster.find("#BookingID").val(row.BookingId);
                            formSampleBookingMaster.find("#Id").val(row.Id);
                            bookingtype = row.BookingType;
                            if (bookingtype == "1") // Sample Booking (Without EWO)
                            {
                                getBookingAnalysisMasterSampleBooking(row.Id, row.BookingId);
                                initTblSampleBookingChild();
                                formSampleBookingMaster.find("#divEditSampleBookingMaster1").fadeIn();;
                                getSampleBookingChildDataSample(row.Id, row.BookingId);
                            }
                            if (bookingtype == "2") // Revised Booking
                            {
                                getBookingAnalysisMaster(row.Id, row.BookingId);
                            }
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
                }
            ],
            onPageChange: function (number, size) {
                var newOffset = (number - 1) * size;
                var newLimit = size;
                if (tableParams.offset == newOffset && tableParams.limit == newLimit)
                    return;

                tableParams.offset = newOffset;
                tableParams.limit = newLimit;

                getSampleBookingMasterData();
            },
            onSort: function (name, order) {
                tableParams.sort = name;
                tableParams.order = order;
                tableParams.offset = 0;

                getSampleBookingMasterData();
            },
            onRefresh: function () {
                resetSampleBookingTableParams();
                getSampleBookingMasterData();
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

                getSampleBookingMasterData();
            }
        });
    }

    function initTblSampleBookingChild() {
        formSampleBookingMaster.find("#TblSampleBookingChild").bootstrapTable({
            detailView: true,
            uniqueId: 'Id',
            columns: [
                {
                    field: "FabricConstruction",
                    title: "Construction",
                    filterControl: "input",
                },
                {
                    field: "FabricComposition",
                    title: "Composition",
                    filterControl: "input"
                },
                {
                    field: "ColorName",
                    title: "Fabric Color",
                    filterControl: "input"
                },
                {
                    field: "TechnicalNameId",
                    title: "Technical Name",
                    editable: {
                        source: fabricTechnicalNameList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Technical Name' }
                    },
                    visible: !isCustomChecked
                },
                {
                    field: "ShadeId",
                    title: "Shade Name",
                    editable: {
                        type: 'select',
                        title: 'Select a Shade',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: shadeList
                    },
                    visible: !isCustomChecked
                },
                {
                    field: "FabricGsm",
                    title: "GSM",
                    align: "center",
                    filterControl: "input"
                },
                {
                    field: "FabricWidth",
                    title: "Width",
                    align: "center",
                    filterControl: "input",
                    width: 50
                },
                {
                    field: "KnittingType",
                    title: "Knitting Type",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "DyeingType",
                    title: "YD/Solid",
                    align: "left",
                    filterControl: "input"
                },
                //{
                //    field: "Finishing",
                //    title: "Finishing",
                //    align: "left",
                //    filterControl: "input"
                //},
                //{
                //    field: "Washing",
                //    title: "Washing",
                //    align: "left",
                //    filterControl: "input"
                //},
                {
                    field: "YarnType",
                    title: "Yarn Type",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "YarnProgram",
                    title: "Yarn Program",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "LabdipNo",
                    title: "Lab Dip No",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "Remarks",
                    title: "Instruction",
                    align: "left",
                    filterControl: "input"
                },
                //{
                //    field: "AOP",
                //    title: "AOP",
                //    align: "left",
                //    filterControl: "input"
                //},
                //{
                //    field: "UsesIn",
                //    title: "Uses In",
                //    align: "left",
                //    filterControl: "input"
                //},
                {
                    field: "BookingQty",
                    title: "Booking Qty",
                    align: "left",
                    filterControl: "input"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "Booking UOM",
                    align: "left",
                    filterControl: "input"
                }
            ],
            onExpandRow: function (index, row, $detail) {
                bookingChildId = row.BookingChildId;
                constructionId = row.ConstructionId;
                fabricGsm = row.FabricGsm;
                fabricWidth = row.FabricWidth;
                shadeId = row.ShadeId;
                TechnicalNameId = row.TechnicalNameId;
                dyeingtype = row.DyeingTypeId;
                knittingtype = row.KnittingTypeId;
                yarnType = row.YarnTypeId;
                buyerId = formSampleBookingMaster.find("#BuyerId").val();
                yComposition = row.FabricComposition;
                yProgram = row.YarnProgram;
                bookingQty = row.BookingQty;

                if (row.FabricComposition.match(/Elastane/g)) {
                    isCustomCheckedElastaneYarnCount = true;
                }
                else {
                    isCustomCheckedElastaneYarnCount = false;
                }
                populateBookingAnalysisChildYarn(row.Id, row.TechnicalNameId, row.FabricComposition, $detail);
            },
            onEditableSave: function (field, row, oldValue, $el) {
                if (row.ShadeId) {
                    var matchingChilds = sampleBookingAnalysisChildList.filter(function (el) {
                        //return el.ConstructionId === row.ConstructionId && el.CompositionId === row.CompositionId && el.FabricColorId === row.FabricColorId;
                        return el.FabricColorId === row.FabricColorId;
                    });

                    bootbox.confirm({
                        size: "small",
                        message: "Do you want to apply this shade for similar items?",
                        buttons: {
                            confirm: {
                                label: 'Yes',
                                className: 'btn-success'
                            },
                            cancel: {
                                label: 'No',
                                className: 'btn-danger'
                            }
                        },
                        callback: function (yes) {
                            if (yes) {
                                $.each(matchingChilds, function (i, item) {
                                    item.ShadeId = row.ShadeId;
                                    item.ShadeName = shadeList.find(function (el) {
                                        return el.value == row.ShadeId;
                                    }).text;

                                    formSampleBookingMaster.find("#TblSampleBookingChild").bootstrapTable('updateByUniqueId', item.Id, item);
                                });
                            }
                            else {
                                $.each(matchingChilds, function (i, item) {
                                    if (!item.shadeId)
                                        return;

                                    item.ShadeId = row.ShadeId;
                                    item.ShadeName = shadeList.find(function (el) {
                                        return el.value == row.ShadeId;
                                    }).text;

                                    formSampleBookingMaster.find("#TblSampleBookingChild").bootstrapTable('updateByUniqueId', item.Id, item);
                                });
                            }
                        }
                    });
                }
            }
        });
    }

    function getConstructionWiseYarnCountLists() {
        axios.get("/api/selectoption/constructionWiseYarnCountLists/" + yarnType + "/" + constructionId + "/" + fabricGsm)
            .then(function (response) {
                yarnCountList = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initBookingAnalysisChildYarn($tableEl, bookingChildId, data) {
        $tableEl.bootstrapTable({
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
                            '<button class="btn btn-success btn-xs edit" onclick="return addNewChildRow(event, ' + bookingChildId + ')" title="Add">',
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
                //{
                //    field: "Id",
                //    title: "Yarn ID",
                //    width: 100,
                //},
                {
                    field: "YarnTypeId",
                    title: "Yarn Type",
                    align: "left",
                    editable: {
                        source: yarnTypeLists,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Yarn Type' }
                    }
                },
                {
                    field: "YarnComposition",
                    title: "Yarn Composition",
                    width: 100,
                },
                {
                    field: "YarnColorId",
                    title: "Yarn Color",
                    width: 80,
                    editable: {
                        type: 'select2',
                        title: 'Select a Color',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: yarnColorList,
                        select2: { width: 100, placeholder: 'Yarn Color' }
                    }
                },
                //{
                //    field: "YarnSubProgramIds",
                //    title: "Yarn Sub Program",
                //    editable: {
                //        type: 'select2',
                //        showbuttons: true,
                //        source: yarnSubProgramList,
                //        select2: { width: 200, height: 300, multiple: true, placeholder: 'Select Sub Program' }
                //    }
                //},
                //{
                //    field: "YarnCategory",
                //    title: "Yarn Category",
                //    align: "left",
                //    filterControl: "input"
                //},
                //{
                //    field: "YarnSpecification",
                //    title: "Yarn Specification",
                //    width: 20,
                //    editable: {
                //        type: 'text',
                //        inputclass: 'input-sm',
                //        showbuttons: false
                //    }
                //},
                //{
                //    field: "YarnShadeCode",
                //    title: "Shade Code",
                //    width: 20,
                //    editable: {
                //        type: 'text',
                //        inputclass: 'input-sm',
                //        showbuttons: false
                //    }
                //},
                //{
                //    field: "IsYD",
                //    title: "YD",
                //    placeholder: 'YD',
                //    checkbox: true
                //},
                {
                    field: "CalcYarnCount",
                    title: "Suggested YC",
                    width: 20,
                    visible: !isCustomCheckedYarnCount
                },
                {
                    field: "FinalYarnCountOthers",
                    title: "Yarn Count",
                    width: 20,
                    editable: {
                        source: yarnCountList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Yarn Count' }
                    },
                    visible: isCustomCheckedYarnCount
                },
                {
                    field: "FinalYarnCount",
                    title: "Production YC",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    },
                    visible: isCustomCheckedYarnCount2
                },
                {
                    field: "ElastaneYC",
                    title: "Elastane YC",
                    width: 20,
                    editable: {
                        source: elastaneYarnCountList,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Yarn Count' }
                    },
                    visible: isCustomCheckedElastaneYarnCount
                },
                {
                    field: "MachineGauge",
                    title: "Suggested MG"
                },
                {
                    field: "FinalMachineGauge",
                    title: "Production MG",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "MachineDia",
                    title: "Suggested MD"
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
                    field: "StitchLength",
                    title: "Suggested SL"
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
                    field: "YarnStatusId",
                    title: "Yarn Status",
                    editable: {
                        source: yarnStatusLists,
                        type: 'select2',
                        showbuttons: false,
                        select2: { width: 100, placeholder: 'Yarn Status' }
                    }
                },
                {
                    field: "YarnLotNo",
                    title: "Yarn Lot No",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "SDR",
                    title: "SDR No",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "KnittingDate",
                    title: "Knitting Date",
                    width: 200,
                    editable: {
                        type: 'combodate',
                        format: 'YYYY-MM-DD',
                        template: 'DD/MM/YYYY',
                        inputclass: 'input-sm'
                    }
                },
                {
                    field: "YarnSupplierId",
                    title: "Yarn Supplier",
                    editable: {
                        source: yarnSupplierLists,
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
                    field: "BookingQtyKg",
                    title: "Booking Qty (KG)",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "AllowancePer",
                    title: "Allowance (%)",
                    width: 20
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
                    field: "RequiredYarnQty",
                    title: "Required Qty"
                },
                {
                    field: "DisplayUnitDesc",
                    title: "UOM"
                },
                {
                    field: "BookingQtyPCs",
                    title: "Booking Qty (PCs)",
                    width: 20,
                    align: "right",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "ReqKnittingQty",
                    title: "Req. Knitting Qty (Kg)",
                    width: 20,
                    align: "right",
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "FlatKnitQtyPCs",
                    title: "Flat Knit Qty (PCs)",
                    width: 20,
                    align: "right",
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
                }
            ],
            onEditableSave: function (field, row, oldValue, $el) {
                if (row.YarnComposition.match(/Elastane/g)) {
                    isCustomCheckedElastaneYarnCount = true;
                    isCustomCheckedYarnCount2 = true;
                    if (row.ElastaneYC) {
                        row.FinalYarnCount = elastaneYarnCountList.find(function (el) { return el.id == row.ElastaneYC }).text;
                    }
                }
                if (constructionId == 14 || constructionId == 15 || constructionId == 16 || constructionId == 18 || constructionId == 19 || constructionId == 21) {
                    if (row.FinalYarnCount2 < (row.CalcYarnCount - 6)) {
                        row.FinalYarnCount2 = 0;
                        $el.closest('table').bootstrapTable('updateByUniqueId', row.Id, row);
                        return bootbox.alert({
                            message: '<span class="text-danger">Production Yarn Count Must be +-6 of Suggested Yarn Count !!!<span>',
                            size: 'small'
                        });
                    }
                    else if ((row.FinalYarnCount2 - 6) > row.CalcYarnCount) {
                        row.FinalYarnCount2 = 0;
                        $el.closest('table').bootstrapTable('updateByUniqueId', row.Id, row);
                        return bootbox.alert({
                            message: '<span class="text-danger">Production Yarn Count Must be +-6 of Suggested Yarn Count !!!<span>',
                            size: 'small'
                        });
                    }
                    else {
                        row.NetConsumption = ((row.BookingQty * row.YarnDistribution) / 100).toFixed(4);
                        if (row.FinalAllowancePer == "0") {
                            row.RequiredYarnQty = row.BookingQtyKg;
                        }
                        else {
                            row.RequiredYarnQty = (((row.BookingQtyKg * row.YarnDistribution) / 100) + ((row.FinalAllowancePer * row.BookingQtyKg) / 100)).toFixed(4);
                        }
                    }
                    $tableEl.bootstrapTable('load', $el);
                }
                else {
                    row.NetConsumption = ((row.BookingQty * row.YarnDistribution) / 100).toFixed(4);
                    if (row.FinalAllowancePer == "0") {
                        row.RequiredYarnQty = row.NetConsumption;
                    }
                    else {
                        row.RequiredYarnQty = (((row.BookingQtyKg * row.YarnDistribution) / 100) + ((row.FinalAllowancePer * row.BookingQtyKg) / 100)).toFixed(4);
                    }
                    $tableEl.bootstrapTable('load', $el);
                }
                $tableEl.bootstrapTable('load', $el);
            }
        });
    }
    function populateBookingAnalysisChildYarn(analysisChildId, technicalNameId, fabricComposition, $detail) {
        getElastaneYarnCountLists(constructionId, fabricGsm);
        if (constructionId == 14 || constructionId == 15 || constructionId == 16 || constructionId == 18 || constructionId == 19 || constructionId == 21) {
            isCustomCheckedYarnCount = false;
            isCustomCheckedYarnCount2 = true;
        }
        else {
            isCustomCheckedYarnCount = true;
            isCustomCheckedYarnCount2 = false;
            getConstructionWiseYarnCountLists(yarnType, constructionId, fabricGsm);
        }

        if (!technicalNameId) {
            return bootbox.alert({
                message: '<span class="text-danger">Your must select a technical name before expand.<span>',
                size: 'small'
            });
        }
        if (!shadeId) {
            return bootbox.alert({
                message: '<span class="text-danger">Your must select a shade before expand.<span>',
                size: 'small'
            });
        }
        var $el = $detail.html('<table id="TblSampleBookingChildYarn-' + bookingChildId + '"></table>').find('table');

        var sampleBookingAnalysisChild = {};
        if (analysisChildId > 0) {
            sampleBookingAnalysisChild = sampleBookingAnalysisChildList.find(function (el) {
                return el.Id == analysisChildId;
                getConstructionWiseYarnCountLists(yarnType, constructionId, fabricGsm);
            });
        }
        else {
            sampleBookingAnalysisChild = sampleBookingAnalysisChildList.find(function (el) {
                return el.BookingChildId == bookingChildId;
            });
        }

        if (sampleBookingAnalysisChild.ChildYarns.length > 0) {
            if (constructionId == 14 || constructionId == 15 || constructionId == 16 || constructionId == 18 || constructionId == 19 || constructionId == 21) {
                isCustomCheckedYarnCount = false;
                isCustomCheckedYarnCount2 = true;
                initBookingAnalysisChildYarn($el, bookingChildId, sampleBookingAnalysisChild.ChildYarns);
            }
            else {
                isCustomCheckedYarnCount = true;
                isCustomCheckedYarnCount2 = false;
                initBookingAnalysisChildYarn($el, bookingChildId, sampleBookingAnalysisChild.ChildYarns);
                getConstructionWiseYarnCountLists(yarnType, constructionId, fabricGsm);
            }
        }
        else {
            var url = "";
            if (analysisChildId > 0) {
                if (constructionId == 14 || constructionId == 15 || constructionId == 16 || constructionId == 18 || constructionId == 19 || constructionId == 21) {
                    isCustomCheckedYarnCount = false;
                    isCustomCheckedYarnCount2 = true;
                    url = "/api/bookinganalysischildyarn/" + analysisChildId + "/" + bookingChildId;
                }
                else {
                    isCustomCheckedYarnCount = true;
                    isCustomCheckedYarnCount2 = false;
                    getConstructionWiseYarnCountLists(yarnType, constructionId, fabricGsm);
                    url = "/api/bookinganalysischildyarn/" + analysisChildId + "/" + bookingChildId;
                }
            }
            else {
                if (constructionId == 14 || constructionId == 15 || constructionId == 16 || constructionId == 18 || constructionId == 19 || constructionId == 21) {
                    isCustomCheckedYarnCount = false;
                    isCustomCheckedYarnCount2 = true;
                    url = "/SBApi/newbookinganalysischildyarn?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&fabricGsm=" + fabricGsm + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + fabricComposition + "&fabTechnicalName=" + technicalNameId + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype;
                }
                else {
                    isCustomCheckedYarnCount = true;
                    isCustomCheckedYarnCount2 = false;
                    getConstructionWiseYarnCountLists(yarnType, constructionId, fabricGsm);
                    url = "/SBApi/newbookinganalysischildyarnOthers?bookingChildId=" + bookingChildId + "&constructionId=" + constructionId + "&fabricGsm=" + fabricGsm + "&fabricWidth=" + fabricWidth + "&shadeId=" + shadeId + "&yarnCompositionName=" + fabricComposition + "&fabTechnicalName=" + technicalNameId + "&dyeingtype=" + dyeingtype + "&buyerId=" + buyerId + "&knittingtype=" + knittingtype;
                }
            }

            axios.get(url)
                .then(function (response) {
                    $.each(response.data, function (i, v) {
                        sampleBookingAnalysisChild.ChildYarns.push(v);
                    });
                    initBookingAnalysisChildYarn($el, bookingChildId, response.data);
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });
        }
    }

    function getSampleBookingMasterData() {
        var queryParams = $.param(tableParams);
        $('#tblSampleBookingMaster').bootstrapTable('showLoading');
        var url = "/SBApi/sampleBookinglists" + "?status=" + status + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                $("#tblSampleBookingMaster").bootstrapTable('load', response.data);
                $('#tblSampleBookingMaster').bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getBookingAnalysisMaster(id, bookingId) {
        var url = "";
        if (id)
            url = "/api/bookinganalysis/" + id + "/" + bookingId;
        else
            url = "/api/newbookinganalysis/" + bookingId;
        axios.get(url)
            .then(function (response) {
                sampleBookingAnalysisChildList = [];
                bookingAnalysisMaster = response.data;
                formSampleBookingMaster.find("#BAnalysisNo").val(response.data.BAnalysisNo);
                formSampleBookingMaster.find("#BAnalysisNoOrigin").val(response.data.BAnalysisNoOrigin);
                formSampleBookingMaster.find("#Id").val(response.data.Id);
                formSampleBookingMaster.find("#PreBAnalysisId").val(response.data.Id);
                formSampleBookingMaster.find("#BuyerId").val(response.data.BuyerId);
                formSampleBookingMaster.find("#BuyerTeamId").val(response.data.BuyerTeamId);
                formSampleBookingMaster.find("#CompanyId").val(response.data.CompanyId);
                formSampleBookingMaster.find("#ExportOrderId").val(response.data.ExportOrderId);
                formSampleBookingMaster.find("#BookingId").val(response.data.BookingId);
                formSampleBookingMaster.find("#SubGroupId").val(response.data.SubGroupId);
                formSampleBookingMaster.find("#AdditionalBooking").val(response.data.AdditionalBooking);
                formSampleBookingMaster.find("#PreProcessRevNo").val(response.data.PreProcessRevNo);
                formSampleBookingMaster.find("#RevisionNo").val(response.data.RevisionNo);
                formSampleBookingMaster.find("#YInHouseDate").val(response.data.YInHouseDate);
                formSampleBookingMaster.find("#YRequiredDate").val(response.data.YRequiredDate);
                formSampleBookingMaster.find("#Remarks").val(response.data.Remarks);

                formSampleBookingMaster.find("#lblBookingNo").val(response.data.BookingNo);
                formSampleBookingMaster.find("#lblExportOrderNo").val(response.data.ExportOrderNo);
                formSampleBookingMaster.find("#lblBuyerName").val(response.data.BuyerName);
                formSampleBookingMaster.find("#lblBuyerTeam").val(response.data.BuyerTeamName);
                formSampleBookingMaster.find("#lblMerchandiserName").val(response.data.MerchandiserName);
                if (response.data.AcknowledgeDate)
                    formSampleBookingMaster.find("#lblAcknowledgeDate1").val(moment(response.data.AcknowledgeDate).format('MM/DD/YYYY'));
                if (response.data.BookingDate)
                    formSampleBookingMaster.find("#lblBookingDate").val(moment(response.data.BookingDate).format('MM/DD/YYYY'));
                formSampleBookingMaster.find("#lblStyleNo").val(response.data.StyleNo);
                formSampleBookingMaster.find("#lblConstructionLists").text(response.data.Constructions);
                formSampleBookingMaster.find("#lblCompositionLists").text(response.data.Compositions);
                formSampleBookingMaster.find("#lblStyleType").text(response.data.StyleTypes);
                formSampleBookingMaster.find("#divEditSampleBookingMaster").fadeIn();
                formSampleBookingMaster.find("#tblTextileProcessMaster").fadeIn();
                $("#divtblSampleBookingMaster").fadeOut();
                if (status == "2") {
                    formSampleBookingMaster.find("#btnEditCancelSampleBooking").fadeIn();
                    formSampleBookingMaster.find("#btnSaveSampleBooking").fadeIn();
                    formSampleBookingMaster.find("#btnApprovedBookingAnalysis").fadeOut();
                    formSampleBookingMaster.find("#btnRejectBookingAnalysis").fadeOut();
                    formSampleBookingMaster.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                }
                if (status == "4") {
                    formSampleBookingMaster.find("#btnEditCancelSampleBooking").fadeIn();
                    formSampleBookingMaster.find("#btnSaveSampleBooking").fadeIn();
                    formSampleBookingMaster.find("#btnApprovedBookingAnalysis").fadeIn();
                    formSampleBookingMaster.find("#btnRejectBookingAnalysis").fadeIn();
                    formSampleBookingMaster.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                }
                if (status == "5") {
                    formSampleBookingMaster.find("#btnEditCancelSampleBooking").fadeIn();
                    formSampleBookingMaster.find("#btnSaveSampleBooking").fadeOut();
                    formSampleBookingMaster.find("#btnApprovedBookingAnalysis").fadeOut();
                    formSampleBookingMaster.find("#btnRejectBookingAnalysis").fadeOut();
                    formSampleBookingMaster.find("#btnAcknowledgeBookingAnalysis").fadeIn();
                }
                if (status == "6") {
                    formSampleBookingMaster.find("#btnEditCancelSampleBooking").fadeIn();
                    formSampleBookingMaster.find("#btnSaveSampleBooking").fadeOut();
                    formSampleBookingMaster.find("#btnApprovedBookingAnalysis").fadeOut();
                    formSampleBookingMaster.find("#btnRejectBookingAnalysis").fadeOut();
                    formSampleBookingMaster.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                }
                if (status == "7") {
                    formSampleBookingMaster.find("#btnEditCancelSampleBooking").fadeIn();
                    formSampleBookingMaster.find("#btnSaveSampleBooking").fadeIn();
                    formSampleBookingMaster.find("#btnApprovedBookingAnalysis").fadeOut();
                    formSampleBookingMaster.find("#btnRejectBookingAnalysis").fadeOut();
                    formSampleBookingMaster.find("#btnAcknowledgeBookingAnalysis").fadeOut();
                }
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function getBookingAnalysisMasterSampleBooking(id, bookingId) {
        var url = "";
        if (id > 0)
            url = "/SBApi/bookinganalysisSample/" + id + "/" + bookingId;
        else
            url = "/SBApi/newbookinganalysisSample/" + bookingId;
        axios.get(url)
            .then(function (response) {
                sampleBookingAnalysisChildList = [];
                getFabricTechnicalNames();
                bookingAnalysisMaster = response.data;
                formSampleBookingMaster.find("#BAnalysisNo").val(response.data.BAnalysisNo);
                formSampleBookingMaster.find("#BAnalysisNoOrigin").val(response.data.BAnalysisNoOrigin);
                formSampleBookingMaster.find("#Id").val(response.data.Id);
                formSampleBookingMaster.find("#PreBAnalysisId").val(response.data.Id);
                formSampleBookingMaster.find("#BuyerId").val(response.data.BuyerId);
                formSampleBookingMaster.find("#BuyerTeamId").val(response.data.BuyerTeamId);
                formSampleBookingMaster.find("#CompanyId").val(response.data.CompanyId);
                formSampleBookingMaster.find("#ExportOrderId").val(response.data.ExportOrderId);
                formSampleBookingMaster.find("#BookingId").val(response.data.BookingId);
                formSampleBookingMaster.find("#SubGroupId").val(response.data.SubGroupId);
                formSampleBookingMaster.find("#AdditionalBooking").val(response.data.AdditionalBooking);
                formSampleBookingMaster.find("#PreProcessRevNo").val(response.data.PreProcessRevNo);
                formSampleBookingMaster.find("#RevisionNo").val(response.data.RevisionNo);
                formSampleBookingMaster.find("#YInHouseDate").val(response.data.YInHouseDate);
                formSampleBookingMaster.find("#YRequiredDate").val(response.data.YRequiredDate);
                formSampleBookingMaster.find("#Remarks").val(response.data.Remarks);

                formSampleBookingMaster.find("#lblBookingNo").val(response.data.BookingNo);
                formSampleBookingMaster.find("#lblExportOrderNo").val(response.data.ExportOrderNo);
                formSampleBookingMaster.find("#lblBuyerName").val(response.data.BuyerName);
                formSampleBookingMaster.find("#lblBuyerTeam").val(response.data.BuyerTeamName);
                formSampleBookingMaster.find("#lblMerchandiserName").val(response.data.MerchandiserName);
                if (response.data.BookingDate)
                    formSampleBookingMaster.find("#lblBookingDate").val(moment(response.data.BookingDate).format('MM/DD/YYYY'));

                formSampleBookingMaster.find("#OrderSession").val(response.data.SessionName);
                formSampleBookingMaster.find("#MerchandisingTeam").val(response.data.MerchandisingTeam);
                formSampleBookingMaster.find("#SeasonId").val(response.data.SeasonId);
                formSampleBookingMaster.find("#MerchandiserTeamId").val(response.data.MerchandiserTeamId);
                formSampleBookingMaster.find("#StyleNo").val(response.data.StyleNo);

                if (id > 0) {
                    formSampleBookingMaster.find("#SampleReference").val(response.data.SampleReference);
                }

                formSampleBookingMaster.find("#EWPNo").val(response.data.EwpNo);

                initSelect2(formSampleBookingMaster.find("#ReviseInfoId"), response.data.ReviseInformationList);
                formSampleBookingMaster.find("#ReviseInfoId").val(response.data.ReviseInfoId).trigger("change");

                initSelect2(formSampleBookingMaster.find("#SPTypeId"), response.data.SPTypeList);
                formSampleBookingMaster.find("#SPTypeId").val(response.data.SpTypeId).trigger("change");

                formSampleBookingMaster.find("#divEditSampleBookingMaster").fadeIn();
                $("#divtblSampleBookingMaster").fadeOut();
                formSampleBookingMaster.find("#divButtonExecutionsSampleBooking").fadeIn();
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function getSampleBookingChildDataSample(id, bookingId) {
        var url = "";
        var id = formSampleBookingMaster.find("#Id").val();
        if (id > 0) {
            url = "/SBApi/bookinganalysischildSample/" + id;
        }
        else {
            url = "/SBApi/newbookinganalysischildSample/" + bookingId;
        }

        axios.get(url)
            .then(function (response) {
                sampleBookingAnalysisChildList = response.data;
                formSampleBookingMaster.find("#TblSampleBookingChild").bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function getFabricCompositionLists() {
        var queryParams = $.param(yarnCompositionTableParams);
        var url = "/api/YarnCompositionlists" + "?" + queryParams;
        axios.get(url)
            .then(function (response) {
                $("#tblSampleBookingYarnComposition").bootstrapTable('load', response.data);
            })
            .catch(function () {
                toastr.error(constants.LOAD_ERROR_MESSAGE);
            })
    }

    function SampleBookingResetForm() {
        formSampleBookingMaster.trigger("reset");
        formSampleBookingMaster.find("#Id").val(-1111);
        formSampleBookingMaster.find("#EntityState").val(4);
    }

    function addNewChildRow(e, bookingChildId) {
        childEl = $("#TblSampleBookingChildYarn-" + bookingChildId);
        getFabricCompositionLists();
        $("#SampleBookingModal-Child").modal('show');
    }

    function getFabricShades() {
        axios.get("/api/selectoption/shadename")
            .then(function (response) {
                $.each(response.data, function (i, v) {
                    var item = { value: v.id, text: v.text };
                    shadeList.push(item);
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getYarnColors() {
        axios.get("/api/selectoption/yarncolor")
            .then(function (response) {
                yarnColorList = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getYarnYDStatus() {
        axios.get("/api/selectoption/YarnYDStatusLists")
            .then(function (response) {
                yarnYDStatusList = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getYarnCountAll() {
        axios.get("/api/selectoption/yarncountlists")
            .then(function (response) {
                yarnCountAllList = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getFabricTechnicalNames() {
        axios.get("/api/selectoption/fabrictechnicalname")
            .then(function (response) {
                fabricTechnicalNameList = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getYarnTypeLists() {
        axios.get("/api/selectoption/yarntypePO")
            .then(function (response) {
                yarnTypeLists = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
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

    function getYarnSubProgramLists() {
        axios.get("/api/selectoption/YarnSubProgram")
            .then(function (response) {
                $.each(response.data, function (i, v) {
                    var item = { value: v.id, text: v.text };
                    yarnSubProgramList.push(item);
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getYarnSupplierNames() {
        axios.get("/api/selectoption/yarn-suppliers")
            .then(function (response) {
                yarnSupplierLists = response.data;
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function getYarnStatus() {
        axios.get("/api/selectoption/SampleBookingYarnStatusLists")
            .then(function (response) {
                yarnStatusLists = response.data;
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

    function backToListSampleBooking() {
        $("#divtblSampleBookingMaster").fadeIn();
        $("#divEditSampleBookingMaster").fadeOut();
        $("#divEditSampleBookingMaster1").fadeOut();
        formSampleBookingMaster.find("#btnEditCancelSampleBooking").fadeIn();
        formSampleBookingMaster.find("#btnSaveSampleBooking").fadeIn();

        getSampleBookingMasterData();
    }

    function getSampleBookingMasterTableCellStyle(value) {
        if (value == "0")
            return { classes: "c-bg-danger" };
        else if (value == "2")
            return { classes: "c-bg-warning" };
        else
            return ""
    }
})();