(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;

    var CDAReceive;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        $tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING;
        initMasterTable();
        initChildTable();
        getMasterTableData();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnPartialLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PARTIALLY_COMPLETED;

            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnCompleteLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            initMasterTable();
            getMasterTableData();
        });

        $formEl.find("#btnSaveYR").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnYREditCancel").on("click", backToList);
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
            showFooter: true,
            columns: [
                {
                    title: "Actions",
                    align: "center",
                    formatter: function (value, row, index, field) {
                        return `<a class="btn btn-xs btn-default edit" href="javascript:void(0)" title="Edit Receive">
                                        <i class="fa fa-edit" aria-hidden="true"></i>
                                    </a>`;
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            getDetails(row.Id, row.LocationId);
                        }
                    }
                },
                {
                    field: "ReceiveNo",
                    title: "Receive No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ReceiveDate",
                    title: "Receive Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "InvoiceNo",
                    title: "Invoice No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "InvoiceDate",
                    title: "Invoice Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "LcNo",
                    title: "LC No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LcDate",
                    title: "LC Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "PoNo",
                    title: "PO No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "PoDate",
                    title: "PO Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RCompany",
                    title: "Rcv Company",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
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
        var url = `/api/CDA-rack-bin-allocation/list?status=${status}&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            detailView: true,
            columns: [
                {
                    field: "ItemName",
                    title: "Item Name",
                    width: 60
                },
                {
                    field: "AgentName",
                    title: "Agent Name",
                    width: 100
                },
                {
                    field: "PoQty",
                    title: "PO Qty",
                    align: 'center',
                    width: 50,
                    footerFormatter: calculateCDAReceiveCITotalPIQty
                },
                {
                    field: "InvoiceQty",
                    title: "Invoice Qty",
                    align: 'center',
                    width: 50,
                    footerFormatter: calculateCITotalPIQty
                },
                {
                    field: "DisplayUnitDesc",
                    title: "Unit",
                    width: 50
                },
                {
                    field: "LotNo",
                    title: "Lot No",
                    width: 30,
                    align: 'center'
                },
               
                {
                    field: "ChallanQty",
                    title: "Challan/PL Qty",
                    width: 30,
                    align: 'center',
                    footerFormatter: calculateCITotalChallanQty
                },
                {
                    field: "ReceiveQty",
                    title: "Receive Qty",
                    width: 30,
                    align: 'center',
                    footerFormatter: calculateCITotalReceiveQty
                },
                {
                    field: "ExcessQty",
                    title: "Excess Qty",
                    align: 'center',
                    width: 50,
                    footerFormatter: calculateCITotalExcessQty
                },
                {
                    field: "ShortQty",
                    title: "Short Qty",
                    align: 'center',
                    width: 50,
                    footerFormatter: calculateCITotalShortQty
                },
                {
                    field: "Remarks",
                    title: "Remarks",
                    width: 50
                }
            ],
            onExpandRow: function (index, row, $detail) {
                populateCDAReceiveRackBin(row.Id, $detail);
            },
        });
    }

    function populateCDAReceiveRackBin(childId, $detail) {
        $el = $detail.html('<table id="TblCDAReceiveChildRackBin-' + childId + '"></table>').find('table');
        initCDAReceiveRackBin($el, childId);
        var ind = getIndexFromArray(CDAReceive.ReceiveChilds, "Id", childId)
        var cnt = CDAReceive.ReceiveChilds[ind].ReceiveChildsRackBin.length;
        if (cnt > 0)
            $el.bootstrapTable('load', CDAReceive.ReceiveChilds[ind].ReceiveChildsRackBin.filter(function (item) {
                return item.EntityState != 8
            }));

    }

    function initCDAReceiveRackBin($el, childId) {
        $el.bootstrapTable({
            showFooter: true,
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
                    field: "Id",
                    title: "Id",
                    align: "left",
                    width: 100,
                    visible: false
                },
                {
                    field: "RackId",
                    title: "Rack No.",
                    editable: {
                        type: 'select2',
                        title: 'Select a Rack',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: CDAReceive.RackList,
                        select2: { width: 100, placeholder: 'Rack No' }
                    },
                    width: 100
                },
                {
                    field: "BinNo",
                    title: "Bin No",
                    formatter: function (value, row, index, field) {
                        var binNo = !row.BinId || row.BinId == 'undefined' || row.BinId == 'null' ? "Empty" : row.BinNo;
                        return ['<span class="btn-group">',
                            '<a href="javascript:void(0)" class="editable-link edit">' + binNo + '</a>',
                            '</span>'].join(' ');
                    },
                    events: {
                        'click .edit': function (e, value, row, index) {
                            e.preventDefault();
                            //if (!row.Segment1ValueId) return toastr.error("CDA Type is not selected");

                            getCDACountByCDAType(row, $el);
                        },
                    }
                },
                
                {
                    field: "Remarks",
                    title: "Remarks",
                    width: 20,
                    editable: {
                        type: 'text',
                        inputclass: 'input-sm',
                        showbuttons: false
                    }
                },
                {
                    field: "EmployeeID",
                    title: "EmployeeName",
                    editable: {
                        type: 'select2',
                        title: 'Select a employee',
                        inputclass: 'input-sm',
                        showbuttons: false,
                        source: CDAReceive.EmployeeList,
                        select2: { width: 200, placeholder: 'Employee' }
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
                            row.NoOfCartoon = oldValue;
                        }

                    })
                    row.NoOfCartoon = oldValue;
                }
                if (isNaN(row.NoOfCone) || row.NoOfCone < 0) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Invalid No Of Cone Qty !!!",
                        callback: function () {
                            row.NoOfCone = oldValue;
                        }

                    })
                    row.NoOfCone = oldValue;
                }
                //row.EntityState = 16;
                var ind = getIndexFromArray(CDAReceive.ReceiveChilds, "Id", childId)
                var noofcart = parseFloat(CDAReceive.ReceiveChilds[ind].NoOfCartoon);
                var noofcon = parseFloat(CDAReceive.ReceiveChilds[ind].NoOfCone);
                var cnt = CDAReceive.ReceiveChilds[ind].ReceiveChildsRackBin.length;
                var noofCartAllowcated = 0
                var noofConAllowcated = 0
                for (var i = 0; i < cnt; i++) {
                    noofCartAllowcated += parseFloat(CDAReceive.ReceiveChilds[ind].ReceiveChildsRackBin[i].NoOfCartoon);
                    noofConAllowcated += parseFloat(CDAReceive.ReceiveChilds[ind].ReceiveChildsRackBin[i].NoOfCone);
                }
                if (noofCartAllowcated > noofcart) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Total Rack Bin NoOfCartoon Qty can't greater then received No Of Cartoon Qty (" + noofcart +")!!!",
                        callback: function () {
                            row.NoOfCartoon = "0";
                        }
                    })
                    row.NoOfCartoon = oldValue;
                }
                if (noofConAllowcated > noofcon) {
                    bootbox.alert({
                        size: "small",
                        title: "Alert !!!",
                        message: "Total Rack Bin No Of Cone Qty can't greater then received No of Cone Qty (" + noofcon +")!!!",
                        callback: function () {
                            row.NoOfCone = oldValue;
                        }
                    })
                    row.NoOfCone = oldValue;
                }
            }
        });
    }

    window.addNewChildRow = function (childId) {
        $el = $("#TblCDAReceiveChildRackBin-" + childId);
        var ind = getIndexFromArray(CDAReceive.ReceiveChilds, "Id", childId)
        var maxId = ind >= 0 ? getMaxIdForArray(CDAReceive.ReceiveChilds[ind].ReceiveChildsRackBin, "Id") : 1;
        var newRow = {
            Id: maxId,
            RackId: 0,
            BinId: 0,
            BinNo: '',
            ChildId: childId,
            ChildRackBinID: 0,
            EmployeeID: "",
            EntityState: 4,
            NoOfCartoon: 0,
            NoOfCone: 0,
            Remarks: ""
        }
        CDAReceive.ReceiveChilds[ind].ReceiveChildsRackBin.push(newRow)
        $el.bootstrapTable('load', CDAReceive.ReceiveChilds[ind].ReceiveChildsRackBin);
    }

    function getCDACountByCDAType(rowData, $tableEl) {
        var id = rowData.RackId;
        var url = "/api/selectoption/bin-no-by-rack-no/" + id;
        axios.get(url)
            .then(function (response) {
                var dataList = response.data;
                showBootboxSelect2Dialog("Select Bin No", "sb", "Select Bin No", dataList, function (result) {
                    if (!result)
                        return toastr.warning("You didn't selected any Bin No.");
                    rowData.BinId = result.id;
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

    function getDetails(id, locationId) {
        axios.get(`/api/CDA-rack-bin-allocation/${id}/${locationId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDAReceive = response.data;
                CDAReceive.ReceiveDate = formatDateToDefault(CDAReceive.ReceiveDate);
                CDAReceive.PoDate = formatDateToDefault(CDAReceive.PoDate);
                CDAReceive.PiDate = formatDateToDefault(CDAReceive.PiDate);
                CDAReceive.LcDate = formatDateToDefault(CDAReceive.LcDate);
                CDAReceive.InvoiceDate = formatDateToDefault(CDAReceive.InvoiceDate);
                CDAReceive.ChallanDate = formatDateToDefault(CDAReceive.ChallanDate);
                setFormData($formEl, CDAReceive);
                $tblChildEl.bootstrapTable("load", CDAReceive.ReceiveChilds);
                $tblChildEl.bootstrapTable('hideLoading');

                if (CDAReceive.PoNo == null || CDAReceive.PoNo == "") {
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
        var data = CDAReceive;
        axios.post("/api/CDA-rack-bin-allocation/save", data)
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

        return ciLCValue.toFixed(2);
    }

    function calculateCDAReceiveCITotalPIQty(data) {
        var ciYRPIQty = 0;

        $.each(data, function (i, row) {
            ciYRPIQty += isNaN(parseFloat(row.PoQty)) ? 0 : parseFloat(row.PoQty);
        });

        return ciYRPIQty.toFixed(2);
    }

    function calculateCITotalPIQty(data) {
        var ciPIQty = 0;

        $.each(data, function (i, row) {
            ciPIQty += isNaN(parseFloat(row.InvoiceQty)) ? 0 : parseFloat(row.InvoiceQty);
        });

        return ciPIQty.toFixed(2);
    }

    function calculateCITotalReceiveQty(data) {
        var yRecQty = 0;

        $.each(data, function (i, row) {
            yRecQty += isNaN(parseFloat(row.ReceiveQty)) ? 0 : parseFloat(row.ReceiveQty);
        });

        return yRecQty.toFixed(2);
    }

    function calculateCITotalChallanQty(data) {
        var yChallancQty = 0;

        $.each(data, function (i, row) {
            yChallancQty += isNaN(parseFloat(row.ChallanQty)) ? 0 : parseFloat(row.ChallanQty);
        });

        return yChallancQty.toFixed(2);
    }

    function calculateCITotalExcessQty(data) {
        var yExchessQty = 0;

        $.each(data, function (i, row) {
            yExchessQty += isNaN(parseFloat(row.ExcessQty)) ? 0 : parseFloat(row.ExcessQty);
        });

        return yExchessQty.toFixed(2);
    }

    function calculateCITotalShortQty(data) {
        var yShortQty = 0;

        $.each(data, function (i, row) {
            yShortQty += isNaN(parseFloat(row.ShortQty)) ? 0 : parseFloat(row.ShortQty);
        });

        return yShortQty.toFixed(2);
    }


})();