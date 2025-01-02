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
    var pageId = "";
    var isCDAPage = false;
    var status;

    var YarnReceive;

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
        //tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        isCDAPage = convertToBoolean($(`#${pageId}`).find("#CDAPage").val());

        status = statusConstants.PENDING;
        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
        });

        $toolbarEl.find("#btnPartialLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;

            initMasterTable();
        });

        $toolbarEl.find("#btnCompleteLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            initMasterTable();
        });

        $formEl.find("#btnSaveYR").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnYREditCancel").on("click", backToList);
    });

    function initMasterTable() {
        var commands = [];
        commands = [
            { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
        ]

        var columns = [
            {
                headerText: '', commands: commands, textAlign: 'Center', width: ch_setActionCommandCellWidth(commands)
            },
            {
                field: 'YDStoreReceiveNo', headerText: 'Receive No'
            },

            {
                field: 'YDStoreReceiveDate', headerText: 'Receive Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            //{
            //    field: 'InvoiceNo', headerText: 'Invoice No', textAlign: 'Right'
            //},
            //{
            //    field: 'InvoiceDate', headerText: 'Invoice Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            //},
            //{
            //    field: 'LCNo', headerText: 'LC No'
            //},
            //{
            //    field: 'LCDate', headerText: 'LC Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            //},
            //{
            //    field: 'PONo', headerText: 'PO No'
            //},
            //{
            //    field: 'PODate', headerText: 'PO Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            //},
            {
                field: 'SupplierName', headerText: 'Supplier Name'
            },
            {
                field: 'CompanyName', headerText: 'Company'
            },
            {
                field: 'CompanyID', headerText: 'CompanyID', visible: false
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: true,
            apiEndPoint: `/api/yd-store-rack-bin-allocation/list?status=${status}&isCDAPage=${isCDAPage}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.YDStoreReceiveMasterID, args.rowData.CompanyID);
        }
    }

    function initChildTable() {
        var columns = [];
        if (isCDAPage) {
            columns.push(
                {
                    field: "Segment1ValueDesc",
                    title: "Item Name",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "Segment2ValueDesc",
                    title: "Agent Name",
                    cellStyle: function () { return { classes: 'm-w-150' } }
                }
            )
        } else {
            columns.push(
                {
                    field: "Segment1ValueDesc",
                    title: "Composition",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "Segment2ValueDesc",
                    title: "Yarn Type",
                    cellStyle: function () { return { classes: 'm-w-150' } }
                },
                {
                    field: "Segment3ValueDesc",
                    title: "Process"
                },
                {
                    field: "Segment4ValueDesc",
                    title: "Sub Process"
                },
                {
                    field: "Segment5ValueDesc",
                    title: "Quality Parameter"
                },
                {
                    field: "Segment6ValueDesc",
                    title: "Count"
                },
                {
                    field: "Segment7ValueDesc",
                    title: "No Of Ply"
                },
                {
                    field: "ShadeCode",
                    title: "Shade Code"
                }
            )
        }
        columns.push(
            //{
            //    field: "BuyerNames",
            //    title: "Buyer",
            //},
            {
                field: "SpinnerName",
                title: "Spinner",
            },
            //{
            //    field: "POQty",
            //    title: "PO Qty",
            //    align: 'center',
            //    footerFormatter: calculateYarnReceiveCITotalPIQty
            //},
            //{
            //    field: "InvoiceQty",
            //    title: "Invoice Qty",
            //    align: 'center',
            //    footerFormatter: calculateCITotalPIQty
            //},
            //{
            //    field: "DisplayUnitDesc",
            //    title: "Unit",
            //},
            {
                field: "YarnCategory",
                title: "Yarn Category",
            },
            {
                field: 'PhysicalCount', title: 'Physical Count', align: 'Right',
                //editable: {
                //    type: "text",
                //    showbuttons: false,
                //    tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                //}
            },
            //{
            //    field: "ChallanLot",
            //    title: "Challan Lot",
            //    align: 'center',
            //    //editable: {
            //    //    type: "text",
            //    //    showbuttons: false,
            //    //    tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
            //    //}
            //},
            {
                field: "LotNo",
                title: "Lot No",
                align: 'center',
                //editable: {
                //    type: "text",
                //    showbuttons: false,
                //    tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                //}
            },
            {
                field: "ReceiveCarton",
                title: "No Of Cartoon",
                align: 'center',
                //editable: {
                //    type: "text",
                //    showbuttons: false,
                //    tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                //}
            },
            {
                field: "ReceiveCone",
                title: "No Of Cone",
                align: 'center',
                //editable: {
                //    type: "text",
                //    showbuttons: false,
                //    tpl: '<input type="text" class="form-control input-sm" style="padding-right: 10px;">'
                //}
            },
            //{
            //    field: "ChallanQty",
            //    title: "Challan/PL Qty",
            //    align: 'center',
            //    footerFormatter: calculateCITotalChallanQty
            //},
            {
                field: "ReceiveQty",
                title: "Receive Qty",
                align: 'center',
                //editable: {
                //    type: "text",
                //    showbuttons: false,
                //    tpl: '<input type="number" class="form-control input-sm" style="padding-right: 10px;">'
                //}
                //footerFormatter: calculateCITotalReceiveQty
            },
            //{
            //    field: "ExcessQty",
            //    title: "Excess Qty",
            //    align: 'center',
            //    footerFormatter: calculateCITotalExcessQty
            //},
            //{
            //    field: "ShortQty",
            //    title: "Short Qty",
            //    align: 'center',
            //    footerFormatter: calculateCITotalShortQty
            //},
            {
                field: "Remarks",
                title: "Remarks"
            },
            {
                field: 'ItemMasterID', title: 'ItemMasterID', align: 'Right', visible: false
            },
            {
                field: 'SupplierID', title: 'SupplierID', align: 'Right', visible: false
            },
            {
                field: 'SpinnerID', title: 'SpinnerID', align: 'Right', visible: false
            },
            {
                field: 'LotNo', title: 'LotNo', align: 'Right', visible: false
            },
            {
                field: 'PhysicalCount', title: 'PhysicalCount', align: 'Right', visible: false
            },
            {
                field: 'ShadeCode', title: 'ShadeCode', align: 'Right', visible: false
            },
            {
                field: 'BookingID', title: 'BookingID', align: 'Right', visible: false
            },
        )

        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            detailView: true,
            columns: columns,
            onExpandRow: function (index, row, $detail) {
                populateYarnReceiveRackBin(row.YDStoreReceiveChildID, $detail);
            },
        });
    }

    function populateYarnReceiveRackBin(childId, $detail) {
        $el = $detail.html('<table id="TblYarnReceiveChildRackBin-' + pageId + '-' + childId + '"></table>').find('table');
        initYarnReceiveRackBin($el, childId);
        var ind = getIndexFromArray(YarnReceive.Childs, "YDStoreReceiveChildID", childId)
        var cnt = YarnReceive.Childs[ind].YDStoreReceiveChildRackBins.length;
        if (cnt > 0)
            $el.bootstrapTable('load', YarnReceive.Childs[ind].YDStoreReceiveChildRackBins.filter(function (item) {
                return item.EntityState != 8
            }));

        //$el.bootstrapTable('load', YarnReceive.ReceiveChilds[ind].ReceiveChildsRackBin)
    }

    function initYarnReceiveRackBin($el, childId) {

        $el.bootstrapTable({
            showFooter: true,
            uniqueId: 'ChildRackBinID',
            rowStyle: function (row, index) {
                if (row.EntityState == 8)
                    return { classes: 'deleted-row' };

                return "";
            },
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
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
                            '<button class="btn btn-success btn-xs add" onclick="addNewChildRow(' + childId + ')" title="Add">',
                            '<i class="fa fa-plus"></i>',
                            ' Add',
                            '</button>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                            //var ids = [row.Id];
                            //$el.bootstrapTable("remove", { field: "Id", values: ids });
                            //$("#hiddenRow").toggleClass("hidden");
                            $el.bootstrapTable('hideRow', { index: index });
                            row.EntityState = 8;
                        }
                    }
                },
                {
                    field: "ChildRackBinID",
                    title: "ChildRackBinID",
                    align: "left",
                    visible: false
                },
                {
                    field: "LocationID",
                    title: "Location",
                    cellStyle: function () { return { classes: 'm-w-80' } },
                    editable: {
                        type: 'select2',
                        title: 'Select a Location',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: YarnReceive.LocationList,
                        select2: { width: 150, placeholder: 'Location' }
                    }
                },
                {
                    field: "RackID",
                    title: "Rack No.",
                    cellStyle: function () { return { classes: 'm-w-80' } },
                    formatter: function (value, row, index, field) {
                        var rackNo = !row.RackID || row.RackID == 'undefined' || row.RackID == 'null' ? "Empty" : row.RackNo;
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + rackNo + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            //if (!row.Segment1ValueId) return toastr.error("Yarn Type is not selected");
                            getReckByLocation(row, $el);
                        },
                    }
                    //editable: {
                    //    type: 'select2',
                    //    title: 'Select a Rack',
                    //    inputclass: 'input-sm',
                    //    showbuttons: false,
                    //    source: YarnReceive.RackList,
                    //    select2: { width: 150, placeholder: 'Rack No' }
                    //}
                },
                {
                    field: "BinID",
                    title: "Bin No",
                    visible: false,
                    cellStyle: function () { return { classes: 'm-w-80' } },
                    formatter: function (value, row, index, field) {

                        var binNo = !row.BinID || row.BinID == 'undefined' || row.BinID == 'null' ? "Empty" : row.BinID;
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + binNo + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            //if (!row.Segment1ValueId) return toastr.error("Yarn Type is not selected");

                            getBinNoByRack(row, $el);
                        },
                    }
                },
                {
                    field: "NoOfCartoon",
                    title: "No Of Cartoon",
                    cellStyle: function () { return { classes: 'm-w-80' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "NoOfCone",
                    title: "No Of Cone",
                    cellStyle: function () { return { classes: 'm-w-80' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "ReceiveQty",
                    title: "Receive Qty",
                    align: 'center',
                    footerFormatter: calculateCITotalReceiveQty,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "Remarks",
                    title: "Remarks",
                    cellStyle: function () { return { classes: 'm-w-80' } },
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "EmployeeID",
                    title: "Employee Name",
                    cellStyle: function () { return { classes: 'm-w-80' } },
                    editable: {
                        type: 'select2',
                        title: 'Select a employee',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: YarnReceive.EmployeeList,
                        select2: { width: 150, placeholder: 'Employee' }
                    }
                }

            ],
            onEditableSave: function (field, row, oldValue, $el) {
                if (isNaN(row.NoOfCartoon) || row.NoOfCartoon < 0) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Invalid No Of Cartoon Qty !!!",
                        callback: function () {
                            //row.NoOfCartoon = oldValue;
                        }

                    })
                    //row.NoOfCartoon = oldValue;
                }
                if (isNaN(row.NoOfCone) || row.NoOfCone < 0) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Invalid No Of Cone Qty !!!",
                        callback: function () {
                            //row.NoOfCone = oldValue;
                        }

                    })
                    //row.NoOfCone = oldValue;
                }
                if (isNaN(row.ReceiveQty) || row.ReceiveQty < 0) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Invalid No Of Cone Qty !!!",
                        callback: function () {
                            //row.ReceiveQty = oldValue;
                        }

                    })
                    //row.ReceiveQty = oldValue;
                }
                //row.EntityState = 16;
                var ind = getIndexFromArray(YarnReceive.Childs, "YDStoreReceiveChildID", childId)
                var noofcart = parseFloat(YarnReceive.Childs[ind].ReceiveCarton);
                var noofcon = parseFloat(YarnReceive.Childs[ind].ReceiveCone);
                var cnt = YarnReceive.Childs[ind].YDStoreReceiveChildRackBins.length;
                var noofCartAllowcated = 0
                var noofConAllowcated = 0
                for (var i = 0; i < cnt; i++) {
                    noofCartAllowcated += parseFloat(YarnReceive.Childs[ind].YDStoreReceiveChildRackBins[i].NoOfCartoon);
                    noofConAllowcated += parseFloat(YarnReceive.Childs[ind].YDStoreReceiveChildRackBins[i].NoOfCone);
                }
                if (noofCartAllowcated > noofcart) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Total Rack Bin NoOfCartoon Qty can't greater then received No Of Cartoon Qty (" + noofcart + ")!!!",
                        callback: function () {
                            //row.NoOfCartoon = "0";
                        }
                    })
                    //row.NoOfCartoon = oldValue;
                }
                if (noofConAllowcated > noofcon) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Total Rack Bin No Of Cone Qty can't greater then received No of Cone Qty (" + noofcon + ")!!!",
                        callback: function () {
                            //row.NoOfCone = oldValue;
                        }
                    })
                    //row.NoOfCone = oldValue;
                }
            }
        });
    }

    window.addNewChildRow = function (childId) {
        $el = $("#TblYarnReceiveChildRackBin-" + pageId + "-" + childId);
        var ind = getIndexFromArray(YarnReceive.Childs, "YDStoreReceiveChildID", childId);
        var maxId = ind >= 0 ? getMaxIdForArray(YarnReceive.Childs[ind].YDStoreReceiveChildRackBins, "ChildRackBinID") : 1;
        var newRow = {
            ChildRackBinID: maxId,
            RackID: 0,
            BinID: 0,
            BinNo: '',
            ChildID: childId,
            ChildRackBinID: 0,
            EmployeeID: 0,
            EntityState: 4,
            NoOfCartoon: 0,
            NoOfCone: 0,
            ReceiveQty: 0,
            Remarks: ""
        }
        YarnReceive.Childs[ind].YDStoreReceiveChildRackBins.push(newRow)
        $el.bootstrapTable('load', YarnReceive.Childs[ind].YDStoreReceiveChildRackBins);
    }

    function getReckByLocation(rowData, $tableEl) {
        if (typeof rowData.LocationID === "undefined" || rowData.LocationID == 0) {
            return toastr.warning("Select Location.");
        }
        var id = rowData.LocationID;
        var rackFor = "Y";
        var url = "/api/selectoption/rack-no-by-locationid-rack-for/" + id + "/" + rackFor;
        axios.get(url)
            .then(function (response) {
                var dataList = response.data;
                showBootboxSelect2Dialog("Select Rack No", "sb", "Select Rack No", dataList, function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected any Rack No.");
                    rowData.RackID = result.id;
                    rowData.RackNo = result.text;
                    $tableEl.bootstrapTable('load', rowData);
                })
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getBinNoByRack(rowData, $tableEl) {
        var id = rowData.RackID;

        var url = "/api/selectoption/bin-no-by-rack-no/" + id;
        axios.get(url)
            .then(function (response) {
                var dataList = response.data;
                showBootboxSelect2Dialog("Select Bin No", "sb", "Select Bin No", dataList, function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected any Bin No.");
                    rowData.BinID = result.id;
                    rowData.BinNo = result.text;
                    $tableEl.bootstrapTable('load', rowData);
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
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#ReceiveID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getDetails(id, companyId) {
        axios.get(`/api/yd-store-rack-bin-allocation/${id}/${companyId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                YarnReceive = response.data;
                YarnReceive.YDStoreReceiveDate = formatDateToDefault(YarnReceive.YDStoreReceiveDate);
                setFormData($formEl, YarnReceive);

                initChildTable();
                $tblChildEl.bootstrapTable("load", YarnReceive.Childs);
                $tblChildEl.bootstrapTable('hideLoading');

                if (YarnReceive.PONo == null || YarnReceive.PONo == "") {
                    $formEl.find(".PO").fadeOut();
                    $formEl.find(".CI").fadeIn();
                } else {
                    $formEl.find(".PO").fadeIn();
                    $formEl.find(".CI").fadeOut();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save() {
        var data = YarnReceive;
        var hasError = false;
        for (var i = 0; i < data.Childs.length; i++) {
            var child = data.Childs[i],
                rowNo = i + 1;

            //if (child.PhysicalCount == null || $.trim(child.PhysicalCount) == "") {
            //    toastr.error("Physical count can't be empty. Row(" + rowNo + ")");
            //    hasError = true;
            //    break;
            //}
            //if (child.LotNo == null || $.trim(child.LotNo) == "") {
            //    toastr.error("Lot No. can't be empty. Row(" + rowNo + ")");
            //    hasError = true;
            //    break;
            //}
            //if (child.NoOfCartoon == null || $.trim(child.NoOfCartoon) == "" || child.NoOfCartoon == 0) {
            //    toastr.error("No of cartoon can't be empty. Row(" + rowNo + ")");
            //    hasError = true;
            //    break;
            //}
            //if (child.NoOfCone == null || $.trim(child.NoOfCone) == "" || child.NoOfCone == 0) {
            //    toastr.error("No of cone can't be empty. Row(" + rowNo + ")");
            //    hasError = true;
            //    break;
            //}
            if (child.ReceiveQty == null || $.trim(child.ReceiveQty) == "" || child.ReceiveQty == 0) {
                toastr.error("Give Receive Qty. Row(" + rowNo + ")");
                hasError = true;
                break;
            }

            if (child.YDStoreReceiveChildRackBins.length == 0) {
                toastr.error("Add minimum 1 Rack for each item.");
                hasError = true;
                break;
            }

            child.YDStoreReceiveChildRackBins = child.YDStoreReceiveChildRackBins.filter(x => x.EntityState != 8 && x.LocationID > 0 && x.RackID > 0 && x.EmployeeID > 0);
            if (child.YDStoreReceiveChildRackBins.length == 0) {
                toastr.error(`Give racks all informations.`);
                hasError = true;
                break;
            }

            var maxNoOfCartoon = child.ReceiveCarton;
            var maxNoOfCone = child.ReceiveCone;
            var maxRcvQty = child.ReceiveQty;

            maxRcvQty = parseFloat(maxRcvQty).toFixed(2);
            maxRcvQty = parseFloat(maxRcvQty);

            var totalNoOfCartoon = 0;
            var totalNoOfCone = 0;
            var totalRcvQty = 0;

            //item.EntityState != 8

            for (var iRack = 0; iRack < child.YDStoreReceiveChildRackBins.length; iRack++) {
                var childRack = child.YDStoreReceiveChildRackBins[iRack];

                childRack.LocationID = getDefaultValueWhenInvalidN(childRack.LocationID);
                childRack.RackID = getDefaultValueWhenInvalidN(childRack.RackID);
                childRack.EmployeeID = getDefaultValueWhenInvalidN(childRack.EmployeeID);

                if (childRack.LocationID == 0) {
                    toastr.error("Select Location");
                    hasError = true;
                    break;
                }
                if (childRack.RackID == 0) {
                    toastr.error("Select Rack");
                    hasError = true;
                    break;
                }
                if (childRack.NoOfCartoon == null || $.trim(childRack.NoOfCartoon) == "" || childRack.NoOfCartoon == 0) {
                    toastr.error("No of Cartoon can't be empty");
                    hasError = true;
                    break;
                }
                totalNoOfCartoon += parseFloat(childRack.NoOfCartoon);

                if (childRack.NoOfCone == null || $.trim(childRack.NoOfCone) == "" || childRack.NoOfCone == 0) {
                    toastr.error("No of Cone can't be empty");
                    hasError = true;
                    break;
                }
                totalNoOfCone += parseFloat(childRack.NoOfCone);

                if (childRack.ReceiveQty == null || $.trim(childRack.ReceiveQty) == "" || childRack.ReceiveQty == 0) {
                    toastr.error("Receive Qty can't be empty");
                    hasError = true;
                    break;
                }
                totalRcvQty += parseFloat(childRack.ReceiveQty);

                if (childRack.EmployeeID == 0) {
                    toastr.error("Select Employee");
                    hasError = true;
                    break;
                }
            }

            totalRcvQty = parseFloat(totalRcvQty).toFixed(2);
            totalRcvQty = parseFloat(totalRcvQty);

            if (!hasError) {
                if (totalNoOfCartoon > maxNoOfCartoon) {
                    toastr.error(`No of cartoon ${totalNoOfCartoon} cannot be greater than max cartoon ${maxNoOfCartoon}`);
                    hasError = true;
                    break;
                }
                if (totalNoOfCone > maxNoOfCone) {
                    toastr.error(`No of cone ${totalNoOfCone} cannot be greater than max cone ${maxNoOfCone}`);
                    hasError = true;
                    break;
                }
                if (totalRcvQty > maxRcvQty) {
                    toastr.error(`Receive Qty ${totalRcvQty} cannot be greater than max Receive Qty ${maxRcvQty}`);
                    hasError = true;
                    break;
                }
            }

            if (hasError) break;
        }

        if (hasError) return false;

        axios.post("/api/yd-store-rack-bin-allocation/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


    function calculateCITotalInvoiceValue(data) {
        var ciLCValue = 0;

        $.each(data, function (i, row) {
            ciLCValue += isNaN(parseFloat(row.CiValue)) ? 0 : parseFloat(row.CiValue);
        });

        return parseFloat(ciLCValue).toFixed(2);
    }

    function calculateYarnReceiveCITotalPIQty(data) {
        var ciYRPIQty = 0;

        $.each(data, function (i, row) {
            ciYRPIQty += isNaN(parseFloat(row.POQty)) ? 0 : parseFloat(row.POQty);
        });

        return parseFloat(ciYRPIQty).toFixed(2);
    }

    function calculateCITotalPIQty(data) {
        var ciPIQty = 0;

        $.each(data, function (i, row) {
            ciPIQty += isNaN(parseFloat(row.InvoiceQty)) ? 0 : parseFloat(row.InvoiceQty);
        });

        return parseFloat(ciPIQty).toFixed(2);
    }

    function calculateCITotalReceiveQty(data) {
        var yRecQty = 0;

        $.each(data, function (i, row) {
            yRecQty += isNaN(parseFloat(row.ReceiveQty)) ? 0 : parseFloat(row.ReceiveQty);
        });

        return parseFloat(yRecQty).toFixed(2);
    }

    function calculateCITotalChallanQty(data) {
        var yChallancQty = 0;

        $.each(data, function (i, row) {
            yChallancQty += isNaN(parseFloat(row.ChallanQty)) ? 0 : parseFloat(row.ChallanQty);
        });

        return parseFloat(yChallancQty).toFixed(2);
    }

    function calculateCITotalExcessQty(data) {
        var yExchessQty = 0;

        $.each(data, function (i, row) {
            yExchessQty += isNaN(parseFloat(row.ExcessQty)) ? 0 : parseFloat(row.ExcessQty);
        });

        return parseFloat(yExchessQty).toFixed(2);
    }

    function calculateCITotalShortQty(data) {
        var yShortQty = 0;

        $.each(data, function (i, row) {
            yShortQty += isNaN(parseFloat(row.ShortQty)) ? 0 : parseFloat(row.ShortQty);
        });

        return parseFloat(yShortQty).toFixed(2);
    }
})();