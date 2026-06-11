# cygnus-tmlink-ble-sdk
An SDK for the Cygnus TM-Link BLE (Bluetooth Low Energy) Service.

## Cygnus Instruments Limited
**Cygnus Instruments** are a manufacturer of industrial Ultrasonic Thickness Gauges (UTGs) which are used for measuring the thickness of materials such as metals, plastics and composites.
[Cygnus Website](https://cygnus-instruments.com/)

The **Cygnus 1 Ex** is an Instrinsically Safe UTG certified for ATEX and IECEx.

## TM-Link Overview
* *'TM-Link' = Thickness Measurement data Link.* *

TM-Link can be used to send and receive data-logging records and B-Scans to a Cygnus 1 Ex ultrasonic thickness gauge, and you can also subscribe to receive Live Measurements from the gauge. 

A typical workflow using TM-Link with APM/RBI software could be,

- A plan for measuring various assets is created in the APM/RBI software, each route will contain a number of TML or CML locations each with a unique ID key.
- These routes will become 'records' in the Cygnus 1 Ex Gauge.
- These empty records are created and transferred to the Cygnus 1 Ex gauge using TM-Link services.
- The Cygnus 1 Ex gauge is taken out into the field where the UT Technician opens each empty record and populates it with thickness measurements.
- When all the measurements have been taken, the populated records are transferred from the Cygnus 1 Ex gauge using TM-Link services.
- The populated records can then be processed and the thickness measurements data inserted back into the APM/RBI software

A typical workflow using TM-Link with a mobile device with an Inspection Application could be,

- The mobile device subscribes to Live Measurements from the Cygnus 1 Ex Gauge.
- On site the UT Technician uses the mobile device Inspection Application to direct the thickness measurement survey.
- The UT Technician takes thickness measurements which the mobile device captures and records against the asset location.

[The Cygnus 1 Ex Gauge](https://cygnus-instruments.com/product/cygnus-1-ex/)

## TM-Link via BLE
The latest Cygnus 1Ex gauge firmware (from V1.4.xx) supports the TM-Link via BLE communication mode. This allows you to connect to the gauge using a smartphone or computer and read data from it in real-time.

If you want to use the latest Cygnus 1Ex firmware and firmware update utilities please contact <service@cygnus-instruments.com>

The characteristics of the BLE TM-Link Service allow writing commands, receiving notifications and reading responses which contain g-zipped, protobuf messages.  The protobuf messages are defined in the [cyg_tml_api_v1.proto](https://github.com/c49nu5/tmlink-dotnet-api/blob/master/Protos/cyg_tml_api_v1.proto) file, along with a description of the process for using them.

There is a .Net API available for the TM-Link BLE Service, which can be found here [tmlink-dotnet-api](https://github.com/c49nu5/tmlink-dotnet-api)

## SDK main components:
- The sample TypeScript app for accessing the TM-Link BLE Service using Web-Bluetooth.
- The Sample.TMLink.Client Maui application that demonstrates how to use the TM-Link .Net API.
- The Cygnus.TMLink.VirtualGauge Maui application that that will simulate a Cygnus 1Ex gauge by hosting the TM-Link BLE Service on a mobile phone, this is useful for testing the TM-Link client apps without needing a physical gauge.

### Sample TypeScript app
This sample app is a good starting point if you want to build a web application that connects to the TM-Link BLE Service.

The code in the app can run in any browser that supports [Web-Bluetooth](https://developer.mozilla.org/en-US/docs/Web/API/Web_Bluetooth_API).

Ensure Node is installed, open this repository in Visual Studio Code, then run the Launch Chrome or Launch Edge configuration, this will install the dependencies, compile the typescript and and run the node app using Vite.

The TypeScript code is located in the `Sample.TMLink.TypeScript\src` folder. The app is loaded by `src\main.ts` but the majority of the logic for connecting to the TM-Link BLE Service and handling the communication is in `src\cygnus-1ex-ble.ts` file.

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

### Sample.TMLink.Client Maui application
Open the `cygnus-tmlink-ble-dotnet-sample.slnx` solution file in Visual Studio, set the `Sample.TMLink.Client` project as the startup project and run it. 

The app will scan for nearby TM-Link BLE Services, you should see your Cygnus 1Ex gauge in the list if it's in range and has Bluetooth enabled. Select the gauge and connect to it, then you can send commands to the gauge and receive responses, as well as view live measurements.

It uses the [TM-Link .Net API](https://github.com/c49nu5/tmlink-dotnet-api) to handle the communication with the TM-Link BLE Service, so you can refer to the code in the `Sample.TMLink.Client` project to see how to use the API in a real application.
Note that it can be run on Windows, Android, MacOS or iOS.

Make sure to have Bluetooth enabled on your device and a Cygnus 1Ex gauge with communication mode configured as TM-Link via BLE.

### Cygnus.TMLink.BLE.VirtualGauge Maui application
Open the `cygnus-tmlink-ble-virtual-gauge.slnx` solution file in Visual Studio and run it on a mobile device with Bluetooth capabilities.
The app has been tested on Android, but it should also work on iOS. 

Once the app is running, it will host the TM-Link BLE Service and simulate a Cygnus 1Ex gauge. You can then connect to it using the Sample.TMLink.Client application or any other TM-Link client.

## Bluetooth Information
To enable TM-Link Bluetooth services on the Cygnus 1Ex gauge, from the Setup menu, set Comms Mode to **TM-Link**.

### Bluetooth LE Profile
The Cygnus 1Ex gauge Bluetooth LE profile contains the folowing services

- **GATT_SERVICE_GENERIC_ACCESS** (0x1800)
  - CHARACTERISTIC_DEVICE_NAME (0x2A00)

- **GATT_SERVICE_DEVICE_INFORMATION** (0x180A)
  - CHARACTERISTIC_SERIAL_NUMBER_STRING (0x2A25)
  - CHARACTERISTIC_MANUFACTURER_NAME_STRING (0x2A29)
  - CHARACTERISTIC_MODEL_NUMBER_STRING (0x2A24)  
  - CHARACTERISTIC_FIRMWARE_REVISION_STRING (0x2A26)
  - CHARACTERISTIC_SOFTWARE_REVISION_STRING (0x2A28)

- **GATT_SERVICE_CUSTOM_TMLINK** (DE670901-8025-4C69-A40E-ECCD60563713)
  - CHARACTERISTIC_WRITE_COMMAND (DE670902-8025-4C69-A40E-ECCD60563713) WRITE
  - CHARACTERISTIC_NOTIFY_MESSAGE_READY (DE670903-8025-4C69-A40E-ECCD60563713) NOTIFY         
  - CHARACTERISTIC_READ_MESSAGE (DE670904-8025-4C69-A40E-ECCD60563713) READ  
  - CHARACTERISTIC_NOTIFY_LIVE_MEASUREMENT (DE670906-8025-4C69-A40E-ECCD60563713) NOTIFY
  - CHARACTERISTIC_READ_LIVE_MEASUREMENT (DE670907-8025-4C69-A40E-ECCD60563713) READ

The CHARACTERISTIC_DEVICE_NAME will return the string "Cygnus1Ex_000000" where the 000000 is the serial number of the gauge.

The CHARACTERISTIC_MODEL_NUMBER_STRING will return one of the following depending on the gauge variant,

- "C1Ex_Basic SC"
- "C1Ex_Basic TC"
- "C1Ex_Plus"
- "C1Ex_Pro"

The CHARACTERISTIC_FIRMWARE_REVISION_STRING will return the gauge firmware version in this format "Major.Minor.Build"

The CHARACTERISTIC_SOFTWARE_REVISION_STRING will return the proto message file version string, this can be used to handle different proto files should the interface be extended in the future. This version will match the package version in the proto file, for example 'package Cygnus.TMLink.Protobuf.V1;' will return string '1'.





