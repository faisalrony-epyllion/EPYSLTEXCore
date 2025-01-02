(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl,
        $tblMasterEl, $tblChildEl, $formEl, $tblChildFabricInfoId,
        tblMasterId, $tblDefChildEl;
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
    var masterData;


    var pageId = "";

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
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $tblDefChildEl = $("#tblDefChild-" + pageId);
        $tblChildFabricInfoId = $("#tblChildFabricInfoId" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        //isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        //isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        var menuParam = $("#" + pageId).find("#txtMenuParam").val();
        if (menuParam == "Ack") isAcknowledgePage = true;
        else if (menuParam == "A") isApprovePage = true;

       
        if (isAcknowledgePage) {
            $formEl.find(".divIsArchive").show();
        } else {
            $formEl.find(".divIsArchive").hide();
        }

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

        $formEl.find("#btnAcknowledge,#btnUpdate").click(function (e) {

            e.preventDefault();
            var isArchived = $("#IsArchive").is(':checked') ? 1 : 0;
            var id = $formEl.find("#RecipeID").val();
            axios.post(`/api/rnd-recipe-defination/acknowledge/${id}/${isArchived}`)
                .then(function () {
                    if (e.target.id == "btnAcknowledge") {
                        toastr.success("Recipe acknowledged successfully.");
                    }
                    else if (e.target.id == "btnUpdate") {
                        toastr.success("Recipe acknowledged successfully Updated.");
                    }
                    backToList();
                })
                .catch(showResponseError);
        });
        //$formEl.find("#btnUpdate").click(function (e) {
        //    alert(e.target.id);
        //    e.preventDefault();
        //    var isArchived = $("#IsArchive").is(':checked') ? 1 : 0;
        //    var id = $formEl.find("#RecipeID").val();
        //    axios.post(`/api/rnd-recipe-defination/acknowledge/${id}/${isArchived}`)
        //        .then(function () {
        //            toastr.success("Recipe acknowledged successfully Updated.");
        //            backToList();
        //        })
        //        .catch(showResponseError);
        //});


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
                            if (response.data.Childs.length > 0) {
                                masterData.Childs = [];
                                var rows = $tblChildEl.bootstrapTable('getData');

                                for (var i = 0; i < rows.length; i++) {
                                    var rowFiber = response.data.Childs.find(x => x.FiberPart == rows[i].FiberPart);
                                    if (rowFiber != null) {
                                        rows[i].ParentRecipeDInfoID = rows[i].RecipeDInfoID;
                                        rows[i].Temperature = rowFiber.Temperature;
                                        rows[i].ProcessTime = rowFiber.ProcessTime;
                                        rows[i].DefChilds = rowFiber.DefChilds;
                                        masterData.Childs.push(rows[i]);
                                    }
                                }
                                $tblChildEl.bootstrapTable("load", rows);
                                $tblChildEl.bootstrapTable('hideLoading');
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
                        { type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }
                    ]
            },
            {
                headerText: 'Action', width: 25, visible: status == statusConstants.PARTIALLY_COMPLETED, commands:
                    [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } },
                        { type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }
                    ]
            },
            {
                headerText: 'Action', width: 25, visible: status !== statusConstants.PENDING && status !== statusConstants.PARTIALLY_COMPLETED, commands:
                    [
                        { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                        { type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fas fa-file-pdf' } }
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
                if (args.rowData.DPName == "Wash") {
                    $formEl.find("#tblRecipeInfoArea").fadeOut();
                }
                else {
                    $formEl.find("#tblRecipeInfoArea").fadeIn();
                }
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

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            uniqueId: 'RecipeChildID',
            editable: isEditable,
            detailView: true,
            checkboxHeader: false,
            columns: [
                //{
                //    title: "Actions",
                //    align: "center",
                //    formatter: function (value, row, index, field) {
                //        return [
                //            '<span class="btn-group">',
                //            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete">',
                //            '<i class="fa fa-remove"></i>',
                //            '</a>',
                //            '</span>'
                //        ].join('');
                //    },
                //    events: {
                //        'click .remove': function (e, value, row, index) {
                //            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                //                if (yes) {
                //                    masterData.Childs.splice(index, 1);
                //                    $tblChildEl.bootstrapTable('load', masterData.Childs);
                //                }
                //            });
                //        }
                //    }
                //},

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
                    align: 'center',
                    editable: {
                        type: "number",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    }
                },
                {
                    field: "ProcessTime",
                    title: "ProcessTime",
                    align: 'center',
                    editable: {
                        type: "number",
                        showbuttons: false,
                        tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                    }
                }
            ],
            onEditableSave: function (field, row, oldValue, $el) {

                if (field === "ParticularsId") {
                    var selectedData = masterData.ParticularsList.find(function (el) { return el.id == row.ParticularsId });
                    if (selectedData) {
                        row.ParticularsName = selectedData.text;
                        $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.RecipeChildID, row: row });
                    }
                }
                // Update the masterData object with the changed row data

                if (field === "Temperature") {
                    masterData.Childs.find(x => x.RecipeDInfoID == row.RecipeDInfoID).Temperature = row.Temperature;
                }
                if (field === "ProcessTime") {
                    masterData.Childs.find(x => x.RecipeDInfoID == row.RecipeDInfoID).ProcessTime = row.ProcessTime;
                }
                //var index = masterData.Childs.findIndex(function (item) { return item.RecipeDInfoID === row.RecipeDInfoID; });
                ////var index = masterData.DefChildList.findIndex(function (item) { return item.id === row.id; });
                //if (index !== -1) {
                //    masterData.Childs[index] = row;
                //}
                //else {
                //    //masterData.Childs.push(row);
                //}
                ////masterData = JSON.parse(localStorage.getItem('masterData'));
                if (localStorage.getItem('masterData') != null) {
                    localStorage.setItem('masterData', JSON.stringify(masterData));
                }
            },
            onCheck: function (row, el) {
                row.UnitID = 0;
                row.Unit = "Empty";
                $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.RecipeChildID, row: row });
            },
            onUncheck: function (row, el) {
                row.UnitID = row.UnitID ? row.UnitID : 0;
                row.Unit = (row.Unit == "") ? "Empty" : row.Unit;
                $tblChildEl.bootstrapTable('updateByUniqueId', { id: row.RecipeChildID, row: row });
            },
            onExpandRow: function (index, row, $detail) {
                populateChild(row.RecipeDInfoID, $detail);
            }
        });
    }

    function populateChild(childId, $detail) {

        localStorage.setItem('childId', JSON.stringify(childId));
        $tblDefChildEl = $detail.html('<table id="TblChildDef-' + pageId + '-' + childId + '"></table>').find('table');
        var childIndex = getIndexFromArray(masterData.Childs, "RecipeDInfoID", childId)
        initDefChildTable(childId, $detail);
        var cnt = (masterData.Childs[childIndex].DefChilds == null) ? 0 : masterData.Childs[childIndex].DefChilds.length;
        if (cnt > 0)
            $tblDefChildEl.bootstrapTable('load', masterData.Childs[childIndex].DefChilds.filter(function (item) {
                return item.EntityState != 8
            }));
    }

    function initDefChildTable(childId, $detail) {
        var columnList = [
            {
                field: "ParticularsId",
                title: "Particulars",
                align: 'center',
                visible: isEditable,
                editable: {
                    type: 'select2',
                    title: 'Select Process',
                    inputclass: 'input-sm',
                    showbuttons: false,
                    source: masterData.ParticularsList,
                    select2: { width: 200, placeholder: 'Select Particulars Name', allowClear: true }
                }
            },
            {
                field: "RawItemId",
                title: "Item",
                align: 'center',
                visible: isEditable,
                formatter: function (value, row, index, field) {
                    if (row.ItemName == '') { row.ItemName = 'Empty'; };
                    return ['<span class="btn-group">',
                        '<a href="javascript:void(0)" class="editable-link edit">' + row.ItemName + '</a>',
                        '</span>'].join(' ');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        e.preventDefault();
                        if (!row.ParticularsId) return toastr.error("Particulars is not selected");
                        getRAWItem(row.ParticularsId, row);
                    },
                }
            },
            {
                field: "ParticularsName",
                title: "Particulars",
                align: 'center',
                visible: !isEditable
            },
            {
                field: "RawItemName",
                title: "Item",
                align: 'center',
                visible: !isEditable
            },
            {
                field: "IsPercentage",
                title: "%",
                checkbox: true,
                showSelectTitle: true,
                visible: isEditable,
            },
            {
                field: "IsPercentageText",
                title: "%",
                visible: !isEditable,
            },
            {
                field: "Qty",
                title: "Qty (gm/ltr)/%",
                align: 'center',
                editable: {
                    type: "text",
                    showbuttons: false,
                    tpl: '<input type="number" step=".00000001" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                }
            }
            //,
            //{
            //    field: "UnitID",
            //    title: "UOM",
            //    align: 'center',
            //    visible: isEditable,
            //    formatter: function (value, row, index, field) {
            //        var template = row.IsPercentage ? "" : '<a href="javascript:void(0)" class="editable-link edit">' + row.Unit + '</a>';
            //        return `<span>${template}</span>`;
            //    },
            //    events: {
            //        'click .edit': function (e, value, row, index) {
            //            e.preventDefault();
            //            if (!row.ParticularsId) return toastr.error("Particulars is not selected");
            //            getRAWUnit(row.ParticularsId, row);
            //        },
            //    }
            //},
            //{
            //    field: "Unit",
            //    title: "UOM",
            //    align: 'center',
            //    visible: !isEditable
            //}
        ];

        if (status != statusConstants.ACKNOWLEDGE && status != statusConstants.APPROVED) {
            columnList.unshift(
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
                            '<button class="btn btn-success btn-xs add" onclick="AddDefChild(' + childId + ', \'' + pageId + '\',' + isAcknowledgePage + ',' + isApprovePage + ')" title="Add Item">',
                            '<i class="fa fa-plus"></i>',
                            ' Add',
                            '</button>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, rowMasterChild, indexOfChild) {
                            var indexOfMasterChild = masterData.Childs.findIndex(x => x.RecipeDInfoID == rowMasterChild.RecipeDInfoID);
                            if (indexOfMasterChild > -1 && indexOfChild > -1) {
                                masterData.Childs[indexOfMasterChild].DefChilds.splice(indexOfChild, 1);
                                $tblDefChildEl = $("#TblChildDef-" + pageId + "-" + rowMasterChild.RecipeDInfoID);
                                $tblDefChildEl.bootstrapTable('load', masterData.Childs[indexOfMasterChild].DefChilds);
                                localStorage.setItem('masterData', JSON.stringify(masterData));
                            }
                        }
                    }
                },
            );
        }
        $tblDefChildEl.bootstrapTable("destroy");
        $tblDefChildEl.bootstrapTable({
            showFooter: true,
            checkboxHeader: false,
            columns: columnList,
            onEditableSave: function (field, row, oldValue, $el) {


                if (field == "ParticularsId") {
                    var selectedData = masterData.ParticularsList.find(function (el) { return el.id == row.ParticularsId });
                    if (selectedData) {
                        var ParticularsName = selectedData.text;
                        if (ParticularsName == 'Dyes') {
                            row.IsPercentage = true;
                        }
                        $tblDefChildEl.bootstrapTable('updateByUniqueId', { id: row.ParticularsId, row: row });
                    }
                }

                // Update the masterData object with the changed row data
                var childId = JSON.parse(localStorage.getItem('childId'));
                ////if (!childId) {
                ////    localStorage.setItem('childId', row.ParentRecipeDInfoID);
                ////    childId = JSON.parse(localStorage.getItem('childId'));
                ////}
                var index = masterData.Childs.find(x => x.RecipeDInfoID == childId).DefChilds.findIndex(function (item) { return item.RecipeChildID === row.RecipeChildID; });
                //var index = masterData.DefChildList.findIndex(function (item) { return item.id === row.id; });
                if (index !== -1) {
                    masterData.Childs.find(x => x.RecipeDInfoID == childId).DefChilds[index] = row;
                }
                else {
                    masterData.Childs.find(x => x.RecipeDInfoID == childId).DefChilds.push(row);
                }
                //masterData = JSON.parse(localStorage.getItem('masterData'));
                if (localStorage.getItem('masterData') != null) {
                    localStorage.setItem('masterData', JSON.stringify(masterData));
                }
            },

        });
    }

    function resetLocalStorage() {
        localStorage.setItem('childId', null);
        localStorage.setItem('masterData', null);
    }

    window.AddDefChild = function (childId, pId, isAckPage, isAppPage) {

        localStorage.setItem('childId', JSON.stringify(childId));
        $tblDefChildEl = $("#TblChildDef-" + pId + "-" + childId);
        //$tblDefChildEl = $("#TblChildDef-RecipeDefinition-836-" + childId);

        //var cntChildDef = 0;
        //if (masterData != null) {
        //    cntChildDef = masterData.Childs.find(x => x.RecipeDInfoID == childId).DefChilds.length;
        //}
        //if (cntChildDef == 0 && isAcknowledgePage==false && isApprovePage==false) {
        //    masterData = JSON.parse(localStorage.getItem('masterData'));
        //}

        //if (localStorage.getItem('masterData') != null) {
        if (isAckPage == false && isAppPage == false) {
            masterData = JSON.parse(localStorage.getItem('masterData'));
        }
        var childs = masterData.Childs;
        var ind = getIndexFromArray(childs, "RecipeDInfoID", childId);
        if (ind > -1) {
            var newChildItem = {
                RecipeChildID: getMaxIdForArray(childs[ind].DefChilds, "RecipeChildID"),
                RecipeDInfoID: childs[ind].RecipeDInfoID,
                EntityState: 4,
                ProcessId: 0,
                RawItemId: 0,
                ItemName: 'Empty',
                ParticularsId: 0,
                Qty: 0,
                UnitID: 0,
                Unit: 'Empty'
            };
            newChildItem.RecipeChildID = newChildItem.RecipeChildID + 50000;
            childs[ind].DefChilds.push(newChildItem);
            $tblDefChildEl.bootstrapTable('load', childs[ind].DefChilds);

            if (isAckPage == false && isAppPage == false) {
                localStorage.setItem('masterData', JSON.stringify(masterData));
            }
        }

    }

    function getRAWItem(particularId, rowData) {
        var url = "/api/selectoption/raw-item-by-type/" + particularId;
        axios.get(url)
            .then(function (response) {
                showBootboxSelect2Dialog("Select Item", "RawItemId", "Select Item", response.data,
                    function (data) {
                        if (!data) return toastr.warning("You didn't selected any Item.");
                        rowData.RawItemId = (data.id == "") ? 0 : data.id;
                        rowData.ItemName = data.text;
                        $tblDefChildEl.bootstrapTable('updateByUniqueId', { id: rowData.RecipeChildID, row: rowData });
                    }
                    , rowData.RawItemId
                )
            })
            .catch(showResponseError);
    }

    function getRAWUnit(particularId, rowData) {
        var url = "/api/selectoption/raw-unit-by-type/" + particularId;
        axios.get(url)
            .then(function (response) {
                showBootboxSelect2Dialog("Select Unit", "UnitID", "Select Unit", response.data,
                    function (data) {
                        if (!data) return toastr.warning("You didn't selected any Item.");
                        rowData.UnitID = (data.id == "") ? 0 : data.id;
                        rowData.Unit = data.text;
                        $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.RecipeChildID, row: rowData });
                    }
                    , rowData.UnitID
                )
            })
            .catch(showResponseError);
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

    function initChildFabricInfoTable() {
        $tblChildFabricInfoId.bootstrapTable("destroy");
        $tblChildFabricInfoId.bootstrapTable({
            uniqueId: 'RecipeItemInfoID',
            editable: isEditable,
            checkboxHeader: false,
            columns: [
                {
                    field: "SubGroup",
                    title: "Sub Group",
                    align: 'left'
                },
                {
                    field: "TechnicalName",
                    title: "Technical Name",
                    align: 'left'
                },
                {
                    field: "FabricComposition",
                    title: "Fabric Composition",
                    align: 'left'
                },
                {
                    field: "RecipeOn",
                    title: "Recipe On?",
                    checkboxEnabled: false,
                    checkbox: true,
                    showSelectTitle: true
                },
                {
                    field: "FabricGsm",
                    title: "Fabric GSM",
                    align: 'center'

                },
                {
                    field: "KnittingType",
                    title: "Knitting Type",
                    align: 'left'
                }
            ]
        });
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    function getNew(id) {

        var url = "/api/rnd-recipe-defination/new/" + id;
        axios.get(url)
            .then(function (response) {

                resetLocalStorage();

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

                // Set masterData in localStorage
                localStorage.setItem('masterData', JSON.stringify(masterData));

                setFormData($formEl, masterData);
                if (masterData.DPName == 'Cross') {
                    $formEl.find("#divProcessInfo").fadeIn();
                }
                else {
                    $formEl.find("#divProcessInfo").fadeOut();
                }

                initChildTable();
                $tblChildEl.bootstrapTable("load", masterData.Childs);
                $tblChildEl.bootstrapTable('hideLoading');

                initChildFabricInfoTable();
                $tblChildFabricInfoId.bootstrapTable("load", masterData.RecipeDefinitionItemInfos);
                $tblChildFabricInfoId.bootstrapTable('hideLoading');
                //loadFabricInfoData();
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

                resetLocalStorage();

                masterData = response.data;
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);
                masterData.JobCardDate = formatDateToDefault(masterData.JobCardDate);
                masterData.RecipeDate = formatDateToDefault(masterData.RecipeDate);
                isEditable = true;
                $formEl.find("#btnSave,#btnAddItem").show();
                /* $formEl.find("#Temperature,#ProcessTime,#Remarks").prop("readonly", false);*/
                $formEl.find("#btnApprove, #btnAcknowledge,#btnUpdate").hide();
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
                    $formEl.find("#btnUpdate").show();
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

                setFormData($formEl, masterData);
                if (masterData.DPName == 'Cross') {
                    $formEl.find("#divProcessInfo").fadeIn();
                }
                else {
                    $formEl.find("#divProcessInfo").fadeOut();
                }
                initChildTable();
                $tblChildEl.bootstrapTable("load", masterData.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
                initChildFabricInfoTable();
                $tblChildFabricInfoId.bootstrapTable("load", masterData.RecipeDefinitionItemInfos);
                $tblChildFabricInfoId.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = formDataToJson($formEl.serializeArray());
        if (isValidChildForm(data)) return;

        isAcknowledgePage = isAcknowledgePage;
        isApprovePage = isApprovePage;
        if (masterData == null && localStorage.getItem('masterData') != null) {
            masterData = JSON.parse(localStorage.getItem('masterData'));
        }

        if (masterData.DPName != "Wash" && masterData.Childs.length == 0) {
            toastr.error("At least one recipe required!", "Error");
            return;
        }

        if ($formEl.find("#ProdComplete").is(':checked') == true)
            data.ProdComplete = true;
        var childs = [];


        masterData.Childs.forEach(function (child) {
            child.DefChilds.forEach(function (defChild) {
                defChild.RecipeDInfoID = child.RecipeDInfoID;
                defChild.Temperature = child.Temperature;
                defChild.ProcessTime = child.ProcessTime;
                childs.push(defChild);
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
        if (data.DPName != "Wash") {
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
                if (child.ParticularsId != 1107 && (child.ItemName == "Empty" || child.ItemName.length == 0)) {
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