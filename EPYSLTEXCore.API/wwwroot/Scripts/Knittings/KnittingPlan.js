(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, $tblYarnEl, tblMasterId;
    var status;
    var masterData;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $tblYarnEl = $("#tblKnittingPlanYarn" + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING;
        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });

        $toolbarEl.find("#btnActiveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ACTIVE;
            initMasterTable();
        });

        $toolbarEl.find("#btnInActiveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.IN_ACTIVE;
            initMasterTable();
        });

        $toolbarEl.find("#btnAllList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.ALL;
            initMasterTable();
        });

        $toolbarEl.find("#btnRefreshList").on("click", function (e) {
            e.preventDefault();
            $tblMasterEl.refresh();
        });
        $('#MCSubClassID').on('select2:select', function (e) {
            var subClassId = e.params.data.id;
            var url = `/api/selectoption/get-machine-by-subclass/${subClassId}`;
            axios.get(url)
                .then(function (response) {
                    masterData.KnittingTypeID = response.data[0].id;
                })
                .catch(function (err) {
                    toastr.error(err.response.data.Message);
                });
        });
        //$formEl.find("#KnittingTypeID").on("select2:select", function (e) {
           
        //    if (masterData.KnittingTypeID != e.params.data.id) {
        //        for (var i = 0; i < masterData.Childs.length; i++) {
        //            masterData.Childs[i].MCSubClassID = 0;
        //            masterData.Childs[i].MCSubClassName = "Empty";
        //        }
        //        $tblChildEl.bootstrapTable("refresh");
        //    }
        //    masterData.KnittingTypeID = e.params.data.id;
        //    masterData.KnittingType = e.params.data.text;

        //})

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnSaveAndAapprove").click(function (e) {
            e.preventDefault();
            save(this, true);
        });
        $('#MCSubClassID').on('select2:select', function (e) {
            masterData.MCSubClassID = e.params.data.id;
        });

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function initMasterTable() {
        var columns = [
            {
                headerText: 'Actions',commands: [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }], visible: false
            },
            {
                field: 'PlanNo', headerText: 'PlanNo', visible: status != statusConstants.PENDING
            },
            {
                field: 'ReqDeliveryDate', headerText: 'Req.Delivery', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING
            },
            {
                field: 'PlanQty', headerText: 'PlanQty', visible: status != statusConstants.PENDING
            },
            {
                field: 'SubGroupName', headerText: 'Sub Group', visible: status != statusConstants.PENDING
            },
            {
                field: 'BookingNo', headerText: 'Booking No'
            },
            {
                field: 'YBookingNo', headerText: 'Yarn Booking No'
            },
            {
                field: 'Buyer', headerText: 'Buyer'
            },
            {
                field: 'BuyerTeam', headerText: 'Buyer Team'
            },
            {
                field: 'StyleNo', headerText: 'Style No'
            },
            {
                field: 'SeasonName', headerText: 'Season'
            },
            {
                field: 'EWONo', headerText: 'EWO No'
            },
            {
                field: 'Company', headerText: 'Company'
            },
            {
                field: 'CompletionStatus', headerText: 'CompletionStatus', type: 'boolean', visible: status != statusConstants.PENDING
            },
            {
                field: 'Active', headerText: 'Active', type: 'boolean', visible: status != statusConstants.PENDING
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/knitting-program/list?type=${knittingProgramType.BULK}&status=${status}`,
            columns: columns,
            hasChildGrid: true,
            autofitColumns: status !== statusConstants.PENDING,
            childOptions: {
                queryString: "YBookingNo",
                apiEndPoint: `/api/knitting-program/booking-child-list/${knittingProgramType.BULK}`,
                allowPaging: false,
                allowFiltering: false,
                hasChildGrid: true,
                columns: [
                    {
                        field: 'SubGroupName', headerText: 'Item Group'
                    },
                    {
                        field: 'BookingDate', headerText: 'BookingDate', textAlign: 'Right', type: 'date', format: _ch_date_format_1,
                    },
                    {
                        field: 'RevisionNo', headerText: 'RevisionNo'
                    },
                    {
                        field: 'BookingQty', headerText: 'Booking Qty'
                    },
                    {
                        field: 'Unit', headerText: 'Unit'
                    }
                ],
                childOptions: {
                    queryString: "YBookingID",
                    apiEndPoint: `/api/knitting-program/booking-child-details-list/${status}/${knittingProgramType.BULK}`,
                    allowPaging: false,
                    allowFiltering: false,
                    commandClick: handleCommands,
                    columns: [
                        {
                            headerText: "Actions" , commands: [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }], width: 100
                        },
                        {
                            field: 'ItemName', headerText: 'Item Name'
                        },
                        {
                            field: 'BookingQty', headerText: 'Booking Qty', width: 100
                        },
                        {
                            field: 'Unit', headerText: 'Unit', width: 100
                        },
                        {
                            field: 'KnittingPlanned', headerText: 'Knitting Planned?', displayAsCheckBox: true
                        }
                    ],
                }
            }
        });
    }

    function handleCommands(args) {
        if (status === statusConstants.PENDING && !args.rowData.KnittingPlanned) {
            getNew(args.rowData.YBookingID, args.rowData.ItemMasterID);
        }
        else {
            getDetails(args.rowData.Id);
        }
    }

    function initTblYarn(records) {
        $tblYarnEl.bootstrapTable("destroy");
        $tblYarnEl.bootstrapTable({
            uniqueId: 'Id',
            editable: true,
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    width: 200,
                    formatter: function (value, row, index, field) {
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-success btn-xs add" href="javascript:void(0)" title="Add a row like this">',
                            '<i class="fa fa-plus"></i>',
                            '</a>',
                            '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Remove this row">',
                            '<i class="fa fa-remove"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .add': function (e, value, row, index) {
                            addNewYarnItem(row);

                        },
                        'click .remove': function (e, value, row, index) {
                            masterData.Yarns.splice(index, 1);
                            $tblYarnEl.bootstrapTable('load', masterData.Yarns);
                        }
                    }

                },
                {
                    field: "ItemName",
                    title: "Item",
                    align: 'center'
                },
                {
                    field: "YarnCount",
                    title: "Yarn Count",
                    align: 'center'
                },
                {
                    field: "YarnType",
                    title: "Yarn Type",
                    align: 'center'
                },
                {
                    field: "YarnLotNo",
                    title: "Yarn Lot",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                    }
                },
                {
                    field: "YarnBrand",
                    title: "Yarn Brand",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                    }
                },
                {
                    field: "StitchLength",
                    title: "Stitch Length",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="decimal" class="form-control input-sm" min="0" style="padding-right: 24px;">',
                        validate: function (value) {
                            if (!value || !value.trim() || isNaN(parseFloat(value)) || parseFloat(value) <= 0) {
                                return 'Must be a positive integer.';
                            }
                        }
                    }
                }
            ],
            data: records
        });
    }
    function addNewYarnItem(row) {
        var obj = {
            Id: getMaxIdForArray(masterData.Yarns, "Id"),
            KPMasterID: row.KPMasterID,
            ItemMasterID: row.ItemMasterID,
            ItemName: row.ItemName,
            YarnCountID: row.YarnCountID,
            YarnTypeID: row.YarnTypeID,
            YarnCount: row.YarnCount,
            YarnType: row.YarnType,
            YarnLotNo: row.YarnLotNo,
            YarnBrand: row.YarnBrand,
            StitchLength: 0
        }
        masterData.Yarns.push(obj);
        $tblYarnEl.bootstrapTable('load', masterData.Yarns);
    }
    function initTblChild() {
        $tblChildEl.bootstrapTable('destroy').bootstrapTable({
            uniqueId: 'ItemMasterID',

            //detailView: true,
            checkboxHeader: false,
            columns: [

                {
                    title: "Actions",
                    align: "center",
                    width: 200,
                    formatter: function (value, row, index, field) {
                        if (row.KJobCardNo == "**<< NEW >>**") {
                            return [
                                '<span class="btn-group">',
                                '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                                '<i class="fa fa-remove"></i>',
                                '</a>',
                                //'<a class="btn btn-success btn-xs addSame" href="javascript:void(0)" title="Add a row like this">',
                                //'<i class="fa fa-plus"></i>',
                                //'</a>',
                                '</span>'
                            ].join('');
                        } else {
                            return [
                                '<span class="btn-group">',
                                '<a class="btn btn-danger btn-xs remove" href="javascript:void(0)" title="Delete Item">',
                                '<i class="fa fa-remove"></i>',
                                '</a>',
                                //'<a class="btn btn-success btn-xs addSame" href="javascript:void(0)" title="Add a row like this">',
                                //'<i class="fa fa-plus"></i>',
                                //'</a>',
                                `<a class="btn btn-xs btn-primary" href="/reports/InlinePdfView?ReportId=1240&JobCardNo=${row.KJobCardNo}" target="_blank" title="Job Card Report">
                                <i class="fa fa-file-pdf-o" aria-hidden="true"></i>
                                </a>`,
                                '</span>'
                            ].join('');
                        }

                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            masterData.Childs.splice(index, 1);
                            $formEl.find("#tblFabricInformation").bootstrapTable('load', masterData.Childs);
                        }
                       
                    }
                },
                {
                    field: "ItemName",
                    title: "Item",
                    cellStyle: function () { return { classes: 'm-w-200' } }
                },
              
                {
                    field: "MachineGauge",
                    title: "Machine Gauge",
                    width: 100,
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + row.MachineGauge + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getMachineGauge(masterData.MCSubClassID, row);
                        },
                    }
                },
                {
                    field: "MachineDia",
                    title: "Machine Dia",
                    width: 100,
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + row.MachineDia + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getChildMachineDia(masterData.MCSubClassID, row.MachineGauge, row);
                        },
                    }
                },
                {
                    field: "IsSubContact",
                    title: "Sub Contact?",
                    width: 80,
                    checkbox: true,
                    showSelectTitle: true
                },
                {
                    field: "ContactID",
                    title: "Floor/Sub-Contractor",
                    width: 100,
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + row.Contact + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getChildContact(masterData.MCSubClassID, row.MachineGauge, row.MachineDia, row.IsSubContact, row);
                        },
                    }
                },
                {
                    field: "BrandID",
                    title: "Brand",
                    width: 100,
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + row.Brand + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getChildBrandByGaugeAndSubclass(masterData.MCSubClassID, row.MachineGauge, row.MachineDia, row.ContactID, row);
                        },
                    }
                },
                {
                    field: "KnittingMachineID",
                    title: "Machine",
                    width: 120,
                    formatter: function (value, row, index, field) {
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + row.KnittingMachineNo + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            if (row.ContactID <= 0) return toastr.error("Please select Contact!");
                            if (row.MachineDia <= 0) return toastr.error("Please Enter Machine Dia!");
                            if (row.MachineGauge <= 0) return toastr.error("Please Enter Machine Gauge!");
                            getChildMachine(row);
                        },
                    }
                },
                {
                    field: "KnittingTypeID",
                    title: "Knitting Type",
                    align: 'center',
                    editable: {
                        type: 'select2',
                        title: 'Select Knitting Type',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: masterData.KnittingTypeList,
                        select2: { width: 130, placeholder: 'Knitting Type', allowClear: true }
                    },

                },
                {
                    field: "StartDate",
                    title: "Start Date",
                    filterControl: "input",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="date" class="form-control input-sm" style="padding-right: 24px;">'
                    }
                },
                {
                    field: "EndDate",
                    title: "End Date",
                    filterControl: "input",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="date" class="form-control input-sm" style="padding-right: 24px;">'
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
                    field: "KJobCardNo",
                    title: "Job Card No",
                    width: 250
                },
                {
                    field: "Remarks",
                    title: "Remarks",
                    filterControl: "input",
                    align: 'center',
                    editable: {
                        type: "text",
                        showbuttons: false,
                        tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                    }
                }

            ],
            onCheck: function (row, $element) {
                ResetNextColoum(row);
            }
        });
    }
    //function addNewChildItem() {
    //    var obj = {
    //        Id: getMaxIdForArray(masterData.Childs, "Id"),
    //        ConceptID: masterData.ConceptID,
    //        SubGroupID: masterData.Childs[0].SubGroupID,
    //        YBookingID: masterData.Childs[0].YBookingID,
    //        ItemMasterID: masterData.Childs[0].ItemMasterID,
    //        FabricConstruction: "",//masterData.Construction,
    //        FabricComposition: "",//masterData.Composition,
    //        CCColorID: 0,
    //        ColorName: "",
    //        FabricGsm: masterData.Childs[0].FabricGsm,
    //        FabricWidth: masterData.Childs[0].FabricWidth,
    //        DyeingType: "",
    //        KnittingType: "", //masterData.KnittingType
    //        MachineDia: 0,
    //        MachineGauge: 0,
    //        StartDate: new Date(),
    //        EndDate: new Date(),
    //        UnitID: 28,
    //        Uom: "Kg",
    //        BookingQty: masterData.Childs[0].BookingQty,
    //        KJobCardQty: masterData.Childs[0].BookingQty,
    //        MCSubClassID: masterData.MCSubClassID,
    //        MCSubClassName: masterData.MCSubClassName,
    //        KJobCardNo: '**<< NEW >>**',
    //        Remarks: "",
    //        KJobCardMasters: [],
    //        Contact: "",
    //        Brand: "",
    //        KnittingMachineNo: ""
    //    }

    //    masterData.Childs.push(obj);
    //    $tblChildEl.bootstrapTable('load', masterData.Childs);
       
    //}
    function ResetNextColoum(rowData) {
        
        rowData.ContactID = 0;
        rowData.Contact = "Empty";
        rowData.BrandID = 0;
        rowData.Brand = "Empty";
        rowData.KnittingMachineID = 0;
        rowData.KnittingMachineNo = "Empty";
        $formEl.find("#tblFabricInformation").bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
    }
    function getMachineGauge(subClassId, rowData) {
        var url = `/api/selectoption/get-machine-gauge-by-subclass/${subClassId}`;

        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0)
                    return toastr.warning("No Gauge Found!");
                var list = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select One", list, "", function (result) {
                    if (!result) return toastr.warning("You didn't selected!");

                    if (rowData.MachineGauge != result) {
                        rowData.MachineDia = 0;
                        rowData.IsSubContact = false;
                        rowData.ContactID = 0;
                        rowData.Contact = "Empty";
                        rowData.BrandID = 0;
                        rowData.Brand = "Empty";
                        rowData.KnittingMachineID = 0;
                        rowData.KnittingMachineNo = "Empty";
                    }

                    rowData.MachineGauge = (result == "") ? 0 : result;
                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getChildMachineDia(subClassId, machineGauge, rowData) {
        var url = `/api/selectoption/get-machine-dia-by-subclass-guage/${subClassId}/${machineGauge}`;

        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0)
                    return toastr.warning("No Dia Found!");
                var list = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select One", list, "", function (result) {
                    if (!result) return toastr.warning("You didn't selected!");
                    if (rowData.MachineDia != result) {
                        rowData.IsSubContact = false;
                        rowData.ContactID = 0;
                        rowData.Contact = "Empty";
                        rowData.BrandID = 0;
                        rowData.Brand = "Empty";
                        rowData.KnittingMachineID = 0;
                        rowData.KnittingMachineNo = "Empty";
                        //for (var i = 0; i < rowData.KJobCardMasters.length; i++) {
                        //    $el = $("#TblJobCardTable-" + rowData.KJobCardMasters[i].Id);
                        //    $el.bootstrapTable('updateByUniqueId', { id: rowData.KJobCardMasters[i].Id, row: rowData.KJobCardMasters[i] });
                        //}
                    }

                    rowData.MachineDia = (result == "") ? 0 : result;
                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getChildContact(subClassId, machineGauge, machineDia, isSubContact, rowData) {
        var url = '';
        if (isSubContact)
            url = `/api/selectoption/get-contacts-by-contact-category/${contactCategoryConstants.SUB_CONTUCT}`;
        else
            url = `/api/selectoption/get-sub-contract-by-subclass-gg-dia/${subClassId}/${machineGauge}/${machineDia}`;

        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0)
                    return toastr.warning("No Contact Found!");
                var contactList = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select Contact", contactList, "", function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected any Contact!");
                    rowData.BrandID = 0;
                    rowData.Brand = "Empty";
                    rowData.KnittingMachineID = 0;
                    rowData.KnittingMachineNo = "Empty";
                    var selectedContact = contactList.find(function (el) { return el.value === result })
                    rowData.ContactID = (result == "") ? 0 : result;
                    rowData.Contact = selectedContact.text;
                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getChildBrandByGaugeAndSubclass(subClassId, machineGauge, machineDia, contactId, rowData) {
        var url = `/api/selectoption/get-brand-by-machine-gauge-dia-and-subclass/${subClassId}/${machineGauge}/${machineDia}/${contactId}`;

        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0)
                    return toastr.warning("No data Found!");
                var list = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select One", list, "", function (result) {
                    if (!result) return toastr.warning("You didn't selected!");

                    if (rowData.BrandID != result) {
                        rowData.KnittingMachineID = 0;
                        rowData.KnittingMachineNo = "Empty";
                    }

                    var selectedRes = list.find(function (el) { return el.value === result })
                    rowData.BrandID = (result == "") ? 0 : result;
                    rowData.Brand = selectedRes.text;
                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getChildMachine(rowData) {
        
        var url = `/api/selectoption/get-machine-by-contact-gauge-dia/${masterData.MCSubClassID}/${rowData.MachineGauge}/${rowData.BrandID}/${rowData.ContactID}/${rowData.MachineDia}`;
        axios.get(url)
            .then(function (response) {
                if (response.data.length == 0) return toastr.warning("No Machine Found!");
                var mcList = convertToSelectOptions(response.data);
                showBootboxSelectPrompt("Select Machine", mcList, "", function (result) {
                    if (!result) return toastr.warning("You didn't selected any Machine!");
                    var selectedMC = mcList.find(function (el) { return el.value === result })
                    rowData.KnittingMachineID = (result == "") ? 0 : result;
                    rowData.KnittingMachineNo = selectedMC.text;
                    $tblChildEl.bootstrapTable('updateByUniqueId', { id: rowData.Id, row: rowData });
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        $("#divFabricInformation").fadeOut();
        $("#divKnittingPlanYarn").fadeOut();
        $tblMasterEl.refresh();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#Id").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getNew(bookingId, itemMasterId) {
        
        axios.get(`/api/knitting-program/new/${knittingProgramType.BULK}/${bookingId}/${itemMasterId}`)
            .then(function (response) {


                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReqDeliveryDate = formatDateToDefault(masterData.ReqDeliveryDate);
                masterData.BookingDate = formatDateToDefault(masterData.BookingDate);
                setFormData($formEl, masterData);

                initTblChild();
                $tblChildEl.bootstrapTable("load", masterData.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
                $("#divItemInformation").fadeIn();

                initTblYarn(masterData.Yarns);
                $("#divKnittingPlanYarn").fadeIn();
                initNewAttachment($formEl.find("#UploadFile"));

                //if (masterData.SubGroupName === subGroupNames.FABRIC) {
                //    $("#divFabricInformation").fadeIn();
                //    $("#divCollarInformation").fadeOut();
                //    $("#divCuffInfomation").fadeOut();
                //    initTblChild();
                //    addNewChildItem();
                //} else if (masterData.SubGroupName === subGroupNames.COLLAR) {
                //    $("#divFabricInformation").fadeOut();
                //    $("#divCollarInformation").fadeIn();
                //    $("#divCuffInfomation").fadeOut();
                //    // init collar table
                //} else if (masterData.SubGroupName === subGroupNames.CUFF) {
                //    $("#divFabricInformation").fadeOut();
                //    $("#divCollarInformation").fadeIn();
                //    $("#divCuffInfomation").fadeIn();
                //    // init cuff table
                //}

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        
        axios.get(`/api/knitting-program/${knittingProgramType.BULK}/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.ReqDeliveryDate = formatDateToDefault(masterData.ReqDeliveryDate);
                masterData.StartDate = formatDateToDefault(masterData.StartDate);
                masterData.EndDate = formatDateToDefault(masterData.EndDate);
                masterData.ActualStartDate = formatDateToDefault(masterData.ActualStartDate);
                masterData.ActualEndDate = formatDateToDefault(masterData.ActualEndDate);
                masterData.BookingDate = formatDateToDefault(masterData.BookingDate);
               /* masterData.KnittingTypeID = masterData.Childs[0].KnittingTypeID;*/
              
                setFormData($formEl, masterData);
                
                initTblChild();
                $tblChildEl.bootstrapTable("load", masterData.Childs);
                $tblChildEl.bootstrapTable('hideLoading');
                $("#divItemInformation").fadeIn();

                initTblYarn(masterData.Yarns);
                $("#divKnittingPlanYarn").fadeIn();
                initAttachment(masterData.FilePath, masterData.AttachmentPreviewTemplate, $formEl.find("#UploadFile"));
                //if (masterData.SubGroupName === subGroupNames.FABRIC) {
                //    $("#divFabricInformation").fadeIn();
                //    $("#divCollarInformation").fadeOut();
                //    $("#divCuffInfomation").fadeOut();
                //    initTblChild();
                //    $tblChildEl.bootstrapTable("load", masterData.Childs);
                //    $tblChildEl.bootstrapTable('hideLoading');
                //    //getChildData();
                //} else if (masterData.SubGroupName === subGroupNames.COLLAR) {
                //    $("#divFabricInformation").fadeOut();
                //    $("#divCollarInformation").fadeIn();
                //    $("#divCuffInfomation").fadeOut();
                //    // init collar table
                //} else if (masterData.SubGroupName === subGroupNames.CUFF) {
                //    $("#divFabricInformation").fadeOut();
                //    $("#divCollarInformation").fadeIn();
                //    $("#divCuffInfomation").fadeIn();
                //    // init cuff table
                //}
            })
            .catch(showResponseError);
    }

    //function save(isApprove = false) {
    //    var kJobCardMaster = [];
    //    for (var i = 0; i < masterData.Childs.length; i++) {
    //        masterData.Childs[i].MCSubClassID = masterData.MCSubClassID;
    //        if (masterData.Childs[i].KJobCardMasters.length == 0) {
    //            kJobCardMaster = {
    //                ConceptID: masterData.ConceptID,
    //                KJobCardNo: masterData.Childs[i].KJobCardNo,
    //                KJobCardDate: masterData.Childs[i].StartDate,
    //                BrandID: masterData.Childs[i].BrandID,
    //                IsSubContact: masterData.Childs[i].IsSubContact,
    //                ContactID: masterData.Childs[i].ContactID,
    //                MachineDia: masterData.Childs[i].MachineDia,
    //                KnittingMachineID: masterData.Childs[i].KnittingMachineID,
    //                KJobCardQty: masterData.Childs[i].BookingQty,
    //                MachineKnittingTypeID: masterData.KnittingTypeID,
    //                MachineGauge: masterData.Childs[i].MachineGauge,
    //                UnitID: 28,
    //                BookingID: masterData.Childs[i].YBookingID,
    //                SubGroupID: masterData.SubGroupID,
    //                BookingQty: masterData.Childs[i].BookingQty,
    //                Remarks: masterData.Childs[i].Remarks
    //            }
    //            masterData.Childs[i]["KJobCardMasters"].push(kJobCardMaster);
    //        }
    //        else {
    //            for (var j = 0; j < masterData.Childs[i].KJobCardMasters.length; j++) {
    //                masterData.Childs[i].KJobCardMasters[j].ConceptID = masterData.ConceptID;
    //                masterData.Childs[i].KJobCardMasters[j].KJobCardNo = masterData.Childs[i].KJobCardNo;
    //                masterData.Childs[i].KJobCardMasters[j].KJobCardDate = masterData.Childs[i].StartDate;
    //                masterData.Childs[i].KJobCardMasters[j].BrandID = masterData.Childs[i].BrandID;
    //                masterData.Childs[i].KJobCardMasters[j].IsSubContact = masterData.Childs[i].IsSubContact;
    //                masterData.Childs[i].KJobCardMasters[j].ContactID = masterData.Childs[i].ContactID;
    //                masterData.Childs[i].KJobCardMasters[j].MachineDia = masterData.Childs[i].MachineDia;
    //                masterData.Childs[i].KJobCardMasters[j].KnittingMachineID = masterData.Childs[i].KnittingMachineID;
    //                masterData.Childs[i].KJobCardMasters[j].KJobCardQty = masterData.Childs[i].BookingQty;
    //                masterData.Childs[i].KJobCardMasters[j].MachineKnittingTypeID = masterData.KnittingTypeID;
    //                masterData.Childs[i].KJobCardMasters[j].MachineGauge = masterData.Childs[i].MachineGauge;
    //                masterData.Childs[i].KJobCardMasters[j].UnitID = 28;
    //                masterData.Childs[i].KJobCardMasters[j].SubGroupID = masterData.SubGroupID;
    //                masterData.Childs[i].KJobCardMasters[j].BookingQty = masterData.Childs[i].BookingQty;
    //                masterData.Childs[i].KJobCardMasters[j].Remarks = masterData.Childs[i].Remarks;
    //            }
    //        }
    //    }
    //    var tempArray = masterData.Childs;
    //    for (var i = 0; i < tempArray.length; i++) {
    //        var filterData = $.grep(masterData.Childs, function (h) {
    //            return h.Id != tempArray[i].Id && h.MCSubClassID == tempArray[i].MCSubClassID && h.MachineDia == tempArray[i].MachineDia &&
    //                h.MachineGauge == tempArray[i].MachineGauge && h.ContactID == tempArray[i].ContactID && h.BrandID == tempArray[i].BrandID &&
    //                h.KnittingMachineID == tempArray[i].KnittingMachineID
    //        });
    //        if (filterData.length > 0) {
    //            toastr.warning('Same row exists in Program Information!!!');
    //            return false;
    //        }
    //    }
    //    var tempYarnArray = masterData.Yarns;
    //    for (var i = 0; i < tempYarnArray.length; i++) {
    //        var filterYarnData = $.grep(masterData.Yarns, function (y) {
    //            return y.Id != tempYarnArray[i].Id && y.StitchLength == tempYarnArray[i].StitchLength
    //        });
    //        if (filterYarnData.length > 0) {
    //            toastr.warning('Same row exists in Yarn Information!!!');
    //            return false;
    //        }
    //    }
      
        
    //    var data = formDataToJson($formEl.serializeArray());
    //    data = getFormData($formEl);
    //    data.SubGroupCount = masterData.Childs.length;
    //    data["Childs"] = masterData.Childs;
    //    data["Yarns"] = masterData.Yarns;
    //    data.Approve = isApprove;
    //    data.KnittingProgramType = knittingProgramType.BULK;
    //    data.KnittingTypeID = masterData.KnittingTypeID;
 
    //    var files = $formEl.find("#UploadFile")[0].files;
    //    data.append("UploadFile", files[0]);

    //    //var sumBookingQty = 0;
    //    //var sumJobCardQty = 0;
    //    //for (var i = 0; i < masterData.Childs.length; i++) {
    //    //    sumBookingQty = parseInt(sumBookingQty) + parseInt(masterData.Childs[i].BookingQty);
    //    //    if (masterData.Childs[i].KJobCardMasters.length >= 1) {
    //    //        for (var j = 0; j < masterData.Childs[i].KJobCardMasters.length; j++) {
    //    //            sumJobCardQty = parseInt(sumJobCardQty) + parseInt(masterData.Childs[i].KJobCardMasters[j].KJobCardQty);
    //    //        }
    //    //        if (parseInt(sumJobCardQty) > parseInt(masterData.Childs[i].BookingQty)) {
    //    //            toastr.warning("Sum of Job Card quantity is not more than program quantity!!!");
    //    //            return false;
    //    //        }
    //    //    }
    //    //}

    //    axios.post("/api/knitting-program/save", data)
    //        .then(function () {
    //            toastr.success("Saved successfully.");
    //            backToList();
    //        })
    //        .catch(showResponseError);
    //}

    function save(e, isApprove = false) {
        var kJobCardMaster = [];
        for (var i = 0; i < masterData.Childs.length; i++) {
            if (masterData.Childs[i].KJobCardMasters.length == 0) {
                kJobCardMaster = {
                    ConceptID: masterData.ConceptID,
                    KJobCardNo: masterData.Childs[i].KJobCardNo,
                    KJobCardDate: masterData.Childs[i].StartDate,
                    BrandID: masterData.Childs[i].BrandID,
                    IsSubContact: masterData.Childs[i].IsSubContact,
                    ContactID: masterData.Childs[i].ContactID,
                    MachineDia: masterData.Childs[i].MachineDia,
                    KnittingMachineID: masterData.Childs[i].KnittingMachineID,
                    KJobCardQty: masterData.Childs[i].BookingQty,
                    MachineKnittingTypeID: masterData.KnittingTypeID,
                    MachineGauge: masterData.Childs[i].MachineGauge,
                    MCSubClassID: masterData.Childs[i].MCSubClassID,
                    UnitID: 28,
                    SubGroupID: masterData.SubGroupID,
                    BookingQty: masterData.Childs[i].BookingQty,
                    Remarks: masterData.Childs[i].Remarks
                }
                masterData.Childs[i]["KJobCardMasters"].push(kJobCardMaster);
            }
            else {
                for (var j = 0; j < masterData.Childs[i].KJobCardMasters.length; j++) {
                    masterData.Childs[i].KJobCardMasters[j].ConceptID = masterData.ConceptID;
                    masterData.Childs[i].KJobCardMasters[j].KJobCardNo = masterData.Childs[i].KJobCardNo;
                    masterData.Childs[i].KJobCardMasters[j].KJobCardDate = masterData.Childs[i].StartDate;
                    masterData.Childs[i].KJobCardMasters[j].BrandID = masterData.Childs[i].BrandID;
                    masterData.Childs[i].KJobCardMasters[j].IsSubContact = masterData.Childs[i].IsSubContact;
                    masterData.Childs[i].KJobCardMasters[j].ContactID = masterData.Childs[i].ContactID;
                    masterData.Childs[i].KJobCardMasters[j].MachineDia = masterData.Childs[i].MachineDia;
                    masterData.Childs[i].KJobCardMasters[j].KnittingMachineID = masterData.Childs[i].KnittingMachineID;
                    masterData.Childs[i].KJobCardMasters[j].KJobCardQty = masterData.Childs[i].BookingQty;
                    masterData.Childs[i].KJobCardMasters[j].MachineKnittingTypeID = masterData.KnittingTypeID;
                    masterData.Childs[i].KJobCardMasters[j].MachineGauge = masterData.Childs[i].MachineGauge;
                    masterData.Childs[i].KJobCardMasters[j].MCSubClassID = masterData.Childs[i].MCSubClassID;
                    masterData.Childs[i].KJobCardMasters[j].UnitID = 28;
                    masterData.Childs[i].KJobCardMasters[j].SubGroupID = masterData.SubGroupID;
                    masterData.Childs[i].KJobCardMasters[j].BookingQty = masterData.Childs[i].BookingQty;
                    masterData.Childs[i].KJobCardMasters[j].Remarks = masterData.Childs[i].Remarks;
                }
            }
        }
        var tempArray = masterData.Childs;
        for (var i = 0; i < tempArray.length; i++) {
            var filterData = $.grep(masterData.Childs, function (child) {
                return child.KPChildID != tempArray[i].KPChildID && child.MCSubClassID == tempArray[i].MCSubClassID && child.MachineDia == tempArray[i].MachineDia &&
                    child.MachineGauge == tempArray[i].MachineGauge && child.ContactID == tempArray[i].ContactID && child.BrandID == tempArray[i].BrandID &&
                    child.KnittingMachineID == tempArray[i].KnittingMachineID
            });
            if (filterData.length > 0) {
                toastr.warning('Same row exists in Program Information!!!');
                return false;
            }
        }

        var tempYarnArray = masterData.Yarns;
        for (var i = 0; i < tempYarnArray.length; i++) {
            if (tempYarnArray[i].YD && !tempYarnArray[i].BatchNo) {
                toastr.warning('Please enter Batch No where YD is true in Yarn Information!');
                return false;
            }

            if (masterData.SubGroupID == 1 && tempYarnArray[i].StitchLength <= 0) {
                toastr.warning('Please enter stitch Length in yarn Information!');
                return false;
            }
        }
        var sumBookingQty = 0;
        if (parseInt(sumBookingQty) > (parseInt(masterData.RemainingQty) + parseInt(masterData.PlanQty))) {
            toastr.warning("Fabric quantity is not more than remaining quantity!!!");
            return false;
        }

        //
        var formData = getFormData($formEl);
        formData.append("SubGroupCount", masterData.Childs.length);
        formData.append("Approve", isApprove);
        formData.append("PlanQty", masterData.Childs[0].BookingQty);

        formData.append("Childs", JSON.stringify(masterData.Childs));
        formData.append("Yarns", JSON.stringify(masterData.Yarns));

        var files = $formEl.find("#UploadFile")[0].files;
        formData.append("UploadFile", files[0]);

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }

        axios.post("/api/knitting-program/save", formData, config)
            .then(function (response) {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(showResponseError);
    }

    function initNewAttachment($el) {
        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            previewFileType: 'any'
        });
    }

    function initAttachment(path, type, $el) {
        if (!path) {
            initNewAttachment($el);
            return;
        }

        if (!type) type = "any";

        var preveiwData = [rootPath + path];
        var previewConfig = [{ type: type, caption: "PI Attachment", key: 1, width: "80px", frameClass: "preview-frame" }];

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

})();
