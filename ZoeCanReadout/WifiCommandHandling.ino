
void WifiCommandHandling(String cmd)
{
  LogEvent(cmd);

  if (cmd == ImportCommand)
  {
    PrintFileContentsToSerial();
  }

  if (cmd == "MxC")
  {
    AddProperty(F("MxC"), (String)MaxChargingPower);
    SendOverWifi(OkCommand, FinishMessageBuffer());
    return;
  }

  if (cmd == "ST")
  {
    AddProperty(F("ST"), (String)State);
    AddProperty(F("CM"), (String)ClimaMode);
    SendOverWifi(OkCommand, FinishMessageBuffer());
    return;
  }

  SendOverWifi(OkCommand, cmd);
}
