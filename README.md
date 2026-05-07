# cygnus-tmlink-ble-sdk
A SDK for the Cygnus TM-Link BLE (Bluetooth Low Energy) Service.

## TM-Link over BLE
The latest Cygnus 1Ex gauge firmware (from V1.4.xx) supports the TM-Link via BLE communication mode. This allows you to connect to the gauge using a smartphone or computer and read data from it in real-time.

If you want to use the latest Cygnus 1Ex firmware and firmware update utilities please contact <service@cygnus-instruments.com>

The characteristics of the BLE TM-Link Service allow writing commands, receiving notifications and reading responses which contain g-zipped, protobuf messages.  The protobuf messages are defined in the [cyg_tml_api_v1.proto](https://github.com/c49nu5/tmlink-dotnet-api/blob/master/Protos/cyg_tml_api_v1.proto) file.

There is a .Net API available for the TM-Link BLE Service, which can be found here [tmlink-dotnet-api](https://github.com/c49nu5/tmlink-dotnet-api)

## SDK main components:
- The sample TypeScript app for accessing the TM-Link BLE Service.
- The Sample.BLE.Client Maui application that demonstrates how to use the TM-Link .Net API.
- The Cygnus.BLE.VirtualGauge Maui application that that will simulate a Cygnus 1Ex gauge by hosting the TM-Link BLE Service on a mobile phone, this is useful for testing the TM-Link client apps without needing a physical gauge.

### Sample TypeScript app
This sample app is a good starting point if you want to build a web application that connects to the TM-Link BLE Service.

The code in the app can run in any browser that supports [Web-Bluetooth](https://developer.mozilla.org/en-US/docs/Web/API/Web_Bluetooth_API).

Open this repository in Visual Studio Code, ensure node is installed and configured correctly and the dependencies are installed by running the following in the terminal.
- configure web page with Vite

npm create vite@latest

- install Bluetooth types

npm install -g @types/web-bluetooth

- install protobuf

npm install protobuf

- install TypeScript generator

npm install ts-proto

Then run the Launch Chrome or Launch Edge configuration, this will build the npm app and run using Vite.

The TypeScript code is located in the `Sample.BLE.TypeScript\src` folder. The app is loaded by `src\main.ts` but the majority of the logic for connecting to the TM-Link BLE Service and handling the communication is in `src\cygnus-1ex-ble.ts` file.

This file imports types from `src/Protos/cyg_tml_api` which was generated from the `cyg_tml_api_v1.proto` file using the `protobufjs` library. The `cyg_tml_api_v1.proto` file is located in the `Protos` folder and is the same protobuf definition used in the .Net API, and in the Cygnus 1Ex gauge firmware.

To recreate it run the following command in the terminal:
```
npx ts-proto --proto_path=Protos Protos/cyg_tml_api_v1.proto --ts_out=src/Protos
```

### Sample.BLE.Client Maui application
Open the `cygnus-tmlink-ble-dotnet-sample.slnx` solution file in Visual Studio, set the `Sample.BLE.Client` project as the startup project and run it. 

Make sure to have Bluetooth enabled on your computer and a Cygnus 1Ex gauge with communication mode configured as TM-Link via BLE.

### Cygnus.BLE.VirtualGauge Maui application
Open the `cygnus-tmlink-ble-dotnet-virtual-gauge.slnx` solution file in Visual Studio, set the `Cygnus.BLE.VirtualGauge` project as the startup project and run it on a mobile device with Bluetooth capabilities.
The app has been tested on Android, but it should also work on iOS. Once the app is running, it will host the TM-Link BLE Service and simulate a Cygnus 1Ex gauge. You can then connect to it using the Sample.BLE.Client application or any other BLE client that supports the TM-Link Service.
