(function () {
    var menuId, pageName;
    var $tblMasterEl, $formEl, tblMasterId;
    var spinnerList, spinnerLists = []
    var grid;
    var _maxYarnPackingID = -999;
    var _isEdit = false;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");

        var pageId = pageName + "-" + menuId;
        toolbarId = pageConstants.TOOLBAR_ID_PREFIX + pageId;
        //$tblMasterEl = $(pageConstants.MASTER_TBL_ID_PREFIX + pageId);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        tblMasterId = pageConstants.MASTER_TBL_ID_PREFIX + pageId;

        getInitData();
    });
    function initMasterTable() {
        var url = "/api/item-wise-rol/list";
        axios.get(url)
            .then(function (response) {
                if (grid) {
                    grid.destroy();
                }
                ej.grids.Grid.Inject(ej.grids.Edit, ej.grids.Toolbar);
                grid = new ej.grids.Grid({
                    dataSource: response.data.Items,
                    toolbar: ['Add', 'Edit', 'Delete'],
                    editSettings: {
                        allowEditing: true,
                        allowAdding: true,
                        allowDeleting: true,
                        mode: 'Dialog',
                        template: '#dialogtemplate'
                    },
                    columns: [
                        {
                            field: 'YarnPackingID',
                            headerText: 'YarnPackingID',
                            textAlign: 'Right',
                            width: 100,
                            isPrimaryKey: true,
                            visible: false
                        },
                        {
                            field: 'SpinnerID',
                            headerText: 'SpinnerID',
                            width: 150,
                            visible: false
                        },
                        {
                            field: 'Spinner',
                            headerText: 'Spinner',
                            width: 150
                        },
                        {
                            field: 'PackNo',
                            headerText: 'PackNo',
                            width: 120,
                        },
                        {
                            field: 'Cone',
                            headerText: 'Cone',
                            width: 120,
                        },
                        {
                            field: 'NetWeight',
                            headerText: 'Net Weight',
                            width: 120,
                        },
                    ],
                    height: 500,
                    actionBegin: function (args) {
                        if (args.requestType === 'save') {
                            //    // cast string to integer value.
                            //    args.data['NetWeight'] =
                            //        parseFloat(args.form.querySelector("#NetWeight").value);
                            console.log(args.data);

                            args.data.YarnPackingID = getDefaultValueWhenInvalidN(args.data.YarnPackingID);
                            args.rowData = setValidPropsValue(args.data, args.rowData);
                            args.data = setDropDownValues(masterData, args.data, args.rowData);
                            args.rowData = args.data;

                            if (args.data.YarnPackingID == 0) {
                                args.data.YarnPackingID = _maxYarnPackingID--;
                            }
                            var allData = grid.dataSource;
                            console.log(allData);

                            debugger;
                            var isExist = false;
                            var list = allData.filter(item => item.SpinnerID === args.data.SpinnerID && item.PackNo === args.data.PackNo);

                            if (list.length > 0 && _isEdit) {
                                list = list.filter(x => x.YarnPackingID != args.data.YarnPackingID);
                            }

                            if (list.length > 0) isExist = true;

                            if (isExist) {
                                toastr.error("Duplicate Spinner and Pack No found!!!");
                                args.cancel = true;
                                return;
                            }

                            var dataObj = {
                                YarnPackingID: args.data.YarnPackingID,
                                SpinnerID: args.data.SpinnerID,
                                PackNo: args.data.PackNo,
                                Cone: args.data.Cone,
                                NetWeight: args.data.NetWeight
                            };

                            args.rowData = DeepClone(args.data);

                            if (!save(dataObj)) {
                                args.cancel = true;
                                return;
                            };
                        }
                        if (args.requestType === 'delete') {
                            debugger;

                        }
                    },
                    actionComplete: function (args) {
                        _isEdit = false;
                        if ((args.requestType === 'beginEdit' || args.requestType === 'add')) {
                            console.log(args);
                            ejDropDownLoad(ej, args, masterData.SpinnerList, "Spinner", "text", "id", "Spinner");
                            args.dialog.header = 'Add Details';

                        }

                        if (args.requestType === 'beginEdit') {
                            _isEdit = true;

                            args.dialog.header = 'Edit Details';
                            $("#YarnPackingID").val(args.rowData.YarnPackingID);
                            $("#Spinner").val(args.rowData.Spinner);
                            $("#PackNo").val(args.rowData.PackNo);
                            $("#Cone").val(args.rowData.Cone);
                            $("#NetWeight").val(args.rowData.NetWeight);
                        }
                    }
                });
                grid.appendTo('#Grid');
            })
            .catch(showResponseError)
    }
    function getInitData() {

        var url = "/api/item-wise-rol/GetMaster";
        axios.get(url)
            .then(function (response) {
                masterData = response.data;

                setFormData($formEl, masterData);
                initMasterTable();
            })
            .catch(showResponseError)
    }
    function save(dataObj) {
        let returnFlag = false;
        debugger;
        //e.preventDefault();
        var data = formElToJson($formEl);
        data.SetupID = 0;
        data.YarnPackingID = dataObj.YarnPackingID;
        data.SpinnerID = dataObj.SpinnerID;
        data.PackNo = dataObj.PackNo;
        data.Cone = dataObj.Cone;
        data.NetWeight = dataObj.NetWeight;
        console.log(data);
        axios.post("/api/item-wise-rol/save", data)
            .then(function () {
                toastr.success("Saved successfully.");
                initMasterTable();
            })
            .catch(showResponseError);
        debugger;
        returnFlag = false;
    }

})();