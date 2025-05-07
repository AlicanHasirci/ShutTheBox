protoc \
--proto_path=. \
--csharp_out=../shut-the-box-client/Assets/Scripts/Network/.  \
--go_out=../shut-the-box-server/api/. \
--go_opt=paths=source_relative \
Api.proto