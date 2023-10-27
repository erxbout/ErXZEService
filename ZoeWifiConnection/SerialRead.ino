
void SerialReadLoop()
{
  bool messageStarted = false;
  bool messageEnded = false;
  String command = "";
  ReportToServerRequest = "";
  ValueReportToServerRequest = "";
  int lastBufferPosition = SerialBufferPosition;
  while (Serial.available() > 0)
  {
    char sRead = Serial.read();

    // check for message start.. command should be finished transmitting by now
    if (sRead == MessageStart)
    {
      // things that are handled by serial command handling like time and send and send_time
      SerialCommandHandling(command);

      if (ReportToServerRequested)
      {
        SerialBufferPosition -= command.length();
      }
      else if (ScheduledServerCommunication.ValueProperty)
      {
        // things that are not handled by serial command handling like rSOC and uSOC
        // logic adds additional message seperator to be able to handle it in app later
        if (command.length() > 0)
        {
          ValueReportToServerRequested = true;
          ValueReportToServerTopic = command;
          SerialBufferPosition -= command.length();
          AddToSerialBuffer(MessageSeperator);

          for (uint i = 0; i < command.length(); i++)
            AddToSerialBuffer(command[i]);
        }
      }

      command = "";
      sRead = MessageSeperator;
      messageStarted = true;
    }

    // transmitting command
    if (!messageStarted)
      command += sRead;

    // finishing Message
    if (sRead == MessageEnd)
    {
      ValueReportToServerRequested = false;
      ReportToServerRequested = false;
      messageEnded = true;
    }

    if (messageStarted && !messageEnded)
    {
      if (ValueReportToServerRequested)
        ValueReportToServerRequest += sRead;

      if (ReportToServerRequested)
        ReportToServerRequest += sRead;
    }

    if (!messageEnded)
    {
      AddToSerialBuffer(sRead);
    }
    delay(1);

    if (messageEnded && SerialBufferPosition > UDP_TX_PACKET_MAX_SIZE - 100 && Serial.available() > 0)
      break;
  }

  //Reset Buffer Position if Message was not transmitted in one go
  if (!messageStarted || !messageEnded)
  {
    SerialBufferPosition = lastBufferPosition;
  }
  else
  {
    SerialBufferLastMessageStart = lastBufferPosition;

    if (messageEnded && SerialBufferPosition > UDP_TX_PACKET_MAX_SIZE - 100)
    {
      //todo: enter last message into serial buffer -> or better remove oldest message from serial buffer
      //String lastMessage = LastMessageInSerialBuffer();
      ClearSerialBuffer();
    }
  }
}

void UdpSendMe(char* buf)
{
  Udp.beginPacket(IPAddress(192, 168, 8, 144), localPort);
  Udp.write(buf);
  Udp.endPacket();
}

void SerialCommandHandling(String cmd)
{
  if (cmd == SerialCommand_ReportAndTimeCalibration)
  {
    ScheduledServerCommunication.Time = true;
    ScheduledServerCommunication.SendStatus = true;
  }

  if (cmd == SerialCommand_ReportToServer)
    ScheduledServerCommunication.SendStatus = true;

  if (cmd == SerialCommand_TimeCalibration)
    ScheduledServerCommunication.Time = true;

  if (cmd == SerialCommand_rSOC || cmd == SerialCommand_uSOC)
    ScheduledServerCommunication.ValueProperty = true;

  ReportToServerRequested = ScheduledServerCommunication.Time || ScheduledServerCommunication.SendStatus;
}

String SerialBufferToString()
{
  String result = "";

  for (int i = 0; i < SerialBufferPosition - 1; i++)
  {
    result += SerialBuffer[i];
  }

  return result;
}

void AddToSerialBuffer(char c)
{
  SerialBuffer[SerialBufferPosition] = c;
  SerialBufferPosition++;
}

String LastMessageInSerialBuffer()
{
  String result = "";

  for (int i = SerialBufferLastMessageStart; i < SerialBufferPosition - 1; i++)
  {
    result += SerialBuffer[i];
  }

  return result;
}

void ClearSerialBuffer()
{
  Warn("ClearSerialBuffer");
  for (int i = 0; i < SerialBufferPosition; i++)
  {
    SerialBuffer[i] = '\0';
  }

  SerialBufferPosition = 0;
}
