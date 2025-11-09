cd ./protoc-28.3-win64/bin/
protoc ^
--csharp_out=./../../GamePb/GamePb/PB/ ^
--proto_path=./../../ProtoFiles/ ^
LaunchPB.proto
pause
