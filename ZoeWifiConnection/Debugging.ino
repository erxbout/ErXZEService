
const bool LoggingActive = false;

void Log(String str)
{
  if (LoggingActive)
  {
    Serial.print("log: ");
    Serial.println(str);
  }
}

void Warn(String str)
{
  Log("[Warn] " + str);
  //AddStringToSerialBuffer("[Warn] " + str);
}

bool IsWaitingTimeOver(ulong lastMillis, ulong waitingTime)
{
  return lastMillis + waitingTime < millis();
}
