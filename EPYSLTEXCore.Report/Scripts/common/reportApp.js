'use strict'

var rootPath;
var apiRootPath;
var HasAutoNumber = false;
var CanInsert = false;
var CanUpdate = false;
var CanDelete = false;
var ImageFileLength, PICINID, SupplierID, BuyerIds;
var currentTab;
var template = '';
var activeMenu = false;
const FILTERCONTROLPLACEHOLDER = "Type & Enter for Search";
var $mainTab;
var $mainTabContent;

var globalControllerName = 'home';
var globalActionName = 'index';

var constants = Object.freeze({
    APPLICATION_ID: 8,
    LOAD_ERROR_MESSAGE: "An error occured in fetching your data",
    SUCCESS_MESSAGE: "Your record saved successfully!",
    PROPOSE_SUCCESSFULLY: "Your record has been sent for approval!",
    APPROVE_SUCCESSFULLY: "Your record approved successfylly!",
    ACCEPTED_SUCCESFULLY: "Your record accepted successfully!",
    REJECT_SUCCESSFULLY: "Your record rejected successfully!",
    UNAPPROVE_SUCCESSFULLY: "Your record unapproved successfully",
    REVISE_BOOKING: "Your booking is under Revised Stage!!!",
    GMT_ERP_BASE_PATH: "https://gmterp.epylliongroup.com",
    GMT_ERP_LOCAL_PATH: "http://192.168.10.231:8080",
    MASTER_GRID_LOADING_TIME: 300000 //5 Min
});

$(document).ready(function () {


    rootPath = window.location.protocol + '//' + window.location.host;
    apiRootPath = "https://localhost:7053/";
    toastr.options.escapeHtml = true;

    $.fn.editable.defaults.mode = 'inline';
    $.fn.editable.defaults.onblur = "submit";

  

    //getBookingAnalysisBookingAcknowledgementCountALL();
    loadProgressBar();

    axios.interceptors.request.use(function (config) {
        if (config.method === "post") {
            HoldOn.open({
                theme: "sk-circle",
                message: "Please wait while we process your request.",
            });
        }
        return config;
    }, function (error) {
        HoldOn.close();
        return Promise.reject(error);
    });

    axios.interceptors.response.use(function (config) {
        HoldOn.close();
        return config;
    }, function (error) {
        HoldOn.close();
        return Promise.reject(error);
    });

    GetMenus(constants.APPLICATION_ID);

    $('[data-toggle="tooltip"]').tooltip({ html: true });

    $mainTab = $("#mainTab");
    $mainTabContent = $("#mainTabContent");
    //when ever any tab is clicked this method will be call
    $mainTab.on("click", "a", function (e) {

        e.preventDefault();
        $(this).tab('show');
        currentTab = $(this);

        var menuLink = e.target.hash.split('#')[1];
        var el = $("[data-action-name='" + menuLink + "']");
        $(this).parent().siblings().removeClass('active');
        $(this).parent().siblings().removeClass('bg-info');
        $(this).parent().addClass("active");
        $(this).parent().addClass("bg-info");
    });

    registerCloseEvent();
});
function registerCloseEvent() {
    $(".closeTab").click(function () {
        var tabContentId = $(this).parent().attr("href");
        $(this).parent().parent().remove(); //remove li of tab
        $('#mainTab a:last').tab('show'); // Select first tab
        $(tabContentId).remove(); //remove respective tab content
    });
}
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
        data: '[data-widget="treeview"]',
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

            var actionName = $(this).data("action-name");
            var controllerName = $(this).data("controller-name");
            var navUrlName = $(this).data("navurl-name");
            var pageType = $(this).data("page-type");
            var pageName = $(this).data("page-name");
            if (pageName) pageName = pageName.split(' ').join('');
            else pageName = actionName;

            var menuId = $(this).data("menu-id");
            var tabCaption = $(this).text().trim();
            var isExists = $('#mainTab a[href="#' + pageName + '"]');
            if (pageType == 'Report')
                return;
            else if (pageType == 'NF') {
                if (isExists.length === 0) GetNotFoundViewMarkup(pageName, tabCaption);
                else showTab(pageName);
            }
            else if (pageType == 'CI') {
                if (isExists.length === 0) getCommonInterfaceMarkup(controllerName, actionName, menuId, pageName, tabCaption, navUrlName);
                else showTab(pageName);
            }
            else {
                if (isExists.length === 0) GetViewMarkup(controllerName, actionName, menuId, pageName, tabCaption, navUrlName);
                else {
                    showTab(pageName);
                    var tableId = $(this).data("table-id");
                    var url = $(this).data("refresh-url");
                    if (tableId && url)
                        refreshTable(tableId, url);
                }
            }
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

function GetMenus(applicationId) {
    axios.get(`${apiRootPath}api/MenuAPI/GetAllMenu/${applicationId}`)
        .then(function (response) {
            generateMenu(response.data);
            $(".sidebar-menu").append(template);

            $(".sidebar-menu").tree();
        })
        .catch(showResponseError);
}

function generateMenu(menuList) {



    $.each(menuList, function (i, item) {
        if (!item.childs.length) {

            if (!item.navigateUrl) return true;
            var navProperties = item.navigateUrl.split('/');

            // Replace all occurrences of '/' with '_'
            var updatednavigateUrl = item.navigateUrl.replace(/\//g, '_');

            if (navProperties[1] == 'notfoundpartial') {
                template += '<li><a href="#!" class="nav-link" data-navurl-name="' + updatednavigateUrl + '" data-controller-name="' + globalControllerName + '" data-action-name="' + globalActionName + '" data-page-name="' + item.pageName + '" data-menu-id="' + item.menuId + '" data-page-type="NF"><i class="nav-icon fa fa-circle-o"></i> <p>' + item.menuCaption + '</p></a></li>';
            }
            else if (item.useCommonInterface) {
                template += '<li><a href="#!" class="nav-link" data-navurl-name="' + updatednavigateUrl + '" data-controller-name="' + globalControllerName + '" data-action-name="' + globalActionName + '" data-page-name="' + item.pageName + '" data-menu-id="' + item.menuId + '" data-page-type="CI"><i class="nav-icon fa fa-circle-o"></i> <p>' + item.menuCaption + '</p></a></li>';
            }
            else if (item.pageName == 'ReportViewer') {
                var path = rootPath + '/reports/index';
                template += '<li><a class="nav-link" href="' + path + '" target="_blank" data-page-type="Report"><i class="nav-icon fa fa-circle-o"></i> <p>' + item.menuCaption + '</p></a></li>';
            }
            else {
                template += '<li><a class="nav-link" href="#!" data-navurl-name="' + updatednavigateUrl + '"  data-controller-name="' + globalControllerName + '" data-action-name="' + globalActionName + '" data-table-id="' + navProperties[2] + '" data-page-name="' + item.pageName + '" data-menu-id="' + item.menuId + '"><i class="nav-icon fa fa-circle-o"></i> <p>' + item.menuCaption + '</p></a></li>';
            }

            activeMenu = false;
            return true;
        }
        else {

            activeMenu = item.menuId == 509 ? true : false;
            var active = activeMenu ? "active" : "";
            template += '<li class="nav-item' + active + '">';
            template += '<a href="#" class="nav-link">'
                + '<i class="nav-icon fa fa-circle"></i><p>' + item.menuCaption + '<i class="right fa fa-angle-left"></i></p>'
                //    + '<p class="pull-right-container">'

                //  + '</p>'
                + '</a>';
            template += '<ul class="nav nav-treeview treeview-menu">';
            activeMenu = false;
        }

        generateMenu(item.childs);

        template += '</ul></li>';


    });
}