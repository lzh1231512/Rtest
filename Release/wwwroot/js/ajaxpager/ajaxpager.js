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
    var cachedData = null;
    pager.gotopage = function (currentpage, container, isFireByChangePage,cached) {
        console.log('gotopage')
        if (!currentpage)
            currentpage = parseInt($(selectors.hicurrentPage, container).val());

        if (typeof (container[0].gotopage) == 'function') {
            var inPageSize = $(selectors.selpagesize, container).val();
            container[0].gotopage(currentpage, container, isFireByChangePage, inPageSize);
            return;
        }

        var pageCount = parseInt($(selectors.hipageCount, container).val());
        if ((pageCount < currentpage || currentpage <= 0) && currentpage!=1) {
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

        if (cached) {
            cachedData = cached;
        }
        if (cached || baseurl.indexOf('cache:') == 0) {
            var pageSize = parseInt($(selectors.selpagesize, container).val());
            var _data = [];
            for (var i = (currentpage - 1) * pageSize, k = 0; i < cachedData.length && k < pageSize; i++ , k++) {
                _data.push(cachedData[i]);
            }
            var tData = {
                data: _data,
                page: {
                    pageCount: ((cachedData.length - cachedData.length % pageSize) / pageSize + (cachedData.length % pageSize == 0 ? 0 : 1)),
                    sort: null,
                    currentPage: currentpage,
                    recordCount: cachedData.length,
                    pageSize: pageSize
                }
            }
            var updateTargetID = $(selectors.hiupdateTargetID, container).val();
            $(container).closest("#" + updateTargetID).html(data2html(tData, 'cache:'));
            var onSuccess = $(selectors.hiOnSuccess, container).val();
            if (onSuccess) {
                var lastchar = onSuccess[onSuccess.length - 1];
                if (lastchar != ')' && lastchar != ';') {
                    eval(onSuccess + "();");
                } else {
                    eval(onSuccess);
                }
            }
            afterRunder(tData);
        }
        else {
            $.ajax({
                cache: false,
                type: requestFun,
                data: getDataInUrl(baseurl),
                url: baseurl.split('?')[0],
                dataType: 'json',
                success: function (data) {
                    var updateTargetID = $(selectors.hiupdateTargetID, container).val();
                    $(container).closest("#" + updateTargetID).html(data2html(data));
                    var onSuccess = $(selectors.hiOnSuccess, container).val();
                    if (onSuccess) {
                        var lastchar = onSuccess[onSuccess.length - 1];
                        if (lastchar != ')' && lastchar != ';') {
                            eval(onSuccess + "();");
                        } else {
                            eval(onSuccess);
                        }
                    }
                    afterRunder(data);
                },
                error: function () {
                    $(selectors.labTotalInfo, container).html('Data loading failed');
                }
            });
        }
    };

    function data2html(data,bUrl) {
        var result = `<div class="table-responsive" style="overflow-x:hidden">`;
        if (data.data.length > 0) {
            result += `<div id="divitems" class="row">`;
            for (var i = 0; i < data.data.length; i++) {
                var dt = data.data[i];
                result += `<div class="col-md-2">`;
                result += `<a href="#" onclick="play(this)" data-id="${dt.id}" data-createdate="${dt.createDate}" data-url="${dt.url}" data-title="${dt.title}" data-filesize="${dt.fileSize}" data-ishd="${dt.isHD}" data-islike="${dt.isLike}" target="_blank"><img data-imgid="${dt.id}" style="width:256px;height:144px;" /></a>`;
                result += `<div class="title" data-id="${dt.id}" data-time="${dt.createDate}" style="min-height:36px;">${dt.title}</div>`;
                result += `</div>`;
                if (i % 6 == 5) {
                    result += `<div class="clearfix"></div>`;
                }
            }
            result += `<div class="clearfix"></div>`;
            result += `</div>`;
        }
        else {
            result += `<div class="text-center EmptyList"><span class="text-danger">No Result</span></div>`;
        }
        var url = IndexForAjaxUrl + "?currentPage=0&&title1=" + (data.title1 || '') + "&&title2=" + (data.title2 || '') + "&&isLike=" + data.isLike + "&&typeId=" + data.typeId;
        if (bUrl) {
            url = bUrl;
        }
        result += `<div class="pagination-sm">
            <div class="pagination-container">
                <input type="hidden" name="hipagebaseurl" value="${url}" />
                <input id="hicurrentPage" type="hidden" name="hicurrentPage" value="${data.page.currentPage}" />
                <input type="hidden" name="hipageCount" value="${data.page.pageCount}" />
                <input type="hidden" name="hiupdateTargetID" value="SalesOrderResult" />
                <input type="hidden" name="hirecordCount" value="${data.page.recordCount}" />
                <input type="hidden" name="hipageSize" value="${data.page.pageSize}" />
                <input type="hidden" name="hiOnSuccess" value="onPageLoad" />
                <input type="hidden" name="hiRequestFun" value="Post" />
                <input type="hidden" name="hionAjaxBegin" value="" />
                <ul class="pagination">
                    <li>
                        <span>
                            <select name="selpagesize">`;
        for (var i = 12; i <= 84; i += 6) {
            result += `<option ${i == data.page.pageSize ? 'selected="selected"' : ''}>${i}</option>`;
        }
        var pageInfo = `Displaying ${((data.page.currentPage - 1) * data.page.pageSize) + 1} to ${(data.page.currentPage * data.page.pageSize) > data.page.recordCount ? data.page.recordCount : (data.page.currentPage * data.page.pageSize)} of ${data.page.recordCount} items`;
        result += `</select>
                        </span>
                    </li>
                    <li class="divider"></li>
                    <li>
                        <span title="Go to the frist page" class="btnfirst pagebutton">
                            <i class="fa fa-step-backward"></i>
                        </span>
                    </li>
                    <li>
                        <span title="Go to previous page" class="btnprevious pagebutton">
                            <i class="fa fa-arrow-left"></i>
                        </span>
                    </li>
                    <li>
                        <span>
                            <input name="inpcurrentpage" type="number" style="width:60px;color: #333 !important;" value="${data.page.currentPage}" />&nbsp;/&nbsp;${data.page.pageCount}
                        </span>
                    </li>
                    <li>
                        <span title="Go to next page" class="btnnext pagebutton">
                            <i class="fa fa-arrow-right"></i>
                        </span>
                    </li>
                    <li>
                        <span title="Go to the last page" class="btnlast pagebutton">
                            <i class="fa fa-step-forward"></i>
                        </span>
                    </li>
                    <li>
                        <span title="Refresh" class="btnrefresh pagebutton">
                            <i class="fa fa-refresh"></i>
                        </span>
                    </li>
                    <li class="divider"></li>
                    <li>
                        <span class="labTotalInfo" title="${pageInfo}"><span>${pageInfo}</span></span>
                    </li>
                </ul>
                <div class="clearfix"></div>
            </div>
        </div>`;
        result += `</div>`;
        return result;
    }
    function afterRunder(data) {
        var ids = '';
        var objs = [];
        function loadImg() {
            if (objs.length > 0) {
                var url = GetImgURL + '?Imgs=' + ids.substr(1);
                var index = 0;
                $(objs).each(function () {
                    $(this).css({
                        'background-image': 'url(' + url + ')',
                        'background-repeat': 'no-repeat',
                        'background-attachment': 'scroll',
                        'background-position': '0px ' + (-1) * (index++) * 144 + 'px',
                        'background-color': 'transparent'
                    });
                });
                objs = [];
                ids = '';
            }
        }
        $('#divitems img[data-imgid]').each(function () {
            ids += ',' + $(this).data('imgid');
            objs.push(this);
        });
        loadImg();
        if (data.nextPageIDs) {
            var nextImg = new Image();
            nextImg.src = GetImgURL + '?Imgs=' + data.nextPageIDs;
        }
        $('div.title').each(function () {
            var time = parseInt($(this).data('time'))+480;
            var hour = parseInt((new Date().getTime() - 631123200000 - time * 60000) / 1000 / 3600);
            var day = parseInt(hour / 24);
            var res = '';
            if (day > 365) {
                res = '[1年前]';
            }
            else if (day > 7) {
                res = "[" + parseInt(day / 7) + "周前]"
            }
            else if (hour > 24) {
                res = "[" + day + "天前]"
            }
            else {
                res = '[' + hour + '小时前]';
            }

            var ntitle = $(this).html();
            var _this = this;
            m3u8.getM3u8Url($(this).data('id')+'').then(function (murl) {
                if (murl) {
                    ntitle = '[Cached]' + ntitle;
                }
                ntitle = res + ntitle;
                ntitle = ntitle.replace('[00:00]', '');
                $(_this).html(ntitle);
            })
        })
    }
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
        var pageSize = document.cookie.match(/(?:^|;)\s*PageSize=([^;]+)/);
        var p = 24;
        if (pageSize && pageSize.length > 1) {
            p = pageSize[1];
        }
        $('#SalesOrderResult').html(data2html({
            data: [{}],
            page: { pageSize: p}
        }));
    };
    pager.init();
    return pager;
})();