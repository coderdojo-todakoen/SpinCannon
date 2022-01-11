var microbitPlugin = {
    // 接続中のBluetoothRemoteGATTServer
    $bluetoothServer: null,

    $accelerometerDataX: 0,
    $accelerometerDataY: 0,
    $buttonAState: 0,
    $buttonBState: 0,

    // Bluetoothデバイスに接続します
    ConnectMicrobit: function () {
        console.log('>>> connect');

        const ACCELEROMETER_SERVICE_UUID = 'e95d0753-251d-470a-a062-fa1922dfa9a8';
        const ACCELEROMETER_DATA_CHARACTERISTIC_UUID = 'e95dca4b-251d-470a-a062-fa1922dfa9a8';
        const ACCELEROMETER_PERIOD_CHARACTERISTIC_UUID = 'e95dfb24-251d-470a-a062-fa1922dfa9a8';
        const BUTTON_SERVICE_UUID = 'e95d9882-251d-470a-a062-fa1922dfa9a8';
        const BUTTON_A_STATE_CHARACTERISTIC_UUID = 'e95dda90-251d-470a-a062-fa1922dfa9a8';
        const BUTTON_B_STATE_CHARACTERISTIC_UUID = 'e95dda91-251d-470a-a062-fa1922dfa9a8';
    
        var accelerometerService = null;
        var buttonService = null;
    
        // Bluetoothデバイスを取得します
        const options = {
            filters: [
                { namePrefix: 'BBC micro:bit' }
            ],
            optionalServices: [ ACCELEROMETER_SERVICE_UUID, BUTTON_SERVICE_UUID ]
        };
        navigator.bluetooth.requestDevice(options)
            .then(function (device) {
                console.log('id:' + device.id);
                console.log('name:' + device.name);

                // 接続が切れたら通知を受け取ります
                device.addEventListener('gattserverdisconnected', function (e) {
                    console.log('gattserverdisconnected');
                });

                // デバイスに接続します
                return device.gatt.connect();
            })
            .then(function (server) {
                console.log('connected.');

                bluetoothServer = server;

                // 加速度計サービスを取得します
                return bluetoothServer.getPrimaryService(ACCELEROMETER_SERVICE_UUID);
            })
            .then(function (service) {
                console.log('getPrimaryService');

                accelerometerService = service;
                return accelerometerService.getCharacteristic(ACCELEROMETER_PERIOD_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                // 加速度計の値の取得間隔を設定します
                const period = new Uint16Array([20]);
                return characteristic.writeValue(period);
            })
            .then(function () {
                console.log('writeValue');

                return accelerometerService.getCharacteristic(ACCELEROMETER_DATA_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                // 加速度計の値の取得を開始します
                return characteristic.startNotifications();
            })
            .then(function (characteristic) {
                console.log('startNotifications');

                // 加速度計の値を受け取ります
                characteristic.addEventListener('characteristicvaluechanged', function (ev) {
                    const value = ev.target.value;
                    const x = value.getInt16(0, true);
                    const y = value.getInt16(2, true);

                    accelerometerDataX = 0.8 * accelerometerDataX + 0.2 * x;
                    accelerometerDataY = 0.8 * accelerometerDataY + 0.2 * y;
                });

                // ボタンサービスを取得します
                return bluetoothServer.getPrimaryService(BUTTON_SERVICE_UUID);
            })
            .then(function (service) {
                console.log('getPrimaryService');

                buttonService = service;
                return buttonService.getCharacteristic(BUTTON_A_STATE_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                // ボタンAの通知の取得を開始します
                return characteristic.startNotifications();
            })
            .then(function (characteristic) {
                console.log('startNotifications');

                // ボタンAの通知を受け取ります
                characteristic.addEventListener('characteristicvaluechanged', function (ev) {
                    const value = ev.target.value;
                    buttonAState = value.getUint8();
                });

                return buttonService.getCharacteristic(BUTTON_B_STATE_CHARACTERISTIC_UUID);
            })
            .then(function (characteristic) {
                console.log('getCharacteristic');

                // ボタンBの通知の取得を開始します
                return characteristic.startNotifications();
            })
            .then(function (characteristic) {
                console.log('startNotifications');

                // ボタンBの通知を受け取ります
                characteristic.addEventListener('characteristicvaluechanged', function (ev) {
                    const value = ev.target.value;
                    buttonBState = value.getUint8();
                });
            })
            .catch(function (err) {
                console.log('err:' + err);

                DisconnectMicrobit();
            });

        console.log('<<< connect');
    },

    // Bluetoothデバイスを切断します
    DisconnectMicrobit: function () {
        console.log('>>> disconnect');

        if (bluetoothServer) {
            console.log('device:' + bluetoothServer.device);
            if (bluetoothServer.device.gatt.connected) {
                // デバイスに接続中なら切断します
                bluetoothServer.device.gatt.disconnect();
            }
            bluetoothServer = null;
        }

        console.log('<<< disconnect');
    },

    GetAccelerometerDataX: function() {
        return accelerometerDataX;
    },

    GetAccelerometerDataY: function() {
        return accelerometerDataY;
    },
    
    GetButtonAState: function() {
        return buttonAState;
    },
    
    GetButtonBState: function() {
        return buttonBState;
    }
};
autoAddDeps(microbitPlugin, '$bluetoothServer');
autoAddDeps(microbitPlugin, '$accelerometerDataX');
autoAddDeps(microbitPlugin, '$accelerometerDataY');
autoAddDeps(microbitPlugin, '$buttonAState');
autoAddDeps(microbitPlugin, '$buttonBState');
mergeInto(LibraryManager.library, microbitPlugin);