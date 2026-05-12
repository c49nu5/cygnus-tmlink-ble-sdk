# cygnus-tmlink-ble-sdk
A SDK for the Cygnus TM-Link BLE (Bluetooth Low Energy) Service.

## TM-Link over BLE
The latest Cygnus 1Ex gauge firmware (from V1.4.xx) supports the TM-Link via BLE communication mode. This allows you to connect to the gauge using a smartphone or computer and read data from it in real-time.

If you want to use the latest Cygnus 1Ex firmware and firmware update utilities please contact <service@cygnus-instruments.com>

The characteristics of the BLE TM-Link Service allow writing commands, receiving notifications and reading responses which contain g-zipped, protobuf messages.  The protobuf messages are defined in the [cyg_tml_api_v1.proto](https://github.com/c49nu5/tmlink-dotnet-api/blob/master/Protos/cyg_tml_api_v1.proto) file, along with a description of the process for using them.

There is a .Net API available for the TM-Link BLE Service, which can be found here [tmlink-dotnet-api](https://github.com/c49nu5/tmlink-dotnet-api)

## SDK main components:
- The sample TypeScript app for accessing the TM-Link BLE Service.
- The Sample.BLE.Client Maui application that demonstrates how to use the TM-Link .Net API.
- The Cygnus.BLE.VirtualGauge Maui application that that will simulate a Cygnus 1Ex gauge by hosting the TM-Link BLE Service on a mobile phone, this is useful for testing the TM-Link client apps without needing a physical gauge.

### Sample TypeScript app
This sample app is a good starting point if you want to build a web application that connects to the TM-Link BLE Service.

The code in the app can run in any browser that supports [Web-Bluetooth](https://developer.mozilla.org/en-US/docs/Web/API/Web_Bluetooth_API).

Ensure Node is installed, open this repository in Visual Studio Code, then run the Launch Chrome or Launch Edge configuration, this will install the dependencies, compile the typescript and and run the node app using Vite.

The TypeScript code is located in the `Sample.BLE.TypeScript\src` folder. The app is loaded by `src\main.ts` but the majority of the logic for connecting to the TM-Link BLE Service and handling the communication is in `src\cygnus-1ex-ble.ts` file.

This file imports types from `src/Protos/cyg_tml_api_v1.ts` which was generated from the `cyg_tml_api_v1.proto` definition file. The `cyg_tml_api_v1.proto` file is located in the `Protos` folder and is the same protobuf definition used in the .Net API, and in the Cygnus 1Ex gauge firmware.

The types can be recreated by running the following command in the terminal:
```
protoc --plugin=protoc-gen-ts_proto=".\\node_modules\\.bin\\protoc-gen-ts_proto.cmd" --ts_proto_opt=esModuleInterop=true --ts_proto_out=src --proto_path=..\ ..\Protos\cyg_tml_api_v1.proto
```
It's the types generated from the protobuf definition that allow the TypeScript code to easily encode and decode the messages sent and received from the TM-Link BLE Service.

The first 4 methods in `cygnus-1ex-ble.ts` are the main ones that demonstrate how to encode/decode the protobuf messages they are:
- serializeCommand, converts a protobuf Command instance to a g-zipped byte array that can be sent to the TM-Link command characteristic with id `de670902-8025-4c69-a40e-eccd60563713`.
- deserializeNotifyReady, converts a byte array received from the notification characteristic `de670903-8025-4c69-a40e-eccd60563713` to a NotifyMessage protobuf instance that indicates a response to a command is ready to be read.
- deserializeMessage, converts a g-zipped byte array received from the TM-Link BLE Service to a protobuf instance; this can be a Message in response to Command sent from characteristic `de670904-8025-4c69-a40e-eccd60563713` or a FrozenLiveMeasurement from `de670907-8025-4c69-a40e-eccd60563713`.
- deserializeNotifyLiveMeasurement, converts a byte array received from the live measurement characteristic `de670906-8025-4c69-a40e-eccd60563713` to a protobuf LiveMeasurement instance.

### Sample.BLE.Client Maui application
Open the `cygnus-tmlink-ble-dotnet-sample.slnx` solution file in Visual Studio, set the `Sample.BLE.Client` project as the startup project and run it. 

The app will scan for nearby TM-Link BLE Services, you should see your Cygnus 1Ex gauge in the list if it's in range and has Bluetooth enabled. Select the gauge and connect to it, then you can send commands to the gauge and receive responses, as well as view live measurements.

It uses the [TM-Link .Net API](https://github.com/c49nu5/tmlink-dotnet-api) to handle the communication with the TM-Link BLE Service, so you can refer to the code in the `Sample.BLE.Client` project to see how to use the API in a real application.
Note that it can be run on Windows, Android, MacOS or iOS, but the BLE functionality has not been tested on iOS.

Make sure to have Bluetooth enabled on your device and a Cygnus 1Ex gauge with communication mode configured as TM-Link via BLE.

### Cygnus.BLE.VirtualGauge Maui application
Open the `cygnus-tmlink-ble-dotnet-virtual-gauge.slnx` solution file in Visual Studio and run it on a mobile device with Bluetooth capabilities.
The app has been tested on Android, but it should also work on iOS. 

Once the app is running, it will host the TM-Link BLE Service and simulate a Cygnus 1Ex gauge. You can then connect to it using the Sample.BLE.Client application or any other TM-Link client.
