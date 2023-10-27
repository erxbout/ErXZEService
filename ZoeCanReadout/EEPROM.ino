const byte minOffset = 5;

void EEPROM_Setup()
{
  byte address = EEPROM_START_ADDRESS;

  YearOffset = EEPROM.read(address++);
  MonthOffset = EEPROM.read(address++);
  DayOffset = EEPROM.read(address++);
  HourOffset = EEPROM.read(address++);
  MinuteOffset = EEPROM.read(address);

  Year = TotalYearOffset + YearOffset;
  Month = MonthOffset;
  Day = DayOffset;
  Hour = HourOffset;
  Minute = MinuteOffset;
  
  LogEvent("RdOffst:" + (String)YearOffset + "." + (String)MonthOffset + "." + (String)DayOffset + " " + (String)HourOffset + ":" + (String)MinuteOffset);
}

void EEPROM_Loop() {
  byte address = EEPROM_START_ADDRESS;

  byte yearOffset = EEPROM.read(address++);
  byte monthOffset = EEPROM.read(address++);
  byte dayOffset = EEPROM.read(address++);
  byte hourOffset = EEPROM.read(address++);
  byte minuteOffset = EEPROM.read(address);

  address = EEPROM_START_ADDRESS;

  if (abs(yearOffset - YearOffset) > 0)
  {
    EEPROM.update(address, YearOffset);
  }
  address++;

  if (abs(monthOffset - MonthOffset) > 0)
  {
    EEPROM.update(address, MonthOffset);
  }
  address++;

  if (abs(dayOffset - DayOffset) > 0)
  {
    EEPROM.update(address, DayOffset);
  }
  address++;

  if (abs(hourOffset - HourOffset) > minOffset)
  {
    EEPROM.update(address, HourOffset);
  }
  address++;

  if (abs(minuteOffset - MinuteOffset) > (minOffset * 10))
  {
    EEPROM.update(address, MinuteOffset);
  }

  LogEvent("Offset:" + (String)YearOffset + "." + (String)MonthOffset + "." + (String)DayOffset + " " + (String)HourOffset + ":" + (String)MinuteOffset);
  OffsetChanged = false;
}
