(function () {
    var menuId, pageName;
    var toolbarId;
    var $divTblEl, $divDetailsEl, $toolbarEl, $tblMasterEl, $formEl, tblMasterId, $tblChildEl, tblChildId;
    var status;
    var masterData;
    var ImagesList = Array();
    var ImagesContentList = Array();

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

        $("#btnAddImages").click(function (e) {
            e.preventDefault();
            var totalfiles = document.getElementById('ProductImage').files.length;

            for (var i = 0; i < totalfiles; i++) {
                //Image Content File
                var ext = getFileExtension(document.getElementById('ProductImage').files[i].name);
                var newFileName = uuidv4()+ '.' + ext;
                newFileName = newFileName.replace(/-/g, '');
                //renameFile(document.getElementById('ProductImage').files[i], newFileName);
                ///Create New File////
                var file = document.getElementById('ProductImage').files[i];
                var blob = file.slice(0, file.size, 'image/png');
                var newFile = new File([blob], newFileName, { type: 'image/png' });
                ///Create New File////
                var objList = new Object;
                objList.LPFormImageID = i + 1;
                objList.LPFormID = selectedId;
                //objList.ImagePath = "/Uploads/RND/" + document.getElementById('ProductImage').files[i].name;
                objList.ImagePath = "/Uploads/RND/" + newFile.name;
                objList.Image64Byte = "";
                //objList.ImageName = document.getElementById('ProductImage').files[i].name;
                objList.ImageName = newFile.name;
                objList.PreviewTemplate = "image";
                objList.DefaultImage = 0;
                ImagesContentList.push(objList);

                //Image File
                /* var obj = document.getElementById('ProductImage').files[i];*/
                var obj = newFile;
                ImagesList.push(obj);
            }
            $("#modal-child-Live-Product").modal('hide');

            $("#ProductImage").val('');
            document.getElementById("ProductImage").innerHTML = "";
            document.getElementById("ProductImagediv").innerHTML = "";
            $(".file-drop-zone").text("");
            //console.log(ImagesContentList);
        });

        $(document).on('click', '.btnImagesRemove', function () {
            var rID = $(this).attr("id").split('_')[1];
            $("#btnMasterRemove_" + rID).remove();
            ImagesContentList.splice($.inArray($("#btnMasterRemove_" + rID), ImagesContentList), 1);
        });
        $(document).on('click', '.chkDImage', function () {
            $(".chkDImage").prop("checked", false);
            var rID = $(this).attr("id").split('_')[1];
            $("#" + $(this).attr("id")).prop("checked", true);

            $.each(ImagesContentList, function (j, obj) {
                if (obj.LPFormImageID == rID) {
                    obj.DefaultImage = 1;
                } else {
                    obj.DefaultImage = 0;
                }
            });
        });
    });

    function uuidv4() {
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }
    function getFileExtension(filename) {
        var ext = /^.+\.([^.]+)$/.exec(filename);
        return ext == null ? "" : ext[1];
    }
    function renameFile(originalFile, newName) {
        return new File([originalFile], newName, {
            type: originalFile.type,
            lastModified: originalFile.lastModified,
        });
    }
    function initMasterTable() {
        var commands = [];
        if (status == statusConstants.PENDING) {
            commands = [
                { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' } }
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
                textAlign: 'Center', width: 60, minWidth: 60, maxWidth: 80
            },
            {
                field: 'FCID', headerText: 'FCID', visible: false
            },
            //{
            //    field: 'LPFormID', headerText: 'LPFormID', visible : false
            //},
            {
                field: 'IsBDS', headerText: 'Type', visible: false
            },
            {
                field: 'ConceptNo', headerText: 'Concept No', width: 50
            },
            {
                field: 'ConceptDate', headerText: 'Concept Date', textAlign: 'Center', type: 'date', format: _ch_date_format_1, width: 50
            },
            {
                field: 'ColorName', headerText: 'Color', width: 70
            },
            {
                field: 'TrialNo', headerText: 'Trial No', visible: false
            },
            {
                field: 'SubGroupName', headerText: 'End Use', visible: false
            },
            {
                field: 'KnittingType', headerText: 'Machine Type', width: 60, visible: false
            },
            {
                field: 'TechnicalName', headerText: 'Technical Name', width: 70
            },
            {
                field: 'Composition', headerText: 'Composition', width: 120, valueAccessor: compositionFormatter
            },
            {
                field: 'GSM', headerText: 'GSM', width: 30, textAlign: 'Center', valueAccessor: gsmFormatter
            },
            {
                field: 'Length', headerText: 'Length(cm)', width: 30, textAlign: 'Center', visible: false
            },
            {
                field: 'Width', headerText: 'Height(cm)', width: 30, textAlign: 'Center', visible: false
            },
            {
                field: 'BoxNo', headerText: 'Box No', width: 30, textAlign: 'Center', visible: status != statusConstants.PENDING
            }
        ];

        if ($tblMasterEl) $tblMasterEl.destroy();

        $tblMasterEl = new initEJ2Grid({
            tableId: tblMasterId,
            autofitColumns: false,
            apiEndPoint: `/api/live-product/list?status=${status}`,
            columns: columns,
            commandClick: handleCommands
        });
    }

    function compositionFormatter(field, data, column) {
        column.disableHtmlEncode = false;
        if (data.FinalComposition) {
            return data.FinalComposition;
        } else {
            return data.Composition;
        }
    }

    function gsmFormatter(field, data, column) {
        column.disableHtmlEncode = false;
        if (data.FinalGSM) {
            return data.FinalGSM;
        } else {
            return data.GSM;
        }
    }

    function handleCommands(args) {
        if (args.commandColumn.type == 'Edit') {
            getDetails(args.rowData.FCID);
           
        }
        else if (args.commandColumn.type == "ReportCHanger") {
            window.open(`/reports/InlinePdfView?ReportName=ConuterHangerFormat.rdl&FirmConceptMasterID=${args.rowData.FCID}`, '_blank');
        }
        else if (args.commandColumn.type == "ReportMHanger") {
            window.open(`/reports/InlinePdfView?ReportName=MarketingHangerFormat.rdl&FirmConceptMasterID=${args.rowData.FCID}`, '_blank');
        }
    }
   
    async function initChildTable(data, subGroupName) {
        if ($tblChildEl) $tblChildEl.destroy();
        var columns = [
            {
                headerText: 'Commands', width: 100, maxWidth: 100, minWidth: 100, commands: [
                    { type: 'Edit', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-edit' } },
                    { type: 'Delete', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-delete' } },
                    { type: 'Save', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-update' } },
                    { type: 'Cancel', buttonOption: { cssClass: 'e-flat', iconCss: 'e-icons e-cancel-icon' } }
                ]
            },
            { field: 'LPFormID', isPrimaryKey: true, visible: false },
            { field: 'FirmConceptMasterID', visible: false },
            {
                field: 'FormID', width: 100, maxWidth: 100, minWidth: 100, headerText: 'Form', valueAccessor: ej2GridDisplayFormatter,
                dataSource: masterData.FormList,
                displayField: "Form",
                edit: ej2GridDropDownObj({})
            },
            {
                field: 'QtyInPcs', width: 100, maxWidth: 100, minWidth: 100, headerText: 'Qty(Pcs)',
                editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } }//,
                //visible: subGroupName == "Fabric" ? false : true
            },
            { field: 'QtyinKG', width: 100, maxWidth: 100, minWidth: 100, headerText: 'Qty(kg)', editType: "numericedit", edit: { params: { showSpinButton: false, decimals: 0, min: 0 } } },
            { field: 'BoxNo', width: 100, maxWidth: 100, minWidth: 100, headerText: 'Box No' },
            { field: 'Remarks', width: 200, maxWidth: 200, minWidth: 200, headerText: 'Remarks', visible: false },
            {
                headerText: '...', width: 30, commands: [
                    { buttonOption: { type: 'ImagesUpload', content: '', cssClass: 'btn btn-success btn-xs', iconCss: 'fa fa-search' } }
                ]
            }
        ];
       
        var tableOptions = {
            tableId: tblChildId,
            data: data,
            columns: columns,
            actionBegin: function (args) {
                if (args.requestType === "add") {
                    args.data.LPFormID = getMaxIdForArray($tblChildEl.getCurrentViewRecords(), "LPFormID");
                    args.data.FirmConceptMasterID = masterData.FCID;
                    args.data.FCItemID = masterData.FCItemID;
                    args.data.QtyInPcs = 0;
                    args.data.QtyinKG = 0;
                    args.data.SubmitedQtyInPcs = 0;
                    args.data.SubmitedQtyinKG = 0;
                    args.data.SeqNo = 0;
                }
                else if (args.requestType === "save") {
                    var index = $tblChildEl.getRowIndexByPrimaryKey(args.rowData.LPFormID);
                    if (index > -1) {
                        masterData.LiveProductForms[index] = args.data;
                    }
                }
            },
            commandClick: commandClick,
            autofitColumns: true,
            showDefaultToolbar: false,
            allowFiltering: false,
            allowPaging: false
        };

        //if (isEditable) {
        tableOptions["toolbar"] = ['Add'];
        tableOptions["editSettings"] = {
            allowAdding: true, allowEditing: true, allowDeleting: true, mode: "Normal",
            showDeleteConfirmDialog: true
        };
        $tblChildEl = new initEJ2Grid(tableOptions);

        //$tblChildEl.refreshColumns;
        //$tblChildEl.appendTo(tblFabricChildId);
    }

    function commandClick(e) {
        cGrid = null;
        var data = e.rowData;
        selectedData = data;
        machinetype = e.commandColumn.buttonOption.type;
        if (e.commandColumn.buttonOption.type == 'ImagesUpload') {
            $("#ProductImage").val('');
            $(".file-drop-zone").text("");
            cGrid = this;
            selectedId = data.LPFormID;
            $("#modal-child-Live-Product").modal('show');

            //Images display
            //console.log(ImagesContentList)
            initMultipleImageAttachment(ImagesContentList.filter(function (el) { return el.LPFormID == selectedId }), $formEl.find("#ProductImage"));
            initNewAttachment($("#ProductImage"));
        }
    }

    function initNewAttachment($el) {
        $el.fileinput('destroy');
        $el.fileinput({
            showUpload: false,
            previewFileType: 'any'
        });

        $(".file-drop-zone").mCustomScrollbar();
    }

    function initMultipleImageAttachment(firmConceptImages, $el) {
        var previewData = [];
        var previewConfig = [];
        $.each(firmConceptImages, function (i, obj) {
            var objImagesList = new Object;
            objImagesList.LPFormImageID = obj.LPFormImageID;
            objImagesList.DefaultImage = obj.DefaultImage;
            //if (obj.ImagePath == "") {
            //    objImagesList.path = obj.Image64Byte;
            //} else {
            objImagesList.path = rootPath + obj.ImagePath;
            //}
            previewData.push(objImagesList);

            var type = !obj.ImageType ? "any" : obj.ImageType;
            previewConfig.push({ type: type, caption: "Attachment", key: obj.LPFormImageID, width: "80px", frameClass: "preview-frame" })
        });

        document.getElementById("ProductImagediv").innerHTML = "";

        for (i = 0; previewData.length > i; i++) {
            var img = new Image(150, 150);
            img.src = previewData[i].path;
            img.width = "150";
            img.height = "150";
            img.style.left = "0";
            img.style.top = "0";
            img.style.position = "absolute";
            //img.className = "zoomA";

            // to created checkbox
            var checkbox = document.createElement('input');
            checkbox.type = "checkbox";
            checkbox.id = "chkDImage_" + previewData[i].LPFormImageID;
            checkbox.className = "chkDImage";
            checkbox.style.width = "30px";
            checkbox.style.height = "30px";
            checkbox.checked = previewData[i].DefaultImage;

            //Div Create for Checkbox
            var divChk = document.createElement("div");
            divChk.className = "btnDefaultImagesWrap";
            divChk.style.right = "0";
            divChk.style.top = "0";
            divChk.style.zIndex = "9999999";
            divChk.style.position = "absolute";
            divChk.appendChild(checkbox);

            //Div Create
            var div = document.createElement("div");
            div.className = "btnImagesRemove";
            div.id = "btnImagesRemove_" + previewData[i].LPFormImageID;
            div.style.width = "40px";
            div.style.height = "40px";
            div.style.color = "red";
            div.style.textAlign = "left";
            div.style.cursor = "pointer";
            div.style.fontWeight = "700";
            div.style.fontSize = "30px";
            div.innerHTML = "X";
            div.style.position = "absolute";

            //2nd Div
            var div2 = document.createElement("div");
            div2.id = "btnMasterRemove_" + previewData[i].LPFormImageID;
            div2.style.width = "150px";
            div2.style.height = "150px";
            div2.style.position = "relative";
            div2.style.textAlign = "center";
            div2.style.float = "left";
            div2.style.marginRight = "10px";
            div2.style.marginBottom = "10px";
            div2.appendChild(img);
            div2.appendChild(div);
            div2.appendChild(divChk);
            var src = document.getElementById("ProductImagediv");
            src.appendChild(div2);
        }
        document.getElementById("ProductImagediv").hidden = false;
    }

    function getBase641(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.readAsDataURL(file);
            reader.onload = () => resolve(reader.result);
            reader.onerror = error => reject(error);
        });
    }

    function getBase64(file) {
        var strm, i;
        var reader = new FileReader();
        reader.readAsDataURL(file);
        //console.log(file.name);
        reader.onload = function (e) {
            var objList = new Object;
            objList.LPFormImageID = i + 1;
            objList.LPFormID = selectedId;
            objList.ImagePath = "";// "/Uploads/RND/" + file.name;
            objList.Image64Byte = reader.result;
            objList.ImageName = file.name;
            objList.PreviewTemplate = "image";
            ImagesContentList.push(objList);

            //$.each(ImagesContentList, function (j, obj) {
            //    obj.Image64Byte = reader.result;
            //});
            //strm = reader.result;
        };
        reader.onerror = function (error) {
            //console.log('Error: ', error);
        };
    }

    function backToList() {
        $divDetailsEl.fadeOut();
        resetForm();
        $divTblEl.fadeIn();
        initMasterTable();
    }

    function resetForm() {
        $formEl.trigger("reset");
        $formEl.find("#FCID").val(-1111);
        $formEl.find("#EntityState").val(4);
    }

    function getDetails(id) {
        axios.get(`/api/live-product/${id}`)
            .then(function (response) {
                $divDetailsEl.fadeIn();
                $divTblEl.fadeOut();
                masterData = response.data;
                masterData.TrialDate = formatDateToDefault(masterData.TrialDate);
                masterData.ConceptDate = formatDateToDefault(masterData.ConceptDate);

                if (masterData.FinalComposition)
                    masterData.Composition = masterData.FinalComposition;
                if (masterData.FinalGSM)
                    masterData.GSM = masterData.FinalGSM;

                setFormData($formEl, masterData);
                initChildTable(masterData.LiveProductForms, masterData.SubGroupName);

                //initMultipleImageAttachment(masterData.LiveProductFormImages, $formEl.find("#ProductImage"));
                ImagesContentList = [];
                $.each(masterData.LiveProductFormImages, function (j, objChild) {
                    var objList = new Object;
                    objList.LPFormImageID = objChild.LPFormImageID;
                    objList.LPFormID = objChild.LPFormID;
                    objList.ImagePath = objChild.ImagePath;
                    objList.PreviewTemplate = objChild.PreviewTemplate;
                    objList.DefaultImage = objChild.DefaultImage;
                    ImagesContentList.push(objList);
                });

                if (masterData.SubGroupName == "Fabric") {
                    $formEl.find("#divSubGroupName").fadeOut();
                    $formEl.find("#divComposition").fadeIn();
                    $formEl.find("#divGsm").fadeIn();
                    $formEl.find("#divLength").fadeOut();
                    $formEl.find("#divWidth").fadeOut();
                } else {
                    $formEl.find("#divSubGroupName").fadeIn();
                    $formEl.find("#divComposition").fadeOut();
                    $formEl.find("#divGsm").fadeOut();
                    $formEl.find("#divLength").fadeIn();
                    $formEl.find("#divWidth").fadeIn();
                }
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }

    //function save() {
    //    var data = masterData;
    //    //data["LiveProductForms"] = masterData.LiveProductForms;
    //    axios.post("/api/live-product/save", data)
    //        .then(function () {
    //            toastr.success("Saved successfully.");
    //            backToList();
    //        })
    //        .catch(function (error) {
    //            toastr.error(error.response.data.Message);
    //        });
    //}

    function save() {
        var formData = getFormData($formEl);
        $.each(ImagesList, function (i, obj) {
            formData.append("Files", ImagesList[i]);
        });

        formData.append("LiveProductFormImages", JSON.stringify(ImagesContentList));
        formData.append("LiveProductForms", JSON.stringify(masterData.LiveProductForms));

        const config = {
            headers: {
                'content-type': 'multipart/form-data',
                'Authorization': "Bearer " + localStorage.getItem("token")
            }
        }
        axios.post("/api/live-product/save", formData, config)
            .then(function () {
                toastr.success("Saved successfully!");
                backToList();
            })
            .catch(showResponseError);
    }
})();