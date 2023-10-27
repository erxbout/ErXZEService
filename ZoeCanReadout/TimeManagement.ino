
String GetTimeString()
{
  String result;

  if (Hour < 10)
    result += F("0");

  result += (String)Hour + Point;

  if (Minute < 10)
    result += F("0");

  result += (String)Minute + Point + F("00");

  return result;
}

String GetDateString()
{
  String result;

  if (Day < 10)
    result += F("0");

  result += (String)Day + Point;

  if (Month < 10)
    result += F("0");

  result += (String)Month + Point + (String)Year;

  return result;
}

String GetDateTimeString()
{
  return GetDateString() + " " + GetTimeString();
}

void TimeCalibration(String serialRead)
{
  int day = serialRead.substring(0, 2).toInt();
  int month = serialRead.substring(3, 5).toInt();
  
  //to ensure calibration is not done with random string
  if (day == 0 || month == 0)
  {
    LogEvent("time skip: " + serialRead);
    return;
  }

  int year = serialRead.substring(6, 10).toInt();
  int hour = serialRead.substring(11, 13).toInt();
  int minute = serialRead.substring(14, 16).toInt();

  OffsetChanged = (year != Year) || (month != Month) || (day != Day) || (hour != Hour) || (minute != Minute);

  if (year != Year)
  {
    YearOffset = year - TotalYearOffset;
    Year = year;
  }

  if (month != Month)
  {
    MonthOffset += (month - Month);
    Month = month;
  }

  while (MonthOffset > 12)
  {
    MonthOffset -= 12;
    YearOffset--;
  }

  if (day != Day)
  {
    DayOffset += (day - Day);
    Day = day;
  }

  if (hour != Hour)
  {
    HourOffset += (hour - Hour);
    Hour = hour;
  }

  if (minute != Minute)
  {
    MinuteOffset += (minute - Minute);
    Minute = minute;
  }

  if (OffsetChanged)
  {
    SendOverWifi(OkCommand, F("Calibration"));
    CalibrationRequestAllowed = false;
    CalibrationDoneThisLifetime = true;
  }
  else
  {
    SendOverWifi(Nope, F("Calibration"));
  }
}
