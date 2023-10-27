
long seqNr = 0;

void UdpSendLoop()
{
  if (!ClientConnected)
  {
    WaitForClient();
  }
  else
  {
    if (SerialBufferPosition > 0 && IsWaitingTimeOver(LastNotifyTime, 300))
      NotifyClient();

    CheckMessageForCommands();
  }

  DoLedBlink();
}

void CheckMessageForCommands()
{
  bool success = UdpReceive();

  if (success)
  {
    if (!IsWaitingTimeOver(LastCommandToArduino, 1000))
      return;

    String msg = ReceivedMessage;
    bool commandExecuted = false;
    int requestStringLength = TimeCalibrationRequest.length();

    if (msg == ConnectionString)
      UdpSend(ConnectionSuccessfullResponse);

    //Command: Calibration
    if (msg.substring(0, requestStringLength) == TimeCalibrationRequest)
    {
      Serial.println(msg.substring(requestStringLength, requestStringLength + 16));
      commandExecuted = true;
    }

    //if no command for wifi then relay to arduino
    if (msg != AckString && msg != ConnectionString && !commandExecuted)
    {
      LastCommandToArduino = millis();
      Serial.println(msg);
      commandExecuted = true;
    }

    ConsumeMessage();
  }
}

void WaitForClient()
{
  //Log("WaitingForClient");
  bool success = AwaitUdpString(ConnectionString, 200);

  if (success)
  {
    Log("ClientConnected");
    ClientConnected = true;
    ClientWasConnectedOnce = true;
    addressLock = Udp.remoteIP();
    UdpSend(ConnectionSuccessfullResponse);
  }
}

void NotifyClient()
{
  LastNotifyTime = millis();
  String buuf = String(SerialBuffer) + "@" + (String)seqNr;
  UdpSend((char*)buuf.c_str());

  bool success = AwaitUdpString(AckString, 200);
  if (success) {
    Log("ackreceived");
    seqNr++;
    Log("seq " + (String)seqNr);
    ClearSerialBuffer();
    notifyTries = 0;
  }
  else
  {
    notifyTries++;

    if (notifyTries >= notifyTriesMax)
    {
      Log("client disconnected");
      ReceivedMessage = "";
      ClientConnected = false;
      notifyTries = 0;
    }
  }
}

void DoLedBlink()
{
  if (!ClientConnected)
  {
    tick++;
    if (ledBlink && tick > 4)
    {
      ledBlink = !ledBlink;
      tick = 0;
      digitalWrite(LED_BUILTIN, LOW);
    }

    if (!ledBlink && tick > 4)
    {
      ledBlink = !ledBlink;
      tick = 0;
      digitalWrite(LED_BUILTIN, HIGH);
    }
  }
  else
  {
    digitalWrite(LED_BUILTIN, HIGH);
  }
}
