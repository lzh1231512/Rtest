var VERSION = '{version}';

// 缓存
self.addEventListener('install', function (event) {
    event.waitUntil(
        caches.open(VERSION).then(function (cache) {
            return cache.addAll([
                './'
            ]);
        })
    );
});

// 缓存更新
self.addEventListener('activate', function (event) {
    event.waitUntil(
        caches.keys().then(function (cacheNames) {
            return Promise.all(
                cacheNames.map(function (cacheName) {
                    // 如果当前版本和缓存版本不一致
                    if (cacheName !== VERSION) {
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});

// 捕获请求并返回缓存数据
self.addEventListener('fetch', function (event) {
    if (event.request.method == 'GET' && event.request.url.indexOf(this.location.origin) == 0) {
        var url = event.request.url.indexOf('?') > 0 ?
            event.request.url.substr(0, event.request.url.indexOf('?'))
            : event.request.url;
        console.log('url:' + url);
        if (url == this.location.origin ||
            url == this.location.origin + '/' ||
            url.lastIndexOf('.js') == url.length - 3 ||
            url.lastIndexOf('.css') == url.length - 4 ||
            url.lastIndexOf('.woff') == url.length - 5 ||
            url.lastIndexOf('.html') == url.length - 5) {
            event.respondWith(
                caches.open(VERSION).then(function (cache) {
                    return cache.match(event.request).then(function (response) {
                        if (response) {
                            console.log('matched:' + url);
                            return response;
                        }
                        return fetch(event.request).then(function (response) {
                            console.log('fetched:' + url);
                            cache.put(event.request, response.clone());
                            return response;
                        });
                    });
                })
            );
        }
    }
});