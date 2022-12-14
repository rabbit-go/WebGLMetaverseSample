var LibrarysebdDataWebGL = {
    byteOffset: Number,
    length: Number,
    InitJavaScriptSharedArray: function (_byteOffset, _length) {
        byteOffset = _byteOffset;
        length = _length;
        dataSharedArray = new Uint8Array(buffer, byteOffset, length);
    },
    $ReInit: function () {
        dataSharedArray = new Uint8Array(buffer, byteOffset, length);
    },
};
autoAddDeps(LibrarysebdDataWebGL, '$ReInit');
mergeInto(LibraryManager.library, LibrarysebdDataWebGL);
