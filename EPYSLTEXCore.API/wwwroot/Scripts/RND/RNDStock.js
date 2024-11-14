(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl, tblMasterId, $tblChildEl, tblChildId;
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
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        status = statusConstants.PENDING;

        initMasterTable();
        $toolbarEl.find("#btnNew").on("click", function (e) {
            e.preventDefault();
            $divDetailsEl.fadeIn();
            initChildTable([]);
            resetForm();
            $divTblEl.fadeOut();
        });
        $toolbarEl.find("#btnPending").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.PENDING;
            initMasterTable();
        });

        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            status = statusConstants.COMPLETED;
            initMasterTable();
        });

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnCancel").on("click", backToList);

        $formEl.find("#btnAddRoll").on("click", function (e) {
            e.preventDefault();
            var batchIds = "";
            for (var i = 0; i < $tblChildEl.getCurrentViewRecords().length; i++) batchIds += $tblChildEl.getCurrentViewRecords()[i].BatchID + ","
            batchIds = batchIds.substr(0, batchIds.length - 1);

            //var finder = new commonFinder({
            //    title: "Select Roll",
            //    pageId: pageId,
            //    //apiEndPoint: `/api/RND_Stock/batch-list/${batchIds}`,
            //    fields: "BatchNo,BatchDate",
            //    headerTexts: "Batch No, Batch Date",
            //    isMultiselect: true,
            //    primaryKeyColumn: "BatchID",
            //    onMultiselect: function (selectedRecords) {
            //        batchIds = "";
            //        for (var i = 0; i < selectedRecords.length; i++) batchIds += selectedRecords[i].BatchID + ","
            //        batchIds = batchIds.substr(0, batchIds.length - 1);
            //        getSelectedBatchDetails(batchIds);
            //    }
            //});
            //finder.showModal();
        });
    });


    function initMasterTable() {
        var commands = [];
        if (status == statusConstants.PENDING) {
            commands = [
                {
                    type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' }
                }
            ];
        } else {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                { type: 'ReportCHanger', title: 'Conuter Hanger', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } },
                { type: 'ReportMHanger', title: 'Marketing Hanger', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-file-pdf-o' } }
            ];
        }

        columns = [
            {
                headerText: '', textAlign: 'Center', commands: commands,
                textAlign: 'Center', width: 100, minWidth: 100, maxWidth: 100
            },
            {
                field: 'ConceptID', headerText: 'ConceptID', visible: false
            },
            {
                field: 'IsBDS', headerText: 'Type', visible: false
            },
            {
                field: 'BatchNo', headerText: 'Batch No'
            },
            {
                field: 'MachineType', headerText: 'Machine Type', textAlign: 'Center', type: 'date', format: 'dd/MM/yyyy'
            },
            {
                field: 'TrialNo', headerText: 'Trial No', visible: false
            },
            {
                field: 'Color', headerText: 'Color', visible: false
            },
            {
                field: 'FabricGSM', headerText: 'Fabric GSM'
            },
            {
                field: 'Qty(Kg)', headerText: 'Qty(Kg)'
            },
            {
                field: 'Qty(Pcs)', headerText: 'Qty(Pcs)'
            }
        ];
        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            allowFiltering: true,
            apiEndPoint: `/api/RND_Stock/list?status= //${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    async function initChildTable(data) {
        if ($tblItemChildE1) {
            $tblItemChildE1.destroy();
            $(tblItemChildId).html("");
        }
        ej.base.enableRipple(true);
        $tblItemChildE1 = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            editSettings: { allowAdding: false, allowEditing: false, allowDeleting: false },
            columns: [
            {
                headerText: 'Commands', width: 100, maxWidth: 100, minWidth: 100, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            },
            { field: 'DBIRollID', isPrimaryKey: true, visible: false },
            { field: 'ConceptID', visible: false },
            {
                field: 'GRollID', width: 100, maxWidth: 100, minWidth: 100, headerText: 'Form', valueAccessor: ej2GridDisplayFormatter,
                //dataSource: masterData.FormList,
                displayField: "RollID",
                edit: ej2GridDropDownObj({})
            },
            {
                field: 'RollQtyPcs', width: 100, maxWidth: 100, minWidth: 100, headerText: 'Qty(Pcs)',
                editType: "numericedit", edit: { params: { decimals: 0, min: 0 } },
                //visible: subGroupName == "Fabric" ? false : true
            },
            { field: 'QtyinKG', width: 100, maxWidth: 100, minWidth: 100, headerText: 'Qty(kg)', editType: "numericedit", edit: { params: { decimals: 0, min: 0 } } },
            { field: 'BoxNo', width: 100, maxWidth: 100, minWidth: 100, headerText: 'Box No' },
            { field: 'Remarks', width: 200, maxWidth: 200, minWidth: 200, headerText: 'Remarks', visible: false },
        ],
            childGrid: {
                queryString: 'DBIID',
                allowResizing: true,
                editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false },
                columns: [
                    { field: 'DBIRollID', isPrimaryKey: true, visible: false },
                    { field: 'RollNo', headerText: 'Roll No', textAlign: 'Center', width: 20, allowEditing: false },
                    { field: 'QCWidth', headerText: 'Width', width: 20 },
                    { field: 'QCGSM', headerText: 'GSM', width: 20 },
                    { field: 'FinishRollQty', headerText: 'Roll Qty (Kg)', width: 20 },
                    { field: 'FinishRollQtyPcs', headerText: 'Roll Qty (Pcs)', width: 20 },
                    { field: 'QCPass', headerText: 'QC Pass', displayAsCheckBox: true, editType: "booleanedit", width: 20, textAlign: 'Center' },
                    { field: 'QCFail', headerText: 'QC Fail', displayAsCheckBox: true, editType: "booleanedit", width: 20, textAlign: 'Center' },
                    { field: 'QCHold', headerText: 'QC Hold', displayAsCheckBox: true, editType: "booleanedit", width: 20, textAlign: 'Center' },
                    { field: 'ActiveByName', headerText: 'Action By', width: 30, visible: status == statusConstants.COMPLETED
    },
    { field: 'InActiveDate', headerText: 'Action Date', type: 'date', format: 'dd/MM/yyyy', editType: 'datepickeredit', width: 30, textAlign: 'Center', visible: status == statusConstants.COMPLETED }
                ],
    actionBegin: function (args) {
        if (args.requestType === 'beginEdit') {

        }
        else if (args.requestType === "save") {
            var countChecked = 0,
                isDisplayMessage = true;
            if (args.data.QCPass) {
                countChecked++;
            }
            if (args.data.QCFail) {
                if (countChecked > 0) {
                    args.data.QCFail = false;
                    isDisplayMessage = false;
                    toastr.error("Select Pass or Fail or Hold.");
                } else {
                    countChecked++;
                }
            }
            if (args.data.QCHold) {
                if (countChecked > 0) {
                    if (isDisplayMessage) toastr.error("Select Pass or Fail or Hold.");
                    args.data.QCHold = false;
                } else {
                    countChecked++;
                }
            }
            $tblItemChildE1.updateRow(args.rowIndex, args.data);
        }
    },
    load: loadFirstLevelChildGrid
}
        });
$tblItemChildE1.refreshColumns;
$tblItemChildE1.appendTo(tblItemChildId);
    }



    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
        initChildTable([]);
        $tblChildEl.destroy();
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.ConceptID);
        }
    }
    function commandClick(e) {
        cGrid = null;
        var data = e.rowData;
        selectedData = data;
        machinetype = e.commandColumn.buttonOption.type;
    }
    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#ConceptID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }
})();