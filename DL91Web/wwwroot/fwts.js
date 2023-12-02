var VERSION = 'v2';

self.addEventListener('install', function (event) {
});

self.addEventListener('activate', function (event) {
});
async function _process(url) {
    var info = url.substr(16);
    var ind = info.indexOf('/');
    var oriUrl = info.substr(ind + 1);
    var inf = info.substr(0, ind).split('.');
    var dir = await ifile.createDic(inf[0]);
    var data = await ifile.readBlob(dir, inf[0] + '#' + inf[1]);
    if (data) {
        const blob = new Blob([data], { type: 'application/video/MP2T' });
        oriUrl = URL.createObjectURL(blob);
    }
    return oriUrl;
}
self.addEventListener('fetch', function (event) {
    var url = event.request.url;
    if (url.indexOf('https://cachedx.') == 0) {
        event.respondWith(
            process(url).then(function (oriUrl) {
                return fetch(oriUrl).then(function (response) {
                    return response;
                });
            })
        );
    }
});