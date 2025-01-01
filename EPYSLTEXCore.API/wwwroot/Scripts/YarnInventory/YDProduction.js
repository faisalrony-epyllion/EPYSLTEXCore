(function () {
    var menuId, pageName, menuParam;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblChildId;
    var YDProductionApprovePage = false;
    var YDProductionAcknowledgePage = false;
    var tableParams = {
        offset: 0,
        limit: 10,
        filter: '',
        sort: '',
        order: ''
    }
    var status;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");
        if (!menuParam)
            menuParam = localStorage.getItem("menuParam");

        var pageId = pageName + "-" + menuId;
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING;
        
        if (menuParam == 'A') {
            YDProductionAcknowledgePage = false; YDProductionApprovePage = true;
        }
        else if (menuParam == 'Ack') { YDProductionApprovePage = false; YDProductionAcknowledgePage = true; }            
        else {
            YDProductionApprovePage = false;
            YDProductionAcknowledgePage = false;
        }


        if (YDProductionApprovePage) {
            status = statusConstants.UN_APPROVE;
            $toolbarEl.find("#btnAwaitingApproveList").show();
            $toolbarEl.find("#btnApproveList").show();
            $toolbarEl.find("#btnPending").hide();
            $toolbarEl.find("#btnAcknowledgeList").hide();

            toggleActiveToolbarBtn($toolbarEl.find("#btnAwaitingApproveList"), $toolbarEl);
            $formEl.find("#btnApprove").show();
            $formEl.find("#btnSave").hide();
            $formEl.find("#btnAcknowledge").hide();
        }

        if (YDProductionAcknowledgePage) {
            status = statusConstants.APPROVED;
            $toolbarEl.find("#btnApproveList").show();
            $toolbarEl.find("#btnAcknowledgeList").show();
            $toolbarEl.find("#btnPending").hide();
            $toolbarEl.find("#btnAwaitingApproveList").hide();
            toggleActiveToolbarBtn($toolbarEl.find("#btnApproveList"), $toolbarEl);
            $formEl.find("#btnApprove").hide();
            $formEl.find("#btnSave").hide();
            $formEl.find("#btnAcknowledge").show();
        }

        initMasterTable();
        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;
            initMasterTable();
        });

        $toolbarEl.find("#btnAwaitingApproveList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.UN_APPROVE;
            initMasterTable();
        });

        $toolbarEl.find("#btnApproveList").on("click", function (e) {
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

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnApprove").click(function (e) {
            e.preventDefault();
            approve();
        });

        $formEl.find("#btnAcknowledge").click(function (e) {
            e.preventDefault();
            acknowledge();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnAddDMNo").on("click", function (e) {
            e.preventDefault();
            var finder = new commonFinder({
                title: "Select Machine",
                pageId: pageId,
                isMultiselect: false,
                modalSize: "modal-md",
                primaryKeyColumn: "DMID",
                fields: "DyeingMcslNo,DyeingMcStatus,DyeingNozzleQty,DyeingMcCapacity,DyeingMcBrand",
                headerTexts: "Machine No,Status,Nozzle,Capacity,Brand",
                apiEndPoint: `/api/dyeing-machine/nozzle-list`,
                onSelect: function (res) {
                    finder.hideModal();
                    $formEl.find("#MCSLNo").val(res.rowData.DyeingMcslNo);
                    $formEl.find("#DMID").val(res.rowData.DMID);
                },

            });
            finder.showModal();
        });

    });

    function initMasterTable() {
        var columns = [
            {
                headerText: '', width: 100, textAlign: 'Center', commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            {
                field: 'YDRecipeNo', headerText: 'YD Recipe No', visible: status == statusConstants.PENDING,
            },
            {
                field: 'RecipeReqNo', headerText: 'Recipe Request No', textAlign: 'Center', visible: status == statusConstants.PENDING,
            },
            {
                field: 'YDDBatchNo', headerText: 'YD Dyeing Batch No', textAlign: 'Center', visible: status == statusConstants.PENDING,
            },
            {
                field: 'YDProductionNo', headerText: 'Production No', visible: status != statusConstants.PENDING,
            },
            {
                field: 'YDProductionDate', headerText: 'Production Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status != statusConstants.PENDING,
            },
            {
                field: 'GroupConceptNo', headerText: 'Group Concept No'
            },
            {
                field: 'YDBookingNo', headerText: 'Booking No'
            },
            {
                field: 'ColorName', headerText: 'Color Name'
            },
            {
                field: 'Segment1ValueDesc', headerText: 'Item Name'
            },
            {
                field: 'Segment2ValueDesc', headerText: 'Yarn Type'
            },
            {
                field: 'Segment3ValueDesc', headerText: 'Process'
            },
            {
                field: 'Segment4ValueDesc', headerText: 'Sub Process'
            },
            {
                field: 'Segment5ValueDesc', headerText: 'Quality Paramenter'
            },
            {
                field: 'Segment6ValueDesc', headerText: 'Count'
            },
            {
                field: 'Segment7ValueDesc', headerText: 'No of ply'
            },
            {
                field: 'YDBookingDate', headerText: 'Booking Date', textAlign: 'Right', type: 'date', format: _ch_date_format_1
            },
            //{
            //    field: 'Buyer', headerText: 'Buyer'
            //}
            {
                field: 'MCSLNo', headerText: 'M/C No', visible: status != statusConstants.PENDING,
            },
            {
                field: 'BatchNo', headerText: 'Batch No', visible: status != statusConstants.PENDING,
            },
            {
                field: 'Shift', headerText: 'Shift', visible: status != statusConstants.PENDING,
            },
            {
                field: 'Operator', headerText: 'Operator', visible: status != statusConstants.PENDING,
            },
            {
                field: 'Remarks', headerText: 'Remarks', visible: status == statusConstants.PENDING
            },
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yarn-production/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function handleCommands(args) {
        if (status === statusConstants.PENDING) {
            getNew(args.rowData.YDBookingMasterID, args.rowData.ItemMasterID, args.rowData.ColorID, args.rowData.YDDBatchID);
            $formEl.find("#btnSave").fadeIn();
            $formEl.find("#btnApprove,#btnAcknowledge").fadeOut();
        }
        else if (YDProductionApprovePage) {
            getDetails(args.rowData.YDProductionMasterID);
            if (status === statusConstants.UN_APPROVE) {
                $formEl.find("#btnApprove").fadeIn();
            }
            else {
                $formEl.find("#btnApprove").fadeOut();
            }
            $formEl.find("#btnSave,#btnAcknowledge").fadeOut();
        }
        else if (YDProductionAcknowledgePage) {
            getDetails(args.rowData.YDProductionMasterID);
            if (status === statusConstants.APPROVED) {
                $formEl.find("#btnAcknowledge").fadeIn();
            }
            else {
                $formEl.find("#btnAcknowledge").fadeOut();
            }
            $formEl.find("#btnSave,#btnApprove").fadeOut();
        }
        else {
            getDetails(args.rowData.YDProductionMasterID);
            $formEl.find("#btnSave,#btnApprove,#btnAcknowledge").fadeOut();
        }
    }


    function initChildTable(records) {
        if ($tblChildEl) $tblChildEl.destroy();

        var columns = [
            { field: 'ItemMasterID', isPrimaryKey: true, visible: false },
            {
                field: 'YDRICRBId', headerText: 'YDRICRBId', visible: false
            },
        ];
        columns.push.apply(columns,
            [
                {
                    headerText: 'Commands', width: 120, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                }
            ]
        );
        columns.push.apply(columns, getYarnItemColumnsForDisplayOnly());
        var additionalColumns = [
            { field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false },
            { field: 'ColorName', headerText: 'Color Name', allowEditing: false },
            { field: 'ColorCode', headerText: 'Color Code', allowEditing: false },
            { field: 'BookingQty', headerText: 'Booking Qty(Kg)', allowEditing: false },
            { field: 'BookingConeQty', headerText: 'Booking Cone Qty(Pcs)', allowEditing: false },
            { field: 'ProducedQty', headerText: 'Produced Qty(kg)', editType: "numericedit", edit: { params: { decimals: 0, min: 1 } }, allowEditing: status === statusConstants.PENDING },
            { field: 'ProducedCone', headerText: 'Produced Cone(pcs)', editType: "numericedit", edit: { params: { decimals: 0, min: 1 } }, allowEditing: status === statusConstants.PENDING },
        ];
        columns.push.apply(columns, additionalColumns);

        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            data: records,
            autofitColumns: false,
            //allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: columns,
            actionComplete: function (e) {
                if (parseInt(e.data.ProducedQty) > parseInt(e.data.BookingQty)) {
                    showBootboxAlert('Produced Qty is not more than ' + e.data.BookingQty);
                    var index = $tblChildEl.getRowIndexByPrimaryKey(e.data.ItemMasterID);
                    e.data.ProducedQty = e.data.BookingQty;
                    $tblChildEl.updateRow(index, e.data);
                }
            }

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
        $formEl.find("#YDProductionMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(newId, itemMasterID, colorID, ydDBatchID) {
        axios.get(`/api/yarn-production/new/${newId}/${itemMasterID}/${colorID}/${ydDBatchID}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.YDProductionDate = formatDateToDefault(masterData.YDProductionDate);
                masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-production/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.YDProductionDate = formatDateToDefault(masterData.YDProductionDate);
                masterData.YDBookingDate = formatDateToDefault(masterData.YDBookingDate);

                setFormData($formEl, masterData);
                initChildTable(masterData.Childs);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    var validationConstraints = {
        MCSLNo: {
            presence: true
        },
        ShiftID: {
            presence: true
        },
        BatchNo: {
            presence: true
        },
        OperatorID: {
            presence: true
        }
    }

    function save() {
        initializeValidation($formEl, validationConstraints);
        if (!isValidForm($formEl, validationConstraints)) return toastr.error("Please correct all validation errors!");
        else hideValidationErrors($formEl);

        var data = formDataToJson($formEl.serializeArray());
        data["Childs"] = $tblChildEl.getCurrentViewRecords();

        axios.post("/api/yarn-production/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function approve() {
        var id = $formEl.find("#YDProductionMasterID").val();
        axios.post(`/api/yarn-production/approve/${id}`)
            .then(function () {
                toastr.success("Approved successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }

    function acknowledge() {
        var id = $formEl.find("#YDProductionMasterID").val();
        axios.post(`/api/yarn-production/acknowledge/${id}`)
            .then(function () {
                toastr.success("Acknowledged successfully!");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();