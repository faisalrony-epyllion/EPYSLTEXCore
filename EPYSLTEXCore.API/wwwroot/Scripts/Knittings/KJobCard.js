(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $tblChildEl, tblChildId, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;
    var pageId;

    var KPChild;
    var masterData = null;
    var KJobCardMasterID = 1000;
    var KJobCardChildID = 1000;
    var newChildList = [];
    var tempKJobCardMasterId = 0;

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
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        //$tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        //$tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING;
        initMasterTable();
        //initChildTable([]);
        getMasterTableData();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnActiveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ACTIVE;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnInActiveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.IN_ACTIVE;
            initMasterTable();
            getMasterTableData();
        });
        $toolbarEl.find("#btnRevisionList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.REVISE;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnAllList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ALL;
            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(false);
        });
        $formEl.find("#btnSaveAndAapprove").click(function (e) {
            e.preventDefault();
            save(false);
        });
        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            save(true);
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnNewItem").on("click", function (e) {
            e.preventDefault();
            var newChildItem = {
                Id: getMaxIdForArray(KPChild.KJobCardMasters, "Id"),
                KJobCardNo: '**<< NEW >>**',
                KJobCardDate: formatDateToDefault(new Date()),
                KPChildID: KPChild.Id,
                BAnalysisChildID: KPChild.BAnalysisChildID,
                ConceptID: KPChild.ConceptID,
                ItemMasterID: KPChild.ItemMasterID,
                IsSubContact: false,
                ContactID: 0,
                Contact: "Empty",
                MachineKnittingTypeID: KPChild.KnittingTypeID,
                MachineKnittingType: KPChild.KnittingType,
                KnittingMachineID: 0,
                KnittingMachineNo: "Empty",
                MachineGauge: KPChild.MachineGauge,
                MachineDia: KPChild.MachineDia,
                BookingID: KPChild.BookingID,
                SubGroupID: KPChild.SubGroupID,
                ExportOrderID: KPChild.ExportOrderID,
                BuyerID: KPChild.BuyerID,
                BuyerTeamID: KPChild.BuyerTeamID,
                UnitID: KPChild.UnitID,
                BookingQty: KPChild.BookingQty,
                KJobCardQty: (KPChild.BookingQty - KPChild.KJobCardQty),
                Remarks: '',
                BrandID: 0,
                Brand: "Empty",
                Width: 0,
                MCSubClassID: KPChild.MCSubClassID,
                MCSubClassName: KPChild.MCSubClassName
                //EntityState: 4
            };

            KPChild.KJobCardMasters.push(newChildItem);
            $tblChildEl.bootstrapTable('load', KPChild.KJobCardMasters);
        });

        $formEl.find("#btnSelectColor").on("click", function (e) {
            e.preventDefault();
            var colors = masterData.KnittingPlanChilds.map(x => x.ColorName);
            colors = [...new Set(colors)];
            var colorList = [];
            colors.map(c => {
                colorList.push({
                    ColorName: c
                });
            });
            var finder = new commonFinder({
                title: "Select Color",
                pageId: pageId,
                data: colorList,
                isMultiselect: false,
                modalSize: "modal-md",
                top: "2px",
                primaryKeyColumn: "ColorName",
                fields: "ColorName",
                headerTexts: "Color Name",
                widths: "100",
                onSelect: function (res) {
                    finder.hideModal();
                    LoadMasters(res.rowData.ColorName);
                },
            });
            finder.showModal();
        });
    });

    function initMasterTable() {
        var commands = [];
        if (status == statusConstants.PENDING) {
            commands = [
                { type: 'New', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-plus' } }
            ]
        } else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-edit' } }
            ]
        }

        var columns = [
            {
                headerText: 'Actions', headerTextAlign: 'Center', commands: commands, width: 50
            },
            {
                field: 'GroupConceptNo', headerText: 'Concept No', width: 60, headerTextAlign: 'Center'
            },
            {
                field: 'Buyer', headerText: 'Buyer', width: 50, headerTextAlign: 'Center'
            },
            {
                field: 'BuyerTeam', headerText: 'Buyer Team', width: 50, headerTextAlign: 'Center'
            },
            {
                field: 'SubGroupName', headerText: 'Sub Group', width: 50, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'GroupID', headerText: 'Group No', width: 30, visible: false
            },
            {
                field: 'MachineGauge', headerText: 'Machine Gauge', width: 50, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'MachineDia', headerText: 'Machine Dia', width: 50, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'ColorName', headerText: 'Color', width: 50, textAlign: 'left', headerTextAlign: 'Center'
            },
            {
                field: 'StartDate', headerText: 'Start Date', type: 'date', format: _ch_date_format_1, width: 50, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'EndDate', headerText: 'End Date', type: 'date', format: _ch_date_format_1, width: 50, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'PlanQty', headerText: 'Plan Qty', width: 40, textAlign: 'Center', headerTextAlign: 'Center'
            },
            {
                field: 'NeedPreFinishingProcess', headerText: 'Pre Finishing Process?', displayAsCheckBox: true, width: 40, textAlign: 'Center', headerTextAlign: 'Center'
            }
            //{ field: 'IsSubContact', headerText: 'Sub-Contact?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },

            //{
            //    field: 'BookingDate', headerText: 'Booking Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 20
            //}
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            //allowGrouping: true,
            apiEndPoint: `/api/Knitting-JobCard/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'New') {
            getNew(args.rowData.GroupID);
        } else if (args.commandColumn.type == 'Edit' && status == statusConstants.REVISE) {
            if (args.rowData) {
                getRevision(args.rowData.GroupID);
                $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge").fadeOut();
                $formEl.find("#btnRevise").fadeIn();
            }
        } else if (args.commandColumn.type == 'Edit' && status == statusConstants.COMPLETED) {
            if (args.rowData) {
                //getRevisionForCompleteList(args.rowData.ConceptNo);
                $formEl.find("#btnSave,#btnSaveComplete,#btnAcknowledge").fadeOut();
                $formEl.find("#btnRevise").fadeOut();
            }
        } else if (args.commandColumn.type == 'Edit') {
            if (args.rowData) {
                getDetails(args.rowData.GroupID);
                $formEl.find("#btnSave").fadeIn();
                $formEl.find("#btnSaveComplete,#btnAcknowledge").fadeOut();
                $formEl.find("#btnRevise").fadeOut();
                if (status == statusConstants.PARTIALLY_COMPLETED) $formEl.find("#btnSave,#btnSaveComplete").fadeIn();
            }
        }
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        //$tblMasterEl.bootstrapTable('showLoading');
        //var url = `/api/Knitting-JobCard/list?gridType=bootstrap-table&status=${status}&${queryParams}`;
        //axios.get(url)
        //    .then(function (response) {
        //        $tblMasterEl.bootstrapTable('load', response.data);
        //        $tblMasterEl.bootstrapTable('hideLoading');
        //    })
        //    .catch(function (err) {
        //        toastr.error(err.response.data.Message);
        //    })
    }

    function initChildTable(data) {
        if ($tblChildEl) $tblChildEl.destroy();
        var parentColumns = [
            {
                headerText: 'Action', textAlign: 'Center', width: 100, commands: [
                    //{ type: 'AddItem', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-add' } },
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } },
                    { type: 'Report', title: 'Job Card Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
                ]
            },
            { field: 'KJobCardMasterID', isPrimaryKey: true, visible: false },
            { field: 'ConceptID', visible: false },
            { field: 'KJobCardNo', headerText: 'Job Card No', textAlign: 'Center', width: 100, allowEditing: false },
            { field: 'KJobCardDate', headerText: 'Job Card Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, allowEditing: false, width: 100, visible: false },
            { field: 'MCSubClassName', headerText: 'Machine Type', textAlign: 'Center', width: 100, allowEditing: false },
            //{ field: 'MachineGauge', headerText: 'Machine Gauge', textAlign: 'Center', allowEditing: false, width: 100 },
            { field: 'Machine', headerText: 'Machine', textAlign: 'Center', allowEditing: false, width: 100 },
            {
                headerText: '', textAlign: 'Center', width: 40, commands: [
                    { buttonOption: { type: 'mainMachine', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select Machine'" } }
                ]
            },
            { field: 'Brand', headerText: 'Brand', textAlign: 'Center', allowEditing: false, width: 100 },
            //{
            //    headerText: '', textAlign: 'Center', width: 40, commands: [
            //        { buttonOption: { type: 'mainBrand', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select Brand'" } }
            //    ]
            //},
            //{
            //    field: 'BrandID',
            //    headerText: 'Brand ID',
            //    valueAccessor: ej2GridDisplayFormatter,
            //    dataSource: [],//masterData.Brands,
            //    displayField: "Brand",
            //    edit: ej2GridDropDownObj({
            //    })
            //},
            { field: 'IsSubContact', headerText: 'Sub-Contact?', displayAsCheckBox: true, editType: "booleanedit", textAlign: 'Center', width: 100 },
            //{
            //    field: 'ContactID',
            //    headerText: 'Floor/Sub-Contractor',
            //    valueAccessor: ej2GridDisplayFormatter,
            //    dataSource: [],// masterData.Contacts,
            //    displayField: "Contact",
            //    edit: ej2GridDropDownObj({
            //    })
            //},
            // { field: 'Contact', headerText: 'Floor/Sub-Contractor', textAlign: 'Center', allowEditing: false, width: 100 },
            //{
            //    headerText: '', textAlign: 'Center', width: 40, commands: [
            //        { buttonOption: { type: 'mainContact', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search', tooltipText: "Select Floor/Sub-Contractor'" } }
            //    ]
            //},

            //{
            //    field: 'MachineDia', headerText: 'Machine Dia', editType: "numericedit",
            //    edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
            //},
            //{ field: 'MachineDia', headerText: 'Machine Dia', textAlign: 'Center', allowEditing: false, width: 100 },
            //{
            //    field: 'KnittingMachineID',
            //    headerText: 'Machine',
            //    valueAccessor: ej2GridDisplayFormatter,
            //    dataSource: [],// masterData.Machines,
            //    displayField: "Machine",
            //    edit: ej2GridDropDownObj({
            //    })
            //},

            {
                field: 'KJobCardQty', headerText: 'Job Card Qty', allowEditing: false, editType: "numericedit",
                edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
            },
            { field: 'Remarks', headerText: 'Remarks', allowEditing: true, width: 100 }
        ];
        var childColumns = [
            {
                headerText: 'Action', textAlign: 'Center', width: 100, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            },
            { field: 'KJobCardChildID', isPrimaryKey: true, visible: false },
            { field: 'KJobCardMasterID', visible: false },
            {
                field: 'Composition', headerText: 'Composition', allowEditing: false, width: 120, visible: masterData.SubGroupID == 1
            },
            {
                field: 'ColorName', headerText: 'Color', allowEditing: false, width: 70
            },
            {
                field: 'GSM', headerText: 'GSM', allowEditing: false, width: 30, textAlign: 'Center', headerTextAlign: 'Center', visible: masterData.SubGroupID == 1
            },
            {
                field: 'Size', headerText: 'Size', width: 40, visible: masterData.SubGroupID != 1, allowEditing: false
            },
            //{
            //    field: 'StartDate', headerText: 'Start Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 20
            //},
            //{
            //    field: 'EndDate', headerText: 'End Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 20
            //},
            {
                field: 'ProdQty', headerText: 'Prod Qty (KG)', editType: "numericedit", allowEditing: true,
                edit: { params: { showSpinButton: false, decimals: 2, format: "N0", min: 1, validateDecimalOnType: true } }, width: 100
            },
            {
                field: 'ProdQtyPcs', headerText: 'Prod Qty (PCS)', editType: "numericedit", allowEditing: true,
                edit: { params: { showSpinButton: false, decimals: 0, format: "N0", min: 0, validateDecimalOnType: true } }, width: 100
            },
        ];

        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            //tableId: tblChildId,
            dataSource: data,
            //toolbar: ['Add'],
            allowResizing: true,
            showColumnChooser: true,
            showDefaultToolbar: false,
            commandClick: childCommandClick,
            editSettings: {
                allowAdding: true,
                allowEditing: true,
                allowDeleting: true,
                mode: "Normal",
                showDeleteConfirmDialog: true
            },
            columns: parentColumns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    var kjCardMasterID = KJobCardMasterID++;
                    tempKJobCardMasterId = kjCardMasterID;
                    args.data.KJobCardMasterID = kjCardMasterID;
                    args.rowData.KJobCardMasterID = kjCardMasterID;

                    masterData.KnittingPlanChilds.map(x => {
                        var newChild = DeepClone(x);
                        newChild.KJobCardMasterID = kjCardMasterID;
                        newChild.KJobCardChildID = KJobCardChildID++;
                        newChildList.push(DeepClone(newChild));

                        var mObj = masterData.KnittingPlans.find(x => x.KPMasterID = x.KPMasterID);
                        if (mObj) {
                            args.data.ConceptID = mObj.ConceptID;
                        }
                    });
                    var child = null,
                        childs = [];
                    if (newChildList) {
                        childs = newChildList.filter(x => x.KJobCardMasterID == kjCardMasterID);
                        if (childs) child = childs[0];
                    }

                    args.data.KJobCardNo = "**<< NEW >>**";
                    args.data.MachineGauge = masterData.MachineGauge;
                    args.data.MachineDia = masterData.MachineDia;
                    args.data.MCSubClassName = masterData.MCSubClass;
                    var firstChild = child ? DeepClone(child) : null;
                    if (firstChild) {
                        args.data.KnittingTypeID = firstChild.KnittingTypeID;
                        args.data.MachineType = firstChild.KnittingType;
                        args.data.MCSubClassID = firstChild.MCSubClassID;
                        args.data.MCSubClassName = firstChild.MCSubClassName;
                        var totalKJobCardQty = 0;
                        childs.map(x => {
                            totalKJobCardQty += x.ProdQty;
                        });
                        args.data.KJobCardQty = totalKJobCardQty;
                        args.data.KJChilds = childs;
                    }
                    //masterData.KJobCardMasters.push(DeepClone(args.data));
                }
                else if (args.requestType === "save") {
                    var indexF = masterData.KJobCardMasters.findIndex(x => x.KJobCardMasterID == args.data.KJobCardMasterID);
                    if (indexF < 0) {
                        masterData.KJobCardMasters.push(DeepClone(args.data));
                        masterData.KJobCardMasters = masterData.KJobCardMasters.reverse();
                        tempKJobCardMasterId = args.data.KJobCardMasterID;
                    } else {
                        masterData.KJobCardMasters[indexF] = args.data;
                    }
                    initChildTable(masterData.KJobCardMasters);
                }
                else if (args.requestType === "delete") {
                    //
                    //args.data.KJChilds = childs;
                }
            },
            childGrid: {
                queryString: 'KJobCardMasterID',
                allowResizing: true,
                autofitColumns: false,
                //toolbar: [{ text: 'Add Item', tooltipText: 'Add Item', prefixIcon: 'e-icons e-add', id: 'addItem' }],
                editSettings: {
                    allowAdding: false,
                    allowEditing: true,
                    allowDeleting: true,
                    mode: "Normal",
                    showDeleteConfirmDialog: true
                },
                //toolbar: ['Add'],
                //editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                toolbarClick: function (args) {
                    //var abc = this.parentDetails.parentRowData.PostFinishingProcessChilds;
                    var evt = args;
                    if (args.item.id === "addItem") {
                        var allChilds = getChilds();
                        var finder = new commonFinder({
                            title: "Select Item",
                            pageId: pageId,
                            data: allChilds,
                            fields: "Composition,ColorName,GSM,Size,ProdQty,ProdQtyPcs",
                            headerTexts: "Composition,Color,GSM,Size,Prod Qty,Prod Qty (Pcs)",
                            isMultiselect: true,
                            allowPaging: false,
                            primaryKeyColumn: "KJobCardChildID",
                            onMultiselect: function (selectedRecords) {
                                if (selectedRecords) {
                                    for (var i = 0; i < selectedRecords.length; i++) {
                                        var obj = DeepClone(selectedRecords[i]);
                                        obj.KJobCardMasterID = tempKJobCardMasterId;

                                        var childs = masterData.KJobCardMasters.find(x => x.KJobCardMasterID == obj.KJobCardMasterID).KJChilds;

                                        var indexF = -1;
                                        if (masterData.SubGroupID == 1) {
                                            indexF = childs.findIndex(c => c.Composition == obj.Composition && c.ColorName == obj.ColorName && c.GSM == obj.GSM);
                                        } else {
                                            indexF = childs.findIndex(c => c.Size == obj.Size);
                                        }
                                        if (indexF < 0) {
                                            obj.KJobCardChildID = KJobCardChildID++;
                                            masterData.KJobCardMasters.find(x => x.KJobCardMasterID == obj.KJobCardMasterID).KJChilds.push(obj);
                                            newChildList.push(obj);
                                        }
                                    }
                                    masterData.KJobCardMasters.map(x => {
                                        x.Childs = x.KJChilds;
                                        var totalKJobCardQty = 0;
                                        x.Childs.map(c => {
                                            totalKJobCardQty += c.KJobCardQty;
                                        });
                                        x.KJobCardQty = totalKJobCardQty;
                                    });
                                    var indexFind = masterData.KJobCardMasters.findIndex(x => x.KJobCardMasterID == tempKJobCardMasterId);
                                    $tblChildEl.updateRow(indexFind, masterData.KJobCardMasters[indexFind]);
                                    $tblChildEl.refreshColumns;
                                }
                            }
                        });
                        finder.showModal();
                    }
                },
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {

                    }
                    else if (args.requestType === "add") {

                    }
                    else if (args.requestType === "save") {
                        var totalJobCardQtyKg = 0,
                            totalJobCardQtyPcs = 0;

                        masterData.KJobCardMasters.map(m => {
                            var child = m.KJChilds.find(x => x.KJobCardChildID == args.data.KJobCardChildID);
                            if (child != null) {
                                var indexF = m.KJChilds.findIndex(x => x.KJobCardChildID == child.KJobCardChildID);
                                if (indexF > -1) {
                                    m.KJChilds[indexF] = args.data;
                                }
                            }
                            m.KJChilds.map(c => {
                                totalJobCardQtyKg += parseFloat(c.ProdQty);
                                totalJobCardQtyPcs += parseFloat(c.ProdQtyPcs);
                            });
                            m.KJobCardQty = masterData.SubGroupID == 1 ? totalJobCardQtyKg : totalJobCardQtyPcs;

                            //var indexFind = masterData.KJobCardMasters.findIndex(x => x.KJobCardMasterID == m.KJobCardQty);
                            //$tblChildEl.updateRow(indexFind, masterData.KJobCardMasters[indexFind]);
                            //$tblChildEl.refreshColumns;
                        });
                        setFormQtys(masterData.SubGroupID == 1 ? totalJobCardQtyKg : totalJobCardQtyPcs, false);

                        //ProdQtyPcs = 60
                    }
                    else if (args.requestType === "delete") {
                        var objMaster = masterData.KJobCardMasters.find(x => x.KJobCardMasterID == args.data[0].KJobCardMasterID);
                        var childs = objMaster.KJChilds;

                        var indexF = -1,
                            glbF = -1;

                        if (masterData.SubGroupID == 1) {
                            indexF = childs.findIndex(x => x.Composition == args.data[0].Composition && x.ColorName == args.data[0].ColorName && x.GSM == args.data[0].GSM);
                            glbF = newChildList.findIndex(x => x.Composition == args.data[0].Composition && x.ColorName == args.data[0].ColorName && x.GSM == args.data[0].GSM);
                        } else {
                            indexF = childs.findIndex(x => x.Size == args.data[0].Size);
                            glbF = newChildList.findIndex(x => x.Size == args.data[0].Size);
                        }

                        if (indexF > -1) {
                            masterData.KJobCardMasters.find(x => x.KJobCardMasterID == args.data[0].KJobCardMasterID).KJChilds.splice(indexF, 1);
                            masterData.KJobCardMasters.map(x => {
                                x.Childs = x.KJChilds;
                                var totalKJobCardQty = 0;
                                x.Childs.map(c => {
                                    totalKJobCardQty += c.KJobCardQty;
                                });
                                x.KJobCardQty = totalKJobCardQty;
                            });
                            var indexFind = masterData.KJobCardMasters.findIndex(x => x.KJobCardMasterID == tempKJobCardMasterId);
                            $tblChildEl.updateRow(indexFind, masterData.KJobCardMasters[indexFind]);
                            $tblChildEl.refreshColumns;
                        }
                        if (glbF > -1) {
                            newChildList.splice(glbF, 1);
                        }
                    }
                },
                load: loadChilds
            },
            /*
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    //var maxId = Math..apply(Math, data.map(function (el) { return el["Distribution"]; }));
                    var totalDis = 0, remainDis = 0;
                    this.dataSource.forEach(l => {
                        totalDis += l.Distribution;
                    })
                    if (totalDis < 100) remainDis = 100 - totalDis;
                    else {
                        toastr.error("Distribution can not more then 100!!");
                        args.cancel = true;
                        return;
                    }
                    var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(remainDis) / 100);
                    var reqQty = netConsumption;
                    args.data.Distribution = remainDis;
                    args.data.BookingQty = netConsumption.toFixed(4);
                    args.data.Allowance = 0.00;
                    args.data.ReqQty = reqQty.toFixed(2);

                    args.data.FCMRChildID = maxColYarn++;
                    args.data.FCMRMasterID = this.parentDetails.parentKeyFieldValue;
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.FCMRChildID);
                    var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(args.data.Distribution) / 100);
                    var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(args.data.Allowance)) / 100);
                    args.data.BookingQty = netConsumption.toFixed(4);
                    args.data.ReqQty = reqQty.toFixed(2);
                }
            },
            childGrid: {
                queryString: 'FCMRMasterID',
                allowResizing: true,
                autofitColumns: false,
                toolbar: ['Add'],
                editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
                columns: childColumns,
                actionBegin: function (args) {
                    if (args.requestType === 'beginEdit') {
                        if (args.rowData.YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                    }
                    else if (args.requestType === "add") {
                        var totalDis = 0, remainDis = 0;
                        this.dataSource.forEach(l => {
                            totalDis += l.Distribution;
                        })
                        if (totalDis < 100) remainDis = 100 - totalDis;
                        else {
                            toastr.error("Distribution can not more then 100!!");
                            args.cancel = true;
                            return;
                        }
                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(remainDis) / 100);
                        var reqQty = netConsumption;
                        args.data.Distribution = remainDis;
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.Allowance = 0.00;
                        args.data.ReqQty = reqQty.toFixed(2);

                        args.data.FCMRChildID = maxColYarn++;
                        args.data.FCMRMasterID = this.parentDetails.parentKeyFieldValue;
                        args.data.ReqCone = 1;
                    }
                    else if (args.requestType === "save") {
                        //Check YDItem when YD field check
                        if (args.data.YD) {
                            args.data.YDItem = true;
                        }

                        var netConsumption = (parseFloat(this.parentDetails.parentRowData.TotalQty) * parseFloat(args.data.Distribution) / 100);
                        var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(args.data.Allowance)) / 100);
                        args.data.BookingQty = netConsumption.toFixed(4);
                        args.data.ReqQty = reqQty.toFixed(2);
                        args.data = getFreeConceptMRChild(args.data);
                        $tblChildEl.updateRow(args.rowIndex, args.data);
                    }
                    else if (args.requestType === "delete") {
                        if (args.data[0].YDProductionMasterID > 0) {
                            toastr.error("Yarn Dyeing found, You cannot modify anything.");
                            args.cancel = true;
                        }
                        //var index = $tblChildEl.getRowIndexByPrimaryKey(args.data[0].FCMRChildID);
                        //if (index > -1) {
                        //    args.data.EntityState = 8;
                        //    masterData.Childs[index] = args.data;
                        //}
                    }
                },
                load: loadChilds
            },
            enableContextMenu: true,
            contextMenuItems: [
                { text: 'Copy Yarn Information', target: '.e-content', id: 'copy' },
                { text: 'Paste Yarn Information', target: '.e-content', id: 'paste' }
            ],
            contextMenuClick: function (args) {
                if (args.item.id === 'copy') {
                    fabYarnItem = objectCopy(args.rowInfo.rowData.Childs);
                    if (fabYarnItem.length == 0) {
                        toastr.error("No Yarn information found to copy!!");
                        return;
                    }
                }
                else if (args.item.id === 'paste') {
                    var rowIndex = args.rowInfo.rowIndex;
                    if (fabYarnItem == null || fabYarnItem.length == 0) {
                        toastr.error("Please copy first!!");
                        return;
                    } else {
                        for (var i = 0; i < fabYarnItem.length; i++) {
                            //var parentRowData = $tblChildEl.getRowByIndex($tblChildEl.getRowIndexByPrimaryKey(args.rowInfo.rowData.FCMRMasterID));
                            var copiedItem = objectCopy(fabYarnItem[i]);
                            copiedItem.FCMRChildID = maxColYarn++;
                            copiedItem.FCMRMasterID = args.rowInfo.rowData.FCMRMasterID;
                            var netConsumption = (parseFloat(args.rowInfo.rowData.TotalQty) * parseFloat(copiedItem.Distribution) / 100);
                            var reqQty = parseFloat(netConsumption) + ((parseFloat(netConsumption) * parseFloat(copiedItem.Allowance)) / 100);
                            copiedItem.BookingQty = netConsumption.toFixed(4);
                            copiedItem.ReqQty = reqQty.toFixed(2);
                            //$tblChildEl.addRecord(copiedItem, 'Child', args.rowInfo.rowIndex);
                            args.rowInfo.rowData.Childs.push(copiedItem);
                        }
                        $tblChildEl.refresh();
                    }
                }
            }
            */
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    async function childCommandClick(e) {
        var childData = e.rowData;

        if (e.commandColumn.buttonOption.type == 'mainMachine' && childData) {
            //if (childData.ContactID == 0) {
            //    return toastr.error("Select Floor/Sub-Contractor.");
            //}
            //var url = `/api/selectoption/get-machine-by-contact-knitting-type-gauge-dia-finder/${childData.MCSubClassID}/${childData.BrandID}/${childData.ContactID}/${childData.KnittingTypeID}/${masterData.MachineDia}`;
            var url = `/api/knitting-program/get-machine-by-gauge-dia-finder/${masterData.MachineGauge}/${masterData.MachineDia}`;
            var finder = new commonFinder({
                title: "Select Machine",
                pageId: pageId,
                height: 320,
                apiEndPoint: url,
                fields: "GG,Dia,Brand,Contact,MachineNo,Capacity,IsSubContact",
                headerTexts: "GG,Dia,Brand,Unit,MachineNo,Capacity,SubContact?",
                widths: "30,30,100,70,50,50,50",
                customFormats: "",
                isMultiselect: false,
                primaryKeyColumn: "KnittingMachineID",//"id",
                onSelect: function (selectedRecord) {
                    finder.hideModal();
                    childData.KnittingMachineID = selectedRecord.rowData.KnittingMachineID;
                    childData.Machine = selectedRecord.rowData.MachineNo;
                    childData.UnitID = selectedRecord.rowData.ContactID;
                    masterData.ContactID = selectedRecord.rowData.ContactID;
                    childData.BrandID = selectedRecord.rowData.BrandID;
                    childData.Brand = selectedRecord.rowData.Brand;
                    var obj = masterData.KJobCardMasters.find(x => x.KJobCardMasterID == childData.KJobCardMasterID);
                    if (obj) {
                        masterData.KJobCardMasters.find(x => x.KJobCardMasterID == childData.KJobCardMasterID).KnittingMachineID = childData.KnittingMachineID;
                    }
                    var index = $tblChildEl.getRowIndexByPrimaryKey(childData.KJobCardMasterID);
                    $tblChildEl.updateRow(index, childData);
                }
            });
            finder.showModal();
        }
        else if (e.commandColumn.type == "Report") {
            if (e.rowData.SubGroupName === 'Fabric') {
                window.open(`/reports/InlinePdfView?ReportName=KnittingJobCardNew.rdl&JobCardNo=${e.rowData.KJobCardNo}`, '_blank');
            } else {
                window.open(`/reports/InlinePdfView?ReportName=KnittingJobCardCCNew.rdl&JobCardNo=${e.rowData.KJobCardNo}`, '_blank');
            }
        }
    }
    function loadChilds() {
        var childs = [];
        tempKJobCardMasterId = this.parentDetails.parentRowData.KJobCardMasterID;
        if (this.parentDetails.parentRowData.Childs) {
            childs = this.parentDetails.parentRowData.Childs;
        } else {

            childs = newChildList.filter(x => x.KJobCardMasterID == this.parentDetails.parentRowData.KJobCardMasterID);
            //childs = masterData.KnittingPlanChilds;
        }
        this.dataSource = DeepClone(childs);
    }
    function getChilds() {
        return masterData.KnittingPlanChilds;
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData();
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

    function getNew(groupId) {
        axios.get(`/api/Knitting-JobCard/new/${groupId}`)
            .then(function (response) {
                $formEl.find("#btnSelectColor").show();

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;

                if (masterData.SubGroupID == 1) {
                    $formEl.find(".divForFabric").fadeIn();
                } else {
                    $formEl.find(".divForFabric").fadeOut();
                }

                masterData.ReqDeliveryDate = formatDateToDefault(masterData.ReqDeliveryDate);
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);

                masterData.KnittingTypeList = masterData.KnittingTypeList;
                masterData.KnittingTypeID = masterData.KnittingTypeID;

                setFormData($formEl, masterData);

                if (masterData.NeedPreFinishingProcess) {
                    $formEl.find("#need-yes").prop("checked", true);
                    $formEl.find("#need-no").prop("checked", false);
                }
                else {
                    $formEl.find("#need-yes").prop("checked", false);
                    $formEl.find("#need-no").prop("checked", true);
                }
                initChildTable([]);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getBrandByGaugeAndSubclass(subClassId, machineGauge, knittingTypeId, rowData) {
        var url = `/api/selectoption/get-brand-by-machine-gauge-and-subclass/${subClassId}/${machineGauge}/${knittingTypeId}`;

        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0)
                    return toastr.warning("No data Found!");
                var list = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select One", list, "", function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected!");

                    var selectedRes = list.find(function (el) { return el.value === result })
                    rowData.BrandID = (result == "") ? 0 : result;
                    rowData.Brand = selectedRes.text;
                    //$tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getContact(subClassID, machineGauge, brandID, isSubContact, knittingTypeId, rowData) {
        var url = '';
        if (isSubContact)
            url = `/api/selectoption/get-contacts-by-contact-category/${contactCategoryConstants.SUB_CONTUCT}`;
        else
            url = `/api/selectoption/get-sub-contract/${subClassID}/${machineGauge}/${brandID}/${knittingTypeId}`;

        axios.get(url)
            .then(function (response) {
                //if (response.data.length == 0)
                //    return toastr.warning("No Contact Found!");
                var contactList = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select Contact", contactList, "", function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected any Contact!");

                    var selectedContact = contactList.find(function (el) { return el.value === result })
                    rowData.ContactID = (result == "") ? 0 : result;
                    rowData.Contact = selectedContact.text;
                    //$tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getMachineDia(subClassID, machineGauge, brandID, contactID, knittingTypeId, rowData) {
        var url = `/api/selectoption/get-machine-dia/${subClassID}/${machineGauge}/${brandID}/${contactID}/${knittingTypeId}`;

        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0)
                    return toastr.warning("No data Found!");
                var list = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select One", list, "", function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected!");

                    //var selectedRes = list.find(function (el) { return el.value === result })
                    rowData.MachineDia = (result == "") ? 0 : result;
                    //rowData.MachineDia = selectedRes.text;
                    // $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getMachine(rowData) {
        var url = `/api/selectoption/get-machine-by-contact-knitting-type-gauge-dia/${KPChild.MCSubClassID}/${rowData.MachineGauge}/${rowData.BrandID}/${rowData.ContactID}/${rowData.MachineKnittingTypeID}/${rowData.MachineDia}`;
        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0)
                    return toastr.warning("No Machine Found!");
                var mcList = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select Machine", mcList, "", function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected any Machine!");
                    var selectedMC = mcList.find(function (el) { return el.value === result })
                    rowData.KnittingMachineID = (result == "") ? 0 : result;
                    rowData.KnittingMachineNo = selectedMC.text;
                    //$tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/Knitting-JobCard/${id}`)
            .then(function (response) {
                $formEl.find("#btnSelectColor").hide();

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReqDeliveryDate = formatDateToDefault(masterData.ReqDeliveryDate);
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);

                masterData.KnittingTypeList = masterData.KnittingTypeList;
                masterData.KnittingTypeID = masterData.KnittingTypeID;

                setFormData($formEl, masterData);
                initChildTable(masterData.KJobCardMasters);

                var kJobCardQty = 0;
                masterData.KJobCardMasters.map(x => {
                    kJobCardQty += x.KJobCardQty
                });
                setFormQtys(kJobCardQty, false, masterData.PlanQty);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getRevision(id) {
        axios.get(`/api/Knitting-JobCard/revision/${id}`)
            .then(function (response) {
                $formEl.find("#btnSelectColor").hide();

                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReqDeliveryDate = formatDateToDefault(masterData.ReqDeliveryDate);
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);

                masterData.KnittingTypeList = masterData.KnittingTypeList;
                masterData.KnittingTypeID = masterData.KnittingTypeID;

                setFormData($formEl, masterData);
                initChildTable(masterData.KJobCardMasters);

                var kJobCardQty = 0;
                masterData.KJobCardMasters.map(x => {
                    kJobCardQty += x.KJobCardQty
                });
                setFormQtys(kJobCardQty, false, masterData.PlanQty);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getObjWithChilds(data) {
        data.KJobCardMasters.map(m => {
            if (m.Childs && m.Childs.length > 0) {
                m.KnittingPlanChilds = m.Childs;
            } else {
                m.KnittingPlanChilds = newChildList.filter(x => x.KJobCardMasterID == m.KJobCardMasterID);
                m.Childs = m.KnittingPlanChilds;
            }
        });
        return data;
    }
    function save(isRevision) {
        masterData["KJobCardMasters"] = $tblChildEl.getCurrentViewRecords();
        var colors = [];
        debugger;
        //KnittingMachineID = 5
        for (var index = 0; index < masterData.KJobCardMasters.length; index++) {
            if (typeof masterData.KJobCardMasters[index].KnittingMachineID === "undefined" ||
                masterData.KJobCardMasters[index].KnittingMachineID == 0) {
                toastr.error("Select machine.");
                return false;
            }
            if (masterData.KJobCardMasters[index].KJChilds.length == 0) {
                toastr.error("No color found.");
                return false;
            }
            if (masterData.KJobCardMasters[index].KJChilds.length > 1) {
                toastr.error("Multi color for one job card is not allowed");
                return false;
            }

            masterData.KJobCardMasters[index].GroupID = masterData.GroupID;
            masterData.KJobCardMasters[index].BuyerID = masterData.BuyerID;
            masterData.KJobCardMasters[index].BuyerTeamID = masterData.BuyerTeamID;
            masterData.KJobCardMasters[index].ItemMasterID = masterData.ItemMasterID;
            masterData.KJobCardMasters[index].IsSubContact = masterData.IsSubContact;
            masterData.KJobCardMasters[index].ContactID = masterData.ContactID;
            masterData.KJobCardMasters[index].MachineKnittingTypeID = masterData.MachineKnittingTypeID;
            //masterData.KJobCardMasters[index].KnittingMachineID = masterData.KnittingMachineID;
            masterData.KJobCardMasters[index].BookingID = masterData.BookingID;
            masterData.KJobCardMasters[index].SubGroupID = masterData.SubGroupID;
            masterData.KJobCardMasters[index].KJChilds.map(c => {
                colors.push(c.ColorName);
            });
        }
        //var unique_array = [...new Set(colors)];

        //if (masterData.KJobCardMasters.length != unique_array.length) {
        //    toastr.error("Multi color for one job card is not allowed");
        //    return;
        //}
        //if (unique_array.length != colors.length) {
        //    toastr.error("One color should have one job card.");
        //    return;
        //}

        var data = masterData;
        data.isModified = status == statusConstants.PENDING ? false : true;
        if (isRevision) {
            data.IsRevision = isRevision;
        }

        axios.post("/api/Knitting-JobCard/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
                initMasterTable();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function LoadMasters(colorName) {
        var masterList = [];
        masterData.KJobCardMasters = [];
        var colors = [] //masterData.KnittingPlanChilds.map(x => x.ColorName);
        colors.push(colorName);
        colors = [...new Set(colors)];
        colors.map(color => {
            var colorWiseKPC = masterData.KnittingPlanChilds.filter(x => x.ColorName == color);
            var kpMaster = masterData.KnittingPlans.find(x => x.KPMasterID == colorWiseKPC[0].KPMasterID);
            var kjCardMasterID = KJobCardMasterID++;
            var masterObj = {
                KJobCardMasterID: kjCardMasterID,
                KJobCardNo: "**<< NEW >>**",
                MachineGauge: masterData.MachineGauge,
                MachineDia: masterData.MachineDia,
                MCSubClassName: masterData.MCSubClass,
                ConceptID: kpMaster.ConceptID,
                KJobCardQty: 0,
                KJChilds: []
            };
            colorWiseKPC.map(kpc => {
                if (masterData.SubGroupID == 1) {
                    masterObj.KJobCardQty += kpc.ProdQty;
                } else {
                    masterObj.KJobCardQty += kpc.ProdQtyPcs;
                }

                var newChild = DeepClone(kpc);
                newChild.KJobCardMasterID = kjCardMasterID;
                newChild.KJobCardChildID = KJobCardChildID++;
                newChild = DeepClone(newChild);
                masterObj.KJChilds.push(newChild);
                newChildList.push(newChild);
            });
            masterObj.KJobCardQty = parseFloat(masterObj.KJobCardQty.toFixed(2));
            masterList.push(masterObj);
            masterData.KJobCardMasters.push(masterObj);

            //var planQty = parseFloat($formEl.find("#PlanQty").val());
            setFormQtys(masterObj.KJobCardQty, true, masterObj.KJobCardQty);
        });
        initChildTable(masterList);
    }
    function setFormQtys(kJobCardQty, isSetPlanQty, planQty) {
        if (isSetPlanQty) {
            $formEl.find("#PlanQty").val(planQty);
        }
        planQty = $formEl.find("#PlanQty").val();
        var balanceQty = parseFloat(planQty) - parseFloat(kJobCardQty);
        $formEl.find("#JobCardQty").val(kJobCardQty);
        $formEl.find("#BalanceQty").val(balanceQty);
    }
    function DeepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
})();