
void AvaliableEnergyChanged(int avaliableEnergyKWH)
{
  if (PilotAmpere > 0 && abs(LastAvaliableEnergy - avaliableEnergyKWH) >= 2)
  {
    LastAvaliableEnergy = avaliableEnergyKWH;
    TimeToFullChanged = true;
    SomethingTheServerHasToKnowChanged = true;
  }

  SomethingChanged();
}

void MaxChargingPowerChange()
{
  MaxChargingPowerChanged = true;
  SomethingChanged();

  //most interesting while charging, do not need to send differences in MaxChargingPower if the change is under 6
  if (PilotAmpere > 0 && AvailableChargingPower >= MaxChargingPower && abs(LastMaxChargingPower - MaxChargingPower) > 6)
  {
    LastMaxChargingPower = MaxChargingPower;
    TimeToFullChanged = true;
    SomethingTheServerHasToKnowChanged = true;
  }
}

void ChargingPilotAmpereChanged(int pilotAmpere)
{
  SomethingChanged();
  //String str = "PA: " + (String) pilotAmpere + " MxPower3~: " + (String)MaxPowerThreePhase;

  if (pilotAmpere > 0 && State != StateCharging)
    ChargingStarted();

  //LogEvent(str);
}

void ChargeTimeChanged()
{
  SomethingChanged();
  TimeToFullChanged = true;
}

void ChargingStarted()
{
  State = StateCharging;
  StateChanged = true;

  StartKWH = AvaliableEnergyKWH;
  StartRange = EstimatedRange;

  SomethingTheServerHasToKnowChanged = true;
  SomethingChanged();
  EventStartTime = ZoesAbsoluteTime;
  //String str = "ChargeStart: NrG: " + (String)AvaliableEnergyKWH + "kWh";

  //LogEvent(str);
}

void ChargingEnd()
{
  int energyLoaded = AvaliableEnergyKWH - StartKWH;
  int rangeLoaded = EstimatedRange - StartRange;

  StartKWH = 0;
  StartRange = 0;
  LoadedKWH = energyLoaded;
  LoadedRange = rangeLoaded;

  ChargingEnded();
}

void ChargingEnded()
{
  State = StateParked;
  StateChanged = true;

  SomethingTheServerHasToKnowChanged = true;
  SomethingChanged();
  TimeToFull = 0;
  TimePassed = ((int)ZoesAbsoluteTime) - EventStartTime;
  //String str = "ChargeStop! LdK: " + (String) LoadedKWH + " kWh LdR: " + (String)LoadedRange + " Time: " + (String)TimePassed;

  //LogEvent(str);
}

void RangeChanged(int estimatedRange)
{
  SomethingChanged();
}

void rSOCChanged(float rSOC)
{
  if (IsWaitingTimeOver(LastrSOC, 10000))
  {
    SendOverWifi("rSOC", (String)rSOC);
    LastrSOC = millis();
  }
}

void UserSoCChanged(int userSOC)
{
  if (IsWaitingTimeOver(LastuSOC, 1000))
  {
    SendOverWifi("uSOC", (String)userSOC);
    LastuSOC = millis();
  }
  SomethingChanged();
  String str = "SOC: " + (String)userSOC;

  if (StartKWH > 0 && userSOC == 100)
    ChargingEnd();
  //LogEvent(str);
}
