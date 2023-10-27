#include <Canbus.h>
#include <defaults.h>
#include <global.h>
#include <mcp2515.h>
#include <mcp2515_defs.h>

#include <SD.h>
#include <SPI.h>

#include <EEPROM.h>

const String ImportCommand = "IMP";
const char OkCommand[3] = "OK";

#define MESSAGE_BUFFER_SIZE 90
char MessageBuffer[MESSAGE_BUFFER_SIZE];
int MessageBufferPosition = 0;

//Disclaimer: Some float Values with a Resolution of 0.1 (one decimal) are converted to integers and therefore the real value is the variable / 10

unsigned long LastBatteryTemperature = 0;
unsigned long LastSendOverWifi = 0;
unsigned long LastNoCardSend = 5000;

//ZoeTimeOffset
bool CalibrationRequestAllowed = false;
bool CalibrationDoneThisLifetime = false;
bool OffsetChanged = false;
const int TotalYearOffset = 2000;
const byte EEPROM_START_ADDRESS = 0x20;
byte YearOffset = 0;
byte MonthOffset = 0;
byte DayOffset = 4;
byte HourOffset = 22;
byte MinuteOffset = 18;

//time
int Year = 0;
byte Month = 0;
byte Day = 0;
byte Hour = 0;
byte Minute = 0;

uint32_t IgnitionTime = 0;
int EventStartTime = 0;
int TimePassed = 0;
uint32_t ZoesAbsoluteTime = 0;

//serial communication
const char MessageStart = '<';
const char MessageEnd = '>';

//Sd and Log
File dataFile;
const char PropertySeperator = ':';
const char Seperator = ';';
const char Point = '.';
const char Underscore = '_';
const char ImportedFileExtension[5] = ".txt";
const char ImportantFile[8] = "snd.txt";
const char LogFile[10] = "log.txt";
const char DebugFile[10] = "debug.txt";
const char MessageSeperator = '#';

const byte ChipSelectSD = 9;
bool SDOnline = false;
const bool Debug = false;
const bool Log = false;
const bool LogEvents = true;

//Events
bool SomethingChangedVar = false;
bool SomethingImportantChangedVar = false;
bool SomethingTheServerHasToKnowChanged = false;
bool TripBChange = false;
bool ClimaModeChangedVar = false;
bool TimeToFullChanged = false;

byte State = 1;
bool StateChanged = true;

const byte StateParked = 1;
const byte StateDriving = 2;
const byte StateCharging = 3;

//Charge
byte PilotAmpere = 0;
bool ChargeAvaliable = true;
int TimeToFull = 0;

int AvailableChargingPower = 0;
int MaxChargingPower = 0;
int LastMaxChargingPower = 0;
bool MaxChargingPowerChanged = false;

//Driving
bool IsDriving = false;
byte Speed = 0;
byte MaxSpeed = 0;
int EstimatedRange = 0;
byte GearLeverPosition = 0;

bool CCModeChange = false;
bool CCSpeedChange = false;
byte CCSpeed = 0;
byte CCMode = 0;
int CurrentTripDistanceStart = 0;
int DrivenDistance = 0;

int LastServerReportTripDistance = 0;

int AverageConsumption = 0;
long TripDistance = 0;
int UsedKWH = 0;
int AverageSpeed = 0;

unsigned long LastEngineRPM = 0;
int EngineRPM = 0;
int Consumption = 0;

bool PreHeatingActivation = false;

//Battery
unsigned int LastrSOC = 0;
unsigned int LastuSOC = 0;
int LastAvaliableEnergy = 0;
int UserSOC = -1;
float SOC = -1;
int AvaliableEnergyKWH = -1;

//Battery Calculations
int DriveStartKWH = 0;
int DrivenKWH = 0;
int StartKWH = 0;
int LoadedKWH = 0;
int StartRange = 0;
int LoadedRange = 0;

//Clima
bool AmbientTemperatureChanged = false;
byte ClimaMode = 0;
int AmbientTemperature = 0;
int BatteryTemperature = 0;

//Tires
unsigned long LastPressure = 0;
int HrPressure = 0;
int HlPressure = 0;
int VrPressure = 0;
int VlPressure = 0;

//from SD Tab.. not sure why position needs to be here after arduino update..?
void DebugToSD(tCAN message)
{
  dataFile = SD.open(DebugFile, FILE_WRITE);

  if (dataFile)
  {
    dataFile.print(F("ID: "));
    dataFile.print(message.id, HEX);
    dataFile.print(F(", "));
    dataFile.print(F("Data: "));

    for (int i = 0; i < message.header.length; i++)
    {
      dataFile.print(message.data[i], HEX);
      dataFile.print(" ");
    }
    dataFile.println("");

    dataFile.close();
  }
}
