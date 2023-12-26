var ifile = (function () {

    window.requestFileSystem = window.requestFileSystem || window.webkitRequestFileSystem;
    var fs = null;

    function init() {
        return new Promise((resolve, reject) => {
            if (fs == null) {
                window.requestFileSystem(window.PERSISTENT, 5 * 1024 * 1024 * 1024, function (_fs) {
                    fs = _fs;
                    console.log('File system opened: ' + _fs.name);
                    resolve();
                }, function (err) {
                    console.log(err);
                });
            }
            else {
                resolve();
            }
        });
    }

    function createDic(directoryName) {
        return new Promise((resolve, reject) => {
            init().then(function () {
                fs.root.getDirectory(directoryName, { create: true },
                    // EYEEREDhAJIERE
                    function (direntry) {
                        console.log(direntry);
                        resolve(direntry);
                    });
            });
        });
    }

    function deleteDic(direntry) {
        return new Promise((resolve, reject) => {
            direntry.removeRecursively(function () {
                resolve();
            });
        });
    }

    function saveBlob(dirEntry, fileName, blob) {
        return new Promise((resolve, reject) => {
            dirEntry.getFile(fileName, { create: true }, function (fileEntry) {
                fileEntry.createWriter(function (fileWriter) {
                    fileWriter.onwriteend = function (e) {
                        console.log('Write completed.');
                        resolve();
                    };
                    fileWriter.onerror = function (e) {
                        console.log('Write failed: ' + e.toString());
                        reject();
                    };
                    fileWriter.write(blob);
                }, reject);
            }, reject);
        });
    }

    function readBlob(dirEntry, fileName) {
        return new Promise((resolve, reject) => {
            dirEntry.getFile(fileName, {}, function (fileEntry) {
                fileEntry.file(function (file) {
                    //var reader = new FileReader();
                    //reader.onloadend = function (e) {
                    //    resolve(this.result);
                    //};
                    resolve(file);
                }, function (a) {
                    resolve();
                });
            }, function (a) {
                resolve();
            });
        });
    }

    return {
        init: init,
        createDic: createDic,
        saveBlob: saveBlob,
        readBlob: readBlob,
        deleteDic: deleteDic
    }
})();

