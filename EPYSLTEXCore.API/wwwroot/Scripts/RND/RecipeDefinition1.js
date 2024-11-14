(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, $tblChildFabricInfoId, stblMasterId, tblChildId, $tblDefChildEl;
    var tblChildFabricInfoId, $tblChildFabricInfoEl;
    var $tblChildReceipeEl, tblReceipeChildId;
    var status = statusConstants.PENDING;
    var isAcknowledgePage = false;
    var isApprovePage = false;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var isEditable = true;
    var masterData = null;
    var pageId = "";
    var _recipeChildID = 99999;

    var particularElem;
    var particularObj;
    var rawItemNameElem;
    var rawItemNameObj;


    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        pageId = pageName + "-" + menuId;

        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;

        tblChildFabricInfoId = "#tblChildFabricInfoId" + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;

        $tblDefChildEl = $("#tblDefChild-" + pageId);
        $tblChildFabricInfoId = $("#tblChildFabricInfoId" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        $("#" + pageId).find("#tblMasterId").val(tblMasterId);

        if (isApprovePage) {
            $toolbarEl.find("#btnList").hide();
            $toolbarEl.find("#btnCompleteList").hide();
            $toolbarEl.find("#btnPendingAcknowledgeList").hide();
            // $toolbarEl.find("#btnAcknowledgeList").hide();
            status = statusConstants.PARTIALLY_COMPLETED;
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingApproveList"), $toolbarEl);
        }
        else if (isAcknowledgePage) {
            $toolbarEl.find("#btnList").hide();
            $toolbarEl.find("#btnCompleteList").hide();
            $toolbarEl.find("#btnPendingApproveList").hide();
            $toolbarEl.find("#btnApproveList").hide();
            status = statusConstants.APPROVED;
            toggleActiveToolbarBtn($toolbarEl.find("#btnPendingAcknowledgeList"), $toolbarEl);
        }
        else {
            $toolbarEl.find("#btnPendingApproveList").hide();
            $toolbarEl.find("#btnApproveList").show();
            $toolbarEl.find("#btnPendingAcknowledgeList").hide();
            //$toolbarEl.find("#btnAcknowledgeList").hide();
            toggleActiveToolbarBtn($toolbarEl.find("#btnList"), $toolbarEl);
        }

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
            status = statusConstants.PARTIALLY_COMPLETED;
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingApproveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnApproveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;
            initMasterTable();
        });

        $toolbarEl.find("#btnPendingAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;
            initMasterTable();
        });

        $toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ACKNOWLEDGE;
            initMasterTable();
        });

        $formEl.find("#btnAddItem").on("click", function (e) {
            e.preventDefault();
            addNewItem();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#RecipeID").val();
            axios.post(`/api/rnd-recipe-defination/approve/${id}`)
                .then(function () {
                    toastr.success("Recipe approved successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            var id = $formEl.find("#RecipeID").val();
            axios.post(`/api/rnd-recipe-defination/acknowledge/${id}`)
                .then(function () {
                    toastr.success("Recipe acknowledged successfully.");
                    backToList();
                })
                .catch(showResponseError);
        });

        $formEl.find("#btnCancel").on("click", backToListWithoutFilter);

        $formEl.find("#btnCopyRecipe").on("click", function (e) {
            e.preventDefault();
            var fComp = masterData.RecipeDefinitionItemInfos.filter(x => x.FabricComposition != null && $.trim(x.FabricComposition).length > 0).map(x => x.FabricComposition).join(",");
            //fComp = "'" + fComp.replace(",", "','") + "'";
            var finder = new commonFinder({
                title: "Select Recipe Defination",
                pageId: pageId,
                apiEndPoint: `/api/rnd-recipe-defination/list-by-buyer-compositon-color?dpID=${masterData.DPID}&buyer=${masterData.BuyerID}&fabricComposition=${fComp}&Color=${masterData.ColorID}`,
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "RecipeReqMasterID",
                fields: "RecipeNo,ColorName,ConceptNo,DPName,Buyer",
                headerTexts: "Recipe No,Color Name,Concept No,Dyeing Process,Buyer",
                widths: "50,50,30,40,50",
                onSelect: function (res) {
                    finder.hideModal();
                    axios.get(`/api/rnd-recipe-defination/dyeingInfo-by-recipe/${res.rowData.RecipeReqMasterID}`)
                        .then(function (response) {
                            //debugger;
                            //var childRecipeDInfoID = masterData.Childs[0].RecipeDInfoID;
                            if (response.data.Childs.length > 0) {
                                masterData.Childs = [];
                                //ABC
                                var rows = $tblChildEl.getCurrentViewRecords();
                                for (var i = 0; i < rows.length; i++) {
                                    var rowFiber = response.data.Childs.find(x => x.FiberPart == rows[i].FiberPart);
                                    if (rowFiber != null) {
                                        rows[i].Temperature = rowFiber.Temperature;
                                        rows[i].ProcessTime = rowFiber.ProcessTime;

                                        rowFiber.DefChilds.map(x => {
                                            x.RecipeDInfoID = rows[i].RecipeDInfoID;
                                        })

                                        rows[i].DefChilds = rowFiber.DefChilds;
                                        masterData.Childs.push(rows[i]);
                                    }
                                }
                                initChildTable2(masterData.Childs);
                                //$tblChildEl.bootstrapTable("load", rows);
                                //$tblChildEl.bootstrapTable('hideLoading');
                            }
                        })
                        .catch(function (err) {
                            toastr.error(err.response.data.Message);
                        });
                },
            });
            finder.showModal();
        });
    });

    function addNewItem() {
        var newChildItem = {
            RecipeChildID: getMaxIdForArray(masterData.Childs, "RecipeChildID"),
            EntityState: 4,
            ProcessId: 0,
            RawItemId: 0,
            ItemName: 'Empty',
            ParticularsId: 0,
            Qty: 0,
            UnitID: 0,
            Unit: 'Empty'
        };
        masterData.Childs.push(newChildItem);
        $tblChildEl.bootstrapTable('load', masterData.Childs);
    }

    function initMasterTable() {
        var columns = [
            {
                headerText: '', visible: status == statusConstants.PENDING, width: 25, commands:
                    [
                        { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } },
                        { type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
            },
            {
                headerText: 'Action', width: 25, visible: status == statusConstants.PARTIALLY_COMPLETED, commands:
                    [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } },
                        { type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
            },
            {
                headerText: 'Action', width: 25, visible: status !== statusConstants.PENDING && status !== statusConstants.PARTIALLY_COMPLETED, commands:
                    [
                        { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                    ]
            },
            {
                field: 'Status', headerText: 'Status', width: 24, visible: status == statusConstants.PENDING
            },
            {
                field: 'RecipeStatus', headerText: 'Status', width: 30, visible: status != statusConstants.PENDING
            },
            {
                field: 'RecipeNo', headerText: 'Recipe No', width: 40, visible: status !== statusConstants.PENDING
            },
            {
                field: 'RecipeDate', headerText: 'Recipe Date', width: 20, textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status !== statusConstants.PENDING
            },
            {
                field: 'RecipeReqNo', headerText: 'Recipe Req. No', width: 40
            },
            {
                field: 'ColorName', headerText: 'Color Name', width: 20
            },
            {
                field: 'ColorCode', headerText: 'Color Code', width: 20, visible: status == statusConstants.PENDING
            },
            {
                field: 'LabDipNo', headerText: 'Lab Dip No', width: 20
            },
            {
                field: 'ConceptNo', headerText: 'Concept No', width: 20
            },
            {
                field: 'Buyer', headerText: 'Buyer', width: 20
            },
            {
                field: 'BuyerTeam', headerText: 'Buyer Team', width: 20
            },
            {
                field: 'RequestAckDate', headerText: 'Request Ack Date', width: 20, textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status === statusConstants.PENDING
            },
            {
                field: 'RecipeForName', headerText: 'Recipe For', width: 20, visible: status !== statusConstants.PENDING
            },
            {
                field: 'AcknowledgedDate', headerText: 'Acknowledge Date', width: 20, textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status === statusConstants.ACKNOWLEDGE
            },
            {
                field: 'Remarks', headerText: 'Remarks', width: 20, visible: status !== statusConstants.PENDING
            },
            {
                field: 'DPName', headerText: 'Dyeing Process', width: 20
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        tblMasterId = $("#" + pageId).find("#tblMasterId").val();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/rnd-recipe-defination/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (status === statusConstants.PENDING) {
            if (args.commandColumn.type == 'Add') {
                getNew(args.rowData.RecipeReqMasterID);
            }
            else if (args.commandColumn.type == 'ViewReport') {
                window.open(`/reports/RecipeRequestForm?RecipeReqNo=${args.rowData.RecipeReqNo}`, '_blank');
            }
        }
        else {
            if (args.commandColumn.type == 'Edit') {
                if (args.rowData.RecipeStatus == "Deactive") {
                    toastr.error("You can't edit deactive recipe, try active recipe.");
                    return false;
                }
                getDetails(args.rowData.RecipeID);
            }
            else if (args.commandColumn.type == 'View') {
                getDetails(args.rowData.RecipeID);
            }
            else if (args.commandColumn.type == 'ViewReport') {
                window.open(`/reports/RecipeRequestForm?RecipeReqNo=${args.rowData.RecipeReqNo}`, '_blank');
            }
        }
    }

    function backToListWithoutFilter() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        $formEl.find("#divProcessInfo").fadeOut();
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
        $formEl.find("#RecipeID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }


    function initChildTable2(data) {
        if ($tblChildEl) $tblChildEl.destroy();
        ej.base.enableRipple(true);
        var columnList = [
            { field: 'RecipeChildID', isPrimaryKey: true, visible: false },
            { field: 'RecipeDInfoID', visible: false },
            {
                field: 'ParticularsName', headerText: 'Process',
                valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        particularElem = document.createElement('input');
                        return particularElem;
                    },
                    read: function () {
                        return particularObj.text;
                    },
                    destroy: function () {
                        particularObj.destroy();
                    },
                    write: function (e) {
                        particularObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.ParticularsList,
                            fields: { value: 'id', text: 'text' },
                            change: function (f) {
                                rawItemNameObj.enabled = true;
                                var tempQuery = new ej.data.Query().where('additionalValue', 'equal', particularObj.value);
                                rawItemNameObj.query = tempQuery;
                                rawItemNameObj.text = null;
                                rawItemNameObj.dataBind();

                                e.rowData.ParticularsId = f.itemData.id;
                                e.rowData.ParticularsName = f.itemData.text;
                            },
                            placeholder: 'Select Process',
                            floatLabelType: 'Never'
                        });
                        particularObj.appendTo(particularElem);
                    }
                }
            },
            {
                field: 'RawItemName', headerText: 'Item', displayField: "RawItemName", valueAccessor: ej2GridDisplayFormatterV2, edit: {
                    create: function () {
                        rawItemNameElem = document.createElement('input');
                        return rawItemNameElem;
                    },
                    read: function () {
                        return rawItemNameObj.text;
                    },
                    destroy: function () {
                        rawItemNameObj.destroy();
                    },
                    write: function (e) {
                        rawItemNameObj = new ej.dropdowns.DropDownList({
                            dataSource: masterData.RawItemList,
                            fields: { value: 'id', text: 'text' },
                            enabled: false,
                            placeholder: 'Item',
                            floatLabelType: 'Never',
                            change: function (f) {
                                if (!f.isInteracted || !f.itemData) return false;
                                e.rowData.RawItemID = f.itemData.id;
                                e.rowData.RawItemName = f.itemData.text;
                            }
                        });
                        rawItemNameObj.appendTo(rawItemNameElem);
                    }
                }
            },
            { field: 'IsPercentage', headerText: '%', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 40 },
            {
                field: 'Qty', headerText: 'Qty (gm/ltr)/%', editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 2, min: 1 } }, width: 50
            }
        ];
        columnList.unshift({
            headerText: 'Commands', width: 80, commands: [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
        });
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser'],
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                { field: 'RecipeDInfoID', isPrimaryKey: true, visible: false },
                { field: 'RecipeID', visible: false },
                { field: 'FiberPart', headerText: 'Fiber Part', allowEditing: false },
                { field: 'ColorName', headerText: 'Color Name', allowEditing: false },
                {
                    field: 'Temperature', headerText: 'Temperature', editType: "numericedit",
                    edit: { params: { showSpinButton: false, decimals: 2, min: 1 } }, width: 80
                },
                {
                    field: 'ProcessTime', headerText: 'ProcessTime', editType: "numericedit",
                    edit: { params: { showSpinButton: false, decimals: 2, min: 1 } }, width: 80
                }
            ],
            actionBegin: function (args) {
                if (args.requestType === "add") {

                }
                else if (args.requestType === "save") {

                }
            },
            childGrid: {
                queryString: 'RecipeDInfoID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: columnList,
                commandClick: childCommandClick,
                actionBegin: function (args) {
                    var parentRow = this.parentDetails.parentRowData;
                    if (args.requestType === 'beginEdit') {
                    }
                    else if (args.requestType === "add") {

                    }
                    else if (args.requestType === "save") {
                        var indexF = masterData.Childs.findIndex(x => x.RecipeDInfoID == parentRow.RecipeDInfoID);
                        var defChild = DeepClone(args.data);
                        if (indexF > -1) {
                            var defChildIndex = masterData.Childs[indexF].DefChilds.findIndex(x => x.RecipeChildID == defChild.RecipeChildID);
                            if (typeof defChild.RecipeChildID === "undefined") defChildIndex = -1;
                            if (defChildIndex > -1) {
                                masterData.Childs[indexF].DefChilds[defChildIndex] = defChild;
                                //$tblChildEl.updateRow(indexF, masterData.Childs);
                            } else {
                                defChild.RecipeChildID = _recipeChildID++;
                                defChild.RecipeDInfoID = parentRow.RecipeDInfoID;
                                masterData.Childs[indexF].DefChilds.unshift(defChild);
                            }
                        }
                    }
                    else if (args.action == "edit") {

                    }
                    else if (args.requestType === "delete") {

                    }
                },
                load: loadFirstLChild
            },
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    function loadFirstLChild() {
        var recipeDInfoID = this.parentDetails.parentRowData.RecipeDInfoID;
        var defChilds = masterData.Childs.find(x => x.RecipeDInfoID == recipeDInfoID).DefChilds;
        this.dataSource = defChilds;
    }

    async function childCommandClick(e) {


        /*
        var childData = e.rowData;
        //particularId
        //var particularId = 1106;
        if (e.commandColumn.buttonOption.type == 'mainItem' && childData) {
            childData = e.rowData;
            var particularId = e.rowData.ParticularsId;
            var finder = new commonFinder({
                title: "Select Item",
                pageId: pageId,
                height: 320,
                apiEndPoint: "/api/selectoption/raw-item-by-type-finder/" + particularId,
                fields: "text",
                headerTexts: "Item",
                isMultiselect: false,
                primaryKeyColumn: "id",
                onSelect: function (selectedRecord) {
                    finder.hideModal();
                    var childIndex = masterData.Childs.findIndex(x => x.RecipeDInfoID == childData.RecipeDInfoID);
                    if (childIndex > -1) {
                        var defChildIndex = masterData.Childs[childIndex].DefChilds.findIndex(x => x.RecipeChildID == childData.RecipeChildID);
                        if (defChildIndex > -1) {
                            masterData.Childs[childIndex].DefChilds[defChildIndex].RawItemId = selectedRecord.rowData.id;
                            masterData.Childs[childIndex].DefChilds[defChildIndex].RawItemName = selectedRecord.rowData.text;
                            var index = $tblChildEl.getRowIndexByPrimaryKey(childData.RecipeDInfoID);
                            $tblChildEl.updateRow(index, masterData.Childs[childIndex]);
                        }
                    }
                }
            });
            finder.showModal();
        }
        */
    }


    function initChildTable1(data) {
        if ($tblChildFabricInfoEl) $tblChildFabricInfoEl.destroy();
        ej.base.enableRipple(true);
        $tblChildFabricInfoEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            toolbar: ['ColumnChooser'],
            columns: [
                { field: 'RecipeID', isPrimaryKey: true, visible: false },
                { field: 'SubGroup', headerText: 'Sub Group', allowEditing: false },
                { field: 'TechnicalName', headerText: 'Technical Name', allowEditing: false },
                { field: 'FabricComposition', headerText: 'Fabric Composition', allowEditing: false },
                { field: 'RecipeOn', headerText: 'Recipe On?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
                { field: 'FabricGsm', headerText: 'Fabric GSM', allowEditing: false },
                { field: 'KnittingType', headerText: 'Knitting Type', allowEditing: false }
            ],
        });
        $tblChildFabricInfoEl.refreshColumns;
        $tblChildFabricInfoEl.appendTo(tblChildFabricInfoId);
    }

    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    function getNew(id) {
        var url = "/api/rnd-recipe-defination/new/" + id;
        axios.get(url)
            .then(function (response) {

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.RecipeDate = formatDateToDefault(masterData.RecipeDate);
                isEditable = true;
                $formEl.find("#btnSave").show();
                $formEl.find("#btnAddItem").show();
                $formEl.find("#RecipeFor").prop("disabled", false);
                /*  $formEl.find("#BatchWeightKG,#Temperature,#ProcessTime").prop("readonly", false);*/
                $formEl.find("#Remarks").prop("disabled", false);
                $formEl.find("#btnApprove, #btnAcknowledge").hide();
                masterData.RecipeFor = 1102; //Production = 1102
                setFormData($formEl, masterData);
                if (masterData.DPName == 'Cross') {
                    $formEl.find("#divProcessInfo").fadeIn();
                }
                else {
                    $formEl.find("#divProcessInfo").fadeOut();
                }
                //For Fabric Info
                initChildTable1(masterData.RecipeDefinitionItemInfos);
                //initChildFabricInfoTable(masterData.RecipeDefinitionItemInfos);
                //$tblChildFabricInfoId.bootstrapTable("load", masterData.RecipeDefinitionItemInfos);
                //$tblChildFabricInfoId.bootstrapTable('hideLoading');
                //loadFabricInfoData();

                //for Child Info
                var recipeChildID = 1;
                masterData.Childs.map(x => {
                    x.RecipeChildID = recipeChildID++;
                });
                initChildTable2(masterData.Childs);
                //initChildTable();
                //$tblChildEl.bootstrapTable("load", masterData.Childs);
                //$tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/rnd-recipe-defination/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.JobCardDate = formatDateToDefault(masterData.JobCardDate);
                masterData.RecipeDate = formatDateToDefault(masterData.RecipeDate);
                isEditable = true;
                $formEl.find("#btnSave,#btnAddItem").show();
                /* $formEl.find("#Temperature,#ProcessTime,#Remarks").prop("readonly", false);*/
                $formEl.find("#btnApprove, #btnAcknowledge").hide();
                if (masterData.IsApproved) {
                    isEditable = false;
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnAddItem").hide();
                    $formEl.find("#RecipeFor").prop("disabled", true);
                    /*  $formEl.find("#Temperature,#ProcessTime").prop("readonly", true);*/
                    $formEl.find("#Remarks").prop("readonly", true);
                }
                if (isApprovePage && !masterData.IsApproved) {
                    isEditable = false;
                    $formEl.find("#RecipeFor").prop("disabled", true);
                    /*      $formEl.find("#Temperature,#ProcessTime").prop("readonly", true);*/
                    $formEl.find("#Remarks").prop("readonly", true);
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnAddItem").hide();
                    $formEl.find("#btnApprove").show();
                }
                else if (isApprovePage && masterData.IsApproved) {
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnAddItem").hide();
                    $formEl.find("#btnApprove").hide();
                }

                if (masterData.Acknowledged) {
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnAddItem").hide();
                    $formEl.find("#btnApprove").hide();
                }

                if (isAcknowledgePage && !masterData.Acknowledged) {
                    $formEl.find("#btnAcknowledge").show();
                    $formEl.find("#btnApprove").hide();
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnAddItem").hide();
                }
                else if (isAcknowledgePage && masterData.Acknowledged) {
                    $formEl.find("#btnApprove").hide();
                    $formEl.find("#btnAcknowledge").hide();
                    $formEl.find("#btnSave").hide();
                    $formEl.find("#btnAddItem").hide();
                }
                masterData.RecipeFor = 1102; //Production = 1102
                setFormData($formEl, masterData);
                if (masterData.DPName == 'Cross') {
                    $formEl.find("#divProcessInfo").fadeIn();
                }
                else {
                    $formEl.find("#divProcessInfo").fadeOut();
                }
                //22-Nov-22-Anis

                //initChildTable();
                //$tblChildEl.bootstrapTable("load", masterData.Childs);
                //$tblChildEl.bootstrapTable('hideLoading');
                //initChildFabricInfoTable();
                initChildTable1(masterData.RecipeDefinitionItemInfos);
                initChildTable2(masterData.Childs);
                //End-22-Nov-22


                //$tblChildFabricInfoId.bootstrapTable("load", masterData.RecipeDefinitionItemInfos);
                //$tblChildFabricInfoId.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        if (isValidChildForm(data)) return;
        if (masterData.DPName != "Wash" && masterData.Childs.length == 0) {
            toastr.error("At least one recipe required!", "Error");
            return;
        }

        if ($formEl.find("#ProdComplete").is(':checked') == true)
            data.ProdComplete = true;
        var childs = [];
        //debugger;
        masterData.Childs.forEach(function (child) {
            child.DefChilds.forEach(function (defChild) {
                defChild.RecipeDInfoID = child.RecipeDInfoID;
                defChild.Temperature = child.Temperature;
                defChild.ProcessTime = child.ProcessTime;
                if (defChild.RecipeChildID > 0) {
                    childs.push(defChild);
                }
            });
        });
        data["Childs"] = childs;
        data["RecipeDefinitionItemInfos"] = masterData.RecipeDefinitionItemInfos;
        if (!checkValidation(data)) {
            axios.post("/api/rnd-recipe-defination/save", data)
                .then(function () {
                    toastr.success("Saved successfully.");
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }


    }
    function checkValidation(data) {
        if (data.Childs.length == 0) {
            toastr.error("Add item(s).");
            return true;
        }
        var hasError = false;
        for (var i = 0; i < data.Childs.length; i++) {
            var child = data.Childs[i];
            if (!child.Temperature) {
                toastr.error("Give temperature.");
                hasError = true;
                break;
            }
            if (!child.ProcessTime) {
                toastr.error("Give process time.");
                hasError = true;
                break;
            }
            if (child.ParticularsId == 0) {
                toastr.error("Select particular.");
                hasError = true;
                break;
            }
            //RawItemName
            //if (child.ParticularsId != 1107 && (child.ItemName == "Empty" || child.ItemName.length == 0)) {
            //    toastr.error("Select item for chemical and dyes.");
            //    hasError = true;
            //    break;
            //}
            if (child.ParticularsId != 1107 && (typeof child.RawItemName === "undefined" || child.RawItemName == null || child.RawItemName == "Empty" || child.RawItemName.length == 0)) {
                toastr.error("Select item for chemical and dyes.");
                hasError = true;
                break;
            }

            if (!child.Qty) {
                toastr.error("Give Qty (gm/ltr)/%.");
                hasError = true;
                break;
            }
        }
        return hasError;
    }

    function isValidChildForm(data) {
        var isValidItemInfo = false;

        $.each(data["Childs"], function (i, el) {
            if (el.ParticularsId != "1107") {
                if (el.RawItemId == "" || el.RawItemId == null) {
                    toastr.error("Please select item.");
                    isValidItemInfo = true;
                }
                if (el.Qty == "" || el.Qty == null || el.Qty <= 0) {
                    toastr.error("Qty must not be empty!");
                    isValidItemInfo = true;
                }
            }
        });
        return isValidItemInfo;
    }
})();