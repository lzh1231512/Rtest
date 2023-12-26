var qbi_request = (function () {
    var originalFetch = window.fetch;
    var originalRequest = window.Request;

    // Request实例取不到body参数
    function Request(input, init) {
        this.input = input;
        this.init = init;

        this.req = new originalRequest(input, init);

        for (var key in this.req) {
            this[key] = this.req[key];
        }
    }

    window.Request = Request;

    var interceptor_req = {};
    var interceptor_res = {};

    function getPath(url) {
        return url.split("?")[0]
    }

    function hijackFetch() {
        var request = null
        if (typeof arguments[0] === 'string') {
            request = new Request(arguments[0], arguments[1] || {})
        } else {
            request = new Request(arguments[0].input, arguments[0].init)
        }
        console.log(request.input);
        $('#divTest').append(request.input + '<br/>');
        var path = getPath(request.input)
        if (interceptor_req[path]) {
            // 清空队列，避免跳转页面后，重复执行callback
            // 这里的循环是请求拦截的关键代码，循环执行拦截器的回调方法，修改request
            while (fn = interceptor_req[path].shift()) {
                request = fn(request)
            }
        }

        return new Promise((resolve, reject) => {
            originalFetch(request.input, request.init)
                .then((res) => {
                    var path = getPath(request.input)
                    if (interceptor_res[path]) {
                        // 这里的循环是响应拦截的关键代码，循环执行拦截器的回调方法，修改res
                        while (fn = interceptor_res[path].shift()) {
                            res = fn(res)
                        }
                    }

                    resolve(res);
                })
                .catch((err) => {
                    reject(err);
                });
        });
    }

    hijackFetch.interceptor = {
        request: {
            use: function (url, callback) {
                if (interceptor_req[url]) {
                    interceptor_req[url].push(callback)
                } else {
                    interceptor_req[url] = [callback]
                }
            },
        },
        response: {
            use: function (url, callback) {
                if (interceptor_res[url]) {
                    interceptor_res[url].push(callback)
                } else {
                    interceptor_res[url] = [callback]
                }
            },
        },
    };

    window.fetch = hijackFetch;

    return {
        hijackFetch: hijackFetch,
    };
})();

