/// <reference path="indexdb.js" />
/// <reference path="fileapi.js" />


var m3u8 = (function () {
    var dbName = "m3u8";
    var mainTable = "m3u8";
    var taskTable = "m3u8_task";
    var dataTable = "m3u8_data";
    var db = null;
    var taskChangeEvent = null;
    var opflagAdd = function () {
        var opflag = parseInt(localStorage.getItem('m3u8.opflag') || 0);
        localStorage.setItem('m3u8.opflag', opflag + 1);
        localStorage.setItem('m3u8.opflagTime', (new Date()).getTime());
    };
    var opflagSub = function () {
        var opflag = parseInt(localStorage.getItem('m3u8.opflag') || 0);
        localStorage.setItem('m3u8.opflag', opflag - 1);
        localStorage.setItem('m3u8.opflagTime', (new Date()).getTime());
    };
    var opflagGet = function () {
        var time = localStorage.getItem('m3u8.opflagTime');
        if (time && (new Date()).getTime() - parseInt(time) > 30000) {
            localStorage.setItem('m3u8.opflag', 0);
        }
        return parseInt(localStorage.getItem('m3u8.opflag') || 0);
    };
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
    const deleteM3u8 = async function (id, progressCallback) {
        opflagAdd();
        await openDb();
        await Idb.deleteData(db, mainTable, id);
        var dt = new Date();
        var tasks = (await Idb.getAllData(db, taskTable)).data;
        var taskids = [];
        for (var i in tasks) {
            if (tasks[i].id.indexOf(id + '#') == 0) {
                taskids.push(tasks[i].id);
            }
        }

        var count = 0;
        var tt = taskids.length;
        for (var i in taskids) {
            await Idb.deleteData(db, taskTable, taskids[i]);
            count++;
            if (progressCallback) {
                progressCallback(count + "/" + tt);
            }
        }
        if (progressCallback) {
            progressCallback(0);
        }
        var dir = await ifile.createDic(id + '');
        await ifile.deleteDic(dir);
        console.log('delete ' + ((new Date()).getTime() - dt.getTime()));
        opflagSub();
    }
    const downloadM3u8 = async function (mdt, url, progressCallback) {
        var id = mdt.id;
        await openDb();
        var exists = await Idb.getData(db, mainTable, id);
        if (exists.data != null) {
            return { code: -1, success: false, data: null, msg: '任务已存在!' };
        }
        var dt = new Date();
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
            opflagAdd();
            mdt.url = url;
            mdt.count = tasks.length;
            mdt.downloaded = 0;
            mdt.m3u8 = newInfo.join('\n');
            await Idb.addData(db, mainTable, mdt);
            var count = 0;
            for (var item in tasks) {
                await Idb.addData(db, taskTable, tasks[item]);
                count++;
                if (progressCallback) {
                    progressCallback(count + "/" + tasks.length);
                }
            }
            if (progressCallback) {
                progressCallback(0);
            }
            console.log('add ' + ((new Date()).getTime() - dt.getTime()));
            opflagSub();
            return { code: 0, success: true, data: null, msg: '' };
        }
        else {
            return { code: -1, success: false, data: null, msg: 'm3u8下载失败!' };
        }
    }


    const getImgUrl = async function (id, url) {
        var dir = await ifile.createDic(id + '');
        var exists = await ifile.readBlob(dir, id + '#img'); //(await Idb.getData(db, dataTable, id + '#img')).data;
        if (exists) {
            var m3u8blob = new Blob([exists], { type: 'image/Jpeg' })
            return URL.createObjectURL(m3u8blob);
        }
        var imgdata = await download(url, 'video/MP2T');
        if (imgdata.data) {
            ifile.saveBlob(dir, id + '#img', imgdata.data);
            var m3u8blob = new Blob([imgdata.data], { type: 'image/Jpeg' })
            return URL.createObjectURL(m3u8blob);
        }
    }

    const getM3u8Url = async function (id) {
        await openDb();
        var exists = await Idb.getData(db, mainTable, id);
        if (exists.data) {
            var m3u8blob = new Blob([exists.data.m3u8], { type: 'application/x-mpegURL' })
            return URL.createObjectURL(m3u8blob);
        }
        else {
            return null;
        }
    }

    const initDownloadVideo = async function () {
        await openDb();
        var tasks = (await Idb.getAllData(db, taskTable)).data;
        for (var i in tasks) {
            tasks[i].status = 0;
            await Idb.updateData(db, taskTable, tasks[i]);
        }
    }
    var dcount = 0;
    var isDownloadIng = 0;
    const downloadVideo = async function () {
        console.log('opflag:' + opflagGet());
        if (isDownloadIng != 0 || opflagGet() > 0)
            return;
        isDownloadIng = 1;
        await openDb();
        var tasks = (await Idb.getAllData(db, taskTable)).data;
        if (taskChangeEvent) {
            taskChangeEvent(tasks.length);
        }
        var downloaded = 0;
        for (var i in tasks) {
            if (tasks[i].status == 0) {
                tasks[i].status = 1;
                await Idb.updateData(db, taskTable, tasks[i]);
                dcount++;
                download(tasks[i].url, 'video/MP2T', tasks[i]).then(async function (result) {
                    if (result.success) {
                        result.task.status = 2;
                        result.task.data = result.data;
                        await Idb.deleteData(db, taskTable, result.task.id);
                        var id = result.task.id.substr(0, result.task.id.indexOf('#'));
                        var dir = await ifile.createDic(id);
                        //await Idb.addData(db, dataTable, result.task);
                        await ifile.saveBlob(dir, result.task.id, result.data);
                    }
                    else {
                        result.task.status = -1;
                        await Idb.updateData(db, taskTable, result.task);
                    }
                    dcount--;
                    downloaded++;
                });
            }
            if (dcount >= 4) {
                await waitdownload(4);
                if (taskChangeEvent) {
                    taskChangeEvent(tasks.length - downloaded);
                }
            }
        }
        await waitdownload(1);
        if (taskChangeEvent) {
            taskChangeEvent(tasks.length - downloaded);
        }
        isDownloadIng = 0;
    }

    const waitdownload = function (wcount) {
        return new Promise((resolve, reject) => {
            var intervalId = setInterval(function () {
                if (dcount < wcount) {
                    clearInterval(intervalId);
                    resolve();
                }
            }, 100);
        })
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

    const download = function (url, type, task) {
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
                    let data = { code: -1, success: false, data: null, task: task }
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
        async function process(_request, url) {
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

    const initFetch = function () {

    }

    const getCachedList = async function () {
        await openDb();
        return (await Idb.getAllData(db, mainTable)).data;
    }

    const updateCache = async function (data) {
        await openDb();
        await Idb.updateData(db, mainTable, data);
    }


    return {
        downloadM3u8: downloadM3u8,
        deleteM3u8: deleteM3u8,
        initXMLHttpRequest: initXMLHttpRequest,
        getM3u8Url: getM3u8Url,
        initDownload: initDownload,
        getCachedList: getCachedList,
        getImgUrl: getImgUrl,
        updateCache: updateCache
    }
})();
