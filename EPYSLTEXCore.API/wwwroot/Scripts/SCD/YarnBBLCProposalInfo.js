(function () {
    //'use strict'

    // #region variables
    var bblcProposal;
    var menuId, pageName;
    var toolbarId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $tblMergeExistingEl, $formEl, $modalMergeExistingEl;
    var status = statusConstants.PENDING;
    var isCDAPage = false;
    var filterBy = {};
    var companyID;
    var isMerge = false;
    var isRevision = false;
    var isContract = false;

    var Data;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var validationConstraints = {
        CompanyID: {
            presence: true
        },
        SupplierID: {
            presence: true
        }
        //,
        //ProposeContract: {
        //    presence: true
        //}
    };
     
    var selectedPIReceiveList = [];
    // #endregion

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
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);
        $tblMergeExistingEl = $('#tblMergeExisting' + pageId);
        $modalMergeExistingEl = $('#modal-merge-existing' + pageId);
        $tblMasterSaveEl = $(pageConstants.MASTER_TBL_ID_PREFIX + 'Save' + pageId);
        isCDAPage = convertToBoolean($(`#${pageId}`).find("#CDAPage").val());
        
        initializeValidation($formEl, validationConstraints);

        initMasterTable();
        getMasterTableData();

        initMergeExistingTable();
        $tblMasterEl.on('post-header.bs.table', function () {
            $('.bootstrap-table .filter-control input').addClass('form-control-sm');
        });
  
        $formEl.find("#btnBackNew").on("click", function (e) {
            e.preventDefault();
            backToList();
        });

        $formEl.find("#btnBackEdit").on("click", function (e) {
            e.preventDefault();
            backToList();
        });

        $toolbarEl.find("#btnPendingList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            $("#div-create-merge-btns").show();
            status = statusConstants.PENDING;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnEditList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            $("#div-create-merge-btns").hide();
            status = statusConstants.PARTIALLY_COMPLETED;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            $("#div-create-merge-btns").hide();
            status = statusConstants.COMPLETED;
            initMasterTable();
            getMasterTableData();
        });

        $toolbarEl.find("#btnRevisionList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            $("#div-create-merge-btns").hide();
            status = statusConstants.REVISE;
            initMasterTable();
            getMasterTableData();
        });

        
        
        $divTblEl.find("#btnCreate").click(getNew);

        $divTblEl.find("#btnMergeExisting").click(btnMergeExistingClick);

        $formEl.find("#btCancel").click(backToList); 
      
        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            Save(e);
        });

        $formEl.find("#btnRevise").click(function (e) {
            e.preventDefault();
            Revise(e);
        });

        $formEl.find("#btnAddPI").on("click", function (e) {
            var queryParams = $.param(tableParams);
            //var url = `/api/ybblcproposal/list?gridType=bootstrap-table&status=${status}&isCDAPage=${isCDAPage}&${queryParams}`;

            //axios.get(`/api/ybblcproposal/list-for-merge/${selectedPIReceiveList[0].CompanyID}/${selectedPIReceiveList[0].SupplierID}/${isCDAPage}`)

            status = statusConstants.PENDING;
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select LC",
                pageId: pageId,
                apiEndPoint: `/api/ybblcproposal/listdata?status=${status}&isCDAPage=${isCDAPage}`,
                fields: "YPINo,CompanyName,SupplierName,PONo,TotalQty,TotalValue",
                headerTexts: "PI No,Company,Supplier,PO No,Total Qty,Total Value",
                widths: "100,50,50,50,30,30",
                isMultiselect: true,
                allowPaging: false,
                primaryKeyColumn: "YPINo",
                onMultiselect: function (selectedRecords) {
                    var pIList = $tblChildEl.bootstrapTable('getData');
                    var piReceiveMasterIds = selectedRecords.map(function (el) { return el.YPIReceiveMasterID; }).join();
                  
                    var isEqual_Spplier = true, isEqual_Company = true;
                    for (var i = 1; i < selectedRecords.length; i++) {
                        isEqual_Spplier = _.isEqual(selectedRecords[i - 1].SupplierID, selectedRecords[i].SupplierID);
                        isEqual_Company = _.isEqual(selectedRecords[i - 1].CompanyID, selectedRecords[i].CompanyID);
                        if (!isEqual_Spplier || !isEqual_Company) break;
                    }

                    if (!isEqual_Spplier || !isEqual_Company) {
                        toastr.error("Company and Supplier must be same.");
                        return;
                    }
                    selectedRecords.forEach(function (value) {
                        var exists = pIList.find(function (el) { return el.YPINo == value.YPINo })
                        if (!exists) pIList.unshift({
                            YPINo: value.YPINo,
                            PIDate: value.PIDate,
                            Unit: value.Unit,
                            TotalQty: value.TotalQty,
                            TotalValue: value.TotalValue,
                            YPIReceiveMasterID: value.YPIReceiveMasterID
                        });
                    });
                   
                    initChildTable();
                    $tblChildEl.bootstrapTable("load", pIList);
                }
            });
            finder.showModal();

        });

        $formEl.find("#btnAddCOntractNo").on("click", function (e) {

            var proposeContract = '';
            var proposeContractID = '';
            e.preventDefault();
            if (Data.BusinessNature == 'PC') {
                isContract = true;
                var finder = new commonFinder({
                    title: "Select Contract No",
                    pageId: pageId,
                    apiEndPoint: `/api/ybblcproposal/ProposeContractForPC/${companyID}`,
                    fields: "ProposeContract",
                    headerTexts: "Propose Contract No",
                    widths: "100",
                    isMultiselect: false,
                    allowPaging: false,
                    primaryKeyColumn: "ProposeContractID",
                    onSelect: function (selectedRecord) {
                        finder.hideModal();
                        
                        proposeContract = selectedRecord.rowData.ProposeContract;
                        proposeContractID = selectedRecord.rowData.ProposeContractID;
                        ProposeBankID = selectedRecord.rowData.ProposeBankID;
                        $formEl.find("#ProposeContract").val(proposeContract);
                        $formEl.find("#ProposeContractID").val(proposeContractID);
                        $formEl.find("#ProposeBankID").val(ProposeBankID).trigger('change');
                    }
                });
                finder.showModal();
            } else {
                isContract = false;
                var finder = new commonFinder({
                    title: "Select Contract No",
                    pageId: pageId,
                    apiEndPoint: `/api/ybblcproposal/LCContractNo/${companyID}`,
                    fields: "ProposeContract,ProposeBankName",
                    headerTexts: "Propose Contract No,Lien Bank",
                    widths: "100,100",
                    isMultiselect: false,
                    allowPaging: false,
                    primaryKeyColumn: "ProposeContractID",
                    onSelect: function (selectedRecord) {
                        finder.hideModal();
                        
                        proposeContract = selectedRecord.rowData.ProposeContract;
                        proposeContractID = selectedRecord.rowData.ProposeContractID;
                        ProposeBankID = selectedRecord.rowData.ProposeBankID;
                        ProposeBankName = selectedRecord.rowData.ProposeBankName;
                        $formEl.find("#ProposeContract").val(proposeContract);
                        $formEl.find("#ProposeContractID").val(proposeContractID);
                        $formEl.find("#ProposeBankID").val(ProposeBankID).trigger('change');
                        $formEl.find("#ProposeBankName").val(ProposeBankName);
                    }
                });
                finder.showModal();
            }
        });

        $formEl.find("#CashStatus").click(function (e) {
            ShowElement(true);            
        });

        //if (isCDAPage) {
        //    $formEl.find(".divCashStatus").fadeIn();
        //    $formEl.find(".divIssueBankID").fadeIn();
        //} else {
        //    $formEl.find(".divCashStatus").fadeOut();
        //    $formEl.find(".divIssueBankID").fadeOut();
        //}
    });

    function getNew(e) {
        e.preventDefault();
        selectedPIReceiveList = $tblMasterEl.bootstrapTable('getSelections');

        if (!selectedPIReceiveList || selectedPIReceiveList.length === 0) return toastr.error("You must select one PI");

        var piReceiveMasterIds = selectedPIReceiveList.map(function (el) { return el.YPIReceiveMasterID; }).join();

        //console.log(selectedPIReceiveList);

        var isEqual_Spplier = true, isEqual_Company = true;
        for (var i = 1; i < selectedPIReceiveList.length; i++) {
            isEqual_Spplier = _.isEqual(selectedPIReceiveList[i - 1].SupplierID, selectedPIReceiveList[i].SupplierID);
            isEqual_Company = _.isEqual(selectedPIReceiveList[i - 1].CompanyID, selectedPIReceiveList[i].CompanyID);
            if (!isEqual_Spplier || !isEqual_Company) break;
        }

        if (!isEqual_Spplier || !isEqual_Company) {
            toastr.error("Company and Supplier must be same.");
            return;
        }

        axios.get(`/api/ybblcproposal/new?piReceiveMasterIds=${piReceiveMasterIds}`)
            .then(function (response) {
                
                bblcProposal = response.data;

                if (bblcProposal.CompanyName == "ESL") {
                    $formEl.find("#lblContractNo").text("Select Contract No");
                } else {
                    $formEl.find("#lblContractNo").text("Select LC / IBC No");
                }

                bblcProposal.ProposalDate = formatDateToDefault(bblcProposal.ProposalDate);
                goToDetails(bblcProposal);
                isMerge = false;

                ShowElement(true);
            })
            .catch(showResponseError);
        
        $formEl.find("#btnSave").fadeIn();
        $formEl.find("#btnRevise").fadeOut();
    }

    function getProposeContract() {
         companyID = $formEl.find("#CompanyID").val(); 
        axios.get(`/api/ybblcproposal/LCType/${companyID}`)
            .then(function (response) {
                 Data = response.data;
           })
            .catch(showResponseError);
    }
  
    function addContact(e) {
          e.preventDefault();
        var finder = new commonFinder({
            title: "Select Contract No",
            pageId: pageId,
            apiEndPoint: `/api/ybblcproposal/LCContractNo/${companyID}`,
            fields: "ProposeContract",
            headerTexts: "BBLC No",
            widths: "100",
            isMultiselect: true,
            allowPaging: false,
            primaryKeyColumn: "BBLCID",
            //onMultiselect: function (selectedRecords) {

            //    var pIList = $tblChildEl.bootstrapTable('getData');
            //    selectedRecords.forEach(function (value) {
            //        var exists = pIList.find(function (el) { return el.YPINo == value.YPINo })
            //        if (!exists) pIList.unshift({
            //            YPINo: value.YPINo,
            //            PIDate: value.PIDate,
            //            Unit: value.Unit,
            //            TotalQty: value.TotalQty,
            //            TotalValue: value.TotalValue
            //        });
            //    });
            //    initChildTable();
            //    $tblChildEl.bootstrapTable("load", pIList);
            //}
        });
        finder.showModal();
    }

    function getDetails(e, value, row, index) {
        e.preventDefault();

        if (status == statusConstants.REVISE) {
            if (row.PIAcceptStatus === false) {
                toastr.error("Waiting for PI Approve");
                return false;
            }
        }

        axios.get(`/api/ybblcproposal/${row.ProposalID}`)
            .then(function (response) {
                
                bblcProposal = response.data;
                bblcProposal.ProposalDate = formatDateToDefault(bblcProposal.ProposalDate);
                goToDetails(bblcProposal);
                if (status == statusConstants.COMPLETED) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnRevise").fadeOut();
                }
                //else {
                //    $formEl.find("#btnSave").fadeIn();
                //}
                if (status == statusConstants.REVISE) {
                    $formEl.find("#btnSave").fadeOut();
                    $formEl.find("#btnRevise").fadeIn();
                }
                if (status == statusConstants.PARTIALLY_COMPLETED || status == statusConstants.PENDING) {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnRevise").fadeOut();
                }

                ShowElement(false);
            })
            .catch(showResponseError);
    } 

    function initMasterTable() {
        $tblMasterEl.bootstrapTable('destroy');
        $tblMasterEl.bootstrapTable({
            classes: 'table-bordered table-striped table-sm',
            theadClasses: 'text-center',
            showToggle:true,
            showRefresh: true,
            showExport: true,
            showColumns: true,
            toolbar: $toolbarEl,
            exportTypes: "['csv', 'excel']",
            pagination: true,
            filterControl: true,
            searchOnEnterKey: true,
            sidePagination: "server",
            pageList: "[10, 25, 50, 100, 500]",
            cache: false,
            checkboxHeader: false,
            clickToSelect: true,
      
            idField: "ProposalID",
            columns: [
                {
                    checkbox: true,
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "Actions",
                    title: "Actions",
                    align: "center",
                    width: 50,
                    //visible: status == statusConstants.PARTIALLY_COMPLETED,
                    visible: status != statusConstants.PENDING,
                    formatter: function (value, row, index, field) {
                        //template =
                        //    `<span class="btn-group">
                        //        <a class="btn btn-xs btn-primary m-w-30 edit" href="javascript:void(0)" title="Edit Proposal">
                        //            <i class="fa fa-pencil-square-o" aria-hidden="true"></i>
                        //        </a>
                        //    </span>`;
                        //return template;
                        if (status == statusConstants.PARTIALLY_COMPLETED) {
                            return [
                                '<a class="btn btn-xs btn-primary m-w-30 edit" href="javascript:void(0)" title="Edit Proposal">',
                                '<i class="fa fa-pencil-square-o" aria-hidden="true"></i>',
                                '</a>'
                            ].join(' ');
                        }
                        if (status == statusConstants.REVISE) {
                            return [
                                '<a class="btn btn-xs btn-primary m-w-30 edit" href="javascript:void(0)" title="Revise Proposal">',
                                '<i class="fa fa-pencil-square-o" aria-hidden="true"></i>',
                                '</a>'
                            ].join(' ');
                        }
                        if (status == statusConstants.COMPLETED) {
                            return [
                                '<a class="btn btn-xs btn-primary m-w-30 edit" href="javascript:void(0)" title="View Proposal">',
                                '<i class="fa fa-eye" aria-hidden="true"></i>',
                                '</a>'
                            ].join(' ');
                        }
                    },
                    events: {
                        'click .edit': getDetails
                    }
                }, 
                {
                    field: "ProposalNo",
                    title: "Proposal No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status != statusConstants.PENDING,
                    filterTemplate: (column) => {
                        return `<input type="text" class="form-control form-control-sm" placeholder="Filter ${column.title}">`;
                    }
                },
                {
                    field: "ProposalDate",
                    title: "Proposal Date",
                   
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    visible: status != statusConstants.PENDING
                },
                {
                    field: "YPINo",
                    title: "PI No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "PIDate",
                    title: "PI Date", width: 120,
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "CompanyName",
                    title: "Company", width: 80,
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SupplierName",
                    title: "Supplier", width: 300,
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "PONo",
                    title: "PO No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status == statusConstants.PENDING
                },
                {
                    field: "CashStatus",
                    title: "Cash L/C?",
                    checkbox: true,
                    showSelectTitle: false,
                    checkboxEnabled: false,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status === statusConstants.PENDING ? false : true,
                },
                {
                    field: "ProposeContractID",
                    title: "Propose Contract",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: false
                },
                {
                    field: "ProposeContract",
                    title: "Propose Contract No",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: status === statusConstants.PENDING ? false : true,
                    
                },
                {
                    field: "ProposeBankID",
                    title: "Propose Bank",
                    filterControl: "input",
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER,
                    visible: false
                },
                {
                    field: "BranchName",
                    title: "Propose Bank",
                    visible: status === statusConstants.PENDING ? false : true,
                },
                

                {
                    field: "TotalQty",
                    title: "Total Qty", width: 120,
                    filterControl: "input",
                    align: 'right',
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    footerFormatter: calculateTotalYarnQtyAll,
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TotalValue",
                    title: "Total Value", width: 120,
                    filterControl: "input",
                    align: 'right',
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    footerFormatter: calculateTotalYarnValueAll,

                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "RetirementMode",
                    title: "Retirement Mode",
                    visible: status === statusConstants.PENDING ? false : true,
                    //filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
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
            onCheck: function (row, $element) {
               
                var rowsList = $tblMasterEl.bootstrapTable('getData');
                var filterData = $.grep(rowsList, function (h, i) {
                    return h.CheckAll == true && h.ProposalID != row.ProposalID;
                });
                if (filterData.length > 0) {
                    row.CheckAll = true;


                    var filterSupplierData = $.grep(filterData, function (h, i) {
                        return h.CheckAll == true && h.SupplierName == row.SupplierName;
                    });
                    if (filterSupplierData.length > 0) {
                        row.CheckAll = true; 
                    }
                    else {
                        row.CheckAll = false;
                        toastr.warning("Must have same supplier");
                    }
                }

                $tblMasterEl.bootstrapTable("updateByUniqueId", row.ProposalID, row);
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

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            //showRefresh: true,
            showExport: true,

           // showColumns: true,
           // toolbar: $toolbarEl,
            exportTypes: "['csv', 'excel']",
            showFooter: true,
            columns: [
                {
                    title: "",
                    align: "center", 
                   
                    formatter: function (value, row, index, field) { 
                        return [
                            '<span class="btn-group">',
                            '<a class="btn btn-danger btn-sm remove" href="javascript:void(0)" title="Delete Item">',
                            '<i class="fa fa-times"></i>',
                            '</a>',
                            '&nbsp;<a class="btn btn-sm btn-primary" href="' + row.PiFilePath + '" target="_blank" title="PI Report">',
                            '<i class="fas fa-file-pdf" aria-hidden="true"></i>',
                            '</a>',
                            '</span>'
                        ].join('');
                    },
                    events: {
                        'click .remove': function (e, value, row, index) {
                           showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                                if (yes) {
                                    bblcProposal.Childs.splice(index, 1);
                                    $tblChildEl.bootstrapTable('load', bblcProposal.Childs);
                                }
                            });
                        }
                    }
                },
                //{
                //    field: "PiFilePath",
                //    title: "View PI",
                //    formatter: function (value, row, index, field) {
                //        return `<a href="${row.PiFilePath}" target="_blank"><i class="fa fa-eye"></i> View PI</a>`;
                //    }
                //},
                //{
                //    field: "YPIReceiveMasterID",
                //    title: "YPIReceiveMasterID"
                //},
                {
                    field: "YPINo",
                    title: "PI No",
                    footerFormatter: function () {
                        return "<label>Total</label>";
                    }
                },
                {
                    field: "PIDate",
                    title: "PI Date",
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    },
                }, 
                {
                    field: "Unit",
                    title: "Unit",
                },
                {
                    field: "TotalQty",
                    title: "PI Qty",
                    align: 'right', 
                    footerFormatter: calculateTotalQty
                },
                {
                    field: "TotalValue",
                    title: "PI Value",
                    align: 'right',
                    footerFormatter: calculateTotalValue
                }
            ]
        });
    }

    function getMasterTableData() {
        var queryParams = $.param(tableParams);
        $tblMasterEl.bootstrapTable('showLoading');        
        var url = `/api/ybblcproposal/list?gridType=bootstrap-table&status=${status}&isCDAPage=${isCDAPage}&${queryParams}`;
        axios.get(url)
            .then(function (response) {
                listData = response.data;
                $tblMasterEl.bootstrapTable('load', response.data);
                $tblMasterEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            })
    }

    function LoadMergeData(id) {
        var piReceiveMasterIds = selectedPIReceiveList.map(function (el) { return el.YPIReceiveMasterID; }).join();

        var url = `/api/ybblcproposal/load-merge-data?id=${id}&piReceiveMasterIds=${piReceiveMasterIds}`;
        axios.get(url)
            .then(function (response) {
                
                bblcProposal = response.data;
                bblcProposal.ProposalDate = formatDateToDefault(bblcProposal.ProposalDate);
                goToDetails(bblcProposal);
                $formEl.find("#btnSave").fadeIn(); 
            })
            .catch(showResponseError);
    }

    function initMergeExistingTable() {
        $tblMergeExistingEl.bootstrapTable('destroy');
        $tblMergeExistingEl.bootstrapTable({
            showRefresh: true,
            filterControl: true,
            searchOnEnterKey: true,
            checkboxHeader: false,
            clickToSelect: true,
            idField: "ProposalID",
            columns: [
                {
                    field: "ProposalNo",
                    title: "Proposal No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "ProposalDate",
                    title: "Proposal Date",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "CompanyName",
                    title: "Company",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "SupplierName",
                    title: "Supplier",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-100' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "YPINo",
                    title: "PI No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LCNo",
                    title: "LC No",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "LCDate",
                    title: "LC Date",
                    filterControl: "input",
                    cellStyle: function () { return { classes: 'm-w-50' } },
                    formatter: function (value, row, index, field) {
                        return formatDateToDefault(value);
                    }
                },
                {
                    field: "TotalQty",
                    title: "Total Qty",
                    filterControl: "input",
                    align: 'right',
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                },
                {
                    field: "TotalValue",
                    title: "Total Value",
                    filterControl: "input",
                    align: 'right',
                    filterControlPlaceholder: FILTERCONTROLPLACEHOLDER
                }
            ],
            onDblClickRow: function (row, $element, field) {
                $modalMergeExistingEl.modal('hide');
                LoadMergeData(row.ProposalID);
            }
        });
    }

    function btnMergeExistingClick(e) {
        e.preventDefault();

        selectedPIReceiveList = $tblMasterEl.bootstrapTable('getSelections');

        if (!selectedPIReceiveList || selectedPIReceiveList.length === 0) return toastr.error("You must select one PI");

        var selectedRecords = selectedPIReceiveList.map(function (item) { return { CompanyID: item.CompanyID, SupplierID: item.SupplierID } });

        var isEqual_Spplier = true, isEqual_Company = true;
        for (var i = 1; i < selectedPIReceiveList.length; i++) {
            isEqual_Spplier = _.isEqual(selectedPIReceiveList[i - 1].SupplierID, selectedPIReceiveList[i].SupplierID);
            isEqual_Company = _.isEqual(selectedPIReceiveList[i - 1].CompanyID, selectedPIReceiveList[i].CompanyID);
            if (!isEqual_Spplier || !isEqual_Company) break;
        }

        if (!isEqual_Spplier || !isEqual_Company) {
            toastr.error("Company and Supplier must be same.");
            return;
        }

        //var isEqual = true;
        //for (var i = 1; i < selectedRecords.length; i++) {
        //    isEqual = _.isEqual(selectedRecords[i - 1], selectedRecords[i]);
        //    if (!isEqual) break;
        //}

        //if (!isEqual) {
        //    toastr.error("Company and Supplier must be same.");
        //    return;
        //}

        axios.get(`/api/ybblcproposal/list-for-merge/${selectedPIReceiveList[0].CompanyID}/${selectedPIReceiveList[0].SupplierID}/${isCDAPage}`)
            .then(function (response) {
                $tblMergeExistingEl.bootstrapTable("load", response.data);
                $modalMergeExistingEl.modal("show");
                isMerge = true; 
            })
            .catch(showResponseError);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function goToDetails(data) {
        $divDetailsEl.fadeIn();
        $divTblEl.fadeOut();
        resetForm();
        setFormData($formEl, data);
        initChildTable();
        $tblChildEl.bootstrapTable("load", data.Childs);
        getProposeContract();
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        getMasterTableData(); 
    }

    function ShowElement(flag) {
        if ($formEl.find("#CashStatus").is(':checked')) {
            $formEl.find(".divProposeContract").fadeOut();
            $formEl.find("#ProposeBankID").prop("disabled", false); 
        } else {
            $formEl.find(".divProposeContract").fadeIn();
            $formEl.find("#ProposeBankID").prop("disabled", true); 
        }
        if (flag) {
            $formEl.find("#ProposeBankID").val("").trigger("change");
            $formEl.find("#ProposeContract").val("");
        }
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#ProposalID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function calculateTotalQty(data) {
        var totalQty = 0;
       
        $.each(data, function (i, row) {
            totalQty += isNaN(parseFloat(row.TotalQty)) ? 0 : parseFloat(row.TotalQty);
        });

        return totalQty.toFixed(2);
    }

    function calculateTotalValue(data) {
        var totalValue = 0;

        $.each(data, function (i, row) {
            totalValue += isNaN(parseFloat(row.TotalValue)) ? 0 : parseFloat(row.TotalValue);
        });

        return totalValue.toFixed(4);
    }

    function calculateTotalYarnQtyAll(data) {
        var yarnPoQtyAll = 0;
        
        $.each(data, function (i, row) {
            yarnPoQtyAll += isNaN(parseFloat(row.TotalQty)) ? 0 : parseFloat(row.TotalQty);
        });

        return yarnPoQtyAll.toFixed(2);
    }

    function calculateTotalYarnValueAll(data) {
        var yarnPoValueAll = 0;

        $.each(data, function (i, row) {
            yarnPoValueAll += isNaN(parseFloat(row.TotalValue)) ? 0 : parseFloat(row.TotalValue);
        });

        return yarnPoValueAll.toFixed(4);
    }

    function Save(e) {
        e.preventDefault();
        var errors = validateForm($formEl, validationConstraints);
        if (errors) {
            showValidationErrorToast(errors)
            return;
        }
        else hideValidationErrors($formEl);

        var data = formDataToJson($formEl.serializeArray());
        data.IsCDA = isCDAPage;
        data.IsContract = isContract; 
        data.isMerge = isMerge;
        //data.ProposeContract = $formEl.find("#ProposeContract option:selected").text();
        data.ProposeBankID = $formEl.find("#ProposeBankID").val();
        data.ProposeBankName = $formEl.find("#ProposeBankID option:selected").text();
        data.RetirementModeID = $formEl.find("#RetirementModeID").val();

        data["Childs"] = bblcProposal.Childs;

        if ($formEl.find("#CashStatus").is(':checked')) {
            data.CashStatus = 1;
        }
        else {
            data.CashStatus = 0;
        } 

        
        if (isMerge) {
            if (parseInt(data.LCID) > 0) {
                data["YarnLCChilds"] = bblcProposal.Childs;
                $.each(data["YarnLCChilds"], function (j, obj) {
                    obj.LCID = data.LCID;
                });
            }
        } 

        if ($formEl.find("#CashStatus").is(':checked')) {
            if ($formEl.find("#ProposeBankID").val() == "" || $formEl.find("#ProposeBankID").val() == null) {
                toastr.error("Bank can't be blank");
                return;
            }
        }
        else {
            if ($formEl.find("#ProposeContract").val() == "") {
                toastr.error("LC / IBC No can't be blank");
                return;
            }
        }
        if (data.RetirementModeID == "" || data.RetirementModeID == null) {
            toastr.error("Please Select Retirement Mode");
            return;
        }
        axios.post("/api/ybblcproposal/save", data)
            .then(function () {
                toastr.success("BBLC Proposal has been saved.");
                backToList();
            })
            .catch(showResponseError);
    }
    function Revise(e) {
        e.preventDefault();
        isRevision = true;
        var errors = validateForm($formEl, validationConstraints);
        if (errors) {
            showValidationErrorToast(errors)
            return;
        }
        else hideValidationErrors($formEl);

        var data = formDataToJson($formEl.serializeArray());
        data.IsCDA = isCDAPage;
        data.IsContract = isContract;
        data.isMerge = isMerge;
        data.isRevision = isRevision;
        //data.ProposeContract = $formEl.find("#ProposeContract option:selected").text();
        data.ProposeBankID = $formEl.find("#ProposeBankID").val();
        data.ProposeBankName = $formEl.find("#ProposeBankID option:selected").text();
        data.RetirementModeID = $formEl.find("#RetirementModeID").val();

        data["Childs"] = bblcProposal.Childs;
        data["YarnLCChilds"] = bblcProposal.YarnLCChilds;


        if ($formEl.find("#CashStatus").is(':checked')) {
            data.CashStatus = 1;
        }
        else {
            data.CashStatus = 0;
        }

        if (isRevision) {
            data["Childs"] = bblcProposal.Childs;
            $.each(data["Childs"], function (j, obj) {
                obj.RevisionNo = obj.PIRevisionNo;
            });
        }

        if (isMerge) {
            if (parseInt(data.LCID) > 0) {
                data["YarnLCChilds"] = bblcProposal.Childs;
                $.each(data["YarnLCChilds"], function (j, obj) {
                    obj.LCID = data.LCID;
                });
            }
        }

        if ($formEl.find("#CashStatus").is(':checked')) {
            if ($formEl.find("#ProposeBankID").val() == "" || $formEl.find("#ProposeBankID").val() == null) {
                toastr.error("Bank can't be blank");
                return;
            }
        }
        else {
            if ($formEl.find("#ProposeContract").val() == "") {
                toastr.error("LC / IBC No can't be blank");
                return;
            }
        }
        if (data.RetirementModeID == "" || data.RetirementModeID == null) {
            toastr.error("Please Select Retirement Mode");
            return;
        }
        axios.post("/api/ybblcproposal/save", data)
            .then(function () {
                toastr.success("BBLC Proposal has been Revised.");
                backToList();
            })
            .catch(showResponseError);
    }
})();