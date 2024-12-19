(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $tblChildEl, $formEl, tblMasterId, tblChildId;
    var filterBy = {};
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    var status;

    var masterData;

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
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING;

        initMasterTable();

        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.PENDING;

            initMasterTable();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            status = statusConstants.COMPLETED;

            initMasterTable();
        });

        //$toolbarEl.find("#btnAllList").on("click", function (e) {
        //    e.preventDefault();
        //    toggleActiveToolbarBtn(this, $toolbarEl);
        //    resetTableParams();
        //    status = statusConstants.ALL;
        //    if (status == statusConstants.ALL) {
        //        $formEl.find("#btnSave").hide();
        //    }

        //    initMasterTable();
        //});
      
        $formEl.find("#btnSave").on("click", save);

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnReport").click(function () {
            window.open(`/reports/InlinePdfView?ReportName=DailyYarnQCRequisitionSlip.rdl&QCReqMasterID=${masterData.QCReqMasterId}`, '_blank');
        });
    });

    function initMasterTable() {
        var columns = [
            {
                headerText: '',
                width: 40,
                //commands: [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
                commands: status == statusConstants.PENDING ? [{ type: 'Add', buttonOption: { cssClass: 'e-flat', iconCss: 'e-add e-icons' } }] : [{ type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            //{
            //    field: 'Status', headerText: 'Status', width: 160, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.COMPLETED
            //},

            {
                field: 'QCReceiveNo', headerText: 'Receive No', width: 80, visible: status !== statusConstants.PENDING
            },
            {
                field: 'QCReceiveDate', headerText: 'Receive Date', width: 150, textAlign: 'Center', type: 'date', visible: status !== statusConstants.PENDING && status != statusConstants.ALL, format: _ch_date_format_5
            },
            {
                field: 'QCReceivedByUser', headerText: 'Receive By', width: 80, visible: status !== statusConstants.PENDING && status != statusConstants.ALL
            },
            {
                field: 'QCIssueNo', width: 100, headerText: 'Issue No', visible: status != statusConstants.ALL
            },
            {
                field: 'Status', headerText: 'Status', width: 160, allowEditing: false
            },
            {
                field: 'QCIssueDate', width: 150, headerText: 'Issue Date', type: 'date', format: _ch_date_format_5, visible: status != statusConstants.ALL
            },
            {
                field: 'QCReqNo', width: 80, headerText: 'Req No'
            },
            {
                field: 'QCReqDate', width: 80, headerText: 'Req Date', type: 'date', format: _ch_date_format_1, textAlign: 'Center'
            },
            {
                field: 'QCIssueByUser', width: 80, headerText: 'Issue By', visible: status != statusConstants.ALL
            },
            {
                field: 'QCReqFor', width: 80, headerText: 'Req For'
            },
            {
                field: 'QCReqByUser', width: 80, headerText: 'Req By'
            }
            //{
            //    field: 'ChallanDate', headerText: 'Challan Date', width: 80, allowEditing: false, textAlign: 'Center', type: 'date', format: _ch_date_format_1, visible: status == statusConstants.ALL
            //},
            //{
            //    field: 'ChallanNo', headerText: 'Challan No', width: 80, allowEditing: false, visible: status == statusConstants.ALL
            //},
            //{
            //    field: 'IsAcknowledgeStr', headerText: 'Acknowledged', width: 80, allowEditing: false, visible: status != statusConstants.PENDING && status != statusConstants.COMPLETED
            //}
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yarn-qc-receive/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        if (status === statusConstants.PENDING) {
            getNew(args.rowData.QCIssueMasterId);
        }
        else {
            getDetails(args.rowData.QCReceiveMasterID);
            //$formEl.find("#btnSave").hide();
        }
    }

    var machineTypeElem;
    var machineTypeObj;
    var technicalNameElem;
    var technicalNameObj;

    function initChildTable(records) {
        if ($tblChildEl) $tblChildEl.destroy();

        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            data: records,
            autofitColumns: true,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 100, visible: status == statusConstants.PENDING, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'QCReceiveChildID', isPrimaryKey: true, visible: false },
                { field: 'QCReceiveMasterID', visible: false },
                { field: 'QCIssueChildID', visible: false },
                { field: 'ReceiveChildID', visible: false },

                /*
                {
                    field: 'MachineType', headerText: 'Machine Type', allowEditing: false,
                    valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            machineTypeElem = document.createElement('input');
                            return machineTypeElem;
                        },
                        read: function () {
                            return machineTypeObj.text;
                        },
                        destroy: function () {
                            machineTypeObj.destroy();
                        },
                        write: function (e) {
                            machineTypeObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.MCTypeForFabricList,
                                fields: { value: 'id', text: 'text' },
                                change: function (f) {
                                    technicalNameObj.enabled = true;
                                    var tempQuery = new ej.data.Query().where('additionalValue', 'equal', machineTypeObj.value);
                                    technicalNameObj.query = tempQuery;
                                    technicalNameObj.text = null;
                                    technicalNameObj.dataBind();

                                    e.rowData.MachineTypeId = f.itemData.id;
                                    e.rowData.MachineType = f.itemData.text;
                                    e.rowData.KTypeId = f.itemData.desc;
                                    e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);
                                },
                                placeholder: 'Select M/C Type',
                                floatLabelType: 'Never'
                            });
                            machineTypeObj.appendTo(machineTypeElem);
                        }
                    }
                },
                {
                    field: 'TechnicalName', headerText: 'Technical Name', allowEditing: false, width: 400, displayField: "TechnicalName", valueAccessor: ej2GridDisplayFormatterV2, edit: {
                        create: function () {
                            technicalNameElem = document.createElement('input');
                            return technicalNameElem;
                        },
                        read: function () {
                            return technicalNameObj.text;
                        },
                        destroy: function () {
                            technicalNameObj.destroy();
                        },
                        write: function (e) {
                            technicalNameObj = new ej.dropdowns.DropDownList({
                                dataSource: masterData.TechnicalNameList,//.filter(x => x.id == _machineTypeId),
                                fields: { value: 'id', text: 'text' },
                                enabled: false,
                                placeholder: 'Select Technical Name',
                                floatLabelType: 'Never',
                                change: function (f) {
                                    if (!f.isInteracted || !f.itemData) return false;
                                    e.rowData.TechnicalTime = parseInt(f.itemData.desc);
                                    e.rowData.TechnicalNameId = f.itemData.id;
                                    e.rowData.TechnicalName = f.itemData.text;
                                    e.rowData = setTotalDaysAndDeliveryDate(e.rowData, e.rowData.CriteriaNames);
                                    //$tblChildEl.updateRow(e.row.rowIndex, e.rowData);
                                }
                            });
                            technicalNameObj.appendTo(technicalNameElem);
                        }
                    }
                },
                {
                    field: 'BuyerID', headerText: 'Buyer', allowEditing: false, width: 400, valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.BuyerList,
                    displayField: "text",
                    edit: ej2GridDropDownObj({
                        width: 350
                    })
                },
                */
                //{ field: 'Segment1ValueDesc', headerText: 'Composition', allowEditing: false },
                //{ field: 'Segment2ValueDesc', headerText: 'Yarn Type', allowEditing: false },
                //{ field: 'Segment3ValueDesc', headerText: 'Process', allowEditing: false },
                //{ field: 'Segment4ValueDesc', headerText: 'Sub process', allowEditing: false },
                //{ field: 'Segment5ValueDesc', headerText: 'Quality Parameter', allowEditing: false },
                //{ field: 'Segment6ValueDesc', headerText: 'Yarn Count', allowEditing: false },
                //{ field: 'Segment7ValueDesc', headerText: 'No of Ply', allowEditing: false },
                //{ field: 'ShadeCode', headerText: 'Shade Code', allowEditing: false },
                { field: 'Status', headerText: 'Status', allowEditing: false },
                { field: 'YarnCategory', headerText: 'Yarn Details', allowEditing: false },
                { field: 'Uom', headerText: 'Uom', allowEditing: false, textAlign: 'Center' },
                { field: 'YarnColor', headerText: 'Yarn Color', allowEditing: false },
                { field: 'ChallanLot', headerText: 'Challan Lot', allowEditing: false },
                { field: 'LotNo', headerText: 'Physical Lot', allowEditing: false, textAlign: 'Center' },
                { field: 'PhysicalCount', headerText: 'Physical Count', allowEditing: false, textAlign: 'Center' },
                { field: 'Spinner', headerText: 'Spinner', allowEditing: false, textAlign: 'Center' },

                { field: 'ReqQtyCarton', headerText: 'Req Qty Bag/Carton(Pcs)', allowEditing: false },
                { field: 'ReqQtyCone', headerText: 'Req Qty Cone(Pcs)', allowEditing: false },
                { field: 'ReqQty', headerText: 'Req Qty(KG)', allowEditing: false },

                { field: 'IssueQtyCarton', headerText: 'Issue Qty Bag/Carton(Pcs)', allowEditing: false },
                { field: 'IssueQtyCone', headerText: 'Issue Qty Cone(Pcs)', allowEditing: false },
                { field: 'IssueQty', headerText: 'Issue Qty(KG)', allowEditing: false },

                { field: 'ReceiveQtyCarton', headerText: 'Receive Qty Bag/Carton(Pcs)' },
                { field: 'ReceiveQtyCone', headerText: 'Receive Qty Cone(Pcs)' },
                { field: 'ReceiveQty', headerText: 'Receive Qty (KG)' },

                { field: 'QCReqRemarks', headerText: 'Remarks', allowEditing: false }
            ]
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
        $formEl.find("#QCReceiveMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew(qcIssuerMasterId) {
        axios.get(`/api/yarn-qc-receive/new/${qcIssuerMasterId}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.QCReceiveDate = formatDateToDefault(masterData.QCReceiveDate);
                masterData.QCReqDate = formatDateToDefault(masterData.QCReqDate);
                masterData.QCIssueDate = formatDateToDefault(masterData.QCIssueDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.YarnQCReceiveChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function getDetails(id) {
        axios.get(`/api/yarn-qc-receive/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.QCReqDate = formatDateToDefault(masterData.QCReqDate);
                masterData.QCIssueDate = formatDateToDefault(masterData.QCIssueDate);
                masterData.QCReceiveDate = formatDateToDefault(masterData.QCReceiveDate);
                setFormData($formEl, masterData);
                initChildTable(masterData.YarnQCReceiveChilds);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(e) {
        e.preventDefault();
        var data = formDataToJson($formEl.serializeArray());
        data["YarnQCReceiveChilds"] = $tblChildEl.getCurrentViewRecords();
        data.ReceiveID = masterData.ReceiveID;
        axios.post("/api/yarn-qc-receive/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
})();