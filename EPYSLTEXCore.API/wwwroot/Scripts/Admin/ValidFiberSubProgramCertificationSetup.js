(function () {
    var menuId, menuParam, pageId, $pageEl, pageName = "VFSCS";

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");
        if (!menuParam) menuParam = localStorage.getItem("menuParam");

        pageId = pageName + "-" + menuId;
        $pageEl = $(`#${pageId}`);
        $formEl = $(pageConstants.FORM_ID_PREFIX + pageId);

        $formEl.find("#btnSave").click(function () {
            save();
        });

        loadSegments();
    });
    function loadSegments() {
        axios.get(`/api/items/yarn/all-composition-segments-list`)
            .then(function (response) {
                var obj = response.data;

                $(".tblSegment tbody").find("tr").remove();

                loadList(obj.Fibers, "tblFiber");
                loadList(obj.SubPrograms, "tblSubProgram");
                loadList(obj.Certifications, "tblCertification");
            })
            .catch(function (err) {
                toastr.error(err.response.data.Message);
            });
    }
    function loadList(list, tableId) {
        var sl = 1;
        list.map(x => {
            var isCheck = x.IsInactive ? "" : "checked";
            var previousActiveStatus = !x.IsInactive;
            var checkBoxUniqueClass = "chkIsActive" + "" + sl;

            var tempTr = `<tr>`;
            tempTr += `<td>${sl++}</td>`;
            tempTr += `<td style="text-align: left;">${x.SegmentValueName}</td>`;
            tempTr += `<td style="width: 100px;"><input type="checkbox" class="chkIsActive ${checkBoxUniqueClass}" style="cursor: pointer;" SegmentValueId = ${x.SegmentValueId} ${isCheck} previousActiveStatus=${previousActiveStatus}></td>`;
            tempTr += `</tr>`;

            $("#" + tableId).append(tempTr);

            $("#" + tableId).find("." + checkBoxUniqueClass).click(function () {
                debugger;
                $("." + checkBoxUniqueClass).change(function () {
                    $(this).addClass("chkChanged");
                });
            });
        });
    }
    function save() {
        
        var data = {
            Fibers: getLists("tblFiber"),
            SubPrograms: getLists("tblSubProgram"),
            Certifications: getLists("tblCertification"),
            Compositions: [],
            YarnTypes: [],
            Processes: [],
            SubProcesses: [],
            QualityParameters: [],
            Counts: []
        };
        axios.post("/api/items/save-composition-segment-active-inactive", data)
            .then(function () {
                toastr.success("Successfully updated.");
            })
            .catch(function (error) {
                toastr.error(error.response.data.Message);
            });
    }
    function getLists(tableId) {
        debugger;
        var objList = [];
        $("#" + tableId).find("tbody").find(".chkChanged").each(function () {
            debugger;
            var previousActiveStatus = $(this).attr("previousActiveStatus");
            previousActiveStatus = previousActiveStatus == "true" ? true : false;
            var currentActiveStatus = $(this).prop('checked');

            if (previousActiveStatus != currentActiveStatus) {
                var segmentValueId = parseInt($(this).attr("SegmentValueId"));
                $(this).attr("previousActiveStatus", currentActiveStatus);

                objList.push({
                    SegmentValueId: segmentValueId,
                    IsInactive: !currentActiveStatus,
                });
            }
        });
        return objList;
    }
})();