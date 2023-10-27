dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true
robocopy "%cd%/bin/Release/netcoreapp3.1/linux-x64/publish/MqttServerExtension" "\\mqttserverextension\binaries"
pause