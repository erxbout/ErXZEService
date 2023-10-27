// Set time via NTP, as required for x.509 validation
// void setClock() {
//   configTime(3 * 3600, 0, "pool.ntp.org", "time.nist.gov");

//   time_t now = time(nullptr);
//   while (now < 8 * 3600 * 2) {
//     delay(500);
//     now = time(nullptr);
//   }
//   struct tm timeinfo;
//   gmtime_r(&now, &timeinfo);
// }

bool Connected()
{
  //-4 -> timeout -2 connect failed
  if (mqttClient.state() <= MQTT_DISCONNECTED)
  {
    int timeToWait = MY_MQTT_CONNECT_TIMEOUT * 2;

    if (ClientConnected)
      timeToWait = timeToWait * 5;

    if (!IsWaitingTimeOver(LastMqttConnect, timeToWait))
      return false;

    if (LastMqttConnected > 0)
    {
      LastMqttConnected = 0;
      espClient.setTimeout(MY_MQTT_CONNECT_TIMEOUT);
    }
  }

  if (!mqttClient.connected())
  {
    Log("Try connect status before:" + (String)mqttClient.state());
    mqttClient.connect(MqttUsername, MqttUsername, MqttPassword);
    Log("statusafter:" + (String)mqttClient.state());

    LastMqttConnect = millis();

    if (mqttClient.connected())
    {
      LastMqttConnected = millis();
      espClient.setTimeout(MY_MQTT_SOCKET_TIMEOUT);

      mqttClient.subscribe("ErXZEService/time");
      Log("ClientConnected");
    }
  }

  return mqttClient.connected();
}

void ServerCommunicationLoop()
{
  ulong currentMillis = millis();

  if (!Connected())
    return;

  if (LoggingActive)
    mqttClient.publish("ErXZEService/debug", "1", false);

  mqttClient.loop();

  String topicPrefix = "ErXZEService/";
  String str = ReportToServerRequest.substring(1, ReportToServerRequest.length());
  String valueStr = ValueReportToServerRequest.substring(1, ValueReportToServerRequest.length());

  if (ScheduledServerCommunication.Time)
    mqttClient.publish("ErXZEService/timeRequested", "1", (boolean)false);

  if (ScheduledServerCommunication.SendStatus && str.length() > 0)
    mqttClient.publish("ErXZEService/status", str.c_str(), (boolean)true);

  if (ScheduledServerCommunication.ValueProperty && valueStr.length() > 0)
    mqttClient.publish((topicPrefix + ValueReportToServerTopic).c_str(), valueStr.c_str(), (boolean)true);

  ValueReportToServerTopic = "";
  ResetScheduledServerCommunication();
  ulong dif = millis() - currentMillis;
  if (dif > 0)
    Log("ComLoopTook: " + (String)dif + "ms");
}

void SubscriptionReceive(char* topic, byte* payload, unsigned int length) {
  String topicStr = String(topic);
  Log("Received [" + topicStr + "]");

  if (topicStr == "ErXZEService/time")
  {
    String timeStr;
    for (uint i = 0; i < length; i++) {
      timeStr += (char)payload[i];
    }

    Serial.println(timeStr);
  }
}
