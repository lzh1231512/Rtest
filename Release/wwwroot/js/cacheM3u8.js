/// <reference path="indexdb.js" />

var m3u8 = (function () {

    var dbName = "m3u8";
    var mainTable = "m3u8";
    var taskTable = "m3u8_task";
    var dataTable = "m3u8_data";
    var db = null;
    var taskChangeEvent = null;
    const openDb = async function () {
        if (!db) {
            db = (await Idb.openDB(dbName, 1, [{ name: mainTable, keyPath: 'id', autoIncrement: false },
            { name: taskTable, keyPath: 'id', autoIncrement: false },
            { name: dataTable, keyPath: 'id', autoIncrement: false }])).data;
            if (!db) {
                alert('m3u8数据库初始化失败');
            }
        }
    }

    const downloadM3u8 = async function (id, url) {
        await openDb();
        var exists = await Idb.getData(db, mainTable, id);
        if (exists.data != null) {
            return { code: -1, success: false, data: null, msg: '任务已存在!' };
        }
        var data = await download(url);
        if (data.data) {
            var info = data.data.split(/\n/g);
            var newInfo = [];
            var vIndex = 0;
            var path = url.substr(0, url.lastIndexOf('/') + 1);
            var tasks = [];
            for (var i = 0; i < info.length; i++) {
                var inf = info[i];
                if (inf == '') {
                    continue;
                }
                if (inf.indexOf('#') == 0) {
                    newInfo.push(inf);
                }
                else {
                    var vurl = inf.indexOf('http') == 0 ? inf : (path + inf);
                    newInfo.push('https://cachedx.' + id + '.' + vIndex + '/' + vurl);
                    tasks.push({
                        id: id + '#' + vIndex,
                        data: null,
                        url: vurl,
                        status: 0
                    });
                    vIndex++;
                }
            }
            await Idb.addData(db, mainTable, {
                id: id,
                url: url,
                count: tasks.length,
                downloaded: 0,
                m3u8: newInfo.join('\n')
            });
            for (var item in tasks) {
                await Idb.addData(db, taskTable, tasks[item]);
            }
            return { code: 0, success: true, data: null, msg: '' };
        }
        else {
            return { code: -1, success: false, data: null, msg: 'm3u8下载失败!' };
        }
    }

    const getM3u8Url = async function (id) {
        await openDb();
        var exists = await Idb.getData(db, mainTable, id);
        var m3u8blob = new Blob([exists.data.m3u8], { type: 'application/x-mpegURL' })
        return URL.createObjectURL(m3u8blob);
    }

    const initDownloadVideo = async function () {
        await openDb();
        var tasks = (await Idb.getAllData(db, taskTable)).data;
        for (var i in tasks) {
            tasks[i].status = 0;
            await Idb.updateData(db, taskTable, tasks[i]);
        }
    }

    const downloadVideo = async function () {
        await openDb();
        var tasks = (await Idb.getAllData(db, taskTable)).data;
        if (taskChangeEvent) {
            taskChangeEvent(tasks.length);
        }
        var dindex = 0;
        for (var i in tasks) {
            if (tasks[i].status == 0) {
                tasks[i].status = 1;
                await Idb.updateData(db, taskTable, tasks[i]);

                download(tasks[i].url, 'video/MP2T', tasks[i]).then(async function (result) {
                    if (result.success) {
                        result.task.status = 2;
                        result.task.data = result.data;
                        await Idb.deleteData(db, taskTable, result.task.id);
                        await Idb.addData(db, dataTable, result.task);
                    }
                    else {
                        result.task.status = -1;
                        await Idb.updateData(db, taskTable, result.task);
                    }
                    downloadVideo();
                });
            }
            dindex++;
            if (dindex >= 4) {
                break;
            }
        }
    }

    const initDownload = async function (taskChangeEventCallback) {
        taskChangeEvent = taskChangeEventCallback;
        await initDownloadVideo();
        var newflag = Math.random();
        sessionStorage.setItem('m3u8.m3u8downloadflag', newflag);
        var intervalId = setInterval(function () {
            var m3u8downloadflag = sessionStorage.getItem('m3u8.m3u8downloadflag');
            if (m3u8downloadflag == newflag) {
                downloadVideo();
            }
            else {
                clearInterval(intervalId);
            }
        }, 3000);
    }
    
    const download = function (url,type,task) {
        return new Promise((resolve, reject) => {
            let xhr = new XMLHttpRequest();
            xhr.open('get', url, true);  //url为地址链接
            //xhr.setRequestHeader('Content-Type', `application/${type}`);
            if (type == 'video/MP2T')
                xhr.responseType = "blob";
            xhr.onload = function () {
                if (this.status == 200) {
                    //const blob = new Blob([res], { type: `application/${type}` });
                    let data = { code: 0, success: true, data: this.response, task: task }
                    resolve(data)
                }
                else {
                    let data = { code: -1, success: false, data: null, task: task}
                    resolve(data)
                }
            }
            xhr.onerror = function () {
                let data = { code: -1, success: false, data: null, task: task }
                resolve(data)
            }
            xhr.send();
        })
    }

    const initXMLHttpRequest = function () {
        openDb();
        var oriXOpen = XMLHttpRequest.prototype.open;
        var oriXsend = XMLHttpRequest.prototype.send;
        XMLHttpRequest.prototype.open = function (method, url, asncFlag, user, password) {
            this.xurl = url;
            if (url.indexOf('https://cachedx.') != 0) {
                oriXOpen.call(this, method, url, asncFlag, user, password);
            }
        };
        async function process(_request,url) {
            var info = url.substr(16);
            var ind = info.indexOf('/');
            var oriUrl = info.substr(ind + 1);
            var inf = info.substr(0, ind).split('.');
            var data = (await Idb.getData(db, dataTable, inf[0] + '#' + inf[1])).data;
            if (data) {
                const blob = new Blob([data.data], { type: 'application/video/MP2T' });
                oriUrl = URL.createObjectURL(blob);
            }
            oriXOpen.call(_request, 'get', oriUrl, true);
            oriXsend.call(_request);
        }
        XMLHttpRequest.prototype.send = function (params) {
            var url = this.xurl;
            if (url.indexOf('https://cachedx.') == 0) {
                process(this, url);
            }
            else {
                oriXsend.call(this, params);
            }
        };
    }

    return {
        downloadM3u8: downloadM3u8,
        initXMLHttpRequest: initXMLHttpRequest,
        getM3u8Url: getM3u8Url,
        initDownload: initDownload
    }
})();
