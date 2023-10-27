
void CanReadoutSetup()
{
  if (Canbus.init(CANSPEED_500)) //Initialise MCP2515 CAN controller at the specified speed
    Serial.println(F("CAN Init ok"));
  else
    Serial.println(F("CAN Init fail"));

  delay(500);
}


void CanReadoutLoop()
{
  tCAN message;

  if (mcp2515_check_message())
  {
    if (mcp2515_get_message(&message))
    {
      ComputeBuffer(message.id, ReadBuffer(message));

      //if (message.id == 0x35c || message.id == 0x673 || message.id == 0x7ec)
      //{
      //DebugToSD(message);
      //}
    }
  }
}

void ComputeBuffer(int messageId, uint64_t bufferVar)
{
  if (messageId == 0x29C)
  {
    float currentSpeed = bufferVar & 0xFFFFu;
    currentSpeed = currentSpeed / 100;

    if (Speed != currentSpeed)
    {
      if (currentSpeed > MaxSpeed && IsDriving)
        MaxSpeed = currentSpeed;
    }

    Speed = currentSpeed;
  }

  if (messageId == 0x42a)
  {
    int climaMode = ((bufferVar >> 13) & 0x7u);

    if (climaMode != ClimaMode)
      ClimaModeChanged(climaMode);

    ClimaMode = climaMode;
  }

  if (messageId == 0x427)
  {
    int avaliableEnergyKWH = ((bufferVar >> 6) & 0x1FFu);
    int availableChargingPower = ((bufferVar >> 16) & 0xFF) * 3;
    bool chargeAvaliable = (bufferVar & 0x20u);

    if (AvailableChargingPower != availableChargingPower)
    {
      AvailableChargingPower = availableChargingPower;
      SomethingChanged();
    }

    if (AvaliableEnergyKWH != avaliableEnergyKWH && avaliableEnergyKWH != (0x1FFu))
    {
      AvaliableEnergyChanged(avaliableEnergyKWH);
      AvaliableEnergyKWH = avaliableEnergyKWH;
      SomethingImportantChanged();
    }

    if (ChargeAvaliable != chargeAvaliable)
    {
      if (chargeAvaliable)
      {
        ChargingStarted();
      }
      else
      {
        if (StartKWH > 0)
          ChargingEnd();
      }
      SomethingImportantChanged();
    }

    ChargeAvaliable = chargeAvaliable;
  }

  if (messageId == 0x646)
  {
    int averageConsumption = ((bufferVar >> 48) & 0xFFu);
    long tripDistance = ((bufferVar >> 31) & 0x1FFFFu);
    int usedKWH = ((bufferVar >> 16) & 0x7FFFu) * 0.1;
    int averageSpeed = ((bufferVar >> 4) & 0xFFFu);

    if (averageConsumption != AverageConsumption || tripDistance != TripDistance || usedKWH != UsedKWH || averageSpeed != AverageSpeed)
    {
      AverageConsumption = averageConsumption;
      TripDistance = tripDistance;
      UsedKWH = usedKWH;
      AverageSpeed = averageSpeed;
      TripBChange = true;
      TripBChanged();
    }
  }

  if (messageId == 0x654)
  {
    int estimatedRange = (bufferVar >> 12) & 0x3FFu;
    int userSOC = (bufferVar >> 32) & 0x7Fu;
    int timeToFull = (bufferVar >> 22) & 0x3FFu;

    if (timeToFull != 0x3FFu && TimeToFull != timeToFull)
    {
      TimeToFull = timeToFull;
      ChargeTimeChanged();
    }

    if (EstimatedRange != estimatedRange && estimatedRange != 0x3FFu)
    {
      EstimatedRange = estimatedRange;
      RangeChanged(estimatedRange);
      SomethingChanged();
    }

    if (UserSOC != userSOC && userSOC != 0x7Fu)
    {
      UserSoCChanged(userSOC);
      UserSOC = userSOC;
    }
  }

  if (messageId == 0x656)
  {
    int temperature = ((bufferVar >> 8) & 0xFFu);
    temperature = temperature - 40;

    if (AmbientTemperature != temperature && temperature != 0xFFu)
    {
      AmbientTemperature = temperature;
      AmbientTemperatureChange();
    }
  }

  if (messageId == 0x66a)
  {
    byte ccMode = (bufferVar >> 56) & 0x7u;
    byte ccSpeed = (bufferVar >> 48) & 0xFFu;

    if (CCSpeed != ccSpeed)
    {
      CCSpeedChange = true;
      CCSpeed = ccSpeed;
    }
    if (CCMode != ccMode)
    {
      CCModeChanged();
      CCMode = ccMode;
    }
  }

  if (messageId == 0x42e)
  {
    float soc = ((bufferVar >> 51) & 0x1FFFu) * 0.02;
    int batteryTemperature = ((bufferVar >> 13) & 0x7Fu) - 40;
    int pilotAmpere = (bufferVar >> 20) & 0x3Fu;
    int maxChargingPower = (bufferVar & 0xFFu) * 3;
    byte batteryTemperatureDiff = BatteryTemperature - batteryTemperature;
    bool isInvalidPacket = pilotAmpere == 0x3Fu && batteryTemperature == 0 && batteryTemperatureDiff > 2;

    if (SOC != soc)
      rSOCChanged(soc);

    if (!isInvalidPacket)
    {
      if (MaxChargingPower != maxChargingPower)
      {
        MaxChargingPower = maxChargingPower;
        MaxChargingPowerChange();
      }

      if (PilotAmpere != pilotAmpere) //pilotAmpere == 0x3Fu (63) wenn 43kW lader, überdenken
      {
        ChargingPilotAmpereChanged(pilotAmpere);

        PilotAmpere = pilotAmpere;
      }

      if (BatteryTemperature != batteryTemperature)
      {
        BatteryTemperature = batteryTemperature;
        BatteryTemperatureChanged();
      }
      SOC = soc;
    }
  }

  if (messageId == 0x17e)
  {
    int gearLeverPosition = (bufferVar >> 12) & 0xFu;

    if (GearLeverPosition != gearLeverPosition && gearLeverPosition != 10)
      GearLeverPositionChanged(gearLeverPosition);

    if (IsDriving != gearLeverPosition > 0 && gearLeverPosition != 10)
    {
      if (gearLeverPosition > 0 && !IsDriving)
        DrivingStarted();

      if (gearLeverPosition == 0 && IsDriving)
        DrivingEnded();

      IsDriving = gearLeverPosition > 0;
    }

    GearLeverPosition = gearLeverPosition;
  }

  if (messageId == 0x1f8)
  {
    int engineRPM = (bufferVar >> 13) & 0x7FF;
    engineRPM = engineRPM * 10;

    if (EngineRPM != engineRPM)
    {
      EngineRPM = engineRPM;

      EngineRPMChanged();
    }
  }

  if (messageId == 0x1fd)
  {
    int consumption = (bufferVar >> 8) & 0xFF;
    int preHeatingActivation = (bufferVar >> 31) & 0x1;
    consumption = consumption - 0x50;

    if (PreHeatingActivation != preHeatingActivation)
    {
      PreHeatingActivation = preHeatingActivation;
      LogEvent("PreHeating:" + (String)preHeatingActivation);
    }

    if (Consumption != consumption)
    {
      Consumption = consumption;
    }
  }

  if (messageId == 0x673)
  {
    //hr.. hinten rechts
    //vl.. vorne links
    long hrPressure = (bufferVar >> 40) & 0xFF;
    long hlPressure = (bufferVar >> 32) & 0xFF;
    int vrPressure = (bufferVar >> 24) & 0xFF;
    int vlPressure = (bufferVar >> 16) & 0xFF;

    if (HrPressure != hrPressure || HlPressure != hlPressure || VrPressure != vrPressure || VlPressure != vlPressure)
    {
      if (hrPressure != 0xFF)
        HrPressure = hrPressure;

      if (hlPressure != 0xFF)
        HlPressure = hlPressure;

      if (vrPressure != 0xFF)
        VrPressure = vrPressure;

      if (vlPressure != 0xFF)
        VlPressure = vlPressure;

      PressureChanged();
    }
  }

  if (messageId == 0x35c)
  {
    uint32_t absTime = (bufferVar >> 24) & 0xFFFFFFu;
    uint32_t timeDiff = absTime - ZoesAbsoluteTime;
    bool absoluteTimeChange = false;

    if (ZoesAbsoluteTime != absTime)
    {
      absoluteTimeChange = ZoesAbsoluteTime == 0;
      ZoesAbsoluteTime = absTime;

      while (timeDiff > 525600) //Year in Minutes
      {
        Year++;
        timeDiff -= 525600;
      }

      while (timeDiff > 43800) //Month in Minutes
      {
        Month++;
        timeDiff -= 43200;
      }

      while (timeDiff > 1440) //Day in Minutes
      {
        Day++;
        timeDiff -= 1440;
      }

      while (timeDiff > 60) //Hour in Minutes
      {
        Hour++;
        timeDiff -= 60;
      }

      Minute += timeDiff;

      if (Minute >= 60)
      {
        Hour++;
        Minute -= 60;
      }

      while (Hour >= 24)
      {
        Day++;
        Hour -= 24;
      }

      int maxDays = 31;
      if (Month % 2 == 0)
      {
        maxDays = 30;
      }

      if (Month == 2)
      {
        maxDays = 29;
      }

      while (Day > maxDays)
      {
        Month++;
        Day -= maxDays;
      }

      while (Month > 12)
      {
        Year++;
        Month -= 12;
      }

      CalibrationRequestAllowed = CalibrationRequestAllowed || Hour > 24 || Minute > 60;

      if (absoluteTimeChange)
      {
        SomethingImportantChanged();
        AbsoluteTimeChange();
      }
    }
  }
}


uint64_t ReadBuffer(tCAN message)
{
  uint64_t bufferVar = 0;
  
  if (Log)
    DebugToSD(message);

  for (int i = 0; i < message.header.length; i++)
  {
    if (bufferVar == 0)
      bufferVar = message.data[i];
    else
    {
      bufferVar = bufferVar << 8;
      bufferVar = bufferVar + message.data[i];
    }
  }
  return bufferVar;
}
