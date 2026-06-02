  document.querySelector('#connect').addEventListener('click', function() {
    if (isWebBluetoothEnabled()) { connect() }
  })

  document.querySelector('#start').addEventListener('click', function(event) {
    if (isWebBluetoothEnabled()) { start() }
  })

  document.querySelector('#stop').addEventListener('click', function(event) {
    if (isWebBluetoothEnabled()) { stop() }
  })

  var bleService = '6e400001-b5a3-f393-e0a9-e50e24dcca9e'//'00001800-0000-1000-8000-00805f9b34fb'
  var bleCharacteristic = '6e400003-b5a3-f393-e0a9-e50e24dcca9e'//'00002a00-0000-1000-8000-00805f9b34fb'
  var bluetoothDeviceDetected
  var gattCharacteristic
  const utf8decoder = new TextDecoder('utf-8');
  
  function isWebBluetoothEnabled() {
    if (!navigator.bluetooth) {
      logMessage('Web Bluetooth API is not available in this browser!')
      return false
    }

    return true
  }

  function getDeviceInfo() {
    let options = {
      optionalServices: [ bleService ],
      filters: [
            { services: [ bleService ] }
      ]
    }

    logMessage('Requesting any Bluetooth Device...')
    return navigator.bluetooth.requestDevice(options).then(device => {
      bluetoothDeviceDetected = device
    }).catch(error => {
      logMessage('Argh! ' + error)
    })
  }

  function connect() {
    return (bluetoothDeviceDetected ? Promise.resolve() : getDeviceInfo())
    .then(connectGATT)
    .then(_ => {
      if (gattCharacteristic.canRead)
      {
        logMessage('Reading measurement ...')
        return gattCharacteristic.readValue()
      }

      logMessage('Connected ...')
      return;
    })
    .catch(error => {
      logMessage('Waiting to start reading: ' + error)
    })
  }

  function connectGATT() {
    if (bluetoothDeviceDetected.gatt.connected && gattCharacteristic) {
      return Promise.resolve()
    }

    return bluetoothDeviceDetected.gatt.connect()
    .then(server => {
      logMessage('Getting GATT Service...')
      return server.getPrimaryService(bleService)
    })
    .then(service => {
      logMessage('Getting GATT Characteristic...')
      return service.getCharacteristic(bleCharacteristic)
    })
    .then(characteristic => {
      gattCharacteristic = characteristic
      gattCharacteristic.addEventListener('characteristicvaluechanged',
          handleChangedValue)
      document.querySelector('#start').disabled = false
      document.querySelector('#stop').disabled = true
    })
  }

  function handleChangedValue(event) {
    let value = event.target.value;
    const decodedText = utf8decoder.decode(value);

    var now = new Date();
    logMessage('> ' + now.getHours() + ':' + now.getMinutes() + ':' + now.getSeconds() + ' message is ' + decodedText);
  }

  function start() {
    gattCharacteristic.startNotifications()
    .then(_ => {
      logMessage('Start reading...')
      document.querySelector('#start').disabled = true
      document.querySelector('#stop').disabled = false
    })
    .catch(error => {
      logMessage('[ERROR] Start: ' + error)
    })
  }

  function stop() {
    gattCharacteristic.stopNotifications()
    .then(_ => {
      logMessage('Stop reading...')
      document.querySelector('#start').disabled = false
      document.querySelector('#stop').disabled = true
    })
    .catch(error => {
      logMessage('[ERROR] Stop: ' + error)
    })
  }

  function logMessage(message)
  {
    message += '\r\n';
    document.querySelector('#log').value += message;
  }