var VERSION = 'v6';

// 缓存
self.addEventListener('install', function (event) {
    event.waitUntil(
        caches.open(VERSION).then(function (cache) {
            return cache.addAll([
                './',
                './js/loadJs.js',
                './fonts/fontawesome-webfont.woff?v=4.2.0'
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
    if (event.request.url.indexOf('.ts') < 0) {
        event.respondWith(
            caches.open(VERSION).then(function (cache) {
                return cache.match(event.request).then(function (response) {
                    if (response) {
                        return response;
                    }
                    return fetch(event.request).then(function (response) {
                        return response;
                    });
                });
            })
        );
    }
});