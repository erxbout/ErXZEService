
const char Fail[4] = "FCK";
const char Nope[4] = "NOP";
const char Ok[3] = "OK";

void SD_Setup()
{
  pinMode(ChipSelectSD, OUTPUT);

  if (SD.begin(ChipSelectSD))
  {
    SDOnline = true;
    dataFile = SD.open(ImportantFile, FILE_WRITE);
    dataFile.println(F("Strt"));
    dataFile.close();
  }
}

void PrintFileContentsToSerial()
{
  bool reImport = false;
  String filename = (String)YearOffset + Underscore + (String)Month + Underscore + (String)Day + ImportedFileExtension;
  File myFile;

  if (SD.exists(filename))
  {
    myFile = SD.open(filename);
    reImport = true;
  }
  else
  {
    myFile = SD.open(ImportantFile);
  }

  if (!CalibrationDoneThisLifetime || State != StateParked)
  {
    SendOverWifi(Nope, Nope);
    delay(1000);

    return;
  }

  if (myFile)
  {
    int linecount = 0;
    while (myFile.available())
    {
      String line = myFile.readStringUntil('\n');
      SendOverWifi(ImportCommand, line);
      linecount++;
      delay(20);

      if (Serial.available() > 0)
      {
        String cmd = Serial.readString();

        if (cmd == F("BRK"))
        {
          delay(2700);
          SendOverWifi(ImportCommand, line);
        }
      }

      delay(80);
    }

    myFile.close();

    SendOverWifi(F("FIN"), (String)linecount);

    if (!reImport)
    {
      //Rename file
      myFile = SD.open(ImportantFile);
      dataFile = SD.open(filename, FILE_WRITE);

      while (myFile.available())
      {
        dataFile.println(myFile.readStringUntil('\n'));
      }

      dataFile.close();
      myFile.close();

      if (SD.exists(filename))
        SD.remove(ImportantFile);
    }
  }
  else
  {
    SendOverWifi(Fail, Fail);
  }
  delay(10000);
}

void LogEvent(String buf)
{
  if (LogEvents)
  {
    dataFile = SD.open(LogFile, FILE_WRITE);

    if (dataFile)
    {
      dataFile.print(millis());
      dataFile.print(" \t");
      dataFile.println(buf);
    }

    dataFile.close();
  }
}

bool AddProperty(String propertyName, String value)
{
  if (MessageBufferPosition + propertyName.length() + value.length() + 4 > MESSAGE_BUFFER_SIZE)
    return false;

  for (int i = 0; i < propertyName.length(); i++)
  {
    MessageBuffer[MessageBufferPosition] = propertyName[i];
    MessageBufferPosition++;
  }

  MessageBuffer[MessageBufferPosition] = PropertySeperator;
  MessageBufferPosition++;

  for (int i = 0; i < value.length(); i++)
  {
    MessageBuffer[MessageBufferPosition] = value[i];
    MessageBufferPosition++;
  }
  MessageBuffer[MessageBufferPosition] = Seperator;
  MessageBufferPosition++;

  return true;
}

String FinishMessageBuffer()
{
  String result = String(MessageBuffer);
  ClearMessageBuffer();
  return result;
}

void ClearMessageBuffer()
{
  for (int i = 0; i < MessageBufferPosition; i++)
  {
    MessageBuffer[i] = '\0';
  }

  MessageBufferPosition = 0;
}

String GetInfoString(bool important)
{
  int timeSinceIgnition = ZoesAbsoluteTime - IgnitionTime;
  String str;
  str += (String)timeSinceIgnition;
  str += Seperator;

  str += (String)AvaliableEnergyKWH;
  str += Seperator;

  str += (String)UserSOC;
  str += Seperator;

  str += (String)EstimatedRange;
  str += Seperator;

  str += (String)BatteryTemperature;
  str += Seperator;

  for (int i = 0; i < str.length(); i++)
  {
    MessageBuffer[MessageBufferPosition] = str[i];
    MessageBufferPosition++;
  }

  if (!important && (CCSpeedChange || CCModeChange))
  {
    AddProperty(F("CCM"), (String)CCMode);
    AddProperty(F("CCS"), (String)CCSpeed);
    CCSpeedChange = false;
    CCModeChange = false;
  }

  if (AmbientTemperatureChanged)
  {
    AddProperty(F("At"), (String)AmbientTemperature);

    if (important)
      AmbientTemperatureChanged = false;
  }

  if (ClimaModeChangedVar || PreHeatingActivation)
  {
    AddProperty(F("CM"), (String)ClimaMode);

    if (PreHeatingActivation)
      SomethingTheServerHasToKnowChanged = true;
    if (important)
      ClimaModeChangedVar = false;
  }

  if (StateChanged)
  {
    if (AddProperty(F("ST"), (String)State) && important)
      StateChanged = false;

    AddProperty(F("GR"), (String)GearLeverPosition);
  }

  if (StartKWH > 0)
  {
    AddProperty(F("PA"), (String)PilotAmpere);
    AddProperty(F("ACh"), (String)AvailableChargingPower);
    MaxChargingPowerChanged = true;
  }

  if (MaxChargingPowerChanged)
  {
    AddProperty(F("MxC"), (String)MaxChargingPower);

    if (important)
      MaxChargingPowerChanged = false;
  }

  if (TimeToFullChanged)
  {
    AddProperty(F("TtF"), (String)TimeToFull);

    if (important)
      TimeToFullChanged = false;
  }

  if (LoadedKWH > 0)
  {
    if (AddProperty(F("LdK"), (String)LoadedKWH) && AddProperty(F("LdR"), (String)LoadedRange) && important)
    {
      LoadedKWH = 0;
      LoadedRange = 0;
    }
  }

  if (DrivenKWH > 0)
  {
    AddProperty(F("MSpd"), (String)MaxSpeed);
    if (AddProperty(F("DrD"), (String)DrivenDistance) && AddProperty(F("DrK"), (String)DrivenKWH) && important)
    {
      DrivenKWH = 0;
      MaxSpeed = 0;
      DrivenDistance = 0;
    }
  }

  if (TimePassed > 0)
  {
    //EventPassedTime in Minutes
    AddProperty(F("EvT"), (String)TimePassed);

    if (important)
      TimePassed = 0;
  }

  if (TripBChange)
  {
    //TripB
    AddProperty(F("tbA"), (String)AverageConsumption);
    AddProperty(F("tbD"), (String)TripDistance);
    AddProperty(F("tbK"), (String)UsedKWH);
    AddProperty(F("tbS"), (String)AverageSpeed);

    if (important)
      TripBChange = false;
  }

  if (!SomethingTheServerHasToKnowChanged && ZoesAbsoluteTime > 0)
  {
    AddProperty(F("Dt"), !CalibrationDoneThisLifetime ? GetDateString() : GetDateTimeString());
  }

  return FinishMessageBuffer();
}

void SaveImportantToSD()
{
  dataFile = SD.open(ImportantFile, FILE_WRITE);

  if (dataFile)
  {
    String str = GetInfoString(true);
    dataFile.println(str);

    dataFile.close();
    SomethingImportantChangedVar = false;
    SomethingChangedVar = false;
  }
}

bool knownMessageId(int messageId)
{
  if (messageId == 0x29C || messageId == 0x42a || messageId == 0x427 || messageId == 0x646 || messageId == 0x654 || messageId == 0x656 || messageId == 0x658 || messageId == 0x66A || messageId == 0x42e || messageId == 0x17e || messageId == 0x653)
    return true;

  return false;
}
