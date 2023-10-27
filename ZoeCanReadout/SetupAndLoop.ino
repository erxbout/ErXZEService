
extern unsigned int __bss_end;
extern unsigned int __heap_start;
extern void *__brkval;

uint16_t getFreeSram() {
  uint8_t newVariable;
  // heap is empty, use bss as start memory address
  if ((uint16_t)__brkval == 0)
    return (((uint16_t)&newVariable) - ((uint16_t)&__bss_end));
  // use heap end as the start of the memory address
  else
    return (((uint16_t)&newVariable) - ((uint16_t)__brkval));
};

void setup() {
  Serial.begin(115200);
  delay(200);

  CanReadoutSetup();
  SD_Setup();
  EEPROM_Setup();
}

void loop() {
  CanReadoutLoop();

  //SomethingChanged();
  //SomethingImportantChanged();
  //TripBChange = true;
  //ClimaModeChangedVar = true;
  //TimeToFullChanged = true;
  //StateChanged = true;
  //MaxChargingPowerChanged = true;
  //AmbientTemperatureChanged = true;

  SerialManagementLoop();

  if (OffsetChanged)
    EEPROM_Loop();

  if (!SDOnline && IsWaitingTimeOver(LastNoCardSend, 10000))
  {
    LastNoCardSend = millis();
    //send with command no card (nc)
    SendOverWifi("NC", "");
  }

  if (State == StateParked && IsWaitingTimeOver(LastNoCardSend, 10000))
  {
    LastNoCardSend = millis();
    //Reuse no card send for state change, would not fire if no card detected
    StateChanged = true;
  }

  // If nothing happened for 10 seconds then just send base stats again
  if (IsWaitingTimeOver(LastSendOverWifi, 10000) && !SomethingChangedVar && !SomethingImportantChangedVar)
  {
    TriggerWifiSend();
  }

  if (SomethingChangedVar || SomethingImportantChangedVar)
  {
    //LogEvent("SRAM" + (String)getFreeSram());
    TriggerWifiSend();

    if (SDOnline)
    {
      if (SomethingImportantChangedVar)
        SaveImportantToSD();
    }
    else
    {
      SomethingImportantChangedVar = false;
    }
  }
}

void TriggerWifiSend()
{
  //!SDOnline as parameter for resetting variables after send on serial if no SDCard is avaliable... no sdcard is NOT RECOMMENDED if you want statistical info
  String str = GetInfoString(!SDOnline);
  String command;

  if (SomethingTheServerHasToKnowChanged)
  {
    if (CalibrationRequestAllowed)
      command = F("SEND_TIME");
    else
      command = F("SEND");

    SomethingTheServerHasToKnowChanged = false;
  }
  else if (CalibrationRequestAllowed)
  {
    command = F("TIME");
  }

  SendOverWifi(command, str);
}
