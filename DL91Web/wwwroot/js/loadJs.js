var geval = eval;
function loadjs(urls, version) {
    clearOldVersion(version);
    if (urls.length == 0) {
        return;
    }
    var nexturl = urls.shift();
    var js = localStorage.getItem('cacheUrl:' + nexturl);
    if (js) {
        geval(js);
        loadjs(urls);
    }
    else {
        ajax(nexturl, function (js) {
            localStorage.setItem('cacheUrl:' +nexturl, js);
            geval(js);
            loadjs(urls);
        },false);
    }
}
function clearOldVersion(version) {
    if (!version)
        return;
    if (localStorage.getItem('loadjs.version') == version) {
        return
    }
    for (var i = localStorage.length - 1; i >= 0; i--) {
        if (localStorage.key(i).indexOf('cacheUrl:') == 0) {
            localStorage.removeItem(localStorage.key(i));
        }
    }
    localStorage.setItem('loadjs.version', version);
}
function loadStyle(urls, version) {
    clearOldVersion(version);
    if (urls.length == 0) {
        return;
    }
    var nexturl = urls.shift();
    var js = localStorage.getItem('cacheUrl:' +nexturl);
    if (js) {
        const style = document.createElement('style');
        style.innerHTML = js;
        document.head.appendChild(style);
        loadStyle(urls);
    }
    else {
        ajax(nexturl, function (js) {
            localStorage.setItem('cacheUrl:' +nexturl, js);
            const style = document.createElement('style');
            document.head.appendChild(style);
            style.innerHTML = js;
            loadStyle(urls);
        },true);
    }
}
function loadHTML(url, version,callback) {
    clearOldVersion(version);
    var js = localStorage.getItem('cacheUrl:' + url);
    if (js) {
        callback(js);
    }
    else {
        ajax(url, function (js) {
            localStorage.setItem('cacheUrl:' + url, js);
            callback(js);
        }, true);
    }
}


function ajax(url, callback, async) {
    url += (url.indexOf('?') > 0 ? "&" : "?") + "_t=" + Math.random();
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.open('get', url, async);
    xmlhttp.send();
    if (async) {
        xmlhttp.onreadystatechange = function () {
            if (xmlhttp.readyState == 4) {
                if (xmlhttp.status == 200) {
                    callback(xmlhttp.responseText);
                }
                else {
                    alert('load failed:' + url);
                }
            }
        };
    }
    else {
        if (xmlhttp.readyState == 4) {
            if (xmlhttp.status == 200) {
                callback(xmlhttp.responseText);
            }
            else {
                alert('load failed:' + url);
            }
        }
    }
}