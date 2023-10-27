

bool AwaitUdpString(String str, int timeout)
{
  uint32_t lastMillis = millis();

  while (true)
  {
    if (IsWaitingTimeOver(lastMillis, timeout))
      return false;

    bool packetAvailable = AwaitUdpPacket(timeout);

    if (packetAvailable)
    {
      String s = UdpReceiveString();

      if (s.indexOf(str) > -1)
      {
        Log("Awaited: " + s);
        return true;
      }
      else
        ReceivedMessage = s;
    }
  }
}

bool AwaitUdpPacket(int timeout)
{
  uint32_t lastMillis = millis();

  while (true)
  {
    if (IsWaitingTimeOver(lastMillis, timeout))
      return false;

    bool packetAvailable = UdpPacketAvailable();

    if (packetAvailable)
      return true;
  }
}

bool UdpPacketAvailable()
{
  int packetSize = Udp.parsePacket();

  if (packetSize)
    return true;
  return false;
}

String UdpReceiveString()
{
  Udp.read(packetBuffer, UDP_TX_PACKET_MAX_SIZE);

  String packet = "";

  for (int i = 0; i < UDP_TX_PACKET_MAX_SIZE; i++)
  {
    char bufferChar = packetBuffer[i];
    if (bufferChar != MessageSeperator)
      packet += packetBuffer[i];
    else
      break;
  }
  return packet;
}

bool UdpReceive()
{
  if (ReceivedMessage.length() > 0)
    return true;

  if (AwaitUdpPacket(100)) {
    ReceivedMessage = UdpReceiveString();

    Log("ReceivedMessage :" + ReceivedMessage);

    if (ReceivedMessage != AckString)
      UdpAck();
    else
    {
      ReceivedMessage = "";
      return false;
    }

    return true;
  }

  if (ReceivedMessage != "")
    return true;

  return false;
}

String ConsumeMessage()
{
  String message = ReceivedMessage;
  ReceivedMessage = "";
  return message;
}

void UdpSend(char* buf)
{
  Udp.beginPacket(addressLock, localPort);
  Udp.write(buf);
  Udp.endPacket();
}

void UdpAck()
{
  UdpSend(AckBuffer);
}
