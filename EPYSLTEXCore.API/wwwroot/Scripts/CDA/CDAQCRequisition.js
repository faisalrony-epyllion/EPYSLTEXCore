(function () {
    var currentChildRowData;
    var menuId, pageName;
    var toolbarId, pageId;
    var $pageEl, $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, tblMasterId, $formEl, $tblChildEl, tblChildId;
    var status;
    var isEditable = false;
    var isAcknowledge = false;
    var tableParams = {
        offset: 0,
        limit: 10,
        sort: '',
        order: '',
        filter: ''
    }

    var CDAMRIR;

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
        isAcknowledge = convertToBoolean($(pageId).find("#Acknowledge").val());

        initMasterTable();
        //initChildTable();
        //getMasterTableData();

        $("#ReceiveID").on("select2:select select2:unselect", function (e) {
            if (e.params.data.selected) {
                var receiveId = $(this).val();
                getNewChilds(receiveId);
            }
            else {
                //CDAMRIR.Childs = [];
                //$tblChildEl.bootstrapTable("load", CDAMRIR.Childs);
                initChildTable(CDAMRIR.Childs);
            }
        })

        $toolbarEl.find("#btnNew").on("click", getNew);

        $formEl.find("#btnSave").click(function (e) {
            e.preventDefault();
            save(this);
        });

        $formEl.find("#btnSaveAndSend").click(function (e) {
            e.preventDefault();
            save(this, true);
        });
        $toolbarEl.find("#btnList").on("click", function (e) {
            e.preventDefault();
            toggleActiveToolbarBtn(this, $toolbarEl);
            resetTableParams();
            initMasterTable();

        });

        $formEl.find("#btnCancel").on("click", backToList);
    });
  
   
    function initMasterTable() {
        
        var columns = [
            {
                headerText: 'Commands', commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }]
            },
            {
                field: 'QCReqNo', headerText: 'Req No'
            },
            {
                field: 'QCReqDate', headerText: 'Req Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1
            },
            {
                field: 'QCReqByUser', headerText: 'Req By', textAlign: 'Center'
            },
            {
                field: 'QCReqFor', headerText: 'Req For'
            },
            {
                field: 'IsApproveStr', headerText: 'Sent for Req'
            },
            {
                field: 'IsAcknowledgeStr', headerText: 'Acknowledged'
            }
           
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();
        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/cda-qc-requisition/list`,
            columns: columns,
            commandClick: handleCommands
        });
    }
    function handleCommands(args) {
        getDetails(args.rowData.QCReqMasterID);
        $formEl.find("#btnSave").fadeIn();
    }

    //function getMasterTableData() {
    //    var queryParams = $.param(tableParams);
    //    $tblMasterEl.bootstrapTable('showLoading');
    //    var url = "/api/cda-qc-requisition/list?gridType=bootstrap-table&" + queryParams;
    //    axios.get(url)
    //        .then(function (response) {
    //            $tblMasterEl.bootstrapTable('load', response.data);
    //            $tblMasterEl.bootstrapTable('hideLoading');
    //        })
    //        .catch(function (err) {
    //            toastr.error(err.response.data.Message);
    //        })
    //}
    function initChildTable(records) {

        if ($tblChildEl) $tblChildEl.destroy();

        $tblChildEl = new initEJ2Grid({
            tableId: tblChildId,
            data: records,
            autofitColumns: false,
            allowSorting: true,
            allowPaging: false,
            allowFiltering: false,
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            columns: [
                {
                    headerText: 'Command', width: 100, commands: [
                        { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                        { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                    ]
                },
                { field: 'ItemName', headerText: 'Item Name', allowEditing: false },
                { field: 'AgentName', headerText: 'Agent Name', allowEditing: false },
                { field: 'Uom', headerText: 'Uom', allowEditing: false, textAlign: 'Center' },
                { field: 'LotNo', headerText: 'Lot No', allowEditing: false, textAlign: 'Center' },
                { field: 'ReceiveQty', headerText: 'Receive Qty', allowEditing: false },
                { field: 'ReqQty', headerText: 'Req Qty' }
             ]
        });

    }
    //function initChildTable() {
    //    $tblChildEl.bootstrapTable("destroy");
    //    $tblChildEl.bootstrapTable({
    //        showFooter: true,
    //        columns: [
    //            {
    //                field: "ItemName",
    //                title: "Item Name",
    //                width: 150
    //            },
    //            {
    //                field: "AgentName",
    //                title: "Agent Name",
    //                width: 150
    //            },
    //            {
    //                field: "Uom",
    //                title: "Uom"
    //            },
    //            {
    //                field: "LotNo",
    //                title: "Lot No"
    //            },
    //            {
    //                field: "ReqQty",
    //                title: "Req Qty",
    //                align: 'center',
    //                editable: {
    //                    type: "text",
    //                    showbuttons: false,
    //                    tpl: '<input type="number" class="form-control input-sm" min="0" style="padding-right: 24px;">',
    //                    validate: function (value) {
    //                        if (!value || !value.trim() || isNaN(parseInt(value)) || parseInt(value) <= 0) {
    //                            return 'Must be a positive integer.';
    //                        }
    //                    }
    //                }
    //            }
    //        ]
    //    });
    //}

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $.each($formEl.find('select'), function (i, el) {
            $(el).select2('');
        });
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

    function getNew() {
        axios.get("/api/cda-qc-requisition/new")
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDAMRIR = response.data;
                CDAMRIR.QCReqDate = formatDateToDefault(CDAMRIR.QCReqDate);
                setFormData($formEl, CDAMRIR);
                //$tblChildEl.bootstrapTable("load", []);
                //$tblChildEl.bootstrapTable('hideLoading');
                $formEl.find("#ReceiveID").prop('disabled', false);
                initChildTable([]);
                initChildTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    //function getDetails(id) {
    //    axios.get(`/api/cda-qc-requisition/${id}`)
    //        .then(function (response) {
    //            $divDetailsEl.fadeIn();
    //            $divTblEl.fadeOut();
    //            CDAMRIR = response.data;
    //            console.log(CDAMRIR);
    //            CDAMRIR.QCReqDate = formatDateToDefault(CDAMRIR.QCReqDate);
    //            setFormData($formEl, CDAMRIR);
    //            /* $tblChildEl.bootstrapTable("load", CDAMRIR.Childs);*/
    //            initChildTable(CDAMRIR.Childs);
    //            $formEl.find("#ReceiveID").prop('disabled', true);
    //            console.log(response.data[0].SupplierId);
    //            $formEl.find("#SupplierId").val(response.data[0].SupplierId);
    //            $formEl.find("#CompanyId").val(response.data[0].CompanyId);
    //            $formEl.find("#RCompanyId").val(response.data[0].RCompanyId);
    //           /* $tblChildEl.bootstrapTable('hideLoading');*/
    //        })
    //        .catch(function (err) {
    //            toastr.error(err.response.data.Message);
    //        });
    //}

    function getDetails(id) {
        axios.get(`/api/cda-qc-requisition/${id}`)
            .then(function (response) {
                
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                CDAMRIR = response.data;
                CDAMRIR.QCReqDate = formatDateToDefault(CDAMRIR.QCReqDate);
               // alert(CDAMRIR.SupplierID);
                //alert(CDAMRIR.QCReqDate);
                setFormData($formEl, CDAMRIR);
                initChildTable(CDAMRIR.Childs);
                $formEl.find("#ReceiveID").prop('disabled', true);

            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getNewChilds(receiveId) {
        initChildTable(CDAMRIR.Childs);
       // $tblChildEl.bootstrapTable('showLoading');
        axios.get(`/api/cda-qc-requisition/new/childs/${receiveId}`)
            .then(function (response) {
               // alert(response.data[0].SupplierId);
                CDAMRIR.Childs = response.data;
                $formEl.find("#SupplierID").val(response.data[0].SupplierId);
                $formEl.find("#CompanyID").val(response.data[0].CompanyId);
                $formEl.find("#RCompanyID").val(response.data[0].RCompanyId);
                initChildTable(CDAMRIR.Childs);
                //$tblChildEl.bootstrapTable("load", CDAMRIR.Childs);
                //$tblChildEl.bootstrapTable('hideLoading');
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function save(isApprove = false) {
        
        var data = formDataToJson($formEl.serializeArray());
        /*  data["Childs"] = CDAMRIR.Childs;*/
        data["Childs"] = $tblChildEl.getCurrentViewRecords();
        CDAMRIR.Approve = isApprove;
        //console.log(data);
        axios.post("/api/cda-qc-requisition/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                backToList();
            })
            .catch(showResponseError);
    }
})();