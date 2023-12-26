var ifile = (function () {
    
    var fs = null;

    function init() {
        return new Promise((resolve, reject) => {
            if (fs == null) {
                window.showDirectoryPicker({ mode: 'readwrite' }).then(function (dirHandle) {
                    if (dirHandle) {
                        fs = dirHandle;
                        resolve();
                    }
                    else {
                        reject();
                    }
                });
            }
            else {
                resolve();
            }
        });
    }

    async function createDic(directoryName) {
        return await fs.getDirectoryHandle(directoryName, {
            create: true,
        });
    }

    async function deleteDic(direntry) {
        await direntry.remove({ recursive: true });
    }

    async function saveBlob(dirEntry, fileName, blob) {
        const newFileHandle = await dirEntry.getFileHandle(fileName, { create: true });
        const writable = await newFileHandle.createWritable();
        // Write the contents of the file to the stream.
        await writable.write(blob);
        // Close the file and write the contents to disk.
        await writable.close();
    }

    async function readBlob(dirEntry, fileName) {
        try {
            const newFileHandle = await dirEntry.getFileHandle(fileName, { create: false });
            return await newFileHandle.getFile();
        }
        catch{
            return null;
        }
    }

    return {
        init: init,
        createDic: createDic,
        saveBlob: saveBlob,
        readBlob: readBlob,
        deleteDic: deleteDic
    }
})();

