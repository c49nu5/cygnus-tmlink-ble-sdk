//import { BinaryReader, BinaryWriter } from "@bufbuild/protobuf/wire";
// import { create } from "../node_modules/@bufbuild/protobuf/dist/cjs/create"
import type { BinaryReader } from "@bufbuild/protobuf/wire";
import { Command, CommandType, commandTypeFromJSON, commandTypeToJSON, ErrorCodes, FrozenLiveMeasurement, LiveMeasurementType, Message, Message_Record, NotifyLiveMeasurement, NotifyMessage } from "./Protos/cyg_tml_api_v1"

export class CygnusBLE {
  private bleServiceId = 'a495ff20-c5b1-4b44-b512-1370f02d74de';
  private TMLinkServiceId = 'de670901-8025-4c69-a40e-eccd60563713'
  private TMLinkCommandCharacteristicId = 'de670902-8025-4c69-a40e-eccd60563713'
  private TMLinkNotificationCharacteristicId = 'de670903-8025-4c69-a40e-eccd60563713'
  private TMLinkReadCharacteristicId = 'de670904-8025-4c69-a40e-eccd60563713'
  private TMLinkLiveCharacteristicId = 'de670906-8025-4c69-a40e-eccd60563713'
  private TMLinkFrozenCharacteristicId = 'de670907-8025-4c69-a40e-eccd60563713'
  private deviceInformationServiceId = '0000180a-0000-1000-8000-00805f9b34fb'
  private genericAccessServiceId = '00001800-0000-1000-8000-00805f9b34fb'//'00002a00-0000-1000-8000-00805f9b34fb'
  private detectedBluetoothDevice: BluetoothDevice | undefined
  private utf8decoder: TextDecoder = new TextDecoder('utf-8');

  public async serializeCommand(command: Command): Promise<BufferSource | null> {
    const cs = new CompressionStream("gzip");

    // Write data to be compressed
    var writer = Command.encode(command);
    var bytes = writer.finish();
    const zipwriter = cs.writable.getWriter();
    zipwriter.write(bytes);
    zipwriter.close();

    // Read compressed data
    const reader = cs.readable.getReader();
    let done = false;
    let output = [];
    while (!done) {
      const result = await reader.read();
      if (result.value) {
        output.push(...result.value);
      }
      done = result.done;
    }

    return new Uint8Array(output);
  }

  public deserializeNotifyReady(bytes: Uint8Array<ArrayBuffer>): NotifyMessage {
    var record = NotifyMessage.decode(bytes);
    return record;
  }

  public async deserializeMessage<T>(bytes: BufferSource, decode: (input: BinaryReader | Uint8Array, length?: number) => T): Promise<T | null> {
    const ds = new DecompressionStream("gzip");
    const writer = ds.writable.getWriter();
    writer.write(bytes);

    // Read the uncompressed chunks
    const reader = ds.readable.getReader();
    let readresult = await reader.read();
    if (readresult.value) {
      var record = decode(readresult.value);
      return record;
    }

    return null;
  }

  public deserializeNotifyLiveMeasurement(bytes: Uint8Array<ArrayBuffer>): NotifyLiveMeasurement {
    var record = NotifyLiveMeasurement.decode(bytes);
    return record;
  }

  public isWebBluetoothEnabled() {
    if (!navigator.bluetooth) {
      this.logMessage('Web Bluetooth API is not available in this browser!')
      return false
    }

    return true
  }

  public connect() {
    if (this.isWebBluetoothEnabled()) {
      return (this.detectedBluetoothDevice ? Promise.resolve() : this.getDeviceInfo())
        .then(_ => this.connectGATT())
        .catch(error => {
          this.logMessage('Waiting to find services: ' + error)
        })
    }
  }

  public getCharacteristics() {
    var serviceSelect = document.querySelector('#serviceSelect') as HTMLSelectElement
    var serviceUuid = serviceSelect.value;
    return this.detectedBluetoothDevice!.gatt!.connect()
      .then(server => {
        this.logMessage('Getting GATT Service...')
        return server.getPrimaryService(serviceUuid)
      })
      .then(service => {
        this.logMessage('Found GATT service... ' + service.uuid)
        return service.getCharacteristics();
      })
      .then(characteristics => {
        this.logMessage('Found ' + characteristics.length + ' GATT characteristics')
        var characteristicsContainer = document.querySelector('#characteristics') as HTMLDivElement;
        while (characteristicsContainer.hasChildNodes()) {
          characteristicsContainer.removeChild(characteristicsContainer.firstChild!);
        }

        characteristics.forEach(characteristic => {
          this.displayCharacteristic(characteristic, characteristicsContainer);
        });
      })
  }

  private displayCharacteristic(characteristic: BluetoothRemoteGATTCharacteristic, characteristicsContainer: HTMLDivElement) {
    this.logMessage('Characteristic ' + characteristic.uuid + ' canRead=' + characteristic.properties.read + ' canWrite=' + characteristic.properties.write + ' canNotify=' + characteristic.properties.notify);

    var characteristicDiv = document.createElement('div');
    if (characteristic.uuid != this.TMLinkReadCharacteristicId &&
      characteristic.uuid != this.TMLinkNotificationCharacteristicId &&
      characteristic.uuid != this.TMLinkLiveCharacteristicId &&
      characteristic.uuid != this.TMLinkFrozenCharacteristicId) {
      var header = document.createElement('h3');
      header.innerHTML = 'Characteristic ' + characteristic.uuid;
      characteristicDiv.appendChild(header);
      let readValue = document.createElement('p');
      readValue.id = characteristic.uuid;
      if (characteristic.properties.read) {
        this.displayCharacteristicValue(characteristic, readValue);
        characteristicDiv.appendChild(readValue);
      }
    }

    if (characteristic.uuid == this.TMLinkReadCharacteristicId) {
      var header = document.createElement('h3');
      header.innerHTML = 'Characteristic ' + characteristic.uuid;
      characteristicDiv.appendChild(header);

      var outputContainer = document.createElement('div');
      outputContainer.className = "outputs";
      appendOutput(outputContainer, "gaugeInfo", "Gauge Info");
      appendOutput(outputContainer, "recordList", "Records");
      appendOutput(outputContainer, "record", "Measurements");
      appendOutput(outputContainer, "live", "Live");
      characteristicDiv.appendChild(outputContainer);
    }

    if (characteristic.properties.write) {
      if (characteristic.uuid == this.TMLinkCommandCharacteristicId) {
        var commandSelect = document.createElement('select') as HTMLSelectElement;
        function AddCommand(commandType: CommandType, title: string) {
          var option = document.createElement('option');
          option.text = title;
          option.value = commandTypeToJSON(commandType);
          commandSelect!.add(option);
        }
        AddCommand(CommandType.GetGaugeInfo, "Get Gauge Info");
        AddCommand(CommandType.GetRecordList, "Get Record List");
        AddCommand(CommandType.GetRecord, "Get Record");
        characteristicDiv.appendChild(commandSelect);
        var writeValue = document.createElement('input') as HTMLInputElement;
        characteristicDiv.appendChild(writeValue);
        var sendButton = document.createElement('button');
        sendButton.innerText = "Send command";
        sendButton.addEventListener('click', () => {
          let command = Command.create();
          command.commandType = commandTypeFromJSON(commandSelect.value);
          switch (command.commandType) {
            case CommandType.GetRecord:
              {
                command.name = writeValue.value;
              }
          }

          this.serializeCommand(command).then(buffer => {
            if (buffer) {
              characteristic.writeValue(buffer);
            }
          });
        });
        characteristicDiv.appendChild(sendButton);
      }
    }

    if (characteristic.properties.notify) {
      var notifiedValue = new ArrayBuffer();
      var notifyValue = document.createElement('p') as HTMLParagraphElement;
      characteristicDiv.appendChild(notifyValue);
      var currentRecord: Message_Record | undefined;
      var pointCount = 0;
      var valueChangedHandler = async (event: Event) => {
        var target = event!.target as BluetoothRemoteGATTCharacteristic;
        if (characteristic.uuid == "a495ff21-c5b1-4b44-b512-1370f02d74de") {
          if (target!.value?.buffer instanceof (ArrayBuffer)) {
            this.logMessage("Notify " + characteristic.uuid + " value length = " + target.value.byteLength + " - " + target.value.byteOffset);
            notifiedValue = appendBuffer(notifiedValue, target.value.buffer);
          }
        }
        else if (characteristic.uuid == this.TMLinkNotificationCharacteristicId) {
          if (target!.value?.buffer instanceof (ArrayBuffer)) {
            var notifyReady = this.deserializeNotifyReady(new Uint8Array(target!.value?.buffer));
            if (notifyReady.errorCode != ErrorCodes.Success) {
              this.logMessage("Error " + notifyReady.errorCode + " for command " + notifyReady.commandType)
            }
            else {
              var readCharacteristic = await characteristic.service.getCharacteristic(this.TMLinkReadCharacteristicId);
              var writeCharacteristic = await characteristic.service.getCharacteristic(this.TMLinkCommandCharacteristicId);
              switch (notifyReady.commandType) {
                case CommandType.GetGaugeInfo:
                  {
                    readCharacteristic.readValue().then(async (gaugeInfoValue) => {
                      if (gaugeInfoValue.buffer instanceof (ArrayBuffer)) {
                        var message = await this.deserializeMessage(gaugeInfoValue.buffer, Message.decode);
                        const gaugeInfoText = document.querySelector('#gaugeInfo') as HTMLTextAreaElement;
                        gaugeInfoText.value = "Gauge variant = " + message!.gaugeInfo?.gaugeVariant + "\r\n";
                        gaugeInfoText.value += "Version Number = " + message!.gaugeInfo?.versionNumber + "\r\n";
                        gaugeInfoText.value += "Serial Number = " + message!.gaugeInfo?.serialNumber + "\r\n";
                        gaugeInfoText.value += "Battery Level = " + message!.gaugeInfo?.batteryLevel + "\r\n";
                      }
                    });

                    break;
                  }
                case CommandType.GetRecordList:
                  {
                    readCharacteristic.readValue().then(async (recordListValue) => {
                      if (recordListValue.buffer instanceof (ArrayBuffer)) {
                        var message = await this.deserializeMessage(recordListValue.buffer, Message.decode);
                        const recordListText = document.querySelector('#recordList') as HTMLTextAreaElement;
                        recordListText.value = '';
                        message?.recordList?.items.forEach(record => {
                          recordListText.value += record.name + " Requires " + record.numPointsRequired + " Taken " + record.numPointsTaken + "\r\n";
                        });
                      }
                    });

                    break;
                  }
                case CommandType.GetRecord:
                  {
                    const recordText = document.querySelector('#record') as HTMLTextAreaElement;

                    readCharacteristic.readValue().then(async (recordValue) => {
                      if (recordValue.buffer instanceof (ArrayBuffer)) {
                        var message = await this.deserializeMessage(recordValue.buffer, Message.decode);
                        currentRecord = message?.record;
                        if (currentRecord) {
                          pointCount = 0;
                          recordText.value = currentRecord.name + "\r\n";
                          let command = Command.create();
                          command.commandType = CommandType.GetRecordPoint;
                          command.name = currentRecord.name;

                          this.serializeCommand(command).then(buffer => {
                            if (buffer) {
                              writeCharacteristic.writeValue(buffer);
                            }
                          });
                        }
                      }
                    });

                    break;
                  }
                case CommandType.GetRecordPoint:
                  {
                    const recordText = document.querySelector('#record') as HTMLTextAreaElement;

                    readCharacteristic.readValue().then(async (pointValue) => {
                      if (pointValue.buffer instanceof (ArrayBuffer)) {
                        var message = await this.deserializeMessage(pointValue.buffer, Message.decode);
                        var point = message?.recordPoint;
                        if (currentRecord && point) {
                          recordText.value += point.name + " " + point.thickness / 1000 + " " + point.velocity + " \r\n";

                          if (pointCount < currentRecord.numPointsTaken) {
                            pointCount++;
                            let command = Command.create();
                            command.commandType = CommandType.GetRecordPoint;
                            command.name = currentRecord.name;

                            this.serializeCommand(command).then(buffer => {
                              if (buffer) {
                                writeCharacteristic.writeValue(buffer);
                              }
                            });
                          }
                        }
                      }
                    });

                    break;
                  }
              }
            }
          }
        }
        else if (characteristic.uuid == this.TMLinkLiveCharacteristicId) {
          if (target!.value?.buffer instanceof (ArrayBuffer)) {
            const liveText = document.querySelector('#live') as HTMLTextAreaElement;
            var notifyLive = this.deserializeNotifyLiveMeasurement(new Uint8Array(target!.value?.buffer));
            liveText.value = "Live measurement\r\n";
            liveText.value += "Index " + notifyLive.index + "\r\n";
            liveText.value += "Thickness " + notifyLive.thickness / 1000 + "\r\n";
            liveText.value += "Velocity " + notifyLive.velocity + "\r\n";
            if (notifyLive.liveMeasurementType == LiveMeasurementType.Frozen) {
              var frozenCharacteristic = await characteristic.service.getCharacteristic(this.TMLinkFrozenCharacteristicId);
              frozenCharacteristic.readValue().then(async (frozenValue) => {
                if (frozenValue.buffer instanceof (ArrayBuffer)) {
                  var frozenMeasurement = await this.deserializeMessage(frozenValue.buffer, FrozenLiveMeasurement.decode);
                  if (frozenMeasurement) {
                    liveText.value = "Frozen measurement\r\n";
                    liveText.value += "Index " + frozenMeasurement.index + "\r\n";
                    liveText.value += "Thickness " + frozenMeasurement.thickness / 1000 + "\r\n";
                    liveText.value += "Velocity " + frozenMeasurement.velocity + "\r\n";
                    liveText.value += "A-Scan point count " + frozenMeasurement.ascan?.ascanPoints.length + "\r\n";
                  }
                }
              });
            }
          }
        }
        else {
          let decodedText = this.utf8decoder.decode(target!.value);
          var now = new Date();
          this.logMessage('> ' + now.toTimeString() + ' message is ' + decodedText);
          notifyValue.innerHTML = 'Value: ' + decodedText;
        }

        function appendBuffer(buffer1: ArrayBuffer, buffer2: ArrayBuffer) {
          var tmp = new Uint8Array(buffer1.byteLength + buffer2.byteLength);
          tmp.set(new Uint8Array(buffer1), 0);
          tmp.set(new Uint8Array(buffer2), buffer1.byteLength);
          return tmp.buffer;
        }
      };

      // Add event handler for notifications
      characteristic.addEventListener('characteristicvaluechanged', valueChangedHandler);//.oncharacteristicvaluechanged = valueChangedHandler;//

      // Ensure event handler for notifications is removed when this characteristic element is removed
      ensureValueChangedHandlerIsTidiedUp(characteristicDiv, characteristic, valueChangedHandler, characteristicsContainer);

      characteristic.startNotifications();
    }

    characteristicsContainer.appendChild(characteristicDiv);

    function ensureValueChangedHandlerIsTidiedUp(
      characteristicDiv: HTMLDivElement,
      characteristic: BluetoothRemoteGATTCharacteristic,
      valueChangedHandler: any,
      characteristicsContainer: HTMLDivElement) {
      const containerChange = function (mutationsList: MutationRecord[]) {
        mutationsList.forEach((mutation: MutationRecord) => {
          if (mutation.type === 'childList') {
            mutation.removedNodes.forEach(node => {
              if (node === characteristicDiv) {
                characteristic.removeEventListener('characteristicvaluechanged', valueChangedHandler);
              }
            });
          }
        });
      };

      const observer = new MutationObserver(containerChange);
      const observerConfig: any = { childList: true, subtree: true };

      observer.observe(characteristicsContainer, observerConfig);
    }
  }

  private displayCharacteristicValue(characteristic: BluetoothRemoteGATTCharacteristic, readValue: HTMLParagraphElement) {
    characteristic.readValue().then(value => {
      if (characteristic.uuid == "a495ff21-c5b1-4b44-b512-1370f02d74de") {
        if (value.buffer instanceof (ArrayBuffer)) {
          readValue.innerHTML = 'Record: ' + value.buffer;
        }
      }
      if (characteristic.uuid == this.TMLinkReadCharacteristicId) {
        if (value.buffer instanceof (ArrayBuffer)) {
          this.deserializeMessage(value.buffer, Message.decode).then(message => {
            this.logMessage("Decoded message = " + JSON.stringify(message));
            readValue.innerHTML = 'Command Type: ' + message?.commandType;
          });
        }
      }
      else {
        readValue.innerHTML = 'Value: ' + this.utf8decoder.decode(value);
      }

    });
  }

  private getDeviceInfo() {
    let options = {
      optionalServices: [this.TMLinkServiceId, this.bleServiceId, this.deviceInformationServiceId, this.genericAccessServiceId],
      filters: [
        { services: [this.TMLinkServiceId] },
        { services: [this.bleServiceId] }
      ]
    }

    this.logMessage('Scanning for gauge...')
    return navigator.bluetooth.requestDevice(options).then(device => {
      this.detectedBluetoothDevice = device
    }).catch(error => {
      this.logMessage('Argh! ' + error)
    })
  }

  private connectGATT() {
    if (this.detectedBluetoothDevice!.gatt!.connected) {
      return Promise.resolve()
    }

    return this.detectedBluetoothDevice!.gatt!.connect()
      .then(server => {
        this.logMessage('Getting GATT Services...')
        return server.getPrimaryServices()
      })
      .then(services => {
        this.logMessage('Found ' + services.length + ' GATT services')
        var serviceSelect = document.querySelector('#serviceSelect') as HTMLSelectElement
        services.forEach(service => {
          var option = document.createElement('option');
          option.text = service.uuid;
          serviceSelect!.add(option);
        });
        this.getCharacteristics();
      })
  }

  public logMessage(message: string) {
    message += '\r\n';
    const logTextArea = document.querySelector('#log') as HTMLTextAreaElement;
    logTextArea.value += message;
    logTextArea.scrollTop = logTextArea.scrollHeight;
  }
}

function appendOutput(outputContainer: HTMLDivElement, id: string, text: string) {
  var output = document.createElement("div");
  output.className = "output";
  var label = document.createElement("label") as HTMLLabelElement;
  label.htmlFor = id;
  label.innerText = text;
  output.appendChild(label);
  var textarea = document.createElement("textarea") as HTMLTextAreaElement;
  textarea.id = id;
  output.appendChild(textarea);
  outputContainer.appendChild(output);
}
