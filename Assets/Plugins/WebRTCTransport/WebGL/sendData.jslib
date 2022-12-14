var LibrarysebdDataWebGL = {
    byteOffset: Number,
    length: Number,
    InitJavaScriptSharedArray: function (_byteOffset, _length) {
        byteOffset = _byteOffset;
        length = _length;
        dataSharedArray = new Uint8Array(buffer, byteOffset, length);
    },
    $JavaScriptSharedArrayCopy: function(dataArray) {
        if (dataSharedArray.length === 0) {
            dataSharedArray = new Uint8Array(buffer, byteOffset, length);
        }
        for (let index = 0; index < dataArray.length; index++) {
            dataSharedArray[index] = dataArray[index];
        }

    }
};
autoAddDeps(LibrarysebdDataWebGL, '$JavaScriptSharedArrayCopy');
mergeInto(LibraryManager.library, LibrarysebdDataWebGL);
