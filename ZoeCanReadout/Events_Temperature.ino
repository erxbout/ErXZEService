
void BatteryTemperatureChanged()
{
  if (IsWaitingTimeOver(LastBatteryTemperature, 1500))
  {
    LastBatteryTemperature = millis();
    SomethingChanged();
  }
}

void ClimaModeChanged(int climaMode)
{
  SomethingTheServerHasToKnowChanged = true;

  SomethingChanged();
  ClimaModeChangedVar = true;
}

void AmbientTemperatureChange()
{
  AmbientTemperatureChanged = true;
}
