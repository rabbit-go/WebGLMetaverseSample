var LibrarySoraWebRTC = {
    $isHost: false,
    $isOnceHostInit: false,
    ///dataプロトコル
    ///index = 0: 0の時はConnection要求，1の時はconnectionID受け渡し，2の時はdisconnect，３の時はData受け渡し
    //それ以降: Data(初めのconnectionID受け渡し時はDataなし)
    EnterRoom: function (channelId_UTF) {
        const channelId = UTF8ToString(channelId_UTF);
        const debug = true;
        const sora = Sora.connection(["wss://0001.canary.sora-labo.shiguredo.app/signaling",
            "wss://0002.canary.sora-labo.shiguredo.app/signaling",
            "wss://0003.canary.sora-labo.shiguredo.app/signaling"], debug);
        const metadata = {
            access_token: "7AYObMrZT2bnOHvzbDed-ZYmkhu0uYpe96xE7DbRB3KQV71NUlYNk1j9tNyEX_VV"
        };
        const options = {
            multistream: true,
            dataChannelSignaling: true,
            dataChannels: [
                {
                    label: "#telepathy",
                    direction: "sendrecv",
                },
            ],
        };
        const parent = document.createElement('div');
        parent.id = "VoiceParent";
        navigator.mediaDevices
            .getUserMedia({ audio: true })
            .then((stream) => {
                let connection = sora.sendrecv(channelId + "@rabbit-go#10007775", metadata, options);
                window.soraConnection = connection;
                Sora_Setting(soraConnection);
                connection.connect(stream);
            })
            .catch((e) => {
                let connection = sora.recvonly(channelId + "@rabbit-go#10007775", metadata, options);
                window.soraConnection = connection;
                Sora_Setting(soraConnection);
                connection.connect();
            });
    },
    $Sora_Setting: function (connection) {
        connection.on("track", (event) => {
            if (event.track.kind == 'audio') {
                const audio = document.createElement('audio');
                audio.id = "audio-" + event.track.id;
                audio.srcObject = event.streams[0];
                audio.controls = true;
                audio.autoplay = true;
                audio.parent = parent;
            }
        });
        connection.on("notify", (message) => {
            if (message.event_type == "connection.created") {
                if (message.channel_connections == 1) {
                    console.log("I'm Host");
                    isHost = true;
                }
            }
        });
        connection.on("datachannel", (message) => {
            if (isOnceHostInit) return;
            isOnceHostInit = true;
            if (isHost) {
                unityInstance.SendMessage("NetworkManager", "StartHost");
            }
            else {
                unityInstance.SendMessage("NetworkManager", "StartClient");
            }
        });
        connection.on("message", (message) => {
            let dataArray = new Uint8Array(message.data);
            Module['JavaScriptArrayCopy'].JavaScriptSharedArrayCopy(dataArray);
            if (isHost) {
                unityInstance.SendMessage("NetworkManager", "ServerDataReceived", dataArray.length);
            }
            else {
                unityInstance.SendMessage("NetworkManager", "ClientDataReceived", dataArray.length);
            }
        });
        connection.on("removetrack", (event) => {
            parent.removeChild(document.getElementById("audio-" + event.track.id));
        });
    },
    Send_Data: function (dataPtr, len) {
        var startIndex = dataPtr / HEAPU8.BYTES_PER_ELEMENT;
        var byteData = HEAPU8.subarray(startIndex, startIndex + len);
        window.soraConnection.sendMessage("#telepathy", byteData);
    },
    ExitRoom: function () {
        window.soraConnection.disconnect();
        window.soraConnection = null;
        isOnceHostInit = false;
    }
};
autoAddDeps(LibrarySoraWebRTC, '$isHost');
autoAddDeps(LibrarySoraWebRTC, '$isOnceHostInit');
autoAddDeps(LibrarySoraWebRTC, '$Sora_Setting');
mergeInto(LibraryManager.library, LibrarySoraWebRTC);