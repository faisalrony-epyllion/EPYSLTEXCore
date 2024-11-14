(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblTestingRequirementId, $divItemInfo, $divTestingRequirement, $tblTestingRequirementEl, tblMasterId;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;
    var isApprovePage = false;
    var isAcknowledgePage = false;
    var masterData;
    var testingRequirementList = [];
  

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        $tblChildEl = $(pageConstants.CHILD_TBL_ID_PREFIX + pageId);
        tblTestingRequirementId = "#tblTestingRequirement" + pageId;
        tblTestingRequirementmodal = "#tblTestingRequirementmodal" + pageId;
        $divItemInfo = $(`#divItemInfo${pageId}`);
        $divTestingRequirement = $(`#divTestingRequirement${pageId}`);
        $modaltestrequirment = $(`#modaltestrequirment${pageId}`);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        isApprovePage = convertToBoolean($(`#${pageId}`).find("#ApprovePage").val());
        isAcknowledgePage = convertToBoolean($(`#${pageId}`).find("#AcknowledgePage").val());
        $formEl.find("#btnAddItem").fadeIn();
        if (isApprovePage) {
            status = statusConstants.COMPLETED;

            $toolbarEl.find("#btnCompleteList,#btnApproveList").fadeIn();
            $toolbarEl.find("#btnPending,#btnAcknowledgeList").fadeOut();

            toggleActiveToolbarBtn($toolbarEl.find("#btnCompleteList"), $toolbarEl);
            $formEl.find("#btnAddItem").fadeOut();
        } else if (isAcknowledgePage) {
            status = statusConstants.APPROVED;

            $toolbarEl.find("#btnApproveList,#btnAcknowledgeList").fadeIn();
            $toolbarEl.find("#btnPending,#btnCompleteList").fadeOut();

            toggleActiveToolbarBtn($toolbarEl.find("#btnApproveList"), $toolbarEl);
            $formEl.find("#btnAddItem").fadeOut();
        } else {
            status = statusConstants.PENDING;

            $toolbarEl.find("#btnPending,#btnCompleteList").fadeIn();
            $toolbarEl.find("#btnApproveList,#btnAcknowledgeList").fadeIn();

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

        $toolbarEl.find("#btnCompleteList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            initMasterTable();
        });

        $toolbarEl.find("#btnApproveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.APPROVED;
            $formEl.find("#btnSave").fadeOut();
            initMasterTable();
        });

        $toolbarEl.find("#btnAcknowledgeList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.ACKNOWLEDGE;
            $formEl.find("#btnSave").fadeOut();
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            approve(true,false);
            
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            approve(true,true);
        });

        $formEl.find("#btnBack").on("click", backToList);

        $formEl.find("#btnAddBuyer").click(function (e) {
            e.preventDefault();
            getBuyer();
        });

        $formEl.find("#btnAddItem").on("click", function (e) {
            e.preventDefault();

            //var selectedUniqueIds = testingRequirementList[0] ? testingRequirementList[0].map(function (el) {
            //    return el.BPID;
            //}) : [];

            var finder = new commonFinder({
                title: "Select Testing Requirment",
                pageId: pageId,
                data: masterData.YarnLabTestRequisitionChilds[0].NewYarnLabTestRequisitionChildParameters,
                fields: "TestName,Requirement",
                headerTexts: "Testing Requirement Type, Requirement",
                isMultiselect: true,
                primaryKeyColumn: "LTPramID",
             /*   selectedUniqueIds: selectedUniqueIds,*/
                onMultiselect: function (selectedRecords) {
                    
                    selectedRecords.forEach(function (item) {
                        var exists = $tblTestingRequirementEl.dataSource.find(function (el) { return el.LTPramID == item.LTPramID });
                        if (!exists) $tblTestingRequirementEl.dataSource.unshift(item);
                    });
                    initTestingRequirementTable($tblTestingRequirementEl.dataSource);
                    $tblTestingRequirementEl.refresh();
                    //var newItems = getNewArrayItems(testingRequirementList, selectedRecords, "BPID");
                    //newItems.forEach(function (item) {
                    //    $tblTestingRequirementEl.dataSource.unshift(item);
                    //});
                    
                    //$tblTestingRequirementEl.refresh();
                }
            });
            finder.showModal();
        });

    });

    function initMasterTable() {
       var commands = status == statusConstants.PENDING
            ? [{ type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }]
            : [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'ViewReport', title: 'View Report', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }];

        var columns = [
            {
                headerText: 'Actions', commands: commands,width:120
            },
            {
                field: 'ReqNo', headerText: 'Req. No', visible: status != statusConstants.PENDING,
            },
            {
                field: 'ReqDate', headerText: 'Req. Date', textAlign: 'Right', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING,
            },
            {
                field: 'QCReqNo', headerText: 'QC.ReqNo'
            },
            {
                field: 'QCReqDate', headerText: 'QC.Req.Date', textAlign: 'Right', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'LocationName', headerText: 'Location Name'
            },
            //{
            //    field: 'BookingNo', headerText: 'Booking No'
            //},
            {
                field: 'CompanyName', headerText: 'Company Name'
            },
            {
                field: 'SupplierName', headerText: 'Supplier Name'
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            apiEndPoint: `/api/yarn-lab-test-requisition/list?status=${status}`,
            autofitColumns: false,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        
        if (status == statusConstants.PENDING) {
            getNew(args.rowData.QCReqMasterID);
            $formEl.find("#btnSave").fadeIn();
            $formEl.find("#btnApprove,#btnAcknowledge").fadeOut();
        }
        else {
            if (args.commandColumn.type == "ViewReport") {
                window.open(`/reports/InlinePdfView?ReportId=1252&ReqID=${args.rowData.YLTReqMasterID}`, '_blank');
            }
            getDetails(args.rowData.YLTReqMasterID);
            if (isApprovePage) {
                $toolbarEl.find("#btnAddBuyer").fadeOut();
                if (status == statusConstants.COMPLETED) {
                    $formEl.find("#btnApprove").fadeIn();
                    $formEl.find("#btnAcknowledge,#btnSave").fadeOut();
                } else {
                    $formEl.find("#btnSave,#btnApprove,#btnAcknowledge").fadeOut();
                }
            } else if (isAcknowledgePage) {
                $toolbarEl.find("#btnAddBuyer").fadeOut();
                if (status == statusConstants.APPROVED) {
                    $formEl.find("#btnAcknowledge").fadeIn();
                    $formEl.find("#btnApprove,#btnSave").fadeOut();
                } else {
                    $formEl.find("#btnSave,#btnApprove,#btnAcknowledge").fadeOut();
                }
            } else {
                $toolbarEl.find("#btnAddBuyer").fadeIn();
                if (status == statusConstants.COMPLETED) {
                    $formEl.find("#btnSave").fadeIn();
                    $formEl.find("#btnAcknowledge,#btnApprove").fadeOut();
                }
            }

        }
        
    }

    function initChildTable() {
        $tblChildEl.bootstrapTable("destroy");
        $tblChildEl.bootstrapTable({
            showFooter: true,
            detailView: true,
            columns: [
                {
                    title: 'Actions',
                    align: 'center',
                    width: 10,
                    visible: (!isApprovePage && !isAcknowledgePage && (status == statusConstants.PENDING || status == statusConstants.COMPLETED)),
                    formatter: function () {
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
                            masterData.LabTestRequisitionBuyers.splice(index, 1);
                            //$tblChildEl.bootstrapTable('hideRow', { index: index });
                            $tblChildEl.bootstrapTable("load", masterData.YarnLabTestRequisitionChilds);
                            $tblChildEl.bootstrapTable('hideLoading');
                        }
                    }
                },
                {
                    field: "BuyerName",
                    title: "Buyer Name",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                }
            ],
            onExpandRow: function (index, row, $detail) {
                populateLabTestRequisitionBuyer(row.YLTReqBuyerID, $detail);
            },
        });
    }

    function initTestingRequirementTable(records) {
        if ($tblTestingRequirementEl) $tblTestingRequirementEl.destroy();
        ej.base.enableRipple(true);
        $tblTestingRequirementEl = new ej.grids.Grid({
            allowPaging: true,
            allowFiltering: true,
            pageSettings: { pageCount: 5, currentPage: 1, pageSize: 10, pageSizes: true },
            editSettings: { allowDeleting: true },
            dataSource: records,
            commandClick: chhandleCommands,
            columns: [
                {
                    headerText: 'Remove', width: 120, commands: [                       
                        { type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-delete e-icons' } }]
                },
                { field: 'TestName', headerText: 'Testing Requirement Type' },
                { field: 'Requirement', headerText: 'Requirement' }
            ]
        });
        $tblTestingRequirementEl.appendTo(tblTestingRequirementId);
    }
    function chhandleCommands(args) {
        if (args.commandColumn.type == "Add") {
            showBootboxConfirm("Delete Record.", "Are you sure want to delete this?", function (yes) {
                if (yes) {
                    var i = 0;
                    while (i < $tblTestingRequirementEl.dataSource.length) {
                        if ($tblTestingRequirementEl.dataSource[i].LTPramID === args.rowData.LTPramID) {
                            $tblTestingRequirementEl.dataSource.splice(i, 1);
                        } else {
                            ++i;
                        }
                    }
                    initTestingRequirementTable($tblTestingRequirementEl.dataSource);
                }
            });
        }
    }
    function getBuyer() {
        var url = "/api/selectoption/buyername";
        axios.get(url)
            .then(function (response) {
                showBootboxSelect2MultipleDialog("Select Buyer", "Ids", "Select Buyer", response.data, function (result) {
                    if (result) {
                        var ids = "";
                        for (var i = 0; i < result.length; i++) ids += result[i].id + ",";
                        getBuyerParameter(ids.substring(0, ids.length - 1));
                    }
                });
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getBuyerParameter(ids) {
        var url = "/api/yarn-lab-test-requisition/buyer-parameter/" + ids;
        axios.get(url)
            .then(function (response) {
                for (var i = 0; i < response.data.length; i++) {
                    //response.data[i].LTReqBuyerID = (masterData.YarnLabTestRequisitionChilds.length == 0) ? 1 : masterData.YarnLabTestRequisitionChilds[masterData.YarnLabTestRequisitionChilds.length - 1].YLTReqChildID + 1;
                    masterData.YarnLabTestRequisitionChilds.push(response.data[i]);
                }

                $tblChildEl.bootstrapTable("load", masterData.YarnLabTestRequisitionChilds);
                $tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function populateLabTestRequisitionBuyer(childId, $detail) {
        $el = $detail.html('<table id="TblYarnReceiveChildRackBin-' + childId + '"></table>').find('table');
        initLabTestRequisitionBuyer($el, childId);
        var ind = getIndexFromArray(masterData.YarnLabTestRequisitionChilds, "YLTReqChildID", childId)
        var cnt = masterData.YarnLabTestRequisitionChilds[ind].YarnLabTestRequisitionChildParameters.length;
        if (cnt > 0)
            $el.bootstrapTable('load', masterData.YarnLabTestRequisitionChilds[ind].YarnLabTestRequisitionBuyerParameters);
    }

    function initLabTestRequisitionBuyer($el, childId) {
        $el.bootstrapTable({
            showFooter: true,
            uniqueId: 'YLTReqChildID',
            columns: [
                {
                    field: "YLTReqChildID",
                    title: "YLTReqChildID",
                    align: "left",
                    width: 100,
                    visible: false
                },
                {
                    field: "ParameterName",
                    title: "Parameter Name",
                    cellStyle: function () { return { classes: 'm-w-100' } }
                },
                {
                    field: "RefValueFrom",
                    title: "Ref Value From",
                    cellStyle: function () { return { classes: 'm-w-80' } }
                },
                {
                    field: "RefValueTo",
                    title: "Ref Value To",
                    cellStyle: function () { return { classes: 'm-w-80' } }
                }
            ]
        });
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
        testingRequirementList = [];
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#YLTReqMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(newId) {
        axios.get(`/api/yarn-lab-test-requisition/new/${newId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;                
                masterData.ReqDate = formatDateToDefault(masterData.ReqDate);
                masterData.DBatchDate = formatDateToDefault(masterData.DBatchDate);
                setFormData($formEl, masterData);
                $tblChildEl.bootstrapTable("load", masterData.YarnLabTestRequisitionChilds);
                $tblChildEl.bootstrapTable('hideLoading');
                initTestingRequirementTable([]);
                if (masterData.ConceptNo != '') {
                    $divItemInfo.fadeOut();
                    $divTestingRequirement.fadeIn();
                }
                else {
                    $divItemInfo.fadeIn();
                    $divTestingRequirement.fadeOut();
                }

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        
        axios.get(`/api/yarn-lab-test-requisition/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                
                masterData = response.data;
                masterData.ReqDate = formatDateToDefault(masterData.ReqDate);
                masterData.DBatchDate = formatDateToDefault(masterData.DBatchDate);
                setFormData($formEl, masterData);
                $tblChildEl.bootstrapTable("load", masterData.YarnLabTestRequisitionChilds);
                $tblChildEl.bootstrapTable('hideLoading');

               
                testingRequirementList = masterData.YarnLabTestRequisitionChilds[0].YarnLabTestRequisitionChildParameters;
                initTestingRequirementTable(testingRequirementList);
                if (masterData.ConceptNo != '') {
                    $divItemInfo.fadeOut();
                    $divTestingRequirement.fadeIn();
                }
                else {
                    $divItemInfo.fadeIn();
                    $divTestingRequirement.fadeOut();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    var validationConstraints = {
        UnitID: {
            presence: true
        },
        FabricQty: {
            presence: true
        }
    }

    function save() {
        initializeValidation($formEl, validationConstraints);
        //if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        //else hideValidationErrors($formEl);
        masterData.YarnLabTestRequisitionChilds[0].YarnLabTestRequisitionChildParameters = $tblTestingRequirementEl.getCurrentViewRecords(); //testingRequirementList;
        var data = formDataToJson($formEl.serializeArray());
        data["YarnLabTestRequisitionChilds"] = masterData.YarnLabTestRequisitionChilds;
        var ChekBox;
        if ($formEl.find("#IsRetest").is(':checked'))
            ChekBox =1;
        else 
            ChekBox = 0;
        data["IsRetest"] =ChekBox;

       

        axios.post("/api/yarn-lab-test-requisition/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function approve(isApprove = false, isAcknowledge = false) {
        var data = formDataToJson($formEl.serializeArray());
        data.IsApproved = isApprove;
        data.IsAcknowledge = isAcknowledge;
        axios.post("/api/yarn-lab-test-requisition/approve", data)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }


})();
