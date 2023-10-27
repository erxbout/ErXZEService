
void DrivingStarted()
{
  State = StateDriving;
  StateChanged = true;
  SomethingChanged();
  EventStartTime = ZoesAbsoluteTime;

  CurrentTripDistanceStart = TripDistance;

  SomethingTheServerHasToKnowChanged = true;
  SomethingImportantChanged();
  DriveStartKWH = AvaliableEnergyKWH;
}

void DrivingEnded()
{
  State = StateParked;
  StateChanged = true;
  SomethingChanged();
  TimePassed = ((int)ZoesAbsoluteTime) - EventStartTime;

  SomethingTheServerHasToKnowChanged = true;
  SomethingImportantChanged();
  DrivenKWH = DriveStartKWH - AvaliableEnergyKWH;
  DrivenDistance = TripDistance - CurrentTripDistanceStart;
}

void CCModeChanged()
{
  SomethingChanged();
  CCModeChange = true;
}

void TripBChanged()
{
  SomethingChanged();

  if (LastServerReportTripDistance == 0)
    LastServerReportTripDistance = TripDistance;

  //Send update to server after each half km
  if (TripDistance - LastServerReportTripDistance > 5)
  {
    LastServerReportTripDistance = TripDistance;
    SomethingTheServerHasToKnowChanged = true;
  }
}

void GearLeverPositionChanged(int gearLeverPosition)
{
  SomethingChanged();
  StateChanged = true;
}

void EngineRPMChanged()
{
  if (LastEngineRPM + 500 < millis() || EngineRPM == 0)
  {
    AddProperty(F("RPM"), (String)EngineRPM);
    AddProperty(F("ConS"), (String)Consumption);
    AddProperty(F("SPD"), (String)Speed);

    SendOverWifi("", FinishMessageBuffer());
    LastEngineRPM = millis();
  }
}

void PressureChanged()
{
  if (LastPressure + 30000 < millis())
  {
    if (HrPressure > 0)
      AddProperty(F("HrP"), (String)HrPressure);

    if (HlPressure > 0)
      AddProperty(F("HlP"), (String)HlPressure);

    if (VrPressure > 0)
      AddProperty(F("VrP"), (String)VrPressure);

    if (VlPressure > 0)
      AddProperty(F("VlP"), (String)VlPressure);

    LastPressure = millis();
  }
}
