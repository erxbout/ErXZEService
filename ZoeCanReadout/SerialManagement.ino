
void SerialManagementLoop()
{
  if (Serial.available() > 0)
  {
    String serialRead = Serial.readStringUntil('\r');
    Serial.readString();
    LogEvent("sRead: " + serialRead);

    if (serialRead.length() <= 3 && serialRead.length() > 1)
      WifiCommandHandling(serialRead);

    if (serialRead.length() >= 16 && serialRead.length() <= 20)
    {
      int minute = serialRead.substring(14, 16).toInt();
      bool minuteFarAway = abs(Minute - minute) > 6;

      if (CalibrationRequestAllowed || minuteFarAway)
      {
        TimeCalibration(serialRead);
      }
      else
      {
        if (CalibrationRequestAllowed)
          LogEvent("skp time");
      }
    }
  }
}

void SendOverWifi(String command, String str)
{
  if (command != "")
    LogEvent("[Wifi] cmd: " + command + " str: " + str);
  else
    LogEvent("[Wifi] send: " + str);

  if (IsWaitingTimeOver(LastSendOverWifi, 200) || command != "")
  {
    Serial.print(command);
    Serial.print(MessageStart);
    Serial.print(str);
    Serial.println(MessageEnd);
    LastSendOverWifi = millis();

    SomethingChangedVar = false;
  }
  else
  {
    LogEvent(F("Skp snd"));
  }
}

bool IsWaitingTimeOver(unsigned long lastMillis, unsigned long waitingTime)
{
  return lastMillis + waitingTime < millis();
}
