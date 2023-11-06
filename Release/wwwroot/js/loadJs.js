function loadjs(urls, callback) {
    if (urls.length == 0) {
        if (callback)
            callback();
        return;
    }
    var nexturl = urls.shift();
    var js = localStorage.getItem(nexturl);
    if (js) {
        eval(js);
        loadjs(urls, callback);
    }
    else {
        ajax(nexturl, function (js) {
            localStorage.setItem(nexturl, js);
            eval(js);
            loadjs(urls, callback);
        },false);
    }
}
function loadStyle(urls) {
    if (urls.length == 0) {
        return;
    }
    var nexturl = urls.shift();
    var js = localStorage.getItem(nexturl);
    if (js) {
        const style = document.createElement('style');
        style.innerHTML = js;
        document.head.appendChild(style);
        loadStyle(urls);
    }
    else {
        ajax(nexturl, function (js) {
            localStorage.setItem(nexturl, js);
            const style = document.createElement('style');
            document.head.appendChild(style);
            style.innerHTML = js;
            loadStyle(urls);
        },true);
    }
}
function ajax(url, callback,async) {
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