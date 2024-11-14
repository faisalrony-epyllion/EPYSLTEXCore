(function () {
    'use strict'
    var currentChildRowData;
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var status, Count=1;
    var isEditable = false;
    var isAcknowledge = false;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }
    
    var masterData;

    $(function () {
        if (!menuId)
            menuId = localStorage.getItem("menuId");
        if (!pageName)
            pageName = localStorage.getItem("pageName");

        pageId = pageName + "-" + menuId;
        $pageEl = $(pageConstants.PAGE_ID_PREFIX + pageId);
        $divTblEl = $(pageConstants.DIV_TBL_ID_PREFIX + pageId);
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        $toolbarEl = $(toolbarId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $divDetailsEl = $(pageConstants.DIV_DETAILS_ID_PREFIX + pageId);

        //if (isAcknowledge) {
        //    $("#btnSave").hide(); 
        //}

        //Load Event
        initMasterTable();

        //Button Click Event
        $toolbarEl.find("#btnReceiveLists").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();
        });

        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save();
        });

        $formEl.find("#btnCancel").on("click", backToList);
    });

    function backToList() {
      /*  toggleActiveToolbarBtn($toolbarEl.find("#btnReceiveLists"), $toolbarEl);*/
        $divDetailsEl.fadeOut();
        $formEl.find("#YarnCountID").val(-1111);
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
        $formEl.find("#LReceiveMasterID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function resetTableParams() {
        tableParams.offset = 0;
        tableParams.limit = 10;
        tableParams.filter = '';
        tableParams.sort = '';
        tableParams.order = '';
    }

    function getNew() {
        axios.get(`/api/yarntypestructure/new`)
            .then(function (response) {
                isEditable = true;
                status = statusConstants.NEW;
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                setFormData($formEl, masterData);
                initChildTable(masterData);
            })
            .catch(showResponseError);

    }

    function initMasterTable() {
        var columns = [
            {
                headerText: '', textAlign: 'Center', width: 90, minWidth: 90, maxWidth: 90, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } },
                    /*{ type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },*/]
            },
            { field: 'YarnTypeGroupName', headerText: 'YarnTypeGroupName' },
            { field: 'SegmentValue', headerText: 'SegmentValue' },
            { field: 'MUnit', headerText: 'MUnit' },
            { field: 'Ply', headerText: 'Ply' },
            { field: 'PlyUnit', headerText: 'PlyUnit' },
            { field: 'NextNumericCount', headerText: 'NextNumericCount' },
            { field: 'NextCountUnit', headerText: 'NextCountUnit' },
            { field: 'SufixUnit', headerText: 'SufixUnit' },
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/yarntypestructure/list`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        getDetails(args.rowData.YarnCountID);
        $formEl.find("#btnSave").fadeIn();
    }

    async function initChildTable(data) {
        isEditable = true;
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [];
        if (isEditable) {
            columns.unshift({
                headerText: 'Commands', width: 120, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }]
            });
        };
        var additionalColumns = [
            { field: 'YarnCountID', isPrimaryKey: true, visible: false },
            
            {
                field: 'YarnTypeGroupNo', headerText: 'Yarn Type GroupNo', width: 300, valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.YarnTypeGroupList,
                displayField: "YarnTypeGroupName", edit: ej2GridDropDownObj({
                    //onChange: function (selectedData, currentRowData) {
                 
                    //    var index = $tblChildEl.getRowIndexByPrimaryKey(currentRowData.YarnCountID);
                    //    if (selectedData.id == 1) {
                    //        currentRowData.MUnit = "D";
                    //    } else {
                    //        currentRowData.MUnit = "Ne/Mn";
                    //    }
                    //    $tblChildEl.updateRow(index, currentRowData);
                   
                    //}
                     
                })
            },
            {
                field: 'NumericCount', headerText: 'Yarn Type GroupNo', width: 300, valueAccessor: ej2GridDisplayFormatter, dataSource: masterData.CountNameList,
                displayField: "SegmentValue", edit: ej2GridDropDownObj({
                })
            },
            {
                field: 'MUnit', headerText: 'Unit', width: 300, edit: ej2GridDropDownObj({
                    dataSource: masterData.MUnitList,
                    valueFieldName: "MUnit",
                })
            },
            { field: 'Ply', headerText: 'Ply' },
            { field: 'PlyUnit', headerText: 'PlyUnit' },
            { field: 'NextNumericCount', headerText: 'NextNumericCount' },
            { field: 'NextCountUnit', headerText: 'NextCountUnit' },
            { field: 'SufixUnit', headerText: 'SufixUnit' },
        ];
        columns.push.apply(columns, additionalColumns);
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            /*showTimeIndicator :false,*/
            actionBegin: function (args) {
               
                if (args.requestType === "add") {
                    args.data.YarnCountID = Count++; /*getMaxIdForArray(masterData.Childs, "YarnCountID");*/
                }
                //else if (args.requestType === "save") {
                //    masterData['Childs']= args.data;
                //}
            },
            autofitColumns: false,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };

        tableOptions["toolbar"] = ['Add'];
        tableOptions["editSettings"] = { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true };
        $tblChildEl = new initEJ2Grid(tableOptions);
    }

    function getDetails(id) {

        var url = `/api/yarntypestructure/${id}`;
        axios.get(url)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                setFormData($formEl, masterData);
                $("#YarnCountID").val(masterData.Childs[0].YarnCountID);
                initChildTable(masterData.Childs);
            })
            .catch(showResponseError);
    }
    function save() {
        var data = formElToJson($formEl);
        data["Childs"] = $tblChildEl.getCurrentViewRecords();
        if (data.Childs.length === 0) return toastr.error("At least 1 Yarn items is required.");

        axios.post("/api/yarntypestructure/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
    
})();