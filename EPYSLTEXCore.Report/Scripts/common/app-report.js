'use strict'

var rootPath;
var apiRootPath;
var HasAutoNumber = false;
var CanInsert = false;
var CanUpdate = false;
var CanDelete = false;
var template = '';
var activeMenu = false;
var reportId, hasExternalReport, buyerId;
var columnValues = [];
var columnValueOptions = [];
var shownColumnValues = [];
var params = "";
var currentRow;
var _finder = null;
var _columnWithValues = [];

$(document).ready(function () {
    rootPath = window.location.protocol + '//' + window.location.host;
    apiRootPath = "https://localhost:7053/";

    $.fn.editable.defaults.mode = 'inline';
    toastr.options.escapeHtml = true;
    axios.defaults.headers.post['Content-Type'] = 'application/x-www-form-urlencoded';
    //if (localStorage.getItem("token")) {
    //    axios.defaults.headers.common['Authorization'] = "Bearer " + localStorage.getItem("token");
    //}

    loadProgressBar();

    getMenus();

    $('[data-toggle="tooltip"]').tooltip({ html: true });

    $("#modal-default").modal('show');

    $("#btnOk").on('click', function (e) {
        e.preventDefault();
        console.log("oka");
        var selectedFilterValue = $("#tblFilterSetValue").bootstrapTable('getSelections');
        selectedFilterValue = selectedFilterValue.map(function (el) { return el.value; });
        currentRow.ColumnValue = selectedFilterValue.toString();
        refreshRow(currentRow);
        $("#modal-filterset").modal('hide');
    });

    $("#btnClear").on("click", function (e) {
        e.preventDefault();
        clearParameters();
    });

    $("#btnPreveiw").on("click", function (e) {
        e.preventDefault();
        
        previewReport();
    });

    $("#btnPdf").on("click", function (e) {
        e.preventDefault();
        previewReport(constants.PDF);
    });

    //setTimeout(refreshToken, 1500000);
});

function refreshToken() {
    var params = new URLSearchParams();
    params.append('grant_type', 'refresh_token');
    params.append('client_id', localStorage.getItem("client_id"));
    params.append('refresh_token', localStorage.getItem("refresh_token"));

    axios.post("/token", params)
        .then(function (response) {
            localStorage.setItem("token", response.data.access_token);
            axios.defaults.headers.common['Authorization'] = "Bearer " + response.data.access_token;
        })
        .catch(showResponseError);

    setTimeout(refreshToken, 1500000);
}

function logout() {
    axios.post('/api/Account/Logout')
        .then(function () {
            window.location.href = "/account/logoff";
        })
        .catch(function () {
            window.location.href = "/account/logoff";
        });
}

// #region Tree Plugin Initialization
/* Tree()
 * ======
 * Converts a nested list into a multilevel
 * tree view menu.
 *
 * @Usage: $('.my-menu').tree(options)
 *         or add [data-widget="tree"] to the ul element
 *         Pass any option as data-option="value"
 */
+function ($) {
    'use strict';

    var DataKey = 'lte.tree';

    var Default = {
        animationSpeed: 500,
        accordion: true,
        followLink: false,
        trigger: '.nav-item a'
    };

    var Selector = {
        tree: '.tree',
        treeview: '.treeview',
        treeviewMenu: '.treeview-menu',
        open: '.menu-open, .active',
        li: 'li',
        data: '[data-widget="tree"]',
        active: '.active'
    };

    var ClassName = {
        open: 'menu-open',
        tree: 'tree'
    };

    var Event = {
        collapsed: 'collapsed.tree',
        expanded: 'expanded.tree'
    };

    // Tree Class Definition
    // =====================
    var Tree = function (element, options) {
        this.element = element;
        this.options = options;

        $(this.element).addClass(ClassName.tree);

        $(Selector.treeview + Selector.active, this.element).addClass(ClassName.open);

        this._setUpListeners();
    };

    Tree.prototype.toggle = function (link, event) {
        var treeviewMenu = link.next(Selector.treeviewMenu);
        var parentLi = link.parent();
        var isOpen = parentLi.hasClass(ClassName.open);

        if (!parentLi.is(Selector.treeview)) {
            return;
        }

        if (!this.options.followLink || link.attr('href') === '#') {
            event.preventDefault();
        }

        if (isOpen) {
            this.collapse(treeviewMenu, parentLi);
        } else {
            this.expand(treeviewMenu, parentLi);
        }
    };

    Tree.prototype.expand = function (tree, parent) {
        var expandedEvent = $.Event(Event.expanded);

        if (this.options.accordion) {
            var openMenuLi = parent.siblings(Selector.open);
            var openTree = openMenuLi.children(Selector.treeviewMenu);
            this.collapse(openTree, openMenuLi);
        }

        parent.addClass(ClassName.open);
        tree.slideDown(this.options.animationSpeed, function () {
            $(this.element).trigger(expandedEvent);
        }.bind(this));
    };

    Tree.prototype.collapse = function (tree, parentLi) {
        var collapsedEvent = $.Event(Event.collapsed);

        //tree.find(Selector.open).removeClass(ClassName.open);
        parentLi.removeClass(ClassName.open);
        tree.slideUp(this.options.animationSpeed, function () {
            //tree.find(Selector.open + ' > ' + Selector.treeview).slideUp();
            $(this.element).trigger(collapsedEvent);
        }.bind(this));
    };

    // Private

    Tree.prototype._setUpListeners = function () {
        var that = this;

        $(this.element).on('click', this.options.trigger, function (event) {
            that.toggle($(this), event);

            if ($(this).parent().hasClass("nav-item")) {
                return;
            }

            $('.treeview li').removeClass('active');
            $(this).parent().addClass("active");
            $('.treeview').removeClass('menu-open');
            $(this).closest('li.treeview').addClass("menu-open");

            reportId = $(this).data("report-id");
            hasExternalReport = $(this).data("has-external-report");
            if (hasExternalReport)
                showReportBuyerSelection();
            else
                loadReportInformation();
        });
    };

    // Plugin Definition
    // =================
    function Plugin(option) {
        return this.each(function () {
            var $this = $(this);
            var data = $this.data(DataKey);

            if (!data) {
                var options = $.extend({}, Default, $this.data(), typeof option === 'object' && option);
                $this.data(DataKey, new Tree($this, options));
            }
        });
    }

    var old = $.fn.tree;

    $.fn.tree = Plugin;
    $.fn.tree.Constructor = Tree;

    // No Conflict Mode
    // ================
    $.fn.tree.noConflict = function () {
        $.fn.tree = old;
        return this;
    };

    //// Tree Data API
    //// =============
    //$(window).on('load', function () {
    //    $(Selector.data).each(function () {
    //        Plugin.call($(this));
    //    });
    //});

}(jQuery);
// #endregion 

function getMenus() {
    axios.get(`${apiRootPath}api/MenuAPI/GetAllMenuReport/` + constants.APPLICATION_ID)
    
        .then(function (response) {
            generateMenu(response.data);
            $(".sidebar-menu").append(template);
            $(".sidebar-menu").tree();
        })
        .catch(showResponseError)
}

function generateMenu(menuList) {

    $.each(menuList, function (i, item) {
        if (!item.childs.length) {
            template += '<li><a href="#!" class="nav-link" data-report-id="' + item.reportId + '" data-has-external-report="' + item.hasExternalReport + '"><i class="nav-icon fa fa-circle-o"></i><p title="' + item.node_Text + '">' + item.node_Text + '</p></a></li>';
            activeMenu = false;
            return true;
        }
        else {
            activeMenu = item.menuId == 509 ? true : false;
            var active = activeMenu ? "active" : "";
            template += '<li class="nav-item' + active + '">';
            template += '<a href="#" class="nav-link">'   
                + '<i class="nav-icon fa fa-circle"></i><p>' + item.node_Text + '<i class="right fa fa-angle-left"></i></p>'            
                + '</a>';
            template += '<ul class="nav nav-treeview treeview-menu">';
            activeMenu = false;


        }
        
        generateMenu(item.childs);
        template += '</ul></li>';
    });
}

function showReportBuyerSelection() {
    console.log("showReportBuyerSelection");
    axios.get("/reports/GetReportBuyers")
        .then(function (response) {
            showBootboxSelect2Dialog("Select Buyer", "BuyerID", "Select Buyer", response.data, function (result) {
                if (result) {

                    buyerId = parseInt(result.id);
                    $("#liBuyer").remove();
                    $("#rightNav").prepend('<li id="liBuyer"><a href="#!">Buyer&nbsp;<i class="fa fa-chevron-right"></i>&nbsp;' + result.text + '</a></li>');
                    loadReportInformation();
                }
            });
        })
        .catch(showResponseError);
}

function loadReportInformation() {

    if (!hasExternalReport) {
        buyerId = 0;
        $("#liBuyer").remove();
    }
    var url = "/reports/GetReportInformation?reportId=" + reportId + "&hasExternalReport=" + hasExternalReport + "&buyerId=" + buyerId;

    axios.get(url)
        .then(function (response) {
            columnValues = response.data.FilterSetList;
            console.log(columnValues);
            columnValueOptions = response.data.ColumnValueOptions;
            shownColumnValues = columnValues.filter(function (el) {
                return !el.IsSystemParameter && !el.IsHidden && el.ColumnName != "Expression";
            });
            _columnWithValues = DeepClone(shownColumnValues);
            initTable(shownColumnValues);
        })
        .catch(showResponseError);
}
function DeepClone(obj) {
    return JSON.parse(JSON.stringify(obj));
}
function initTable(data) {
    $("#tblReportFilters").bootstrapTable('destroy');
    $("#tblReportFilters").bootstrapTable({
        cache: false,
        uniqueId: 'ColumnName',
        filterControl: true,
        searchOnEnterKey: true,
        columns: [
            {
                field: "Caption",
                title: "...",
                align: 'center',
                width: 50
            },
            {
                field: "ColumnName",
                title: "Expression",
                width: 100
            },
            {
                field: "Operators",
                title: "...",
                align: 'center',
                width: 50
            },
            {
                field: "ColumnValue",
                title: "Value",
                width: 300,
                formatter: function (value, row, index, field) {
                    var cValue = value && (value.contains(":00 AM") || value.contains(":00 PM"))
                        ? formatDateToDefault(value)
                        : value ? value : "Set " + row.ColumnName;
                    return ['<a href="javascript:void(0)" class="editable-link edit">' + cValue + '</a>'].join(' ');
                },
                events: {
                    'click .edit': function (e, value, row, index) {
                        e.preventDefault();
                        currentRow = DeepClone(row);


                        if (row.DataType == "DateTime" || row.DataType == "System.DateTime") {
                            $(e.target).datepicker({
                                autoclose: true,
                                todayHighlight: true,
                                endDate: "0d",
                                todayBtn: true
                            }).on('changeDate', function (e) {
                                try {
                                 
                                    row.ColumnValue = formatDateToDefault(e.date);
                                    console.log(columnValues);
                                    refreshRow(row);
                                   
                           
                                } catch (e) {
                                    console.log("asdasdasd");
                                    row.ColumnValue = "";
                                    
                                }
                            });
                        }
                        else if (row.DataType == "String" || row.DataType == "System.String" || row.DataType == "Int32" || row.DataType == "System.Int32") {
                            var selectOptions = [];
                            if (row.IsMultipleSelection) { // Multiple Value
                                var title = "Select " + row.ColumnName;

                                if (row.HasParent) { // If Parent/Dependency column is required
                                    params = "?ReportId=" + reportId + "&MethodName=" + encodeURIComponent(row.MethodName) + "&IsSP=" + true;
                                    var parentColumns = row.ParentColumn.split(',');
                                    $.each(parentColumns, function (i, column) {
                                        var cv = columnValues.find(function (el) { return el.ColumnName == column });
                                        if (!cv) {
                                            toastr.warning("Please set " + column + " value first");
                                            return false;
                                        }
                                        params += '&' + column + '=' + encodeURIComponent(cv.ColumnValue);
                                    });

                                    var url = "/Reports/GetFilterColumnOptions" + params;
                                    axios.get(url)
                                        .then(function (response) {
                                            $.each(response.data, function (i, v) {
                                                var item = v.find(function (el) { return el.Key === row.ValueColumnId });
                                                selectOptions.push({ value: item.Value, text: item.Value })
                                            });

                                            initTableFilterValue(selectOptions, reportId);

                                            if (!isUseCommonFinderReport(reportId)) {
                                                $("#modal-filterset-title").text(title);
                                                $("#modal-filterset").modal('show');
                                            }
                                        })
                                        .catch(showResponseError)
                                }
                                else { // If no Parent/Dependency column
                                    //$.each(columnValueOptions, function (i, v) {
                                    //    var el = v.find(function (el) { return el.Key === row.ColumnName });
                                    //    var option = { value: el.Value, text: el.Value };
                                    //    selectOptions.push(option);
                                    //});

                                    //initTableFilterValue(selectOptions, reportId);
                                    //$("#modal-filterset-title").text(title);
                                    //$("#modal-filterset").modal('show');

                                    params = "?ReportId=" + reportId + "&MethodName=" + encodeURIComponent(row.MethodName) + "&IsSP=" + true;
                                    var url = "/Reports/GetFilterColumnOptions" + params;
                                    axios.get(url)
                                        .then(function (response) {
                                            $.each(response.data, function (i, v) {
                                                var item = v.find(function (el) { return el.Key === row.ValueColumnId });
                                                selectOptions.push({ value: item.Value, text: item.Value })
                                            });
                                            if (selectOptions.length == 0) {
                                                $.each(columnValueOptions, function (i, v) {
                                                    var el = v.find(function (el) { return el.Key === row.ColumnName });
                                                    var option = { value: el.Value, text: el.Value };
                                                    selectOptions.push(option);
                                                });
                                            }

                                            initTableFilterValue(selectOptions, reportId);

                                            if (!isUseCommonFinderReport(reportId)) {
                                                $("#modal-filterset-title").text(title);
                                                $("#modal-filterset").modal('show');
                                            }
                                        })
                                        .catch(showResponseError)
                                }
                            }
                            else { // Single Value
                                var title = "Select " + row.ColumnName;

                                if (row.HasParent) { // If Parent/Dependency column is required
                                    params = "?ReportId=" + reportId + "&MethodName=" + row.MethodName
                                    var parentColumns = row.ParentColumn.split(',');
                                    $.each(parentColumns, function (i, column) {
                                        var cv = columnValues.find(function (el) { return el.ColumnName == column });
                                        if (!cv) {
                                            toastr.warning("Please set " + column + " value first");
                                            return false;
                                        }
                                        params += '&' + column + '=' + cv.ColumnValue;
                                    });

                                    var url = "/Reports/GetFilterColumnOptions" + params;
                                    axios.get(url)
                                        .then(function (response) {
                                            showBootboxSelect2Dialog(title, row.ColumnName, title, response.data, function (result) {
                                                if (result) {
                                                    row.ColumnValue = result.text;
                                                    refreshRow(row);
                                                }
                                            })
                                        })
                                        .catch(showResponseError)
                                }
                                else { // If no Parent/Dependency column
                                    $.each(columnValueOptions, function (i, v) {
                                        var el = v.find(function (el) { return el.Key === row.ColumnName });
                                        var option = { id: el.Value, text: el.Value };
                                        selectOptions.push(option);
                                    });

                                    showBootboxSelect2Dialog(title, row.ColumnName, title, selectOptions, function (result) {
                                        if (result) {
                                            row.ColumnValue = result.text;
                                            refreshRow(row);
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
            },
            {
                field: "OrAnd",
                title: "...",
                align: 'center',
                width: 50
            }
        ],
        data: data
    });
}

function refreshRow(data) {
    debugger;
    var columnValuesArray = data.ColumnValue.split(',');
    
    data.Operators = columnValuesArray.length > 1 ? "In" : "=";
    console.log("Before Update: ", $("#tblReportFilters").bootstrapTable('getData'));
    $("#tblReportFilters").bootstrapTable('updateByUniqueId', { id: data.ColumnName, row: data });
    console.log("After Update: ", $("#tblReportFilters").bootstrapTable('getData'));
}
function isUseCommonFinderReport(reportId) {
    return true;

    //var reportIdList = [];
    //var findIndexR = reportIdList.findIndex(x => x == reportId);
    //if (findIndexR > -1) return true;
    //return false;
}
function initTableFilterValue(data, reportId) {
    console.log("initTableFilterValue");
    if (isUseCommonFinderReport(reportId)) {
        //Ratin
        if (data.length == 0) return toastr.error("No list found");
        _finder = new commonFinder({
            title: "Select Item(s)",
            pageId: "modal-filterset-commonFinder",
            data: data,
            isMultiselect: true,
            modalSize: "modal-md",
            top: "2px",
            primaryKeyColumn: "value",
            fields: "text",
            headerTexts: "...",
            //widths: "400",
            allowFiltering: true,
            autofitColumns: false,
            onMultiselect: function (selectedRecords) {
                _finder.hideModal();

                var selectedFilterValue = selectedRecords;
                selectedFilterValue = selectedFilterValue.map(function (el) { return el.value; });
                currentRow = DeepClone(currentRow);
                currentRow.ColumnValue = selectedFilterValue.toString();

                var tblData = $("#tblReportFilters").bootstrapTable('getData');
                if (tblData != null && tblData.length > 0) {
                    tblData.map(x => {
                        if (x.ColumnValue.length == 0 && x.ColumnName != currentRow.ColumnName) {
                            x.ColumnValue = $("#tblReportFilters").find("tbody").find("tr[data-uniqueId='" + x.ColumnName + "']").find(".editable-link").text();
                        }
                        x.Operators = x.ColumnValue.split(',').length > 1 ? "In" : "=";
                    });
                }
                var columnValuesArray = currentRow.ColumnValue.split(',');
                currentRow.Operators = columnValuesArray.length > 1 ? "In" : "=";

                var columnIndex = _columnWithValues.findIndex(x => x.ColumnName == currentRow.ColumnName);
                if (columnIndex < 0) {
                    _columnWithValues.push({
                        ColumnName: currentRow.ColumnName,
                        ColumnValue: currentRow.ColumnValue,
                        Operators: currentRow.Operators
                    });
                } else {
                    _columnWithValues[columnIndex].ColumnValue = currentRow.ColumnValue;
                    _columnWithValues[columnIndex].Operators = currentRow.Operators;
                }

                var indexF = tblData.findIndex(x => x.ColumnName == currentRow.ColumnName);
                tblData[indexF].ColumnValue = _columnWithValues[columnIndex].ColumnValue;
                tblData[indexF].Operators = _columnWithValues[columnIndex].Operators;

                //$("#tblReportFilters").bootstrapTable('updateByUniqueId', { id: currentRow.ColumnName, row: currentRow });

                $("#tblReportFilters").bootstrapTable('load', tblData);
            },
        });
        _finder.showModal();
    }
    else {
        $("#tblFilterSetValue").bootstrapTable('destroy');
        $("#tblFilterSetValue").bootstrapTable({
            pagination: true,
            showFooter: true,
            pageSize: 10,
            pageList: [10, 25, 50, 100],
            columns: [
                {
                    align: 'center',
                    checkbox: true,
                    width: 50
                },
                {
                    field: "value",
                    title: "...",
                    align: 'center',
                    sortable: true
                }
            ],
            data: data
        });
    }
}

function clearParameters() {
    $.each(columnValues, function (i, cv) {
        cv.ColumnValue = "";
        cv.Operators = "=";
    });
}

function previewReport(reportType) {
    debugger;
    toastr.info("Please wait while we process your report.");
   
    var requiredFilterSets = columnValues.filter(function (el) { return el.Caption == "***" });
    console.log(columnValues);
    var isValid = true;
    $.each(requiredFilterSets, function (i, cv) {
        if (!cv.ColumnValue) {
            isValid = false;
            toastr.error(cv.ColumnName + " is required.");
        }
    });

    if (!isValid) return;

    var params = $.param({ ReportId: reportId, FilterSetList: JSON.stringify(columnValues), HasExternalReport: hasExternalReport, buyerId: buyerId });
    if (reportType == constants.PDF) {
        var url = rootPath + "/reports/pdfview?" + params;
        window.open(url, "_blank");
    } else {
        var url = rootPath + "/reports/reportview.aspx?" + params;
        window.open(url, "_blank");
    }
}

// #region Constants
var constants = Object.freeze({
    APPLICATION_ID: 8,
    LOAD_ERROR_MESSAGE: "An error occured in fetching your data",
    SUCCESS_MESSAGE: "Your record saved successfully!",
    PROPOSE_SUCCESSFULLY: "Your record has been sent for approval!",
    APPROVE_SUCCESSFULLY: "Your record approved successfylly!",
    ACKNOWLEDGE_SUCCESSFULLY: "Your record acknowledged successfylly!",
    ACCEPTED_SUCCESFULLY: "Your record accepted successfully!",
    REJECT_SUCCESSFULLY: "Your record rejected successfylly!",
    CANCEL_SUCCESSFULLY: "Your record cancelled successfylly!",
    UNAPPROVE_SUCCESSFULLY: "Your record unapproved successfully",
    REVISE_BOOKING: "Your booking is under Revised Stage!!!",
    PDF: "PDF"
});
// #endregion