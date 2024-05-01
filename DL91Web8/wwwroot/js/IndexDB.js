var Idb = (function () {

    /**
     * 打开/创建数据库
     * @param {object} dbName 数据库的名字
     * @param {Array} storeNames 仓库名称
     * @param {string} version 数据库的版本
     * @return {object} 该函数会返回一个数据库实例
    */
    const openDB = function (dbName, version, storeNames) {
        return new Promise((resolve, reject) => {
            //  兼容浏览器
            let indexedDB =
                window.indexedDB ||
                window.mozIndexedDB ||
                window.webkitIndexedDB ||
                window.msIndexedDB
            let db = null
            const req = indexedDB.open(dbName, version)
            // 操作成功
            req.onsuccess = function (event) {
                db = event.target.result // 数据库对象
                resolve({ code: 0, success: true, data: db, msg: '数据库打开成功!' })
            }
            // 操作失败
            req.onerror = function (event) {
                resolve({ code: -1, success: false, data: null, msg: '数据库打开失败!' })
            }
            // 创建表和索引
            req.onupgradeneeded = function (event) {
                // 数据库创建或升级的时候会触发
                db = event.target.result // 数据库对象
                for (var i = 0; i < storeNames.length; i++) {
                    var storeName = storeNames[i];
                    if (!db.objectStoreNames.contains(storeName.name)) {
                        db.createObjectStore(storeName.name, { keyPath: storeName.keyPath || 'id', autoIncrement: storeName.autoIncrement }) // 创建表
                    }
                }
            }
        })
    }

    /**
     * 新增数据
     * @param {object} db 数据库实例
     * @param {string} storeName 仓库名称
     * @param {any} data 数据
     **/
    const addData = function (db, storeName, data) {
        return new Promise((resolve, reject) => {
            let req = db
                .transaction([storeName], 'readwrite')
                .objectStore(storeName) // 仓库对象
                .add(data)
            // 操作成功
            req.onsuccess = function (event) {
                console.log('数据写入成功')
                resolve({ code: 0, success: true, data: null, msg: '数据写入成功!' })
            }
            // 操作失败
            req.onerror = function (event) {
                console.log('数据写入失败')
                let data = { code: -1, success: false, data: null, msg: '数据写入失败!' }
                resolve(data)
            }
        })
    }

    /**
    * 更新数据
    * @param {object} db 数据库实例
    * @param {string} storeName 仓库名称
    * @param {object} data 数据
    */
    const updateData = function (db, storeName, data) {
        return new Promise((resolve, reject) => {
            const req = db
                        .transaction([storeName], 'readwrite')
                        .objectStore(storeName)
                        .put(data)
            // 操作成功
            req.onsuccess = function (event) {
                console.log('数据更新成功')
                resolve({ code: 0, success: true, data: null, msg: '数据更新成功!' })
            }
            // 操作失败
            req.onerror = function (event) {
                console.log('数据更新失败')
                let data = { code: -1, success: false, data: null, msg: '数据更新失败!' }
                resolve(data)
            }
        })
    }
    const deleteData = function (db, storeName, key) {
        return new Promise((resolve, reject) => {
            const req = db
                .transaction([storeName], 'readwrite')
                .objectStore(storeName)
                .delete(key)
            // 操作成功
            req.onsuccess = function (event) {
                resolve({ code: 0, success: true, data: null, msg: '' })
            }
            // 操作失败
            req.onerror = function (event) {
                let data = { code: -1, success: false, data: null, msg: '' }
                resolve(data)
            }
        })
    }
    const getData = function (db, storeName, key) {
        return new Promise((resolve, reject) => {
            const req = db
                        .transaction([storeName], 'readwrite')
                        .objectStore(storeName)
                        .get(key)
            // 操作成功
            req.onsuccess = function (event) {
                console.log('获取数据成功')
                resolve({ code: 0, success: true, data: req.result, msg: '获取数据成功!' })
            }
            // 操作失败
            req.onerror = function (event) {
                console.log('获取数据失败')
                let data = { code: -1, success: false, data: null, msg: '获取数据失败!' }
                resolve(data)
            }
        })
    }
    const getAllData = function (db, storeName) {
        return new Promise((resolve, reject) => {
            const req = db
                .transaction([storeName], 'readwrite')
                .objectStore(storeName)
                .openCursor()
            // 操作成功
            var result = [];
            req.onsuccess = function (event) {
                const cursor = req.result
                if (cursor) {
                    result.push(cursor.value);
                    cursor.continue();
                }
                else {
                    let data = { code: 0, success: true, data: result, msg: '' };
                    resolve(data);
                };
            }
            // 操作失败
            req.onerror = function (event) {
                let data = { code: -1, success: false, data: null, msg: '获取数据失败!' }
                resolve(data)
            }
        })
    }
    return {
        openDB: openDB,
        addData: addData,
        updateData: updateData,
        getData: getData,
        getAllData: getAllData,
        deleteData: deleteData
    }
})();

