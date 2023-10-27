
void SomethingChanged()
{
  SomethingChangedVar = true;
}

void SomethingImportantChanged()
{
  SomethingImportantChangedVar = true;
}

void AbsoluteTimeChange()
{
  //Serial.println(GetDateTimeString());
  CalibrationRequestAllowed = true;
  
  if (IgnitionTime == 0)
  {
    IgnitionTime = ZoesAbsoluteTime;
  }
}
