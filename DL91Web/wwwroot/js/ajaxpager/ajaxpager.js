// ######################################################################################
// #
// # Date			Name		Comments     
// # 11/25/2016     zack
// #
// ######################################################################################

"use strict";
var ajaxPager = ajaxPager || {};
ajaxPager.pager = (function () {

    var selectors = {
        hipagebaseurl: "input[name='hipagebaseurl']",
        hicurrentPage: "input[name='hicurrentPage']",
        hipageCount: "input[name='hipageCount']",
        selpagesize: "select[name='selpagesize']",
        inpcurrentpage: "input[name='inpcurrentpage']",
        hiupdateTargetID: "input[name='hiupdateTargetID']",
        hiSortArea: "input[name='hiSortArea']",
        btnfirst: "span.btnfirst",
        btnprevious: "span.btnprevious",
        btnnext: "span.btnnext",
        btnlast: "span.btnlast",
        btnrefresh: "span.btnrefresh",
        labTotalInfo: "a.labTotalInfo",
        hiOnSuccess: "input[name='hiOnSuccess']",
        hiRequestFun: "input[name='hiRequestFun']",
        hionAjaxBegin: "input[name='hionAjaxBegin']"
    };
    function getDataInUrl(url) {
        var data = url.split('?')[1].split('&');
        var result = {};
        for (var i = 0; i < data.length; i++) {
            if (data[i].split('=')[0] != 'hipagebaseurl') {
                result[data[i].split('=')[0]] = unescape(data[i].split('=')[1]);
            }
        }
        return result;
    }
    var pager = {};
    pager.gotopage = function (currentpage, container, isFireByChangePage) {
        console.log('gotopage')
        if (!currentpage)
            currentpage = parseInt($(selectors.hicurrentPage, container).val());

        if (typeof (container[0].gotopage) == 'function') {
            var inPageSize = $(selectors.selpagesize, container).val();
            container[0].gotopage(currentpage, container, isFireByChangePage, inPageSize);
            return;
        }

        var pageCount = parseInt($(selectors.hipageCount, container).val());
        if (pageCount < currentpage || currentpage <= 0) {
            if (isFireByChangePage == 1) {
                alert('invalid page number');
            }
            $(selectors.inpcurrentpage, container).val($(selectors.hicurrentPage, container).val());
            return;
        }
        var baseurl = $(selectors.hipagebaseurl, container).val();
        baseurl = baseurl.replace("currentPage=0", "currentPage=" + currentpage);
        baseurl += "&inPageSize=" + $(selectors.selpagesize, container).val();
        var hiSortAreaName = $(selectors.hiSortArea, container).val();
        if (hiSortAreaName) {
            baseurl += "&" + hiSortAreaName + "=" + $("[name='" + hiSortAreaName + "']", container).val();
        }
        var onAjaxBegin = $(selectors.hionAjaxBegin, container).val();
        if (onAjaxBegin) {
            var newUrl = eval(onAjaxBegin + "('" + baseurl + "');");
            if (newUrl) {
                baseurl = newUrl;
            }
        }
        $(selectors.labTotalInfo, container).html('Processing, please wait...');
        var requestFun = $(selectors.hiRequestFun, container).val();
        if (requestFun != 'Get' && requestFun != 'Post') {
            requestFun = 'Get';
        }
        $.ajax({
            cache: false,
            type: requestFun,
            data: getDataInUrl(baseurl),
            url: baseurl.split('?')[0],
            dataType: 'html',
            success: function (data) {
                var updateTargetID = $(selectors.hiupdateTargetID, container).val();
                $(container).closest("#" + updateTargetID).html(data);
                var onSuccess = $(selectors.hiOnSuccess, container).val();
                if (onSuccess) {
                    var lastchar = onSuccess[onSuccess.length - 1];
                    if (lastchar != ')' && lastchar != ';') {
                        eval(onSuccess + "();");
                    } else {
                        eval(onSuccess);
                    }

                }
            },
            error: function () {
                $(selectors.labTotalInfo, container).html('Data loading failed');
            }
        });
    };
    pager.OnPageChange = function () {
        var container = $(this).closest("div.pagination-container");
        var page = $(this).val();
        if (page) {
            console.log('OnPageChange');
            pager.gotopage(page, container, 1);
        }
    };
    pager.OnPageSizeChange = function () {
        console.log('OnPageSizeChange');
        var container = $(this).closest("div.pagination-container");
        pager.gotopage(1, container);
    };
    pager.bindPageChange = function () {
        $(document).off("change", "div.pagination-container " + selectors.inpcurrentpage);
        $(document).off("keydown", "div.pagination-container " + selectors.inpcurrentpage);
        $(document).on("change", "div.pagination-container " + selectors.inpcurrentpage, pager.OnPageChange);
        $(document).on("keydown", "div.pagination-container " + selectors.inpcurrentpage, function (e) {
            if (event.keyCode == 13) {
                //pager.OnPageChange.apply(this);
                $(this).blur();
                return false;
            }
        });
    };
    pager.bindPageSizeChange = function () {
        $(document).off("change", "div.pagination-container " + selectors.selpagesize);
        $(document).on("change", "div.pagination-container " + selectors.selpagesize, pager.OnPageSizeChange);
    };
    pager.bindbtnevent = function () {
        $(document).off("click", "div.pagination-container " + selectors.btnfirst);
        $(document).off("click", "div.pagination-container " + selectors.btnprevious);
        $(document).off("click", "div.pagination-container " + selectors.btnnext);
        $(document).off("click", "div.pagination-container " + selectors.btnlast);
        $(document).off("click", "div.pagination-container " + selectors.btnrefresh);
        $(document).off("click", ".pagination-sortcolumn");
        $(document).on("click", "div.pagination-container " + selectors.btnfirst, function () {
            var container = $(this).closest("div.pagination-container");
            var currentpage = parseInt($(selectors.hicurrentPage, container).val());
            if (currentpage != 1) {
                console.log('btnfirst');
                pager.gotopage(1, container);
            }
        });
        $(document).on("click", "div.pagination-container " + selectors.btnprevious, function () {
            var container = $(this).closest("div.pagination-container");
            var currentpage = parseInt($(selectors.hicurrentPage, container).val());
            if (currentpage != 1) {
                console.log('btnprevious');
                pager.gotopage(currentpage - 1, container);
            }
        });
        $(document).on("click", "div.pagination-container " + selectors.btnnext, function () {
            var container = $(this).closest("div.pagination-container");
            var hipageCount = parseInt($(selectors.hipageCount, container).val());
            var currentpage = parseInt($(selectors.hicurrentPage, container).val());
            if (currentpage < hipageCount) {
                console.log('btnnext');
                pager.gotopage(currentpage + 1, container);
            }
        });
        $(document).on("click", "div.pagination-container " + selectors.btnlast, function () {
            var container = $(this).closest("div.pagination-container");
            var hipageCount = parseInt($(selectors.hipageCount, container).val());
            var currentpage = parseInt($(selectors.hicurrentPage, container).val());
            if (currentpage < hipageCount) {
                console.log('btnlast');
                pager.gotopage(hipageCount, container);
            }
        });
        $(document).on("click", "div.pagination-container " + selectors.btnrefresh, function () {
            var container = $(this).closest("div.pagination-container");
            console.log('btnrefresh');
            pager.gotopage(0, container);
        });
        $(document).on("click", ".pagination-sortcolumn", function () {
            var columnname = $(this).data("columnname");
            var hiSortArea = $(this).data("sortarea");
            var container = $('input[name=\'hiSortArea\'][value=\'' + hiSortArea + '\']')
                .closest("div.pagination-container");

            if (container.length == 0) {
                console.error("Can not find the paging control by sort key \"" + hiSortArea + "\"");
                return;
            }

            var hiSortAreaName = $(selectors.hiSortArea, container).val();

            var status = $(this).data("status");
            if (status == 1) {
                $("[name='" + hiSortAreaName + "']", container).val("");
            } else if (status == 2) {
                $("[name='" + hiSortAreaName + "']", container).val(columnname + " asc");
            } else {
                $("[name='" + hiSortAreaName + "']", container).val(columnname + " desc");
            }
            pager.gotopage(1, container);
        });
    };
    pager.init = function () {
        pager.bindPageChange();
        pager.bindPageSizeChange();
        pager.bindbtnevent();
    };
    pager.init();
    return pager;
})();