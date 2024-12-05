(function () {
    var menuId, pageName;
    var $formEl, $pageEl, tblChildId, $tblChildEl;
    var _contactDocSetupID = 99999;
    var ImagesList = Array();
    var _docTypeSetupList = [];
    var _imagesList = [];
    var supplierId;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!pageName) pageName = localStorage.getItem("pageName");
       

        pageId = pageName + "-" + menuId;
        tblChildId = pageConstants.CHILD_TBL_ID_PREFIX + pageId;

        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);
        $pageEl = $(`#${pageId}`);
        $formEl.find("#btn-add-supplier").on("click", showAddSupplier);
        $formEl.find("#btnSave").click(save);
        $formEl.find("#btnAdd").click(add);
        //tblImageId = "#tblImage" + pageId
        getDocType();
        $formEl.find("#btnSave").hide();

    });

    function showAddSupplier(e) {
        e.preventDefault();
        axios.get(`/cdsAPI/supplier-list`)
            .then(function (response) {
                var supplierList = response.data.SupplierList;
                var finder = new commonFinder({
                    title: "Select Supplier",
                    pageId: pageId,
                    data: supplierList,
                    fields: "text",
                    headerTexts: "Supplier",
                    widths: "100%",
                    selectedIds: 0,
                    allowPaging: true,
                    primaryKeyColumn: "id",
                    onSelect: function (res) {
                        finder.hideModal();
                        $formEl.find("#SupplierID").val(res.rowData.id);
                        $formEl.find("#SupplierName").val(res.rowData.text);
                        supplierId = $formEl.find("#SupplierID").val();
                        getDetails(supplierId);

                    },
                });
                finder.showModal();
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function initTbl(data) {

        if ($tblChildEl) $tblChildEl.destroy();
        ej.base.enableRipple(true);
        $tblChildEl = new ej.grids.Grid({
            dataSource: data,
            allowResizing: true,
            autofitColumns: false,
            //toolbar: ['Add'],
            editSettings: { allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal", showDeleteConfirmDialog: true },
            actionBegin: function (args) {
                if (args.requestType === 'add') {
                    args.data.supplierID = $formEl.find("#SupplierID").val();
                }
            },
            commandClick: handleCommands,
            columns: [
                {
                    headerText: '', width: 20, commands: [
                        { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                        { type: 'View', buttonOption: { cssClass: 'e-flat', iconCss: 'fa fa-eye' } },
                    ]
                },
                { field: 'ContactDocSetupID', isPrimaryKey: true, visible: false },
                { field: 'DocTypeName', headerText: 'Doc Type', width: 100, allowEditing: false },
                {
                    field: 'ExpiryDate', headerText: 'Expiry Date', type: 'date', format: _ch_date_format_1, editType: 'datepickeredit', width: 40, textAlign: 'Center', allowEditing: true
                },
                {
                    field: 'ImagePath', headerText: 'Attached File', width: 100, allowEditing: false,
                    template: '<a><img src="${ImagePath}" alt="${ImagePath}" width="70px" height="60px" /></a>',
                },
                { field: 'SupplierID', headerText: 'Supplier ID', visible: false }
            ],
            height: 315
        });
        $tblChildEl.refreshColumns;
        $tblChildEl.appendTo(tblChildId);
    }
    function handleCommands(args) {
        if (args.commandColumn.type == 'View') {
            var filePath = args.rowData.ImagePath;
            window.open(filePath);
        }
    }


    function getDetails(supplierID) {
        
        axios.get(`/cdsAPI/get-attachments/${supplierID}`)
            .then(function (response) {
                var docs = response.data;
                _docTypeSetupList = docs;
                initTbl(docs);
                $formEl.find("#btnSave").show();
                _imagesList = [];
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function getDocType() {
        axios.get(`/cdsAPI/docType-list`)
            .then(function (response) {
                var docs = response.data;
                response.data.ExpiryDate = formatDateToDefault(response.data.ExpiryDate);
                setFormData($formEl, response.data);
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    function add(e) {
        e.preventDefault();
        var file = document.getElementById('UploadFile').files[0];
        var docTypeId = $formEl.find("#DocTypeID").val();
        if (docTypeId == 0) {
            toastr.error('Please Select Doc Type');
            return false;
        }
        if (file == null) {
            toastr.error('Please Select Any File');
            return false;
        }

        _docTypeSetupList = $tblChildEl.getCurrentViewRecords();
        var indexF = _docTypeSetupList.findIndex(x => x.DocTypeID == docTypeId);
        if (indexF > -1) {
            toastr.error('Same Doc Type already selected');
            return false;
        }
            var image = $formEl.find("#UploadFile");
            var fileName = image.val().split('\\').pop();

            _docTypeSetupList.push({
                ContactDocSetupID: _contactDocSetupID++,
                DocTypeID: $formEl.find("#DocTypeID").val(),
                DocTypeName: $formEl.find("#DocTypeID option:selected").text(),
                ImagePath: fileName,
                SupplierID: $formEl.find("#SupplierID").val()
                //ExpiryDate: $formEl.find("#ExpiryDate").val()
            });

            var blob = file.slice(0, file.size, file.type);
            //fileName1 = removeInvalidChar(fileName1);
            var newFile = new File([blob], fileName, { type: file.type });
            //newFile.RowIndex = rowIndex;
            _imagesList.push(newFile);
            initTbl(_docTypeSetupList);
            image.val("");
    }

    function save(e) {
        e.preventDefault();

        var formData = getFormData($formEl);
        for (var i = 0; i < _imagesList.length; i++) {
            formData.append("UploadFile", _imagesList[i]);
        }

        //Because of using formData we have to use .toDateString() for Date fields.
        _docTypeSetupList.map(x => {
            x.ExpiryDate = x.ExpiryDate.toDateString();
        });

        formData.append("DocTypeSetupList", JSON.stringify(_docTypeSetupList));

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }

        axios.post(`/cdsAPI/save/${supplierId}`, formData, config)
            .then(function () {
                toastr.success("Successfully saved.");
                getDetails(supplierId);
                
            })
            .catch(showResponseError);
    }
})();