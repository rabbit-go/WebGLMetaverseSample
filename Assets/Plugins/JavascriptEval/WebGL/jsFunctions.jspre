Module['JavaScriptArrayCopy'] = {
    JavaScriptSharedArrayCopy(dataArray) {
        if (dataSharedArray.length === 0) {
            _ReInit();
        }
        for (let index = 0; index < dataArray.length; index++) {
            dataSharedArray[index] = dataArray[index];
        }

    }
}