(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl, tblMasterId, $tblDefDyeingInfoEl, pageId;
    var tableParams = {
        offset: 0,
        limit: 10,
        filter: '',
        sort: '',
        order: ''
    }

    var isAcknowledgePage = false;
    var status;
    var masterData = {};
    var isRemovable = false;
    var allFiberParts = [];
    var selectedMasterItems = [];
    var isRework = false;
    var recipeCopy = false;
    var ydRecipeReqMasterID = 0;
    var ydDBatchID = 0;
    var ydRecipeID = 0;
    var validationConstraints = {
        //DPProcessInfo: {
        //    presence: true
        //},
    }

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
        $tblDefDyeingInfoEl = $("#tblDefDyeingInfoId" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $tblChildFabricId = $("#tblChildFabricId" + pageId);
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());

        if (isAcknowledgePage) {
            $toolbarEl.find("#btnPending").hide();
            $toolbarEl.find("#btnCreate").hide();
            status = statusConstants.COMPLETED;
            toggleActiveToolbarBtn($toolbarEl.find("#btnRequestForRecipie"), $toolbarEl);
        } else {
            $toolbarEl.find("#btnPending").show();
            $toolbarEl.find("#btnCreate").show();
            status = statusConstants.PENDING;
            toggleActiveToolbarBtn($toolbarEl.find("#btnPending"), $toolbarEl);
        }

        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
        });

        $toolbarEl.find("#btnRequestForRecipie").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $toolbarEl.find("#btnRequestForAcknowledge").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ACKNOWLEDGE;

            initMasterTable();
        });

        $toolbarEl.find("#btnRequestForUnAcknowledge").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.UN_ACKNOWLEDGE;

            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            //
            e.preventDefault();
            save(false);
        });
        $formEl.find("#btnSaveNMail").click(function (e) {
            //
            e.preventDefault();
            save(true);
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            acknowledge(null, true);
        });
        $formEl.find("#btnUnAcknowledge").click(function (e) {
            e.preventDefault();


            bootbox.prompt("Enter your UnAcknowledge reason:", function (result) {
                if (!result) {
                    return toastr.error("UnAcknowledge reason is required.");
                }
                acknowledge(result, false);
            });



        });

        $formEl.find("#btnRevise").click(function (e) {
            //
            e.preventDefault();
            revise(false);
        });


        $formEl.find("#btnCancel").on("click", backToListWithoutFilter);
        $toolbarEl.find("#btnCreate").on("click", createNew);

        $formEl.find('#DPID').on('select2:select', function (e) {

            var dpName = e.params.data.text;
            var recipeOnList = masterData.YDRecipeRequestChilds.filter(x => x.RecipeOn == true);
            //if ( recipeOnList.length == 0) {
            if (dpName != "Wash" && recipeOnList.length == 0) {
                $formEl.find("#divProcessInfo").fadeOut();
                $formEl.find('#DPID').val(0).trigger('change');
                $tblDefDyeingInfoEl.bootstrapTable('load', []);
                return toastr.error("Select recipe on item(s)");
            }
            masterData.DPName = dpName;
            if (dpName == 'Cross') {
                $formEl.find("#divProcessInfo").fadeIn();
            }
            else {
                $formEl.find("#divProcessInfo").fadeOut();
            }
            masterData.YDRecipeDefinitionDyeingInfos = [];
            if (dpName == 'Single Part' || dpName == 'Overdye') {

                $formEl.find("#tblDefDyeingInfoArea").fadeIn();
                //for collar & cuff
                if (recipeOnList[0].SubGroup != "Fabric") {
                    if (masterData.Composition != null) {
                        var fiberList = [];
                        masterData.FiberPartList.forEach(function (fiber) {
                            if (masterData.Composition.toLowerCase().indexOf(' ' + fiber.text.toLowerCase()) >= 0) {
                                fiberList.push(fiber);
                            }
                        });

                        masterData.FiberPartList = fiberList;
                        masterData.FiberPartListCC.map(cc => {
                            var index = masterData.FiberPartList.findIndex(x => x.text.toLowerCase() == cc.text.toLowerCase());
                            if (index < 0) {
                                masterData.FiberPartList.push(cc);
                            }
                        });
                    }
                    //End collar & Cuff
                }
                else {

                    masterData.FiberPartList = getFibarPartList(recipeOnList);
                }


                var newChildItem = {
                    YDRecipeDInfoID: getMaxIdForArray(masterData.YDRecipeDefinitionDyeingInfos, "YDRecipeDInfoID"),
                    CCColorID: masterData.CCColorID,
                    YDRecipeID: 0,
                    FiberPartID: masterData.FiberPartList.length == 1 ? masterData.FiberPartList[0].id : 0,
                    FiberPart: masterData.FiberPartList.length == 1 ? masterData.FiberPartList[0].text : 'Empty',
                    ColorID: masterData.ColorID,
                    ColorName: masterData.ColorName,
                    ColorCode: masterData.ColorCode
                };
                masterData.YDRecipeDefinitionDyeingInfos.push(newChildItem);
                isRemovable = false;
                initDefDyeingInfoTable();
                $tblDefDyeingInfoEl.bootstrapTable('load', masterData.YDRecipeDefinitionDyeingInfos);
            }
            else if (dpName == 'Wash') {
                for (var i = 0; i < masterData.YDRecipeRequestChilds.length; i++) {
                    if (masterData.YDRecipeRequestChilds[i].RecipeOn === false) {
                        masterData.YDRecipeRequestChilds[i].RecipeOn = true;
                    }
                }

                $tblChildFabricId.bootstrapTable("load", masterData.YDRecipeRequestChilds);
                $formEl.find("#tblDefDyeingInfoArea").fadeOut();
                $tblDefDyeingInfoEl.bootstrapTable("destroy");
            }
            else {
                $formEl.find("#tblDefDyeingInfoArea").fadeIn();
                //masterData.FiberPartList = getFibarPartList(recipeOnList);

                if (recipeOnList[0].SubGroup != "Fabric") {
                    if (masterData.Composition != null) {
                        var fiberList = [];
                        masterData.FiberPartList.forEach(function (fiber) {
                            if (masterData.Composition.toLowerCase().indexOf(' ' + fiber.text.toLowerCase()) >= 0) {
                                fiberList.push(fiber);
                            }
                        });

                        masterData.FiberPartList = fiberList;
                        masterData.FiberPartListCC.map(cc => {
                            var index = masterData.FiberPartList.findIndex(x => x.text.toLowerCase() == cc.text.toLowerCase());
                            if (index < 0) {
                                masterData.FiberPartList.push(cc);
                            }
                        });
                    }
                    //End collar & Cuff
                }
                else {

                    masterData.FiberPartList = getFibarPartList(recipeOnList);
                }

                masterData.FiberPartList.map(fiber => {
                    var newChildItem = {
                        YDRecipeDInfoID: getMaxIdForArray(masterData.YDRecipeDefinitionDyeingInfos, "YDRecipeDInfoID"),
                        CCColorID: masterData.CCColorID,
                        YDRecipeID: 0,
                        FiberPartID: fiber.id,
                        FiberPart: fiber.text,
                        ColorID: masterData.ColorID,
                        ColorName: masterData.ColorName,
                        ColorCode: masterData.ColorCode
                    };
                    masterData.YDRecipeDefinitionDyeingInfos.push(newChildItem);
                });

                /*
                masterData.FiberPartList.forEach(function (fiber) {
                    if (masterData.Composition.toLowerCase().indexOf(' ' + fiber.text.toLowerCase()) >= 0) {
                        var newChildItem = {
                            YDRecipeDInfoID: getMaxIdForArray(masterData.YDRecipeDefinitionDyeingInfos, "YDRecipeDInfoID"),
                            CCColorID: masterData.CCColorID,
                            YDRecipeID: 0,
                            FiberPartID: fiber.id,
                            FiberPart: fiber.text,
                            ColorID: masterData.ColorID,
                            ColorName: masterData.ColorName,
                            ColorCode: masterData.ColorCode
                        };
                        masterData.YDRecipeDefinitionDyeingInfos.push(newChildItem);
                    }
                });
                */

                isRemovable = true;
                initDefDyeingInfoTable();
                $tblDefDyeingInfoEl.bootstrapTable('load', masterData.YDRecipeDefinitionDyeingInfos);
            }
        });

        $formEl.find("#btnAddItem").on("click", function (e) {
            e.preventDefault();
            if ($formEl.find('#DPID').val() == null) {
                toastr.error("Please select Dyeing Process!");
                return;
            }

            if ((masterData.DPName == 'Single Part' || masterData.DPName == 'Overdye') && masterData.YDRecipeDefinitionDyeingInfos.length == 1) {
                toastr.error("You can't add more than one dyeing information for Single Part and Overdye!");
                return;
            }
            var newChildItem = {
                YDRecipeDInfoID: getMaxIdForArray(masterData.YDRecipeDefinitionDyeingInfos, "YDRecipeDInfoID"),
                CCColorID: masterData.CCColorID,
                YDRecipeID: 0,
                FiberPartID: 0,
                ColorID: 0,
                FiberPart: 'Empty',
                ColorName: 'Empty',
                ColorCode: 'Empty'
            };
            masterData.YDRecipeDefinitionDyeingInfos.push(newChildItem);
            $tblDefDyeingInfoEl.bootstrapTable('load', masterData.YDRecipeDefinitionDyeingInfos);
        });

        $formEl.find("#btnAddChild").on("click", function (e) {
            e.preventDefault();

            var finder = new commonFinder({
                title: "Select Item",
                pageId: pageId,
                fields: "ConceptNo,Composition,Construction,SubGroup,TechnicalName",
                headerTexts: "Concept No,Composition,Construction,SubGroup,Technical Name",
                widths: "20,20,20,20,20",
                allowEditing: false,
                apiEndPoint: `/api/yd-recipe-request/get-concept-item/${masterData.ConceptNo}/${masterData.ColorID}/${masterData.IsBDS}`,
                isMultiselect: true,
                //selectedIds: item[0].ItemIDs,
                allowPaging: true,
                primaryKeyColumn: "ConceptID",
                onMultiselect: function (selectedRecords) {
                    for (var i = 0; i < selectedRecords.length; i++) {
                        var exists = masterData.YDRecipeRequestChilds.find(function (el) { return el.ConceptID == selectedRecords[i].ConceptID });
                        if (!exists) {
                            masterData.YDRecipeRequestChilds.push(selectedRecords[i]);
                        }
                    }

                    $tblChildFabricId.bootstrapTable('load', masterData.YDRecipeRequestChilds);
                },
                onFinish: function () {
                    finder.hideModal();
                }
            });
            finder.showModal();

        });
        //Need to Change
        $formEl.find("#btnAddBatch").on("click", function (e) {
            e.preventDefault();

            var colorName = $.trim($formEl.find('#ColorName').val()),
                conceptNo = $.trim($formEl.find('#ConceptNo').val());

            //masterData.DyeingBatchList = masterData.DyeingBatchList.filter(x => x.ColorName.toLowerCase() == colorName.toLowerCase() && x.ConceptNo.toLowerCase() == conceptNo.toLowerCase());

            var finder = new commonFinder({
                title: "Select Batch",
                pageId: pageId,
                fields: "DBatchNo,DBatchDate,ColorName",
                headerTexts: "D. Batch No,D. Batch Date,Color Name",
                widths: "20,20,20",
                customFormats: ",ej2GridDateFormatter,",
                allowEditing: false,
                // data: masterData.DyeingBatchList,
                apiEndPoint: `/api/yd-dyeing-batch/get-dyeing-batch/${colorName}/${conceptNo}`,
                isMultiselect: false,
                allowPaging: true,
                primaryKeyColumn: "YDDBatchID",
                onSelect: function (selectedRecord) {
                    $formEl.find("#YDDBatchID").val(selectedRecord.rowData.YDDBatchID);
                    axios.get(`/api/yd-dyeing-batch/${selectedRecord.rowData.YDDBatchID}`)
                        .then(function (response) {
                            var dbList = [];
                            response.data.DyeingBatchItems.forEach(function (res) {
                                var oDB = {
                                    ConceptID: res.ConceptID,
                                    KnittingType: res.KnittingType,
                                    Composition: res.FabricComposition,
                                    //ConstructionID: res.ConceptID,
                                    Construction: res.FabricConstruction,
                                    GSM: res.FabricGsm,
                                    SubGroupID: res.ItemSubGroupID,
                                    SubGroup: res.ItemSubGroup,
                                    //TechnicalNameId: res.ConceptID,
                                    TechnicalName: res.TechnicalName,
                                    Qty: res.Qty,
                                    ItemMasterId: res.ItemMasterID
                                }
                                dbList.push(oDB);
                            });
                            masterData.YDRecipeRequestChilds = dbList;
                            if (masterData.YDRecipeRequestChilds != null && masterData.YDRecipeRequestChilds.length > 0) {
                                isDyeingProcessEnabled(true);
                            } else {
                                isDyeingProcessEnabled(false);
                            }

                            $tblChildFabricId.bootstrapTable("load", masterData.YDRecipeRequestChilds);
                            $tblChildFabricId.bootstrapTable('hideLoading');
                        })
                        .catch(function (err) {
                            toastr.error(err.response.data.Message);
                        });
                    finder.hideModal();
                },
                onFinish: function () {
                    finder.hideModal();
                }
            });
            finder.showModal();

        });
        //Need to Change
        $formEl.find("#btnCopyRecipe").on("click", function (e) {
            e.preventDefault();
            var fComp = masterData.YDRecipeDefinitionItemInfos.filter(x => x.FabricComposition != null && $.trim(x.FabricComposition).length > 0).map(x => x.FabricComposition).join(",");
            //fComp = "'" + fComp.replace(",", "','") + "'";
            var finder = new commonFinder({
                title: "Select Recipe Defination",
                pageId: pageId,
                apiEndPoint: `/api/yd-recipe-defination/list-by-groupconcept-compositon-color?fabricComposition=${fComp}&color=${masterData.ColorID}&groupConceptNo=${masterData.GroupConceptNo}`,
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "YDRecipeReqMasterID",
                fields: "RecipeNo,ColorName,ConceptNo,TechnicalName,Composition,Gsm,KnittingType,Buyer",
                headerTexts: "Recipe No,Color Name,Concept No,Technical Name,Composition,GSM,Knitting Type,Buyer",
                widths: "50,50,30,40,50,50,50,50",
                onSelect: function (res) {
                    finder.hideModal();
                    recipeCopy = true;
                    ydRecipeReqMasterID = res.rowData.YDRecipeReqMasterID;
                    ydRecipeID = res.rowData.YDRecipeID;
                    getDataForRecipe(res.rowData.YDRecipeReqMasterID, res.rowData.ConceptNo, res.rowData.RecipeNo);
                    //axios.get(`/api/rnd-recipe-defination/dyeingInfo-by-recipe/${res.rowData.YDRecipeReqMasterID}`)
                    //    .then(function (response) {
                    //        if (response.data.Childs.length > 0) {
                    //            masterData.Childs = [];
                    //            var rows = $tblChildEl.bootstrapTable('getData');
                    //            for (var i = 0; i < rows.length; i++) {
                    //                var rowFiber = response.data.Childs.find(x => x.FiberPart == rows[i].FiberPart);
                    //                if (rowFiber != null) {
                    //                    rows[i].Temperature = rowFiber.Temperature;
                    //                    rows[i].ProcessTime = rowFiber.ProcessTime;
                    //                    rows[i].DefChilds = rowFiber.DefChilds;
                    //                    masterData.Childs.push(rows[i]);
                    //                }
                    //            }
                    //            $tblChildEl.bootstrapTable("load", rows);
                    //            $tblChildEl.bootstrapTable('hideLoading');
                    //        }
                    //    })
                    //    .catch(function (err) {
                    //        toastr.error(err.response.data.Message);
                    //    });
                },
            });
            finder.showModal();
        });
    });

    function getFibarPartList(recipeOnList) {
        var selectedFiberParts = [];
        recipeOnList.map(x => {
            var compositions = getFiberParts(x.Composition);
            compositions = compositions.split(' ');
            compositions.map(c => c = $.trim(c));
            compositions.filter(c => c.length > 0).map(comp => {
                var composition = $.trim(comp);
                var fiber = allFiberParts.find(f => f.text.toLowerCase() == composition.toLowerCase());
                if (fiber) {
                    var indexF = selectedFiberParts.findIndex(rd => rd.text.toLowerCase() == fiber.text.toLowerCase());
                    if (indexF < 0) {
                        selectedFiberParts.push(fiber);
                    }
                }
            });
        });
        return selectedFiberParts;
    }

    function getFiberParts(value) {
        value = value.replace(new RegExp('%', 'g'), "")
            .replace(/\d+/g, '');
        return $.trim(value);
    }

    function isDyeingProcessEnabled(isEnabled) {
        var isDisable = isEnabled ? false : true;
        if (isDisable) {
            $formEl.find("#DPID").val(null);
        }
        $formEl.find("#DPID").attr('disabled', isDisable);
    }

    function initMasterTable() {
        var commands = [];
        if (status === statusConstants.PENDING) {
            commands = [
                { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }
            ]
        } else if (status === statusConstants.COMPLETED) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                //{ type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ]
        } else if (status === statusConstants.ACKNOWLEDGE) {
            commands = [
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                //{ type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ]
        }
        else if (status === statusConstants.UN_ACKNOWLEDGE) {
            commands = [
                { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                //{ type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ]
        }

        var columns = [
            { headerText: 'Actions', visible: status != statusConstants.PENDING, commands: commands, width: 60, textAlign: 'Center' },
            { field: 'YDRecipeReqMasterID', headerText: 'YDRecipeReqMasterID', width: 50, textAlign: 'left', visible: false },
            { field: 'RecipeStatus', headerText: 'Status', width: 50, textAlign: 'left', visible: status != statusConstants.PENDING },
            { field: 'RecipeReqNo', headerText: 'Recipe Req No', width: 70, textAlign: 'left', visible: status != statusConstants.PENDING },
            { field: 'RecipeReqDate', headerText: 'Recipe Req Date', width: 70, textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING },
            { field: 'YDBookingNo', headerText: 'Booking No', width: 80, textAlign: 'left', visible: true },
            { field: 'YDBookingDate', headerText: 'Booking Date', width: 70, textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: true },
            { field: 'ColorName', headerText: 'Color', width: 70, textAlign: 'left', visible: true },
            { field: 'GroupConceptNo', headerText: 'Group Concept No', textAlign: 'left', width: 90, visible: true },
            { field: 'ColorCode', headerText: 'Color Code', width: 70 },
            { field: 'Buyer', headerText: 'Buyer', width: 50 },
            { field: 'BuyerTeam', headerText: 'Buyer Team', width: 50 },
            { field: 'ExistingDBatchNo', headerText: 'Existing D.Batch No', width: 40, visible: status == statusConstants.PENDING }
        ];

        var selectionType = "Single";
        if (status == statusConstants.PENDING) {
            columns.unshift({ type: 'checkbox', width: 20 });
            selectionType = "Multiple";
        }

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/yd-recipe-request/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands,
            autofitColumns: false,
            allowSelection: status == statusConstants.PENDING,
            selectionSettings: { type: selectionType, checkboxOnly: true, persistSelection: true }
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Add') {
            $formEl.find("#btnSave").fadeIn();
            $formEl.find("#btnSaveNMail").fadeIn();
            $formEl.find("#btnAcknowledge").fadeOut();
            $formEl.find("#btnUnAcknowledge").fadeOut();
            ydDBatchID = args.rowData.YDDBatchID;

            getNew(args.rowData.CCColorID, args.rowData.GroupConceptNo, args.rowData.IsBDS, args.rowData.RecipeReqNo, args.rowData.YDBookingChildID, args.rowData.YDDBatchID);
        }
        else if (args.commandColumn.type == 'Edit') {
            if (args.rowData.DPName == "Wash") {
                $formEl.find("#tblDefDyeingInfoArea").fadeOut();
            }
            else {
                $formEl.find("#tblDefDyeingInfoArea").fadeIn();
            }
            if (args.rowData.RecipeStatus == "Deactive") {
                toastr.error("You can't edit deactive recipe, try active recipe.");
                return false;
            }
            if (isAcknowledgePage) {
                $formEl.find("#btnSave").fadeOut();
                $formEl.find("#btnSaveNMail").fadeOut();
                $formEl.find("#btnAcknowledge").fadeIn();
                $formEl.find("#btnUnAcknowledge").fadeIn();

            }
            else {
                if (args.rowData.Approved) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnSaveNMail").fadeOut();
                    $formEl.find("#btnAcknowledge").fadeOut();
                    $formEl.find("#btnUnAcknowledge").fadeOut();
                } else {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnSaveNMail").fadeIn();
                    $formEl.find("#btnAcknowledge").fadeOut();
                    $formEl.find("#btnUnAcknowledge").fadeOut();
                }
            }
            debugger;
            ydDBatchID = args.rowData.YDDBatchID;
            getData(args.rowData.YDRecipeReqMasterID, args.rowData.GroupConceptNo);
        }
        else if (args.commandColumn.type == 'View') {

            $formEl.find("#btnSave, #btnSaveNMail, #btnAcknowledge,#btnUnAcknowledge").fadeOut();
            if (args.rowData.DPName == 'Wash') {
                $formEl.find("#tblDefDyeingInfoArea").fadeOut();
            } else {
                $formEl.find("#tblDefDyeingInfoArea").fadeIn();
            }
            debugger;
            ydDBatchID = args.rowData.YDDBatchID;
            getData(args.rowData.YDRecipeReqMasterID, args.rowData.GroupConceptNo);

        }
        else if (args.commandColumn.type == "ViewReport") {
            window.open(`/reports/InlinePdfView?ReportName=RecipeRequestForm.rdl&RecipeReqNo=${args.rowData.RecipeReqNo}`, '_blank');
        }
    }

    function initChildFabricTable() {

        $tblChildFabricId.bootstrapTable("destroy");
        $tblChildFabricId.bootstrapTable({
            uniqueId: 'CCColorID',
            checkboxHeader: false,
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    visible: !masterData.IsBDS,
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
                            if (masterData.YDRecipeRequestChilds.length == 1) {
                                toastr.warning("Atleast one item required. You can't delete this!");
                                return;
                            }
                            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                                if (yes) {
                                    masterData.YDRecipeRequestChilds.splice(index, 1);

                                    $tblChildFabricId.bootstrapTable('load', masterData.YDRecipeRequestChilds);
                                }
                            });
                        }
                    }
                },
                {
                    field: "SubGroup",
                    title: "Sub Group",
                    align: 'center'
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
                    field: "RecipeOn",
                    title: "Recipe On?",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    checkbox: true,
                    showSelectTitle: true,
                    checkboxEnabled: isAcknowledgePage ? false : true,
                    visible: true,
                    events: {
                        'click': function (e, value, row, index) {
                            e.preventDefault();
                            alert(1);
                        },
                    }
                },
                {
                    field: "GSM",
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

    function initDefDyeingInfoTable() {
        $tblDefDyeingInfoEl.bootstrapTable("destroy");
        $tblDefDyeingInfoEl.bootstrapTable({
            uniqueId: 'YDRecipeDInfoID',
            checkboxHeader: false,
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    visible: isRemovable,
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
                            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                                if (yes) {
                                    masterData.YDRecipeDefinitionDyeingInfos.splice(index, 1);
                                    $tblDefDyeingInfoEl.bootstrapTable('load', masterData.YDRecipeDefinitionDyeingInfos);
                                }
                            });
                        }
                    }
                },
                {
                    field: "FiberPartID",
                    title: "Fiber Part",
                    align: 'center',
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    editable: {
                        type: 'select2',
                        title: 'Select Fiber Type',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: masterData.FiberPartList,
                        select2: { width: 130, placeholder: 'Fiber Type', allowClear: true }
                    },
                },
                {
                    field: "ColorID",
                    title: "Color",
                    align: 'center',
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + row.ColorName + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getColor(row);
                        },
                    }
                },
                {
                    field: "RecipeOn",
                    title: "Recipe On?",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    checkbox: true,
                    showSelectTitle: true,
                    checkboxEnabled: isAcknowledgePage ? false : true
                },
                {
                    field: "ColorCode",
                    title: "Color Code",
                    align: 'center',
                    cellStyle: function () { return { classes: 'm-w-100' } }
                }
            ]
        });
    }

    function getColor(rowData) {
        var finder = new commonFinder({
            title: "Select Color",
            pageId: pageId,
            height: 350,
            apiEndPoint: "/api/fabric-color-book-setups",
            fields: "ColorSource,ColorCode,ColorName,RGBOrHex",
            headerTexts: "Source,Code,Name,Visual",
            customFormats: ",,,ej2GridColorFormatter",
            isMultiselect: false,
            primaryKeyColumn: "PTNID",
            onSelect: function (res) {
                finder.hideModal();

                rowData.ColorID = res.rowData.ColorID;
                rowData.ColorName = res.rowData.ColorName;
                rowData.ColorCode = res.rowData.ColorCode;
                $tblDefDyeingInfoEl.bootstrapTable('updateByUniqueId', { id: rowData.YDRecipeDInfoID, row: rowData });
            }
        });
        finder.showModal();
    }

    function getNew(ccColorId, grpConceptNo, isBDS, isRework, recipeReqNo, YDBookingChildID, YDDBatchID) {
        recipeCopy == false;
        masterData = {};
        var url = `/api/yd-recipe-request/new/${ccColorId}/${grpConceptNo}/${isBDS}/${isRework}/${recipeReqNo}/${YDBookingChildID}/${YDDBatchID}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();

                masterData = response.data;
                masterData.RecipeFor = 1102; //Production = 1102
                allFiberParts = masterData.FiberPartList;
                masterData.DPID = 0;
                setFormData($formEl, masterData);

                if (status === statusConstants.PENDING) {
                    $formEl.find("#divDPID").fadeIn();
                    $formEl.find("#divDPName").fadeOut();
                }
                else {
                    $formEl.find("#divDPID").fadeOut();
                    $formEl.find("#divDPName").fadeIn();
                    if (masterData.DPName == 'Cross') {
                        $formEl.find("#divProcessInfo").fadeIn();
                        $formEl.find("#DPProcessInfo").prop("readonly", true);
                    }
                    else {
                        $formEl.find("#divProcessInfo").fadeIn();
                    }
                }

                var dpParam = false;
                if (masterData.YDRecipeRequestChilds.length > 0) dpParam = true;

                if (masterData.IsBDS != 1) {
                    $formEl.find("#btnAddChild").fadeOut();
                    $formEl.find("#btnAddBatch").fadeIn();
                    dpParam = true;
                }

                initChildFabricTable();
                //$tblChildFabricId.bootstrapTable("load", DeepClone(masterData.YDRecipeRequestChilds));
                $tblChildFabricId.bootstrapTable('load', masterData.YDRecipeRequestChilds);
                $tblChildFabricId.bootstrapTable('hideLoading');
                $formEl.find("#btnAddChild").fadeIn();
                $formEl.find("#btnAddBatch").fadeOut();
                isDyeingProcessEnabled(dpParam);

                if (masterData.DPName == 'Single Part' || masterData.DPName == 'Overdye') {
                    isRemovable = false;
                } else {
                    isRemovable = true;
                }
                //for collar & cuff
                if (masterData.YDRecipeRequestChilds[0].SubGroupID != 1) {
                    if (masterData.Composition != null) {
                        var fiberList = [];
                        masterData.FiberPartList.forEach(function (fiber) {
                            if (masterData.Composition.toLowerCase().indexOf(' ' + fiber.text.toLowerCase()) >= 0) {
                                fiberList.push(fiber);
                            }
                        });

                        masterData.FiberPartList = fiberList;
                        masterData.FiberPartListCC.map(cc => {
                            var index = masterData.FiberPartList.findIndex(x => x.text.toLowerCase() == cc.text.toLowerCase());
                            if (index < 0) {
                                masterData.FiberPartList.push(cc);
                            }
                        });
                    }
                }
                //End collar & Cuff

                initDefDyeingInfoTable();
                $tblDefDyeingInfoEl.bootstrapTable("load", masterData.YDRecipeDefinitionDyeingInfos);
                $tblDefDyeingInfoEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }

    function getData(id, groupConceptNo) {
        var url = `/api/yd-recipe-request/${id}/${groupConceptNo}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.GroupConceptNo = masterData.ConceptNo;
                masterData.RecipeFor = 1102; //Production = 1102
                allFiberParts = masterData.FiberPartList;
                var recipeOnList = masterData.YDRecipeRequestChilds.filter(x => x.RecipeOn == true);

                masterData.FiberPartList = getFibarPartList(recipeOnList);
                //for collar & cuff
                if (masterData.YDRecipeRequestChilds[0].SubGroupID != 1) {
                    if (masterData.Composition != null) {
                        var fiberList = [];
                        masterData.FiberPartList.forEach(function (fiber) {
                            if (masterData.Composition.toLowerCase().indexOf(' ' + fiber.text.toLowerCase()) >= 0) {
                                fiberList.push(fiber);
                            }
                        });

                        masterData.FiberPartList = fiberList;
                        masterData.FiberPartListCC.map(cc => {
                            var index = masterData.FiberPartList.findIndex(x => x.text.toLowerCase() == cc.text.toLowerCase());
                            if (index < 0) {
                                masterData.FiberPartList.push(cc);
                            }
                        });
                    }
                }
                //End collar & Cuff

                //if (masterData.Composition != null) {
                //    var fiberList = [];
                //    masterData.FiberPartList.forEach(function (fiber) {
                //        if (masterData.Composition.toLowerCase().indexOf(' ' + fiber.text.toLowerCase()) >= 0) {
                //            fiberList.push(fiber);
                //        }
                //    });
                //    masterData.FiberPartList = fiberList;
                //    masterData.FiberPartListCC.map(cc => {
                //        var index = masterData.FiberPartList.findIndex(x => x.text.toLowerCase() == cc.text.toLowerCase());
                //        if (index < 0) {
                //            masterData.FiberPartList.push(cc);
                //        }
                //    });
                //}

                setFormData($formEl, masterData);
                isDyeingProcessEnabled(false);
                if (status === statusConstants.PENDING) {
                    $formEl.find("#divDPID").fadeIn();
                    $formEl.find("#divDPName").fadeOut();
                    $formEl.find("#btnRevise").fadeOut();
                    $formEl.find("#revReasonId").fadeOut();
                    $formEl.find("#UnAckReasonAreaID").fadeOut();
                }
                else if (status === statusConstants.UN_ACKNOWLEDGE) {
                    //$formEl.find("#divDPName").fadeIn();
                    if (isAcknowledgePage) {
                        $formEl.find("#btnRevise").fadeOut();
                        $formEl.find("#revReasonId").fadeIn();
                        $formEl.find("#UnAckReasonAreaID").fadeIn();

                    } else {
                        $formEl.find("#DPID").attr('disabled', false);
                        $formEl.find("#divDPID").fadeIn();
                        $formEl.find("#btnRevise").fadeIn();
                        $formEl.find("#revReasonId").fadeIn();
                        $formEl.find("#UnAckReasonAreaID").fadeIn();
                    }
                }
                else {
                    $formEl.find("#divDPID").fadeOut();
                    $formEl.find("#divDPName").fadeIn();
                    $formEl.find("#btnRevise").fadeOut();
                    $formEl.find("#revReasonId").fadeOut();
                    $formEl.find("#UnAckReasonAreaID").fadeOut();
                    if (masterData.DPName == 'Cross') {
                        $formEl.find("#divProcessInfo").fadeIn();
                        $formEl.find("#DPProcessInfo").prop("readonly", true);
                    }
                    else {
                        $formEl.find("#divProcessInfo").fadeOut();
                    }
                }
                initChildFabricTable();


                $tblChildFabricId.bootstrapTable("load", masterData.YDRecipeRequestChilds);
                $tblChildFabricId.bootstrapTable('hideLoading');

                if (!masterData.IsBDS) {
                    $formEl.find("#btnAddChild").fadeIn();
                    $formEl.find("#btnAddBatch").fadeOut();
                } else {
                    $formEl.find("#btnAddChild").fadeOut();
                    $formEl.find("#btnAddBatch").fadeIn();
                }

                if (masterData.DPName == 'Single Part' || masterData.DPName == 'Overdye') {
                    isRemovable = false;
                } else {
                    isRemovable = true;
                }
                initDefDyeingInfoTable();
                $tblDefDyeingInfoEl.bootstrapTable("load", masterData.YDRecipeDefinitionDyeingInfos);
                $tblDefDyeingInfoEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDataForRecipe(id, groupConceptNo, recipeNo) {

        var url = `/api/yd-recipe-request/${id}/${groupConceptNo}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.GroupConceptNo = masterData.ConceptNo;
                masterData.RecipeFor = 1102; //Production = 1102
                allFiberParts = masterData.FiberPartList;
                var recipeOnList = masterData.YDRecipeRequestChilds.filter(x => x.RecipeOn == true);

                masterData.FiberPartList = getFibarPartList(recipeOnList);
                //for collar & cuff
                if (masterData.YDRecipeRequestChilds[0].SubGroupID != 1) {
                    if (masterData.Composition != null) {
                        var fiberList = [];
                        masterData.FiberPartList.forEach(function (fiber) {
                            if (masterData.Composition.toLowerCase().indexOf(' ' + fiber.text.toLowerCase()) >= 0) {
                                fiberList.push(fiber);
                            }
                        });

                        masterData.FiberPartList = fiberList;
                        masterData.FiberPartListCC.map(cc => {
                            var index = masterData.FiberPartList.findIndex(x => x.text.toLowerCase() == cc.text.toLowerCase());
                            if (index < 0) {
                                masterData.FiberPartList.push(cc);
                            }
                        });
                    }
                }
                //End collar & Cuff

                //if (masterData.Composition != null) {
                //    var fiberList = [];
                //    masterData.FiberPartList.forEach(function (fiber) {
                //        if (masterData.Composition.toLowerCase().indexOf(' ' + fiber.text.toLowerCase()) >= 0) {
                //            fiberList.push(fiber);
                //        }
                //    });
                //    masterData.FiberPartList = fiberList;
                //    masterData.FiberPartListCC.map(cc => {
                //        var index = masterData.FiberPartList.findIndex(x => x.text.toLowerCase() == cc.text.toLowerCase());
                //        if (index < 0) {
                //            masterData.FiberPartList.push(cc);
                //        }
                //    });
                //}

                setFormData($formEl, masterData);
                isDyeingProcessEnabled(false);
                if (status === statusConstants.PENDING) {
                    $formEl.find("#divDPID").fadeIn();
                    $formEl.find("#divDPName").fadeOut();
                    $formEl.find("#btnRevise").fadeOut();
                    $formEl.find("#revReasonId").fadeOut();
                    $formEl.find("#UnAckReasonAreaID").fadeOut();
                }
                else if (status === statusConstants.UN_ACKNOWLEDGE) {
                    //$formEl.find("#divDPName").fadeIn();
                    if (isAcknowledgePage) {
                        $formEl.find("#btnRevise").fadeOut();
                        $formEl.find("#revReasonId").fadeIn();
                        $formEl.find("#UnAckReasonAreaID").fadeIn();

                    } else {
                        $formEl.find("#DPID").attr('disabled', false);
                        $formEl.find("#divDPID").fadeIn();
                        $formEl.find("#btnRevise").fadeIn();
                        $formEl.find("#revReasonId").fadeIn();
                        $formEl.find("#UnAckReasonAreaID").fadeIn();
                    }
                }
                else {
                    $formEl.find("#divDPID").fadeOut();
                    $formEl.find("#divDPName").fadeIn();
                    $formEl.find("#btnRevise").fadeOut();
                    $formEl.find("#revReasonId").fadeOut();
                    $formEl.find("#UnAckReasonAreaID").fadeOut();
                    if (masterData.DPName == 'Cross') {
                        $formEl.find("#divProcessInfo").fadeIn();
                        $formEl.find("#DPProcessInfo").prop("readonly", true);
                    }
                    else {
                        $formEl.find("#divProcessInfo").fadeOut();
                    }
                }
                initChildFabricTable();


                $tblChildFabricId.bootstrapTable("load", masterData.YDRecipeRequestChilds);
                $tblChildFabricId.bootstrapTable('hideLoading');

                if (!masterData.IsBDS) {
                    $formEl.find("#btnAddChild").fadeIn();
                    $formEl.find("#btnAddBatch").fadeOut();
                } else {
                    $formEl.find("#btnAddChild").fadeOut();
                    $formEl.find("#btnAddBatch").fadeIn();
                }

                if (masterData.DPName == 'Single Part' || masterData.DPName == 'Overdye') {
                    isRemovable = false;
                } else {
                    isRemovable = true;
                }
                initDefDyeingInfoTable();
                $tblDefDyeingInfoEl.bootstrapTable("load", masterData.YDRecipeDefinitionDyeingInfos);
                $tblDefDyeingInfoEl.bootstrapTable('hideLoading');

                $formEl.find('#RecipeNo').val(recipeNo);
                $formEl.find("#btnAddChild").fadeOut();
                $formEl.find("#btnAddBatch").fadeOut();
                $formEl.find("#btnAddItem").fadeOut();
                $formEl.find("#btnSaveNMail").fadeOut();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function loadFabricData() {
        var Obj = {
            BookingID: 0,
            ItemMasterID: 0,
            ConceptID: masterData.ConceptID,
            SubGroupID: masterData.SubGroupID,
            Pcs: 0,
            Qty: 0,
            FabricConstruction: masterData.Construction,
            TechnicalName: masterData.TechnicalName,
            FabricComposition: masterData.Composition,
            FabricColor: '',
            FabricGsm: masterData.GSM,
            KnittingType: masterData.KnittingType,
            DyeingType: '',
            SubGroup: masterData.SubGroup,
            Construction: masterData.Construction,
            ConstructionID: masterData.ConstructionID
        }

        $tblChildFabricId.bootstrapTable("load", [Obj]);
        $tblChildFabricId.bootstrapTable('hideLoading');
        masterData.YDRecipeDefinitionItemInfos = [Obj];
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
        $formEl.find("#CCColorID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function requestRecipie(isRequest = false, isAck = false) {
        if (dprName == 'Double Part') {
            var IsMatch = true;
            var colorName = masterData.YDRecipeDefinitionDyeingInfos[0].ColorName;
            for (var i = 1; i < masterData.YDRecipeDefinitionDyeingInfos.length; i++) {
                if (colorName != masterData.YDRecipeDefinitionDyeingInfos[i].ColorName) {
                    IsMatch = false;
                    break;
                }
            }
            if (!IsMatch) {
                toastr.warning('All color should be same for Double Part!');
                return;
            }
        }

        var data = formDataToJson($formEl.serializeArray());
        data.RequestRecipe = isRequest;
        data.RequestAck = isAck;
        data["YDRecipeDefinitionDyeingInfos"] = masterData.YDRecipeDefinitionDyeingInfos;
        axios.post("/api/yd-recipe-request/save", data)
            .then(function () {
                toastr.success("Saved successfully!");
                resetTableParams();
                initMasterTable();
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }



    function save(flag) {
        if (recipeCopy == false) {
            var data = masterData.YDRecipeRequestChilds; // test

            data.YDDBatchID = ydDBatchID;
            if (masterData.DPName == 'Cross' && $formEl.find("#DPProcessInfo").val() == '') {
                initializeValidation($formEl, validationConstraints);
                if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
                else hideValidationErrors($formEl);
            }
            var dprName = $('#DPID').find(":selected").text();

            if (masterData.DPName != "Wash" && masterData.YDRecipeDefinitionDyeingInfos.length == 0) {
                toastr.warning('No Dyeing Information Found!');
                return;
            }
            if ((status === statusConstants.PENDING) && dprName == "") {
                toastr.warning('Please select Dyeing Process');
                return;
            }

            if (dprName == 'Double Part') {
                var IsMatch = true;
                var colorName = masterData.YDRecipeDefinitionDyeingInfos[0].ColorName;
                for (var i = 1; i < masterData.YDRecipeDefinitionDyeingInfos.length; i++) {
                    if (colorName != masterData.YDRecipeDefinitionDyeingInfos[i].ColorName) {
                        IsMatch = false;
                        break;
                    }
                }
                if (!IsMatch) {
                    toastr.warning('All color should be same for Double Part!');
                    return;
                }
            }
            var data = formDataToJson($formEl.serializeArray());
            data.YDDBatchID = ydDBatchID;
            data.IsBDS = masterData.IsBDS;
            if (masterData.DPID != 0) {
                data.DPID = masterData.DPID;
            }
            data.RequestRecipe = true;
            data.Approved = flag;
            data.GroupConceptNo = $formEl.find("#ConceptNo").val();
            data["YDRecipeRequestChilds"] = masterData.YDRecipeRequestChilds;
            data["YDRecipeDefinitionDyeingInfos"] = masterData.YDRecipeDefinitionDyeingInfos;
            data.IsRework = isRework;

            if (data.DPID == 0) {
                toastr.warning('Please select Dyeing Process');
                return;
            }
            //if (data.YDRecipeDefinitionDyeingInfos.length == 0) {
            if (masterData.DPName != "Wash" && data.YDRecipeDefinitionDyeingInfos.length == 0) {
                toastr.warning('No dyeing information found');
                return;
            }

            var hasError = false;
            for (var i = 0; i < data.YDRecipeDefinitionDyeingInfos.length; i++) {
                var obj = masterData.FiberPartList.find(y => y.id == data.YDRecipeDefinitionDyeingInfos[i].FiberPartID);
                if (!obj) {
                    hasError = true;
                    toastr.error("Select fiber part");
                    break;
                } else {
                    data.YDRecipeDefinitionDyeingInfos[i].FiberPart = obj.text;
                }
            }

            if (!hasError) {

                axios.post("/api/yd-recipe-request/save", data)
                    .then(function () {
                        toastr.success("Saved successfully!");
                        resetTableParams();
                        initMasterTable();
                        backToList();
                    })
                    .catch(function (error) {
                        toastr.error(error.response.data.Message);
                    });
            }
        }
        else {
            var data = {};
            data.YDRecipeID = YDRecipeID;
            data.YDDBatchID = ydDBatchID;
            axios.post("/api/yd-recipe-request/batchRecipeUpdate", data)
                .then(function () {
                    toastr.success("Saved successfully!");
                    resetTableParams();
                    initMasterTable();
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }
    }

    function revise() {
        if (masterData.DPName == 'Cross' && $formEl.find("#DPProcessInfo").val() == '') {
            initializeValidation($formEl, validationConstraints);
            if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
            else hideValidationErrors($formEl);
        }

        if (masterData.DPName != "Wash" && masterData.YDRecipeDefinitionDyeingInfos.length == 0) {
            toastr.warning('No Dyeing Information Found!');
            return;
        }

        if (masterData.DPName == 'Double Part') {
            var IsMatch = true;
            var colorName = masterData.YDRecipeDefinitionDyeingInfos[0].ColorName;
            for (var i = 1; i < masterData.YDRecipeDefinitionDyeingInfos.length; i++) {
                if (colorName != masterData.YDRecipeDefinitionDyeingInfos[i].ColorName) {
                    IsMatch = false;
                    break;
                }
            }
            if (!IsMatch) {
                toastr.warning('All color should be same for Double Part!');
                return;
            }
        }
        var data = formDataToJson($formEl.serializeArray());
        data.IsBDS = masterData.IsBDS;
        data.RequestRecipe = true;
        //data.Approved = flag;
        data.GroupConceptNo = $formEl.find("#ConceptNo").val();
        data["YDRecipeRequestChilds"] = masterData.YDRecipeRequestChilds;
        data["YDRecipeDefinitionDyeingInfos"] = masterData.YDRecipeDefinitionDyeingInfos;
        data.IsRework = isRework;


        //if (data.YDRecipeDefinitionDyeingInfos.length == 0) {
        if (masterData.DPName != "Wash" && data.YDRecipeDefinitionDyeingInfos.length == 0) {
            toastr.warning('No dyeing information found');
            return;
        }

        var hasError = false;
        for (var i = 0; i < data.YDRecipeDefinitionDyeingInfos.length; i++) {
            var obj = masterData.FiberPartList.find(y => y.id == data.YDRecipeDefinitionDyeingInfos[i].FiberPartID);
            if (!obj) {
                hasError = true;
                toastr.error("Select fiber part");
                break;
            } else {
                data.YDRecipeDefinitionDyeingInfos[i].FiberPart = obj.text;
            }
        }

        if (!hasError) {

            axios.post("/api/yd-recipe-request/revise", data)
                .then(function () {
                    toastr.success("Saved successfully!");
                    resetTableParams();
                    initMasterTable();
                    backToList();
                })
                .catch(function (error) {
                    toastr.error(error.response.data.Message);
                });
        }
    }


    function acknowledge(unAcknowledgeReason, isAcknowledge) {
        var data = formDataToJson($formEl.serializeArray());
        data.Acknowledge = isAcknowledge;
        data.UnAcknowledgeReason = unAcknowledgeReason;
        axios.post("/api/yd-recipe-request/acknowledge", data)
            .then(function () {
                if (isAcknowledge)
                    toastr.success("Acknowledge successfully!");
                else
                    toastr.success("UnAcknowledge successfully!");

                resetTableParams();
                initMasterTable();
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function createNew() {
        $formEl.find("#btnSave").fadeIn();
        $formEl.find("#btnSaveNMail").fadeIn();
        $formEl.find("#tblDefDyeingInfoArea").fadeIn();
        $formEl.find("#btnAcknowledge").fadeOut();
        $formEl.find("#btnUnAcknowledge").fadeOut();
        $formEl.find("#btnRevise").fadeOut();
        $formEl.find("#revReasonId").fadeOut();
        $formEl.find("#UnAckReasonAreaID").fadeOut();

        var selectedRecords = $tblMasterEl.getSelectedRecords();
        selectedMasterItems = selectedRecords;
        if (selectedRecords.length == 0) {
            toastr.error("Select single item.");
            return;
        }
        if (selectedRecords.length > 1) {
            toastr.error("Select single item.");
            return;
        }
        var conceptList = [],
            statusType = [],
            colors = [];

        selectedRecords.map(x => {
            statusType.push(x.Status);
            conceptList.push(x.GroupConceptNo);
            colors.push(x.ColorName);
        });

        var isNew = true;
        var unique_array = [...new Set(statusType)];
        if (unique_array.length > 1) {
            toastr.error("Status must be same.");
            return;
        }
        if (conceptList.length > 1 && unique_array[0] == "New") {
            toastr.error("Multiple select allow for rework only.");
            return;
        }
        isRework = unique_array[0] == "Rework" ? true : false;
        if (isRework) isNew = false;

        unique_array = [...new Set(conceptList)];
        if (unique_array.length > 1) {
            toastr.error("Concept & color must be same.");
            return;
        }
        unique_array = [...new Set(colors)];
        if (unique_array.length > 1) {
            toastr.error("Color must be same.");
            return;
        }
        var recipeReqNo = "TestReqNo";
        if (isRework) recipeReqNo = selectedRecords[0].RecipeReqNo;

        ydDBatchID = selectedRecords[0].YDDBatchID;
        //ydBookingChildID = selectedRecords[0].YDBookingChildID;
        getNew(selectedRecords[0].CCColorID, selectedRecords[0].GroupConceptNo, selectedRecords[0].IsBDS, isRework, recipeReqNo, selectedRecords[0].YDBookingChildID, selectedRecords[0].YDDBatchID);
    }
})();