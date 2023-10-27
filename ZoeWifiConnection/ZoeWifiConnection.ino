#include <PubSubClient.h>

#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

#include <WiFiClientSecureBearSSL.h>
#include <time.h>

#ifndef STASSID
#define STASSID "ErXZEServiceESP"

//Define with what your external AP SSID starts with 
//(why starts with? because I have different network names for at home and in my car but they start with the same)
#define STASSID_CONTAINMENT "SSID_Startswith" 
#define STAPSK  "password"
#endif

#define MY_MQTT_SOCKET_TIMEOUT 4000
#define MY_MQTT_CONNECT_TIMEOUT 30000

//MQTT
WiFiClientSecure espClient;
PubSubClient mqttClient(espClient);
const char* MqttBroker = "mqtt.server";
const char* MqttUsername = "username";
const char* MqttPassword = "password";
const int BrokerPort = 1907;
bool subscribed = false;

//SHA1 of certificate 65 c4 21 da 0d f4 99 68 83 07 de d5 3c 41 95 29 f4 bd a0 c6
// 25:56:E3:FE:55:F3:EE:4C:21:CD:86:D8:A3:DF:54:8C:0B:CE:87:D0
const char* HttpsCertificate = "25:56:E3:FE:55:F3:EE:4C:21:CD:86:D8:A3:DF:54:8C:0B:CE:87:D0";
const char* HttpsCertificateLetsencrypt = "A0:53:37:5B:FE:84:E8:B7:48:78:2C:7C:EE:15:82:7A:6A:F5:A4:05";

//UDP
WiFiUDP Udp;
IPAddress broadcast = IPAddress(255,255,255,255);

IPAddress myLocalIp = IPAddress(192,168,8,1);
IPAddress myLocalSubnet = IPAddress(255,255,255,0);
IPAddress addressLock;
unsigned int localPort = 1905;      // local port to listen on

// buffers for receiving and sending data
char packetBuffer[UDP_TX_PACKET_MAX_SIZE]; //buffer to hold incoming packet,
char AckBuffer[] = "Ack#";       // a string to send back

//communication Strings
const char MessageSeperator = '#';   //On send its on Start of message on receive its on end of message
const String TimeCalibrationRequest = "Calibration";
const String AckString = "Ack";
const String ConnectionString = "InitializeZOEConnection";
char ConnectionSuccessfullResponse[] = "Connected";

struct ServerCommunication {
  bool ValueProperty;
  bool Time;
  bool SendStatus;
} ScheduledServerCommunication;

String ReceivedMessage = "";    //the last received Message
bool ReportToServerRequested = false;
bool ValueReportToServerRequested = false;
String ReportToServerRequest = "";
String ValueReportToServerRequest = "";
String ValueReportToServerTopic = "";
long LastReportToServerRequest = -20000;
bool TimeCalibrationRequested = false;
long LastTimeCalibrationRequest = -30000;

long LastNotifyTime = 0;
long LastCommandToArduino = 0;

long LastMqttConnect = 0;
long LastMqttConnected = 0;

long LastWiFiScan = 0;
long WifiScanInterval = 10000;
bool WiFiInit = true;

//Serial
char SerialBuffer[UDP_TX_PACKET_MAX_SIZE];
int SerialBufferPosition = 0;
int SerialBufferLastMessageStart = 0;

const String SerialCommand_ReportAndTimeCalibration = "SEND_TIME";
const String SerialCommand_ReportToServer = "SEND";
const String SerialCommand_TimeCalibration = "TIME";
const String SerialCommand_rSOC = "rSOC";
const String SerialCommand_uSOC = "uSOC";

const char MessageStart = '<';
const char MessageEnd = '>';

//Status
bool SoftAPActive = false;
bool OnlineMode = true;
bool ClientConnected = false;
bool ClientWasConnectedOnce = false;

bool ledBlink = true;
int tick = 0;

byte notifyTries = 0;
const byte notifyTriesMax = 20;
