#generate TypeScript - run in the Sample.BLE.TypeScript folder
protoc --plugin=protoc-gen-ts_proto=".\\node_modules\\.bin\\protoc-gen-ts_proto.cmd" --ts_proto_opt=esModuleInterop=true --ts_proto_out=src --proto_path=..\ ..\Protos\cyg_tml_api_v1.proto
## NB: This generates a couple of TypeScript errors - you may need to replace globalThis. with (globalThis as any).
