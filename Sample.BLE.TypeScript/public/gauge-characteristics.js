  document.querySelector('#connect').addEventListener('click', function() {
    if (isWebBluetoothEnabled()) { connect() }
  })

  document.querySelector('#serviceSelect').addEventListener('change', function() {
    if (isWebBluetoothEnabled()) { getCharacteristics() }
  })

  var bleService = '6e400001-b5a3-f393-e0a9-e50e24dcca9e'
  var nameService = '00001800-0000-1000-8000-00805f9b34fb'
  var measurementCharacteristic = '6e400003-b5a3-f393-e0a9-e50e24dcca9e'//'00002a00-0000-1000-8000-00805f9b34fb'
  var detectedBluetoothDevice
  const utf8decoder = new TextDecoder('utf-8');
  const utf8encoder = new TextEncoder();
  const observerConfig = { childList: true, subtree: true };

  function isWebBluetoothEnabled() {
    if (!navigator.bluetooth) {
      logMessage('Web Bluetooth API is not available in this browser!')
      return false
    }

    return true
  }

  function getDeviceInfo() {
    let options = {
      optionalServices: [ bleService, nameService ],
      filters: [
            { services: [ bleService ] }
      ]
    }

    logMessage('Scanning for gauge...')
    return navigator.bluetooth.requestDevice(options).then(device => {
      detectedBluetoothDevice = device
    }).catch(error => {
      logMessage('Argh! ' + error)
    })
  }

  function connect() {
    return (detectedBluetoothDevice ? Promise.resolve() : getDeviceInfo())
    .then(connectGATT)
    .catch(error => {
      logMessage('Waiting to find services: ' + error)
    })
  }

  function connectGATT() {
    if (detectedBluetoothDevice.gatt.connected) {
      return Promise.resolve()
    }

    return detectedBluetoothDevice.gatt.connect()
    .then(server => {
      logMessage('Getting GATT Services...')
      return server.getPrimaryServices()
    })
    .then(services => {
      logMessage('Found ' + services.length + ' GATT services')
      var serviceSelect = document.querySelector('#serviceSelect')
      services.forEach(service => {
        var option = document.createElement('option');
        option.text = service.uuid;
        serviceSelect.add(option);
      });
      getCharacteristics();
    })
  }

  function getCharacteristics() {
    var serviceUuid = document.querySelector('#serviceSelect').value;
    return detectedBluetoothDevice.gatt.connect()
    .then(server => {
      logMessage('Getting GATT Service...')
      return server.getPrimaryService(serviceUuid)
    })
    .then(service => {
      logMessage('Found GATT service... ' + service.uuid)
      return service.getCharacteristics();
    })
    .then(characteristics => {
      var characteristicsContainer = document.querySelector('#characteristics');
      while (characteristicsContainer.hasChildNodes()) 
      { 
        characteristicsContainer.removeChild(characteristicsContainer.firstChild); 
      }

      characteristics.forEach(characteristic => {
        logMessage('Characteristic ' + characteristic.uuid + ' canRead=' + characteristic.properties.read + ' canWrite=' + characteristic.properties.write + ' canNotify=' + characteristic.properties.notify);
        var characteristicDiv = document.createElement('div');
        var header = document.createElement('h3');
        header.innerHTML = 'Characteristic';
        characteristicDiv.appendChild(header);
        var subheader = document.createElement('h3');
        subheader.innerHTML = characteristic.uuid;
        characteristicDiv.appendChild(subheader);
        if (characteristic.properties.read)
        {
          characteristic.readValue().then(value => {
            var readValue = document.createElement('p');
            readValue.innerHTML = 'Value: ' + utf8decoder.decode(value);
            characteristicDiv.appendChild(readValue);
          });
        }

        if (characteristic.properties.write)
        {
            var writeValue = document.createElement('input');
            characteristicDiv.appendChild(writeValue);
            var sendButton = document.createElement('button');
            sendButton.innerText = "Write value";
            sendButton.addEventListener('click', function() {
              characteristic.writeValue(writeValue.innerText);
            })
            characteristicDiv.appendChild(sendButton);
        }

        if (characteristic.properties.notify)
        {
          var readValue = document.createElement('p');
          readValue.innerHTML = 'Value:';
          characteristicDiv.appendChild(readValue);
          const valueChangedHandler = characteristicValueChangedHandler(readValue)

          // Add event handler for notifications
          characteristic.addEventListener('characteristicvaluechanged', valueChangedHandler);

          // Ensure event handler for notifications is removed when this characteristic element is removed
          ensureValueChangedHandlerIsTidiedUp(characteristicDiv, characteristic, valueChangedHandler, characteristicsContainer)

          characteristic.startNotifications();
        }

        characteristicsContainer.appendChild(characteristicDiv);
      });
    })

    function characteristicValueChangedHandler(readValue) {
      return function (event) {
        let decodedText = utf8decoder.decode(event.target.value)
        var now = new Date()
        logMessage('> ' + now.getHours() + ':' + now.getMinutes() + ':' + now.getSeconds() + ' message is ' + decodedText)
        readValue.innerHTML = 'Value: ' + decodedText
      }
    }

    function ensureValueChangedHandlerIsTidiedUp(characteristicDiv, characteristic, valueChangedHandler, characteristicsContainer) {
      const containerChange = function (mutationsList, observer) {
        mutationsList.forEach((mutation) => {
          if (mutation.type === 'childList') {
            for (let node of mutation.removedNodes) {
              if (node === characteristicDiv) {
                characteristic.removeEventListener('characteristicvaluechanged', valueChangedHandler)
              }
            }
          }
        })
      }

      const observer = new MutationObserver(containerChange)
      observer.observe(characteristicsContainer, observerConfig)
    }
  }

  function logMessage(message)
  {
    message += '\r\n';
    document.querySelector('#log').value += message;
  }