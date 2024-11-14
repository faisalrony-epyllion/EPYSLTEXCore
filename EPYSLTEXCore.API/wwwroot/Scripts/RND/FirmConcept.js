(function () {
    var menuId, pageName;
    var toolbarId, pageId;
    var $divTblEl, $pageEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblYarnEl, $tblProductLimitation, $tblDyeingEl, $tblPreFinishingEl,
        $tblPostFinishingEl, $tblTestEl, $tblPostFinishingEl, $tblMachineEl, $formEl, $tblChildFabricId, $tblDefChildEl, $tblPreProcessTable, $tblPostProcessTable;
    var tblCreateCompositionId, $tblCreateCompositionEl, compositionComponents = [];
    var filterBy = {};
    var status = statusConstants.PENDING;
    var _rowConcept = null;
    var ImagesContentList = [];
    var ImagesList = [];
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var reportData = [];
    var masterData;
    var isEditable = true;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $tblYarnEl = $("#tblYarnId" + pageId);
        $tblProductLimitation = $("#tblProductLimitation" + pageId);
        $tblDyeingEl = $("#tblDyeingId" + pageId);
        $tblPreFinishingEl = $("#tblFinishingId" + pageId);
        $tblPostFinishingEl = $("#tblPostFinishingId" + pageId);
        $tblTestEl = $("#tblTestId" + pageId);
        $tblCapacityDeclarationEl = $("#tblCapacityDeclarationId" + pageId);
        $tblMachineEl = $("#tblMachineId" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $tblChildFabricId = $("#tblChildFabricId" + pageId);
        tblCreateCompositionId = `#tblCreateComposition-${pageId}`;
        tblReportId = "#tblReport" + pageId;
        initMasterTable();
        getMasterTableData();

        //$divTblEl.find(".select2-search__field").on('click', function (e) {
        $(document).on('click', '.select2-search__field', function (e) {
            return false;
            //e.preventDefault();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });
        $formEl.find("#btnPreFinishingProcessReport").click(function (e) {
            e.preventDefault();
            viewReport("PreFinishingProcess");
        });
        $formEl.find("#btnFinishingProcessReport").click(function (e) {
            e.preventDefault();
            viewReport("FinishingProcess");
        });
        $formEl.find("#btnTestingReport").click(function (e) {
            e.preventDefault();
            viewReport("Testing");
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btn-add-composition").on("click", showAddComposition);
        $pageEl.find("#btnAddComposition").click(saveComposition);

        $formEl.find("#btnAddChild").on("click", function (e) {
            e.preventDefault();

            var finder = new commonFinder({
                title: "Select Item",
                pageId: pageId,
                fields: "ConceptNo,Composition,Construction,SubGroup,TechnicalName",
                headerTexts: "Concept No,Composition,Construction,SubGroup,Technical Name",
                widths: "20,20,20,20,20",
                allowEditing: false,
                apiEndPoint: `/api/recipe-request/get-concept-item/${masterData.ConceptNo}/${masterData.ColorID}/${masterData.IsBDS}`,
                isMultiselect: true,
                //selectedIds: item[0].ItemIDs,
                allowPaging: true,
                primaryKeyColumn: "ConceptID",
                onMultiselect: function (selectedRecords) {
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var exists = masterData.FirmConceptItems.find(function (el) { return el.ConceptID == selectedRecords[i].ConceptID });
                        if (!exists) {
                            masterData.FirmConceptItems.push(selectedRecords[i]);
                        }
                    }
                    $tblChildFabricId.bootstrapTable('load', masterData.FirmConceptItems);

                   
                },
                onFinish: function () {
                    finder.hideModal();
                }
            });
            finder.showModal();
        });

        $formEl.find("#CamArrangement").on('change', function () {
            var camFile = $formEl.find("#CamArrangement")[0].files;

            getBase64(camFile[0]).then(data => {
                var objList = new Object;
                objList.FCItemID = _rowConcept.FCItemID;
                objList.FCImageTypeID = firmConceptImageTypes.CAM;
                objList.ImagePath = "";// "/Uploads/RND/" + file.name;
                objList.Image64Byte = data;
                objList.ImageFile = camFile[0];
                objList.ImageName = camFile[0].name;
                objList.PreviewTemplate = "image";
                objList.EntityState = 4;
                var selectedFile = ImagesContentList.find(function (el) { return el.FCItemID == _rowConcept.FCItemID && el.FCImageTypeID == firmConceptImageTypes.CAM });
                if (!selectedFile)
                    ImagesContentList.push(objList);
                else {
                    var ind = ImagesContentList.findIndex(function (el) { return el.FCItemID == _rowConcept.FCItemID && el.FCImageTypeID == firmConceptImageTypes.CAM });
                    ImagesContentList[ind] = objList;
                }
            });

            //ImagesList.push(camFile[0]);
        });

        var thisPage = $("#" + pageId);
        thisPage.find("#btnLabTestReport").click(function () {
            //_rowConcept ->Get Selected Row Data
            debugger;
            axios.get(`/api/rnd-firm-concept/LabTest/${masterData.ColorName}/${masterData.ConceptNo}`)
                .then(function (response) {
                reportData = response.data;
                thisPage.find('#divModalReport').modal('show');
                showReportList();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
        });
        //$formEl.find("#NeedleArrangement").on('change', function () {
        //    var needleFile = $formEl.find("#NeedleArrangement")[0].files;

        //    getBase64(needleFile[0]).then(data => {
        //        var objList = new Object;
        //        objList.FCItemID = _rowConcept.FCItemID;
        //        objList.FCImageTypeID = firmConceptImageTypes.NEEDLE;
        //        objList.ImagePath = "";// "/Uploads/RND/" + file.name;
        //        objList.Image64Byte = data;
        //        objList.ImageFile = needleFile[0];
        //        objList.ImageName = needleFile[0].name;
        //        objList.PreviewTemplate = "image";
        //        objList.EntityState = 4;
        //        var selectedFile = ImagesContentList.find(function (el) { return el.FCItemID == _rowConcept.FCItemID && el.FCImageTypeID == firmConceptImageTypes.NEEDLE });
        //        if (!selectedFile)
        //            ImagesContentList.push(objList);
        //        else {
        //            var ind = ImagesContentList.findIndex(function (el) { return el.FCItemID == _rowConcept.FCItemID && el.FCImageTypeID == firmConceptImageTypes.NEEDLE });
        //            ImagesContentList[ind] = objList;
        //        }
        //    });

        //    //ImagesList.push(needleFile[0]);
        //});

        $formEl.find("#btnAddProductLimitation").on("click", function (e) {
            e.preventDefault();

            var finder = new commonFinder({
                title: "Select Limitations",
                pageId: pageId,
                data: masterData.AllItemLimitations,
                fields: "PLimitationName",
                headerTexts: "Name",
                widths: "100",
                allowEditing: false,
                isMultiselect: true,
                allowPaging: true,
                primaryKeyColumn: "PLimitationID",
                onMultiselect: function (selectedRecords) {
                    var indexF = masterData.FirmConceptItems.findIndex(x => x.FCItemID == _rowConcept.FCItemID);
                    if (indexF > -1) {
                        masterData.FirmConceptItems[indexF].FirmConceptItemLimitations = [];
                        var fCItemLimitationID = 1;
                        selectedRecords.map(x => {
                            x.FCItemLimitationID = fCItemLimitationID++;
                            x.FCID = _rowConcept.FCID;
                            x.FCItemID = masterData.FirmConceptItems[indexF].FCItemID;
                            x.LimitationValue = ''
                            //x.PLimitationID = x.PLimitationID;
                            masterData.FirmConceptItems[indexF].FirmConceptItemLimitations.push(x);
                        });
                    }
                    $tblProductLimitation.bootstrapTable('load', masterData.FirmConceptItems[indexF].FirmConceptItemLimitations);
                },
                onFinish: function () {
                    finder.hideModal();
                }
            });
            finder.showModal();
        });
    });

    function getBase64(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsDataURL(file);
            reader.onload = () => resolve(reader.result);
            reader.onerror = error => reject(error);
        });
    }

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
            showFooter: true,
            columns: [
                {
                    title: "Action",
                    align: "center",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    formatter: function (value, row, index, field) {
                        if (status == statusConstants.PENDING) {
                            return [
                                '<a class="btn btn-xs btn-default new" href="javascript:void(0)" title="New">',
                                '<i class="fa fa-plus" aria-hidden="true"></i>',
                                '</a>'
                            ].join(' ');
                        } else {
                            return [
                                '<a class="btn btn-xs btn-default view" href="javascript:void(0)" title="View">',
                                '<i class="fa fa-eye" aria-hidden="true"></i>',
                                '</a>',
                                `<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportName=FirmConceptMaster.rdl&ConceptNo=${row.ConceptNo}" target="_blank" title="Firm Concept Report">
                                <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                </a>`
                            ].join(' ');
                        }
                    },
                    events: {
                        'click .new': function (e, value, row, index) {
                            e.preventDefault();
                            getNew(row.ConceptID, row.CCColorID, row.ConceptNo);
                        },
                        'click .view': function (e, value, row, index) {
                            e.preventDefault();
                            if (row) {
                                HoldOn.open({
                                    theme: "sk-circle"
                                });
                                getDetails(row.FCID, row.ConceptID, row.CCColorID, row.ConceptNo);
                            }
                        }
                    }
                },
                {
                    field: "ConceptNo",
                    title: "Concept No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ConceptDate",
                    title: "Concept Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "ColorName",
                    title: "Color",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                //{
                //    field: "BatchNo",
                //    title: "Batch No",
                //    filterControl: "input",
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                //{
                //    field: "BatchWeightKG",
                //    title: "Batch Weight(KG)",
                //    filterControl: "input",
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                //{
                //    field: "FabricQty",
                //    title: "Fabric Qty",
                //    filterControl: "input",
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                //{
                //    field: "Uom",
                //    title: "Uom",
                //    filterControl: "input",
                //    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                //},
                {
                    field: "FirmConceptDate",
                    title: "Firm Concept Date",
                    filterControl: "input",
                    visible: status !== statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "CommercialName",
                    title: "Commercial Name",
                    filterControl: "input",
                    visible: status !== statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ProductDetails",
                    title: "Product Details",
                    filterControl: "input",
                    visible: status !== statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ConceptDeclarationName",
                    title: "Concept Declaration",
                    filterControl: "input",
                    visible: status !== statusConstants.PENDING,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },

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
                resetTableParams();
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

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');
        var url = "/api/rnd-firm-concept/list?gridType=bootstrap-table&status=" + status + "&" + queryParams;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initChildFabricTable() {
        //Product Information
        //FirmConceptItem
        $tblChildFabricId.bootstrapTable("destroy");
        $tblChildFabricId.bootstrapTable({
            uniqueId: 'ConceptID',
            checkboxHeader: false,
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    cellStyle: function () { return { classes: 'm-w-10' } },
                    formatter: function (value, row, index, field) {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            if (masterData.FirmConceptItems.length == 1) {
                                toastr.warning("Atleast one item required. You can't delete this!");
                                return;
                            }
                            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                                if (yes) {
                                    masterData.FirmConceptItems.splice(index, 1);
                                    $tblChildFabricId.bootstrapTable('load', masterData.FirmConceptItems);
                                }
                            });
                        }
                    }
                },
                {
                    field: "FCItemID",
                    title: "FCItemID",
                    align: 'center',
                    visible: false
                },
                {
                    field: "RecipeID",
                    title: "RecipeID",
                    align: 'center',
                    visible: false
                },
                {
                    field: "FCID",
                    title: "FCID",
                    align: 'center',
                    visible: false
                },
                {
                    field: "SubGroup",
                    title: "End Use",
                    align: 'center'
                },
                {
                    field: "KnittingType",
                    title: "Machine Type",
                    align: 'left'
                },
                {
                    field: "TechnicalName",
                    title: "Technical Name",
                    align: 'left'
                },
                {
                    field: "Composition",
                    title: "Composition",
                    align: 'left'
                },
                {
                    field: "Length",
                    title: "Length(CM)",
                    align: 'left'
                },
                {
                    field: "Width",
                    title: "Width(CM)",
                    align: 'left'
                },
                {
                    field: "GSM",
                    title: "Fabric GSM",
                    align: 'center'
                },
                //{
                //    field: "RPM",
                //    title: "RPM",
                //    cellStyle: function () { return { classes: 'm-w-100' } },
                //    editable: {
                //        type: "text",
                //        showbuttons: false,
                //        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                //    }
                //},
                {
                    field: "SMV",
                    title: "SMV",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                },
                //{
                //    field: "FinalCompositionID",
                //    title: "Final Composition",
                //    cellStyle: function () { return { classes: 'm-w-100' } },
                //    editable: {
                //        type: 'select2',
                //        title: 'Select Composition',
                //        inputclass: 'input-sm',
                //        showbuttons: false,
                //        source: masterData.FinalCompositionList,
                //        select2: { width: 200, placeholder: 'Select Composition', allowClear: true }
                //    }
                //},
                //// {
                ////    field: "FinalCompositionID",
                ////    title: "Final Composition",
                ////    cellStyle: function () { return { classes: 'm-w-100' } },
                ////    formatter: function (value, row) {
                ////        // Assuming row.FinalCompositionID holds the current value for the cell
                ////        return `<select class="select2s">` + CompOptions +`</select>
                ////        <script>
                ////          $(document).ready(function() {
                ////            $('.select2s').select2({
                ////              width: 200,
                ////              placeholder: 'Select Composition',
                ////              allowClear: true,
                ////              dropdownCssClass: 'custom-select2-dropdown input-sm'});
                ////          });
                ////        </script>`;
                ////    }
                ////},
                {
                    field: "FinalCompositionID",
                    title: "Final Composition",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'select2',
                        title: 'Select Composition',
                        showbuttons: false, // Text shown when no option is selected
                        source: masterData.FinalCompositionList, // Source for options
                        select2: {
                            width: 200,
                            placeholder: 'Select Composition',
                            allowClear: true,
                            dropdownCssClass: 'custom-select2-dropdown input-sm'
                        }
                    }
                },

                {
                    field: "FinalWidth",
                    title: "Final Width",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    //        '<script type = "text/javascript"> function SetCapacityDeclaration(data) {'+ 
                    //        'for(var i = 0; i<_rowConcept.FabricWidthAndCapacityDeclaration.length; i++) {' +
                    //        'if (i == 0) {' +
                    //        '_rowConcept.FabricWidthAndCapacityDeclaration[i].DeclaredCapacityPerDay = row.DeclaredCapacityPerDay;' +
                    //        '}' +
                    //        'else {' +
                    //        '_rowConcept.FabricWidthAndCapacityDeclaration[i].DeclaredCapacityPerDay = Math.round((_rowConcept.FabricWidthAndCapacityDeclaration[i - 1].DeclaredCapacityPerDay * _rowConcept.FabricWidthAndCapacityDeclaration[i].MachineDia) / _rowConcept.FabricWidthAndCapacityDeclaration[i - 1].MachineDia);' +
                    //        '}' +
                    //        '}' +
                    //        'initCapacityDeclarationTable();' +
                    //        '$tblCapacityDeclarationEl.bootstrapTable("load", _rowConcept.FabricWidthAndCapacityDeclaration);' +
                    //        '$tblCapacityDeclarationEl.bootstrapTable("hideLoading");'+
                    //'}</script> ',
                        
                    },
                    
                },
                {
                    field: "FinalGSM",
                    title: "Final GSM",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                    }
                },
                {
                    field: "HangerRemarks",
                    title: "Hanger Remarks",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                    }
                },
                {
                    field: "ProductDetails",
                    title: "Product Details",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                    }
                },
                {
                    field: "CommercialName",
                    title: "Commercial Name",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                    }
                },
                {
                    field: "ProposedWidth",
                    title: "Proposed Width(+/-)",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                    }
                },
                {
                    field: "IsLive",
                    title: "Live?",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    checkbox: true,
                    showSelectTitle: true,
                    checkboxEnabled: false
                },
                {
                    field: "CapacityIn",
                    title: "Capacity In",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                    //editable: {
                    //    type: "text",
                    //    showbuttons: false,
                    //    tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">'
                    //}
                },
                {
                    field: "TotalMinutes",
                    title: "Total Minutes",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "ProdWOElastane",
                    title: "Prod WO Elastane",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                //{
                //    field: "CalculatedCapacity",
                //    title: "Calculated Capacity",
                //    cellStyle: function () { return { classes: 'm-w-100' } }
                //},
            ],
            onClickRow: function (row, $element) {
                 for (var i = 0; i < row.FabricWidthAndCapacityDeclaration.length; i++) {
                    if (i == 0) {
                        row.FabricWidthAndCapacityDeclaration[i].FinalWidth = row.FinalWidth;
                    }
                    else {
                        row.FabricWidthAndCapacityDeclaration[i].FinalWidth =Math.round( (row.FabricWidthAndCapacityDeclaration[i - 1].FinalWidth * row.FabricWidthAndCapacityDeclaration[i].MachineDia) / row.FabricWidthAndCapacityDeclaration[i - 1].MachineDia);
                    }
                }
                _rowConcept = row;
                _rowConcept.Elastane = GetElastanePer(_rowConcept.Composition);
                $formEl.find("#divKnittingInfo,#divYarnInfo,#divProductLimitation,#divDyeingInfo,#divPreDyeingProcessInfo,#divFinishingProcess,#divTestingInfo,#divCapacityDeclarationInfo").show();
                var fci = masterData.FirmConceptItems.find(x => x.FCItemID == row.FCItemID);
                var childs = [],
                    yarns = [],
                    productLimites = [];
                //ABC
                if (fci != null) {
                    if (!masterData.FirmConceptItems.find(x => x.FCItemID == row.FCItemID).FirmConceptChilds) {
                        masterData.FirmConceptItems.find(x => x.FCItemID == row.FCItemID).FirmConceptChilds = [];
                    }
                    if (!masterData.FirmConceptItems.find(x => x.FCItemID == row.FCItemID).FirmConceptItemYarns) {
                        masterData.FirmConceptItems.find(x => x.FCItemID == row.FCItemID).FirmConceptItemYarns = [];
                    }

                    masterData.FirmConceptItems.find(x => x.FCItemID == row.FCItemID).FirmConceptChilds.map(x => {
                        x.ActiveNeedlePer = GetActiveNeedlePercentage(_rowConcept.TechnicalName, x.IsNeedleDopped, x.CylinderIn, x.CylinderOut, x.DialIn, x.DialOut);
                        x.TotalNumberOfActiveNeedle = GetTotalNumberOfActiveNeedle(_rowConcept.TechnicalName, x.IsNeedleDopped, x.MachineDia, x.MachineGauge, x.CylinderIn, x.CylinderOut, x.DialIn, x.DialOut);
                        x.TotalNeedle = GetTotalNeedle(_rowConcept.TechnicalName, x.MachineDia, x.MachineGauge);
                        x.Elastane = _rowConcept.Elastane;
                    });
                    var totalFeederRepeat = 0;
                    masterData.FirmConceptItems.find(x => x.FCItemID == row.FCItemID).FirmConceptItemYarns.map(x => {
                        x.FinalCount = GetFinalCountNe(x.CountValue, x.YarnPly);
                        totalFeederRepeat += getNumberValue(x.FeederRepeat);

                        var yarnParentIndex = masterData.FirmConceptItems.findIndex(ci => ci.ConceptID == x.ConceptID);
                        var yarnIndex = masterData.FirmConceptItems.find(ci => ci.FCItemID == row.FCItemID).FirmConceptItemYarns.findIndex(ciy => ciy.KPYarnID == x.KPYarnID);
                        var elastanePer = GetElastanePer(masterData.FirmConceptItems[yarnParentIndex].Composition);
                        x.ConsumptionPer = GetConsumption(elastanePer, yarnIndex, masterData.FirmConceptItems[yarnParentIndex].FirmConceptItemYarns);
                    });
                    masterData.FirmConceptItems.find(x => x.FCItemID == row.FCItemID).FirmConceptItemYarns.map(x => {
                        x.TotalFeeder = GetTotalFeeder(fci.FirmConceptChilds[0].ActiveFeeder, totalFeederRepeat);
                    });
                    fci = masterData.FirmConceptItems.find(x => x.FCItemID == row.FCItemID);

                    childs = fci.FirmConceptChilds;
                    yarns = fci.FirmConceptItemYarns;
                    productLimites = fci.FirmConceptItemLimitations;
                }

                initMachineTable();
                $tblMachineEl.bootstrapTable("load", childs);
                $tblMachineEl.bootstrapTable('hideLoading');

                initYarnTable();
                $tblYarnEl.bootstrapTable("load", yarns);
                $tblYarnEl.bootstrapTable('hideLoading');

                initProductLimitation();
                $tblProductLimitation.bootstrapTable("load", productLimites);
                $tblProductLimitation.bootstrapTable('hideLoading');


                var recipeList = masterData.BatchWiseRecipeChilds.filter(x => x.RecipeID == _rowConcept.RecipeID);
                initDyeingTable();
                $tblDyeingEl.bootstrapTable("load", recipeList);
                $tblDyeingEl.bootstrapTable('hideLoading');

                initPreFinishingTable();
                $tblPreFinishingEl.bootstrapTable("load", masterData.PreFinishingInfos);
                $tblPreFinishingEl.bootstrapTable('hideLoading');

                initPostFinishingTable();
                $tblPostFinishingEl.bootstrapTable("load", masterData.PostFinishingInfos);
                $tblPostFinishingEl.bootstrapTable('hideLoading');

                initTestingTable();
                $tblTestEl.bootstrapTable("load", masterData.LabTestRequisitionBuyers);
                $tblTestEl.bootstrapTable('hideLoading');

                initCapacityDeclarationTable();
                $tblCapacityDeclarationEl.bootstrapTable("load", row.FabricWidthAndCapacityDeclaration);
                $tblCapacityDeclarationEl.bootstrapTable('hideLoading');

                var imgPath = "../Uploads/No_Image_Available.jpg";
                if (masterData.LabTestRequisitionBuyers.length > 0) {
                    if (masterData.LabTestRequisitionBuyers[0].ImagePath != null)
                        imgPath = ".." + masterData.LabTestRequisitionBuyers[0].ImagePath;
                }
                var lastChildDiv = $("#divTesting");
                lastChildDiv.find("a").attr("href", imgPath);

                $("#CamArrangement").val('');
                //$("#NeedleArrangement").val('');
                $(".file-drop-zone").text("");
                initImageAttachment(ImagesContentList.filter(function (el) { return el.FCItemID == row.FCItemID && el.FCImageTypeID == firmConceptImageTypes.CAM })[0], $formEl.find("#CamArrangement"));
                initImageAttachment(ImagesContentList.filter(function (el) { return el.FCItemID == row.FCItemID && el.FCImageTypeID == firmConceptImageTypes.NEEDLE })[0], $formEl.find("#NeedleArrangement"));
            },
  
        });
        // Add event listener for cell data change
        $(document).on('blur', '#tblChildFabricIdFirmConcept-782 td[contenteditable="true"]', function (e) {
            var $cell = $(this);
            var newValue = $cell.text();
            var field = $cell.data('field');
            var rowId = $cell.closest('tr').data('uniqueid');

            // Perform any necessary actions with the updated data
            // For example, update the corresponding object or send an AJAX request
            alert();
            console.log('Cell data changed:');
            console.log('Field:', field);
            console.log('Row ID:', rowId);
            console.log('New Value:', newValue);
        });
       
    }

    function SetCapacityDeclaration(row) {
        alert();
        debugger;
    }
    function initImageAttachment(firmConceptImage, $el) {
        if (!firmConceptImage) {
            $el.fileinput('destroy');
            $el.fileinput({
                showUpload: false,
                previewFileType: 'any'
            });
        }
        else {
            if (firmConceptImage.FCImageTypeID == firmConceptImageTypes.CAM) {
                $(".CArrangement .file-drop-zone").html("");
                if (firmConceptImage.ImagePath == "") {
                    firmConceptImage['path'] = firmConceptImage.Image64Byte;

                    var previewData = [firmConceptImage];
                    var type = !firmConceptImage.ImageType ? "any" : firmConceptImage.ImageType;
                    var previewConfig = [{ type: type, caption: "Attachment", key: firmConceptImage.FCItemID + '-' + firmConceptImage.FCImageTypeID, width: "80px", frameClass: "preview-frame" }];

                    $(".file-preview").css({ "display": "block" });
                    $(".CArrangement .file-drop-zone").html("<img src='" + previewData[0].path + "' style='width:150px; height:130px;' />");
                } else {
                    firmConceptImage['path'] = rootPath + firmConceptImage.ImagePath;

                    var preveiwData = [rootPath + firmConceptImage.ImagePath];
                    var previewConfig = [{ type: type, caption: "Attachment", key: firmConceptImage.FCItemID + '-' + firmConceptImage.FCImageTypeID, width: "80px", frameClass: "preview-frame" }];

                    $el.fileinput('destroy');
                    $el.fileinput({
                        showUpload: false,
                        initialPreview: preveiwData,
                        initialPreviewAsData: true,
                        initialPreviewFileType: 'image',
                        initialPreviewConfig: previewConfig,
                        maxFileSize: 4096
                    });
                }
            }
            else {
                $(".NeedleArrangement .file-drop-zone").html("");
                if (firmConceptImage.ImagePath == "") {
                    firmConceptImage['path'] = firmConceptImage.Image64Byte;
                    var previewData = [firmConceptImage];
                    var type = !firmConceptImage.ImageType ? "any" : firmConceptImage.ImageType;
                    var previewConfig = [{ type: type, caption: "Attachment", key: firmConceptImage.FCItemID + '-' + firmConceptImage.FCImageTypeID, width: "80px", frameClass: "preview-frame" }];

                    $(".file-preview").css({ "display": "block" });
                    $(".NeedleArrangement .file-drop-zone").html("<img src='" + previewData[0].path + "' style='width:150px; height:130px;' />");
                } else {
                    //firmConceptImage['path'] = rootPath + firmConceptImage.ImagePath;

                    var preveiwData = [rootPath + firmConceptImage.ImagePath];
                    var previewConfig = [{ type: type, caption: "Attachment", key: firmConceptImage.FCItemID + '-' + firmConceptImage.FCImageTypeID, width: "80px", frameClass: "preview-frame" }];

                    $el.fileinput('destroy');
                    $el.fileinput({
                        showUpload: false,
                        initialPreview: preveiwData,
                        initialPreviewAsData: true,
                        initialPreviewFileType: 'image',
                        initialPreviewConfig: previewConfig,
                        maxFileSize: 4096
                    });
                }
            }
        }
    }

    function initMachineTable() {
        //Knitting Information
        //FirmConceptChild
        $tblMachineEl.bootstrapTable("destroy");
        $tblMachineEl.bootstrapTable({
            uniqueId: 'FCChildID',
            checkboxHeader: false,
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        if (row.SubGroupID == 1) {
                            return [
                                '<span class="btn-group">',
                                `<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportName=KnittingJobCard.rdl&JobCardNo=${row.KJobCardNo}" target="_blank" title="Job Card Report For Fabric">
                                <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                </a>`,
                                '</span>'
                            ].join('');
                        }
                        else {
                            return [
                                '<span class="btn-group">',
                                `<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportName=KnittingJobCardCC.rdl&JobCardNo=${row.KJobCardNo}" target="_blank" title="Job Card Report For Other Item">
                                <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                </a>`,
                                '</span>'
                            ].join('');
                        }
                    }
                },
                {
                    field: "KnittingMachineNo",
                    title: "Machine No"
                },
                {
                    field: "MachineGauge",
                    title: "Machine Gauge",
                    width: 100
                },
                {
                    field: "MachineDia",
                    title: "Machine Dia",
                    width: 100
                },
                {
                    field: "IsSubContact",
                    title: "Sub Contact?",
                    checkboxEnabled: false,
                    width: 80,
                    checkbox: true,
                    showSelectTitle: true,
                    visible: true
                },
                {
                    field: "Brand", //BrandID
                    title: "Brand",
                    width: 100
                },
                {
                    field: "Contact", //ContactID
                    title: "Floor/Sub-Contractor",
                    width: 100
                },
                {
                    field: "KnittingMachineNo", //KnittingMachineID
                    title: "Machine",
                    width: 120
                },
                {
                    field: "FUPartName",
                    title: "End Use",
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
                },
                {
                    field: "KJobCardNo",
                    title: "Job Card No",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "SubContractor",
                    title: "Floor/Sub-Contractor",
                },
                {
                    field: "KnittingType",
                    title: "Knitting Type"
                },
                {
                    field: "StitchLength",
                    title: "SL",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" min="0" style="padding-right: 10px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                            SetPropertiesFirmConceptChild("StitchLength", value);
                        }
                    }
                },
                {
                    field: "ActiveNeedle",
                    title: "Active Needle",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                            SetPropertiesFirmConceptChild("ActiveNeedle", value);
                        }
                    }
                },
                {
                    field: "RPM",
                    title: "RPM",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                            SetPropertiesFirmConceptChild("RPM", value);
                        }
                    }
                },
                {
                    field: "Efficiency",
                    title: "Efficiency (%)",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                            SetPropertiesFirmConceptChild("Efficiency", value);
                        }
                    }
                },
                {
                    field: "Elastane",
                    title: "Elastane (%)",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                },
                {
                    field: "ActiveFeeder",
                    title: "Active Feeder",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                            SetPropertiesFirmConceptChild("ActiveFeeder", value);
                        }
                    }
                },
                {
                    field: "TotalNeedle",
                    title: "Total Needle",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                },
                {
                    field: "IsNeedleDopped",
                    title: "Needle Dopped?",
                    width: 80,
                    showSelectTitle: true,
                    formatter: function (value, row, index, field) {
                        var checked = value ? "checked" : "";
                        return `<input class="form-check-input IsNeedleDopped" type="checkbox" ${checked} />`;
                    },
                    events: {
                        'click .IsNeedleDopped': function (e, value, row, index) {
                            row.IsNeedleDopped = e.currentTarget.checked;
                            SetPropertiesFirmConceptChild("IsNeedleDopped", row.IsNeedleDopped);
                        }
                    }
                },
                {
                    field: "CylinderIn",
                    title: "Cylinder In",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                            SetPropertiesFirmConceptChild("CylinderIn", value);
                        }
                    }
                },
                {
                    field: "CylinderOut",
                    title: "Cylinder Out",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                            SetPropertiesFirmConceptChild("CylinderOut", value);
                        }
                    }
                },
                {
                    field: "DialIn",
                    title: "Dial In",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                            SetPropertiesFirmConceptChild("DialIn", value);
                        }
                    }
                },
                {
                    field: "DialOut",
                    title: "Dial Out",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                            SetPropertiesFirmConceptChild("DialOut", value);
                        }
                    }
                },
                {
                    field: "ActiveNeedlePer",
                    title: "Active Needle (%)",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                },
                {
                    field: "TotalNumberOfActiveNeedle",
                    title: "Total No of Active Needle",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                },
                {
                    field: "CalculatedCapacityPerDay",
                    title: "Calculated Capacity/Day (KG)",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                },
                {
                    field: "DeclaredCapacityPerDay",
                    title: "Declared Capacity/Day (KG)",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                }

                //{
                //    field: "ActiveDeedle",
                //    title: "Active Deedle",
                //    cellStyle: function () { return { classes: 'm-w-100' } },
                //    editable: {
                //        type: "text",
                //        showbuttons: false,
                //        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                //        validate: function (value) {
                //            if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                //                return 'Must be a positive integer.';
                //            }
                //        }
                //    }
                //}
            ],
            onClickRow: function (row, $element) {
                for (var i = 0; i < _rowConcept.FabricWidthAndCapacityDeclaration.length; i++) {
                    if (i == 0) {
                        _rowConcept.FabricWidthAndCapacityDeclaration[i].DeclaredCapacityPerDay = row.DeclaredCapacityPerDay;
                    }
                    else {
                        _rowConcept.FabricWidthAndCapacityDeclaration[i].DeclaredCapacityPerDay = Math.round((_rowConcept.FabricWidthAndCapacityDeclaration[i - 1].DeclaredCapacityPerDay * _rowConcept.FabricWidthAndCapacityDeclaration[i].MachineDia) / _rowConcept.FabricWidthAndCapacityDeclaration[i - 1].MachineDia);
                    }
                }

                

                initCapacityDeclarationTable();
                $tblCapacityDeclarationEl.bootstrapTable("load", _rowConcept.FabricWidthAndCapacityDeclaration);
                $tblCapacityDeclarationEl.bootstrapTable('hideLoading');

            },
        });
}
    function SetPropertiesFirmConceptChild(propertyName, value) {
    var isSetActiveNeedlePer = false;
    if (propertyName == "Efficiency") {
        value = parseFloat(value);
    }
    else if (propertyName == "CylinderIn") {
        isSetActiveNeedlePer = true;
        value = parseInt(value);
    }
    else if (propertyName == "CylinderOut") {
        isSetActiveNeedlePer = true;
        value = parseInt(value);
    }
    else if (propertyName == "DialIn") {
        isSetActiveNeedlePer = true;
        value = parseInt(value);
    }
    else if (propertyName == "DialOut") {
        isSetActiveNeedlePer = true;
        value = parseInt(value);
    }
    else if (propertyName == "IsNeedleDopped") {
        isSetActiveNeedlePer = true;
    } else {
        value = parseInt(value);
    }

    var indexF = masterData.FirmConceptItems.findIndex(x => x.FCItemID == _rowConcept.FCItemID);
    if (indexF > -1) {
        masterData.FirmConceptItems[indexF].FirmConceptChilds.map(x => {
            x[propertyName] = value;
            if (isSetActiveNeedlePer) {

                x.ActiveNeedlePer = GetActiveNeedlePercentage(_rowConcept.TechnicalName, x.IsNeedleDopped, x.CylinderIn, x.CylinderOut, x.DialIn, x.DialOut);
                x.TotalNumberOfActiveNeedle = GetTotalNumberOfActiveNeedle(_rowConcept.TechnicalName, x.IsNeedleDopped, x.MachineDia, x.MachineGauge, x.CylinderIn, x.CylinderOut, x.DialIn, x.DialOut);
            }
        });
        $tblMachineEl.bootstrapTable("load", masterData.FirmConceptItems[indexF].FirmConceptChilds);
    }
}

function initYarnTable() {
    $tblYarnEl.bootstrapTable("destroy");
    $tblYarnEl.bootstrapTable({
        uniqueId: 'FCMRChildID',
        checkboxHeader: false,
        columns: [
            {
                field: "Segment1ValueDesc",
                title: "Composition",
                cellStyle: function () { return { classes: 'm-w-150' } }
            },
            {
                field: "Segment2ValueDesc",
                title: "Yarn Type",
                cellStyle: function () { return { classes: 'm-w-150' } }
            },
            {
                field: "Segment3ValueDesc",
                title: "Process",
                cellStyle: function () { return { classes: 'm-w-150' } }
            },
            {
                field: "Segment4ValueDesc",
                title: "Sub Process"
            },
            {
                field: "Segment5ValueDesc",
                title: "Quality Parameter",
                cellStyle: function () { return { classes: 'm-w-150' } }
            },
            {
                field: "Segment6ValueDesc",
                title: "Count"
            },
            {
                field: "ShadeCode",
                title: "Shade Code",
                cellStyle: function () { return { classes: 'm-w-80' } }
            },
            {
                field: "CountValue",
                title: "Count Value (Ne)",
                align: 'center'
            },
            {
                field: "YarnPly",
                title: "No of Ply",
            },
            {
                field: "PhysicalCount",
                title: "Physical Count"
            },
            {
                field: "YarnCategory",
                title: "Yarn Category"
            },
            {
                field: "YD",
                title: "YD",
                width: 80,
                checkbox: true,
                showSelectTitle: true
            },
            {
                field: "ReqQty",
                title: "Req Qty",
                align: 'center'
            },
            {
                field: "FinalCount",
                title: "Final Count (Ne)",
                align: 'center'
            },
            {
                field: "FeederRepeat",
                title: "Feeder Repeat",
                align: 'center',
                editable: {
                    type: "text",
                    showbuttons: false,
                    tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    validate: function (value) {
                        if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
                            return 'Must be a positive integer.';
                        }
                    }
                }
            },
            {
                field: "StitchLength",
                title: "Stitch Length",
                align: 'center'
            },
            {
                field: "TotalFeeder",
                title: "Total Feeder",
                align: 'center'
            },
            {
                field: "ConsumptionPer",
                title: "Consumption (%)",
                align: 'center'
            },
        ],
        onEditableSave: function (field, row, oldValue, $el) {
            if (field == "FeederRepeat") {
                var rows = $tblYarnEl.bootstrapTable('getData');
                var yarnParentIndex = masterData.FirmConceptItems.findIndex(x => x.ConceptID == row.ConceptID);
                rows.map(yRow => {
                    var yarnIndex = rows.findIndex(x => x.KPYarnID == yRow.KPYarnID);
                    var elastanePer = GetElastanePer(masterData.FirmConceptItems[yarnParentIndex].Composition);
                    yRow.ConsumptionPer = GetConsumption(elastanePer, yarnIndex, masterData.FirmConceptItems[yarnParentIndex].FirmConceptItemYarns);
                    rows[yarnIndex] = yRow;
                });
                masterData.FirmConceptItems[yarnParentIndex].FirmConceptItemYarns = rows;
                $tblYarnEl.bootstrapTable('load', masterData.FirmConceptItems[yarnParentIndex].FirmConceptItemYarns);
            }
        }
    });
}
function initProductLimitation() {
    $tblProductLimitation.bootstrapTable("destroy");
    $tblProductLimitation.bootstrapTable({
        uniqueId: 'FCItemLimitationID',
        checkboxHeader: false,
        columns: [
            {
                field: "FCItemLimitationID",
                title: "FCItemLimitationID",
                visible: false
            },
            {
                field: "FCID",
                title: "FCID",
                visible: false
            },
            {
                field: "FCItemID",
                title: "FCItemID",
                visible: false
            },
            {
                field: "PLimitationID",
                title: "PLimitationID",
                visible: false
            },
            {
                field: "PLimitationName",
                title: "Feature Name"
            },
            {
                field: "LimitationValue",
                title: "Feature Value",
                cellStyle: function () { return { classes: 'm-w-100' } },
                editable: {
                    type: "text",
                    showbuttons: false,
                    tpl: '<input type="text" class="form-control input-sm" style="padding-right: 24px;">',
                    validate: function (value) {

                    }
                }
            },
        ]
    });
}


//function initDyeingTable() {
//    $tblDyeingEl.bootstrapTable("destroy");
//    $tblDyeingEl.bootstrapTable({
//        uniqueId: 'RecipeChildID',
//        columns: [
//            {
//                field: "ParticularsName",
//                title: "Particulars"
//            },
//            {
//                field: "RawItemName",
//                title: "Raw Item"
//            },

//            {
//                field: "Qty",
//                title: "Qty"
//            },
//            {
//                field: "Unit",
//                title: "UOM"
//            },
//            {
//                field: "TempIn",
//                title: "Temp (C)"
//            },
//            {
//                field: "ProcessTime",
//                title: "Process Time (Minute)"
//            }

//        ]
//    });
//}

function initDyeingTable() {
    $tblDyeingEl.bootstrapTable("destroy");
    $tblDyeingEl.bootstrapTable({
        uniqueId: 'RecipeChildID',
        //editable: isEditable,
        detailView: true,
        checkboxHeader: false,
        showFooter: true,
        columns: [
            {
                field: "FiberPart",
                title: "Fiber Part",
                align: 'center'
            },
            {
                field: "ColorName",
                title: "Color Name",
                align: 'center'
            },
            {
                field: "Temperature",
                title: "Temperature",
                align: 'center'
            },
            {
                field: "ProcessTime",
                title: "Process Time",
                align: 'center',
            }
        ],
        onExpandRow: function (index, row, $detail) {
            populateChildex(row.RecipeDInfoID, $detail);
        }
    });
}

function populateChildex(childId, $detail) {
    $tblDefChildEl = $detail.html('<table id="TblChildDef-' + childId + '"></table>').find('table');
    var childIndex = getIndexFromArray(masterData.BatchWiseRecipeChilds, "RecipeDInfoID", childId)
    initDefChildTable(childId, $detail);
    var cnt = (masterData.BatchWiseRecipeChilds[childIndex].DefChilds == null) ? 0 : masterData.BatchWiseRecipeChilds[childIndex].DefChilds.length;
    if (cnt > 0)
        $tblDefChildEl.bootstrapTable('load', masterData.BatchWiseRecipeChilds[childIndex].DefChilds.filter(function (item) {
            return item.EntityState != 8
        }));
}

function initDefChildTable(childId, $detail) {
    $tblDefChildEl.bootstrapTable("destroy");
    $tblDefChildEl.bootstrapTable({
        showFooter: true,
        checkboxHeader: false,
        columns: [
            {
                field: "ParticularsName",
                title: "Particulars",
                align: 'center'

            },
            {
                field: "RawItemName",
                title: "Item",
                align: 'center'

            },
            {
                field: "IsPercentageText",
                title: "%"

            },
            {
                field: "Qty",
                title: "Qty (gm/ltr)/%",
                align: 'center'
            }
        ]
    });
}

function initPreFinishingTable() {
    $tblPreFinishingEl.bootstrapTable("destroy");
    $tblPreFinishingEl.bootstrapTable({
        uniqueId: 'FPChildID',
        detailView: true,
        checkboxHeader: false,
        showFooter: true,
        columns: [
            {
                field: "ProcessName",
                title: "Process",
                align: 'center',
            },
            {
                field: "ProcessType",
                title: "Process Type",
                align: 'center',
            },
            {
                field: "MachineName",
                title: "Machine Name"
            },
            {
                field: "MachineNo",
                title: "Machine No"
            },
            {
                field: "UnitName",
                title: "Unit"
            },
            {
                field: "BrandName",
                title: "Brand"
            }
        ],
        onExpandRow: function (index, row, $detail) {
            var nFMSID = row.PFMSID == 0 ? row.FMSID : row.PFMSID;
            getPreFinishingInfo(nFMSID, row.FPChildID, $detail);
        }
    });
}

function initPostFinishingTable() {
    $tblPostFinishingEl.bootstrapTable("destroy");
    $tblPostFinishingEl.bootstrapTable({
        uniqueId: 'DBCFPID',
        detailView: true,
        checkboxHeader: false,
        showFooter: true,
        columns: [
            {
                field: "ProcessName",
                title: "Process",
                align: 'center',
            },
            {
                field: "ProcessType",
                title: "Process Type",
                align: 'center',
            },
            {
                field: "MachineName",
                title: "Machine Name"
            },
            {
                field: "MachineNo",
                title: "Machine No"
            },
            {
                field: "UnitName",
                title: "Unit"
            },
            {
                field: "BrandName",
                title: "Brand"
            }
        ],
        onExpandRow: function (index, row, $detail) {
            var nFMSID = row.PFMSID == 0 ? row.FMSID : row.PFMSID;
            getPostFinishingInfo(nFMSID, row.DBCFPID, $detail);
        }
    });
}

function initTestingTable() {
    $tblTestEl.bootstrapTable("destroy");
    $tblTestEl.bootstrapTable({
        uniqueId: 'LTReqBuyerID',
        columns: [
            {
                field: "BuyerName",
                title: "Buyer Name"
            },
            {
                field: "Remarks",
                title: "Remarks"
            },
            {
                field: "Status",
                title: "Status"
            }
        ]
    });
}
    function initCapacityDeclarationTable() {
        $tblCapacityDeclarationEl.bootstrapTable("destroy");
        $tblCapacityDeclarationEl.bootstrapTable({
            columns: [
                {
                    field: "MachineDia",
                    title: " Dia"
                },
                {
                    field: "MachineGauge",
                    title: "Gauge"
                },
                {
                    field: "FinalWidth",
                    title: "Width"
                },
                {
                    field: "DeclaredCapacityPerDay",
                    title: "Capacity"
                }
            ]
        });
    }
function backToList() {
    $divDetailsEl.fadeOut();
    resetForm();
    $divTblEl.fadeIn();
    getMasterTableData();
    //document.getElementById("ProductImagediv").innerHTML = "";
    //document.getElementById("NeedleArrangementdiv").innerHTML = "";
    //document.getElementById("CamArrangementdiv").innerHTML = "";
    ////document.getElementById("ProductImagediv").hidden = true;
    //document.getElementById("NeedleArrangementdiv").hidden = true;
    //document.getElementById("CamArrangementdiv").hidden = true;
    //$formEl.find("#NeedleArrangementdiv").innerHTML = "";
    //$formEl.find("#CamArrangementdiv").innerHTML = "";
}

function resetForm() {
    $formEl.trigger("reset");
    $.each($formEl.find('select'), function (i, el) {
        $(el).select2('');
    });
    $formEl.find("#FCMRChildID").val(-1111);
    $formEl.find("#EntityState").val(4);
}

function resetTableParams() {
    tableParams.offset = 0;
    tableParams.limit = 10;
    tableParams.filter = '';
    tableParams.sort = '';
    tableParams.order = '';
}

function getNew(ConceptID, CCColorID, ConceptNo) {
    var url = "/api/rnd-firm-concept/new/" + ConceptID + "/" + CCColorID + "/" + ConceptNo;
    axios.get(url)
        .then(function (response) {
            $divDetailsEl.fadeIn();
            $divTblEl.fadeOut();
            masterData = response.data;
            masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
            masterData.TrialDate = formatDateToDefault(masterData.TrialDate);

            CompOptions = "<option value=''>Select Composition</option>";
            for (var i = 0; i < masterData.FinalCompositionList.length; i++) {
                CompOptions = CompOptions + "<option value=" + masterData.FinalCompositionList[i].id + ">" + masterData.FinalCompositionList[i].text + "</option>";
            }


            $formEl.find("#btnSave").show();
            $formEl.find("#CommercialName").prop("readonly", false);
            $formEl.find("#ProductDetails").prop("readonly", false);
            $formEl.find("#ActiveNeedle").prop("readonly", false);
            $formEl.find("#RPM").prop("readonly", false);
            $formEl.find("#ConceptDeclaration").prop("disabled", false);
            $formEl.find("#divKnittingInfo,#divYarnInfo,#divProductLimitation,#divDyeingInfo,#divPreDyeingProcessInfo,#divFinishingProcess,#divTestingInfo,#divCapacityDeclarationInfo").hide();

            setFormData($formEl, masterData);

            initChildFabricTable();
            //GetFirmConceptChildsWithPropValues(ConceptID);
            $tblChildFabricId.bootstrapTable("load", masterData.FirmConceptItems);
            $tblChildFabricId.bootstrapTable('hideLoading');
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        });
}

function getDetails(id, conceptId, CCColorID, ConceptNo) {
    axios.get(`/api/rnd-firm-concept/${id}/${conceptId}/${CCColorID}/${ConceptNo}`)
        .then(function (response) {
            $divDetailsEl.fadeIn();
            $divTblEl.fadeOut();
            masterData = response.data;
            masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
            masterData.TrialDate = formatDateToDefault(masterData.TrialDate);

            CompOptions = "<option value=''>Select Composition</option>";
            for (var i = 0; i < masterData.FinalCompositionList.length; i++) {
                CompOptions = CompOptions + "<option value=" + masterData.FinalCompositionList[i].id + ">" + masterData.FinalCompositionList[i].text + "</option>";
            }


            $formEl.find("#CommercialName").prop("readonly", true);
            $formEl.find("#ProductDetails").prop("readonly", true);
            $formEl.find("#ActiveNeedle").prop("readonly", true);
            $formEl.find("#RPM").prop("readonly", true);
            $formEl.find("#divKnittingInfo,#divYarnInfo,#divProductLimitation,#divDyeingInfo,#divPreDyeingProcessInfo,#divFinishingProcess,#divTestingInfo,#divCapacityDeclarationInfo").hide();

            setFormData($formEl, masterData);

            initChildFabricTable();

            masterData.FirmConceptItems.map(ci => {
                ci.FirmConceptChilds.map(cc => {

                    cc.ActiveNeedlePer = GetActiveNeedlePercentage(ci.TechnicalName, cc.IsNeedleDopped, cc.CylinderIn, cc.CylinderOut, cc.DialIn, cc.DialOut);
                    cc.TotalNumberOfActiveNeedle = GetTotalNumberOfActiveNeedle(ci.TechnicalName, cc.IsNeedleDopped, cc.MachineDia, cc.MachineGauge, cc.CylinderIn, cc.CylinderOut, cc.DialIn, cc.DialOut);
                    cc.TotalNeedle = GetTotalNeedle(ci.TechnicalName, cc.MachineDia, cc.MachineGauge);
                    cc.Elastane = ci.Elastane;
                });
            });

            $tblChildFabricId.bootstrapTable("load", masterData.FirmConceptItems);
            $tblChildFabricId.bootstrapTable('hideLoading');

            ImagesContentList = masterData.FirmConceptImages;


        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        });
}

function GetFirmConceptChildsWithPropValues(conceptID) {
    var indexF = masterData.FirmConceptItems.findIndex(x => x.ConceptID == conceptID);
    if (indexF > -1) {
        masterData.FirmConceptItems[indexF].TotalMinutes = GetTotalMinutes(masterData.FirmConceptItems[indexF].CapacityIn);
        var child = masterData.FirmConceptChilds.find(c => c.ConceptID == conceptID);
        var childIndex = masterData.FirmConceptChilds.findIndex(c => c.ConceptID == conceptID);
        if (childIndex > -1) {

            masterData.FirmConceptItems[indexF].ProdWOElastane = GetProdWOElastane(masterData.FreeConceptMRChilds, masterData.FirmConceptItems[indexF].TechnicalName, child.IsNeedleDopped, child.MachineDia, child.MachineGauge, child.CylinderIn, child.CylinderOut, child.DialIn, child.DialOut, masterData.FirmConceptItems[indexF].CapacityIn);

            var elastanePer = GetElastanePer(masterData.FirmConceptItems[indexF].Composition);
            var totalNeedle = GetTotalNeedle(masterData.FirmConceptItems[indexF].TechnicalName, child.MachineDia, child.MachineGauge);
            var activeNeeldePer = GetActiveNeedlePercentage(masterData.FirmConceptItems[indexF].TechnicalName, child.IsNeedleDopped, child.CylinderIn, child.CylinderOut, child.DialIn, child.DialOut);

            //Childs Prop value set in MasterData
            masterData.FirmConceptChilds[childIndex].Elastane = elastanePer;
            masterData.FirmConceptChilds[childIndex].TotalNeedle = totalNeedle;
            masterData.FirmConceptChilds[childIndex].ActiveNeedlePer = activeNeeldePer;

            //Childs Prop value set in FirmConceptItems
            childIndex = masterData.FirmConceptItems[indexF].FirmConceptChilds.findIndex(c => c.ConceptID == conceptID);
            masterData.FirmConceptItems[indexF].FirmConceptChilds[childIndex].Elastane = elastanePer;
            masterData.FirmConceptItems[indexF].FirmConceptChilds[childIndex].TotalNeedle = totalNeedle;
            masterData.FirmConceptItems[indexF].FirmConceptChilds[childIndex].ActiveNeedlePer = activeNeeldePer;

            //FreeConceptMRChilds
            var activeFeeder = 0;
            masterData.FirmConceptChilds.map(x => {
                activeFeeder += parseInt(x.ActiveFeeder);
            });

            var feederRepeatSum = 0;
            var yarnIndex = -1;
            masterData.FreeConceptMRChilds.map(x => {
                yarnIndex++;
                feederRepeatSum += parseInt(x.FeederRepeat);

                x.FinalCount = GetFinalCountNe(x.CountValue, x.YarnPly);
                x.FeederRepeat = 0;
                x.ConsumptionPer = GetConsumption(elastanePer, yarnIndex, masterData.FreeConceptMRChilds);
            });

            masterData.FreeConceptMRChilds.map(x => {
                x.TotalFeeder = GetTotalFeeder(activeFeeder, feederRepeatSum);
            });
        }
    }
}


function showAddComposition(e) {
    e.preventDefault();
    initTblCreateComposition();
    $pageEl.find(`#modal-new-composition-${pageId}`).modal("show");
    $pageEl.find(`#modal-new-composition-${pageId}`).removeAttr('tabindex');
}

function initTblCreateComposition() {
    compositionComponents = [];
    var columns = [
        {
            field: 'Id', isPrimaryKey: true, visible: false
        },
        {
            headerText: 'Commands', width: 120, commands: [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
        },
        {
            field: 'Percent', headerText: 'Percent(%)', editType: "numericedit", params: { decimals: 0, format: "N", min: 1, validateDecimalOnType: true }
        },
        {
            field: 'Fiber', headerText: 'Component', editType: 'dropdownedit', edit: new ej2DropdownParams({ dataSource: masterData.FabricComponents, field: "Fiber" })
            //field: 'Fiber', headerText: 'Component', dataSource: masterData.FabricComponents, displayField: "Fiber", edit: ej2GridDropDownObj({})
        }
    ];

    var gridOptions = {
        tableId: tblCreateCompositionId,
        data: compositionComponents,
        columns: columns,
        actionBegin: function (args) {
            if (args.requestType === "add") {
                if (compositionComponents.length === 4) {
                    toastr.info("You can only add 4 components.");
                    args.cancel = true;
                    return;
                }

                args.data.Id = getMaxIdForArray(compositionComponents, "Id");
            }
            else if (args.requestType === "save" && args.action === "edit") {
                if (!args.data.Fiber) {
                    toastr.warning("Fabric component is required.");
                    args.cancel = true;
                    return;
                }
                else if (!args.data.Percent || args.data.Percent <= 0 || args.data.Percent > 100) {
                    toastr.warning("Composition percent must be greater than 0 and less than or equal 100.");
                    args.cancel = true;
                    return;
                }
            }
        },
        autofitColumns: false,
        showDefaultToolbar: false,
        allowFiltering: false,
        allowPaging: false,
        toolbar: ['Add'],
        editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true }
    };

    if ($tblCreateCompositionEl) $tblCreateCompositionEl.destroy();
    $tblCreateCompositionEl = new initEJ2Grid(gridOptions);
}

function saveComposition() {
    var totalPercent = sumOfArrayItem(compositionComponents, "Percent");
    if (totalPercent != 100) return toastr.error("Sum of compostion percent must be 100");

    compositionComponents.reverse();
    compositionComponents.sort((a, b) => (a.Percent > b.Percent) ? -1 : ((b.Percent > a.Percent) ? 1 : 0));

    var composition = "";
    compositionComponents.forEach(function (component) {
        composition += composition ? ` ${component.Percent}%` : `${component.Percent}%`;
        composition += ` ${component.Fiber}`;
    });

    var data = {
        SegmentValue: composition
    };

    axios.post("/api/rnd-free-concept/save-fabric-composition", data)
        .then(function (response) {
            $pageEl.find(`#modal-new-composition-${pageId}`).modal("hide");
            toastr.success("Composition added successfully.");
            masterData.CompositionList.unshift({ id: response.data.Id, text: response.data.SegmentValue });
            initSelect2($formEl.find("#CompositionId"), masterData.CompositionList);
        })
        .catch(showResponseError)
}

function viewReport(type) {
    var tt = 0;
    if (type === "PreFinishingProcess") {
        if (masterData.PreFinishingInfos.length > 0) {
            tt = masterData.PreFinishingInfos[0].PFBatchNo;
        }
        if (tt != 0) {
            window.open(`/reports/InlinePdfView?ReportName=PreFinishProcessBatchCard.rdl&PFBatchNo=${tt}`, '_blank');
        }
    } else if (type === "FinishingProcess") {
        if (masterData.PostFinishingInfos.length > 0) {
            tt = masterData.PostFinishingInfos[0].DBatchNo;
        }
        if (tt != 0) {
            window.open(`/reports/InlinePdfView?ReportName=SampleBatchProcessCard.rdl&DBatchNo=${tt}`, '_blank');
        }
    } else if (type === "Testing") {
        if (masterData.PostFinishingInfos.length > 0) {
            tt = masterData.LabTestRequisitionBuyers[0].LTReqMasterID;
        }
        if (tt != 0) {
            window.open(`/reports/InlinePdfView?ReportName=LabTestTestingRequirementForm.rdl&ReqID=${tt}`, '_blank');
        }
    }
}

function save() {
    if (!$formEl.find("#ConceptDeclaration").val()) return toastr.error("Concept declaration required!");
    var formData = getFormData($formEl);

    var firmConceptItems = masterData.FirmConceptItems,
        firmConceptChilds = [],
        firmConceptItemYarns = [],
        firmConceptItemLimitations = [];

    firmConceptItems.map(x => {
        firmConceptChilds.push(...x.FirmConceptChilds);
        firmConceptItemYarns.push(...x.FirmConceptItemYarns);
        firmConceptItemLimitations.push(...x.FirmConceptItemLimitations);
    });
    formData.append("FirmConceptItems", JSON.stringify(firmConceptItems));
    formData.append("FirmConceptChilds", JSON.stringify(firmConceptChilds));
    formData.append("FirmConceptItemYarns", JSON.stringify(firmConceptItemYarns));
    formData.append("FirmConceptItemLimitations", JSON.stringify(firmConceptItemLimitations));

    for (var i = 0; i < ImagesContentList.length; i++) {
        formData.append(ImagesContentList[i].FCItemID + "-" + ImagesContentList[i].FCImageTypeID, ImagesContentList[i].ImageFile);
    }

    const config = {
        headers: {
            'content-type': 'multipart/form-data',
            'Authorization': "Bearer " + localStorage.getItem("token")
        }
    }
    axios.post("/api/rnd-firm-concept/save", formData, config)
        .then(function () {
            toastr.success("Saved successfully!");
            backToList();
        })
        .catch(showResponseError);
}

//function save() {
//    if (!$formEl.find("#ConceptDeclaration").val()) return toastr.error("Concept declaration required!");
//    var formData = getFormData($formEl);
//    formData.append("FirmConceptItems", JSON.stringify(masterData.FirmConceptItems));

//    for (var i = 0; i < ImagesContentList.length; i++) {
//        formData.append(ImagesContentList[i].FCItemID + "-" + ImagesContentList[i].FCImageTypeID, ImagesContentList[i].ImageFile);
//    }

//    const config = {
//        headers: {
//            'content-type': 'multipart/form-data',
//            'Authorization': "Bearer " + localStorage.getItem("token")
//        }
//    }
//    axios.post("/api/rnd-firm-concept/save", formData, config)
//        .then(function () {
//            toastr.success("Saved successfully!");
//            backToList();
//        })
//        .catch(showResponseError);
//}

function GetElastanePer(composition) {
    if (composition != null && composition.length > 1) {
        var splitComposition = composition.split(' ');
        var eIndex = splitComposition.findIndex(c => c.toLowerCase() == "elastane");
        if (eIndex > 0) {
            return parseFloat(splitComposition[eIndex - 1].slice(0, -1));
        }
    }
    return 0;
}
function getNumberValue(val) {
    if (val == null || typeof val === "undefined" || isNaN(val) || val == "-") return 0;
    return val;
}
//Calculations
function GetTotalNeedle(technicalName, dia, gauge) {
    var A7 = getNumberValue(dia);
    var E7 = getNumberValue(gauge);

    if (technicalName != null && checkTechnicalName(technicalName)) {
        return Math.round(3.1416 * A7 * E7);
    }
    return Math.round(3.1416 * A7 * E7 * 2);

    /*
    IF(OR(AC7="Single",AC7="SJ Jacquard"),ROUNDUP(3.1416*A7*E7,0),ROUNDUP(3.1416*A7*E7*2,0))
    A7 = Dia
    E7 = Gauge
    */
}
function checkTechnicalName(technicalName) {
    var techNames = ["Single", "SJ", "Single Jersey", "Jacquard Single Jersey", "JS Jersey", "SJ Jacquard"];
    var findIndexTN = techNames.findIndex(tn => tn.toLowerCase() == technicalName.toLowerCase());
    if (findIndexTN > -1) return true;
    return false;
}
function GetActiveNeedlePercentage(technicalName, isNeedleDopped, cylinderIn, cylinderOut, dialIn, dialOut) {
    var AC7 = technicalName;
    var K10 = isNeedleDopped;
    var T12 = getNumberValue(cylinderIn);
    var X12 = getNumberValue(cylinderOut);
    var T14 = getNumberValue(dialIn);
    var X14 = getNumberValue(dialOut);
    var output = 0;

    if (!K10) {
        return 100;
    }
    else if (AC7 != null && checkTechnicalName(technicalName)) {
        if ((T12 + X12) == 0) return 0;
        output = ((T12) / (T12 + X12)) * 100;
    }
    else {
        if ((T12 + T14 + X12 + X14) == 0) return 0;
        output = ((T12 + T14) / (T12 + T14 + X12 + X14)) * 100;
    }
    output = Math.round(output);
    return parseFloat(output.toFixed(2));

    /*
    =IF(K10="No",1,IF(OR(AC7="Single",AC7="SJ Jacquard"),(T12)/(T12+X12),(T12+T14)/(T12+T14+X12+X14)))
    K10 = isNeedleDopped
    T12 = cylinderIn
    X12 = cylinderOut
    T14 = dialIn
    X14 = dialOut
    */
}
function GetTotalNumberOfActiveNeedle(technicalName, isNeedleDopped, dia, gauge, cylinderIn, cylinderOut, dialIn, dialOut) {
    var AC7 = technicalName;
    var K10 = isNeedleDopped;
    var Y7 = GetTotalNeedle(technicalName, dia, gauge);
    var X16 = GetActiveNeedlePercentage(technicalName, isNeedleDopped, cylinderIn, cylinderOut, dialIn, dialOut);

    if (AC7 != null && AC7.toLowerCase() == "Interlock".toLowerCase()) {
        if (!K10) return Math.round(Y7 / 2);
        else return Math.round((Y7 / 2) * X16);
    } else if (!K10) {
        return Math.round(Y7);
    } else if (K10) {
        return Math.round(Y7 * X16);
    }
    return 0;

    /*
     =IF(AC7="Interlock",(IF(K10="No",ROUNDUP(Y7/2,0),ROUNDUP(Y7/2*X16,0))),IF(K10="No",ROUNDUP(Y7,0),ROUNDUP(Y7*X16,0)))
     K10 = isNeedleDopped
     Y7 = GetTotalNeedle(technicalName, dia, gauge)
     X16 = GetTotalNumberOfActiveNeedle(technicalName, isNeedleDopped, cylinderIn, cylinderOut, dialIn, dialOut)
     */
}
function GetFinalCountNe(countValueNe, noOfPly) {
    var G24 = getNumberValue(countValueNe);
    var K24 = getNumberValue(noOfPly);

    if (K24 == 0) return 0;
    return Math.round(G24 / K24);

    /*
     =ROUNDUP(G24/K24,1)
     */
}
function GetTotalFeeder(activeFeeder, feederRepeatSum) {
    feederRepeatSum = getNumberValue(getNumberValue); //P24 + P25 + P26 + P27
    var U7 = getNumberValue(activeFeeder);
    if (feederRepeatSum == 0) {
        return 0;
    }
    return U7 * P24 / feederRepeatSum;

    /*
     =$U$7*P24/($P$24+$P$25+$P$26+$P$27)
     */
}
function GetConsumption(elastanePer, yarnIndex, yarnList) {
    var Q7 = getNumberValue(elastanePer);

    var totalCalculation = 0,
        ownCalculation = 0,
        index = 0,
        result = 0;
    yarnList.map(yarn => {
        var W = typeof yarn.StitchLength === "undefined" ? 0 : yarn.StitchLength;
        var M = yarn.FinalCount;
        var P = yarn.FeederRepeat;
        var result = 0;
        if (parseInt(M) > 0 && parseInt(P) > 0) {
            result = parseFloat(W) / parseInt(M) * parseInt(P);
        }
        if (index == yarnIndex) {
            ownCalculation = result;
        }
        totalCalculation += result;
        index++;
    });
    var output = parseFloat((ownCalculation / totalCalculation / 100 * (100 - Q7)).toFixed(2));
    if (isNaN(output)) return 0;
    return output;

    //yarnList[yarnIndex].ConsumptionPer = ownCalculation / totalCalculation / 100 * (100 - Q7);
    //return yarnList[yarnIndex];

    /*
     =(W24/M24*P24)/((W24/M24*P24)+(W25/M25*P25)+(W26/M26*P26)+(W27/M27*P27))/100*(100-Q7)
     */
}
function GetTotalMinutes(capacityIn) {
    if (capacityIn == "Hour") return 60;
    else if (capacityIn == "Shift") return 8 * 60;
    else if (capacityIn == "Day") return 24 * 60;
    else if (capacityIn == "Month") return 30 * 24 * 60;
    return 0;

    /*
     =IF(F31="Hour",60,IF(F31="Shift",8*60,IF(F31="Day",24*60,IF(F31="Month",30*24*60))))
     */
}
function GetProdWOElastane(yarnList, technicalName, isNeedleDopped, dia, gauge, cylinderIn, cylinderOut, dialIn, dialOut, capacityIn) {
    var Y18 = GetTotalNumberOfActiveNeedle(technicalName, isNeedleDopped, dia, gauge, cylinderIn, cylinderOut, dialIn, dialOut);

    var prodWOElastane = 0,
        totalMinutes = GetTotalMinutes(capacityIn);
    yarnList.map(yarn => {
        var W = typeof yarn.StitchLength === "undefined" ? 0 : yarn.StitchLength;
        var S = yarn.TotalFeeder;
        var I = typeof yarn.RPM === "undefined" ? 0 : yarn.RPM; //From Knitting Information
        var O = totalMinutes;
        var M1 = typeof yarn.Efficiency === "undefined" ? 0 : yarn.Efficiency; //$M$7 => M1 => From Knitting Information
        var M2 = yarn.FinalCount; //M24 = M2

        prodWOElastane += M2 > 0 ? Y18 * W * S * I * O * M1 / M2 * 0.00109361 / 840 / 2.2046 : 0;
    });
    return Math.round(prodWOElastane);

    /*
     =ROUNDUP(($Y$18*W24*S24*$I$7*$O$31*$M$7/M24*0.00109361/840/2.2046)+($Y$18*W25*S25*$I$7*$O$31*$M$7/M25*0.00109361/840/2.2046)+($Y$18*W26*S26*$I$7*$O$31*$M$7/M26*0.00109361/840/2.2046)+($Y$18*W27*S27*$I$7*$O$31*$M$7/M27*0.00109361/840/2.2046),0)
     */
}
function GetCalculatedCapacity(elastanePer, prodWOElastane) {
    var Q7 = getNumberValue(elastanePer);
    var V32 = getNumberValue(prodWOElastane);
    if (Q7 == 0) {
        return V32;
    } else if (Q7 == 100) {
        return 0;
    }
    return V32 * 100 / (100 - Q7);

    /*
     =IF(Q7=0,V32,V32*100/(100-Q7))
     */
}
function getPreFinishingInfo(fmsId, fpChildId, $detail) {
    $tblPreProcessTable = $detail.html('<table id="TblPreDyeingProcess-' + fpChildId + '"></table>').find('table');
    initPreFinishingingProcessTable(fpChildId, $detail);
    axios.get(`/api/finishing-process-production/machine/${fmsId}/${fpChildId}`)
        .then(function (response) {
            $tblPreProcessTable.bootstrapTable('load', response.data.ProcessMachineList);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        });
}
function initPreFinishingingProcessTable(fpChildId, $detail) {
    $tblPreProcessTable.bootstrapTable("destroy");
    $tblPreProcessTable.bootstrapTable({
        showFooter: true,
        checkboxHeader: false,
        columns: [
            {
                field: 'SerialNo',
                isPrimaryKey: true,
                visible: false
            },
            {
                field: "ParamDispalyName",
                title: "Param Name",
            },
            {
                field: "ParamValue",
                title: "Default Value",
            },
            {
                field: "PlanParamValue",
                title: "Plan Value",
            },
            {
                field: "ActulaPlanParamValue",
                title: "Actual Value",
            }
        ]
    });
}

function getPostFinishingInfo(fmsId, dbcFpId, $detail) {
    $tblPostProcessTable = $detail.html('<table id="TblPostDyeingProcess-' + dbcFpId + '"></table>').find('table');
    initPostFinishingProcessTable(dbcFpId, $detail);
    axios.get(`/api/roll-finishing/machine/${fmsId}/${dbcFpId}`)
        .then(function (response) {
            $tblPostProcessTable.bootstrapTable('load', response.data.ProcessMachineList);
        })
        .catch(function (err) {
            toastr.error(err.response.data.Message);
        });
}
function initPostFinishingProcessTable(dbcFpId, $detail) {
    $tblPostProcessTable.bootstrapTable("destroy");
    $tblPostProcessTable.bootstrapTable({
        showFooter: true,
        checkboxHeader: false,
        columns: [
            {
                field: 'SerialNo',
                isPrimaryKey: true,
                visible: false
            },
            {
                field: "ParamDispalyName",
                title: "Param Name",
            },
            {
                field: "ParamValue",
                title: "Default Value",
            },
            {
                field: "PlanParamValue",
                title: "Plan Value",
            },
            {
                field: "ActulaPlanParamValue",
                title: "Actual Value",
            }
        ]
    });
    }
    async function reportCommandClick(e) {
        if (e.commandColumn.buttonOption.type == 'Result') {
            window.open(`/reports/InlinePdfView?ReportName=LabTestReport.rdl&LTReqMasterID=${e.rowData.LTReqMasterID}&BuyerId=${e.rowData.BuyerID}`, '_blank');
        }
        else if (e.commandColumn.buttonOption.type == 'Form') {
            window.open(`/reports/InlinePdfView?ReportName=LabTestTestingRequirementForm.rdl&ReqID=${e.rowData.LTReqMasterID}&BuyerId=${e.rowData.BuyerID}`, '_blank');
        }
    }

    function showReportList() {
        if (reportData != null) {
            $(tblReportId).html("");
            var grid = new ej.grids.Grid({
                dataSource: reportData,
                columns: [
                    { field: 'LTReqMasterID', headerText: 'ID', visible: false },
                    { field: 'BuyerID', visible: false },
                    { field: 'ConceptNo', headerText: 'Concept No' },
                    { field: 'TechnicalName', headerText: 'Technical Name' },
                    { field: 'SubClassName', headerText: 'Machine Type' },
                    {
                        headerText: '', textAlign: 'Center', width: 80, commands: [
                            {
                                title: 'Lab Test Result Report',
                                buttonOption: {
                                    type: 'Result', content: '', cssClass: 'btn btn-xs btn-primary', iconCss: 'fa fa-file-pdf-o'
                                }

                            }
                        ]
                    },
                    {
                        headerText: '', textAlign: 'Center', width: 80, commands: [
                            {
                                title: 'Lab Test Form Report',
                                buttonOption: {
                                    type: 'Form', content: '', cssClass: 'btn btn-xs btn-danger', iconCss: 'fa fa-file-pdf-o'
                                }

                            }
                        ]
                    }
                ],
                commandClick: reportCommandClick,
                autofitColumns: false,
                showDefaultToolbar: false,
                allowFiltering: false,
                allowPaging: false,
                editSettings: {
                    allowAdding: false,
                    allowEditing: false,
                    allowDeleting: false,
                    mode: "Normal",
                    showDeleteConfirmDialog: true
                },
            });

            grid.appendTo(tblReportId);
        }
    }
}) ();