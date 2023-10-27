using ErXZEService.Converter;
using ErXZEService.Helper;
using SQLite;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ErXZEService.Utils;
using System.Linq;
using System.Text.RegularExpressions;
using ErXZEService.Services.Log;
using ErXZEService.Services;

namespace ErXZEService.Models
{
    public enum ElectricCarState : byte
    {
        none,
        Parked,
        Driving,
        Charging
    }

    public enum GearLeverPosition : byte
    {
        none = byte.MaxValue,
        Park = 0,
        Reverse = 1,
        Neutral = 2,
        Drive = 7,
        Between = 10
    }

    public enum ClimaMode : byte
    {
        none,
        Cool = 1,
        De_Ice = 2,
        Heat = 4,
        Demist = 6,
        Idle = 7
    }

    public enum CruiseControlMode : byte
    {
        none,
        off,
        limiter,
        helper
    }

    public class IndivFields
    {
        public static List<IIndivFieldConverter> mapping = new List<IIndivFieldConverter>()
        {
            { new IndivFieldsConverter<int>() { SerializedPropertyName = "RPM", PropertyName = nameof(ElectricCarDataItem.EngineRPM) } },
            { new IndivFieldsConverter<byte>() { SerializedPropertyName = "SPD", PropertyName = nameof(ElectricCarDataItem.Speed) } },
            { new IndivFieldsConverter<short>() { SerializedPropertyName = "ConS", PropertyName = nameof(ElectricCarDataItem.Consumption) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "ACh", PropertyName = nameof(ElectricCarDataItem.AvaliableChargePower) } },
            { new IndivFieldsConverter<byte>() { SerializedPropertyName = "GR", PropertyName = nameof(ElectricCarDataItem.GearLeverPositionNumber) } },
            { new IndivFieldsConverter<byte>() { SerializedPropertyName = "CM", PropertyName = nameof(ElectricCarDataItem.ClimaStateNumber) } },
            { new IndivFieldsConverter<byte>() { SerializedPropertyName = "ST", PropertyName = nameof(ElectricCarDataItem.StateNumber) } },
            { new IndivFieldsConverter<byte>() { SerializedPropertyName = "PA", PropertyName = nameof(ElectricCarDataItem.PilotAmpere) } },
            { new IndivFieldsConverter<byte>() { SerializedPropertyName = "CCM", PropertyName = nameof(ElectricCarDataItem.CCMode) } },
            { new IndivFieldsConverter<byte>() { SerializedPropertyName = "CCS", PropertyName = nameof(ElectricCarDataItem.CCSpeed) } },
            { new IndivFieldsConverter<byte>() { SerializedPropertyName = "MSpd", PropertyName = nameof(ElectricCarDataItem.MaxSpeed) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "DrK", PropertyName = nameof(ElectricCarDataItem.DrivenKWH) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "DrD", PropertyName = nameof(ElectricCarDataItem.DrivenDistance) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "LdK", PropertyName = nameof(ElectricCarDataItem.ChargedKWH) } },
            { new IndivFieldsConverter<short>() { SerializedPropertyName = "At", PropertyName = nameof(ElectricCarDataItem.AmbientTemperature) } },
            { new IndivFieldsConverter<short>() { SerializedPropertyName = "EvT", PropertyName = nameof(ElectricCarDataItem.TimeSinceLastEvent) } },
            { new IndivFieldsConverter<short>() { SerializedPropertyName = "TtF", PropertyName = nameof(ElectricCarDataItem.TimeToFull) } },
            { new IndivFieldsConverter<short>() { SerializedPropertyName = "LdR", PropertyName = nameof(ElectricCarDataItem.ChargedRange) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "tbA", PropertyName = nameof(ElectricCarDataItem.TripB_AverageConsumption) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "tbD", PropertyName = nameof(ElectricCarDataItem.TripB_Distance)} },
            { new IndivFieldsConverter<short>() { SerializedPropertyName = "tbK", PropertyName = nameof(ElectricCarDataItem.TripB_UsedBattery) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "tbS", PropertyName = nameof(ElectricCarDataItem.TripB_AverageSpeed) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "MxC", PropertyName = nameof(ElectricCarDataItem.MaxChargePower) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "HrP", PropertyName = nameof(ElectricCarDataItem.BackRightPressure) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "HlP", PropertyName = nameof(ElectricCarDataItem.BackLeftPressure) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "VrP", PropertyName = nameof(ElectricCarDataItem.FrontRightPressure) } },
            { new IndivFieldsConverter<decimal>() { SerializedPropertyName = "VlP", PropertyName = nameof(ElectricCarDataItem.FrontLeftPressure) } },
            { new IndivFieldsConverter<DateTime>() { SerializedPropertyName = "Dt", PropertyName = nameof(ElectricCarDataItem.Timestamp) } }
        };
    }

    public class NoInstantDropToZeroPossible : Attribute
    {
        public int Threshold { get; set; }

        public NoInstantDropToZeroPossible()
        {
            Threshold = 1;
        }

        public NoInstantDropToZeroPossible(int threshold)
        {
            Threshold = threshold;
        }
    }

    public class FullSpecificationNeeded : Attribute
    {
    }

    public class CanDropToZero : Attribute
    {
    }

    public class IgnoredByMerge : Attribute
    {
    }
    public class IgnoredByMqttServerExtension : Attribute
    {
    }

    [Table(nameof(ElectricCarDataItem))]
    public class ElectricCarDataItem : Entity
    {
        [PrimaryKey, AutoIncrement]
        [IgnoredByMerge]
        new public long Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [Ignore]
        [IgnoredByMqttServerExtension]
        public bool IsFullySpecified
        {
            get
            {
                var properties = GetType().GetProperties();

                foreach (var property in properties)
                {
                    if (!property.CanRead || property.PropertyType.IsEnum)
                        continue;

                    var fullSpecificationNeeded = (FullSpecificationNeeded)property.GetCustomAttributeOrDefault(typeof(FullSpecificationNeeded));

                    if (fullSpecificationNeeded == null)
                        continue;

                    var propertyValue = property.GetValue(this);

                    if (propertyValue.Equals(0) || propertyValue.Equals(0m))
                        return false;
                }

                return true;
            }
        }

        [IgnoredByMqttServerExtension]
        public bool HasSpecifiedTimestamp { get; set; }

        public decimal? AvaliableChargePower { get; set; }

        #region Battery
        [NoInstantDropToZeroPossible]
        [FullSpecificationNeeded]
        public decimal AvaliableEnergy { get; set; }

        [NoInstantDropToZeroPossible]
        [FullSpecificationNeeded]
        public short SoC { get; set; }
        #endregion

        #region Charging
        public byte? PilotAmpere { get; set; }
        public bool? ChargeAvaliable { get; set; }

        public decimal? ChargedKWH { get; set; }

        public decimal? MaxChargePower { get; set; }

        public short? ChargedRange { get; set; }

        public short? TimeToFull { get; set; }

        public short? TimeSinceLastEvent { get; set; }
        #endregion

        #region Tires
        public decimal? FrontLeftPressure { get; set; }
        public decimal? BackLeftPressure { get; set; }
                      
        public decimal? FrontRightPressure { get; set; }
        public decimal? BackRightPressure { get; set; }

        public List<decimal> GetTirePressures => new List<decimal>()
        {
            FrontLeftPressure.GetValueOrDefault(0) * 13.725m,
            BackLeftPressure.GetValueOrDefault(0) * 13.725m,
            FrontRightPressure.GetValueOrDefault(0) * 13.725m,
            BackRightPressure.GetValueOrDefault(0) * 13.725m
        };
        #endregion

        #region Driving
        public byte? StateNumber { get; set; }

        [Ignore]
        [IgnoredByMerge]
        public ElectricCarState State
        {
            get
            {
                if (StateNumber.HasValue)
                    return (ElectricCarState)StateNumber;

                return ElectricCarState.none;
            }
            set
            {
                if (value != ElectricCarState.none)
                    StateNumber = (byte)value;
            }
        }

        public byte? GearLeverPositionNumber { get; set; }

        [Ignore]
        [IgnoredByMerge]
        public GearLeverPosition GearLeverPosition
        {
            get
            {
                if (GearLeverPositionNumber.HasValue && GearLeverPositionNumber != (byte)GearLeverPosition.Between)
                    return (GearLeverPosition)GearLeverPositionNumber;

                return GearLeverPosition.none;
            }
            set
            {
                if (value != GearLeverPosition.none && value != GearLeverPosition.Between)
                    GearLeverPositionNumber = (byte)value;
            }
        }

        [NoInstantDropToZeroPossible(660)]
        [IgnoredByMqttServerExtension]
        public int? EngineRPM { get; set; }

        [NoInstantDropToZeroPossible(15)]
        [IgnoredByMqttServerExtension]
        public byte? Speed { get; set; }

        [CanDropToZero]
        [IgnoredByMqttServerExtension]
        public short? Consumption { get; set; }

        [NoInstantDropToZeroPossible(14)]
        [FullSpecificationNeeded]
        public short EstimatedRange { get; set; }

        public decimal? DrivenKWH { get; set; }

        public decimal? DrivenDistance { get; set; }

        public byte? MaxSpeed { get; set; }

        /// <summary>
        /// 5 = tempomat aus, 4 = tempomat ein
        /// </summary>
        [IgnoredByMqttServerExtension]
        public byte? CCMode { get; set; }

        [IgnoredByMqttServerExtension]
        public byte? CCSpeed { get; set; }
        #endregion

        #region TripB
        public decimal? TripB_AverageConsumption { get; set; }

        public decimal? TripB_Distance { get; set; }

        public short? TripB_UsedBattery { get; set; }

        public decimal? TripB_AverageSpeed { get; set; }
        #endregion

        #region Clima
        public byte? ClimaStateNumber { get; set; }

        [Ignore]
        [IgnoredByMerge]
        public ClimaMode ClimaState
        {
            get
            {
                if (ClimaStateNumber.HasValue)
                    return (ClimaMode)ClimaStateNumber;

                return ClimaMode.none;
            }
            set
            {
                if (value != ClimaMode.none)
                    ClimaStateNumber = (byte)value;
            }
        }

        public short? AmbientTemperature { get; set; }

        [NoInstantDropToZeroPossible(1)]
        [FullSpecificationNeeded]
        public short BatteryTemperature { get; set; }
        #endregion

        public DateTime? Timestamp { get; set; }

        [FullSpecificationNeeded]
        public short ArduinoTimeSinceIgnition { get; set; }

        [IgnoredByMqttServerExtension]
        public string DataSource { get; set; }

        [Ignore]
        [IgnoredByMqttServerExtension]
        public decimal MaxPowerThreePhase
        {
            get
            {
                decimal result = (decimal)(PilotAmpere.GetValueOrDefault(0) * 400 * Math.Sqrt(3) / 1000);
                decimal onePhaseResult = PilotAmpere.GetValueOrDefault(0) * 230 / 1000;

                //if (AvaliableChargePower != null && AvaliableChargePower < result)
                //{
                //    float onePhaseDif = Math.Abs(AvaliableChargePower.Value - onePhaseResult);
                //    float threePhaseDif = Math.Abs(AvaliableChargePower.Value - result);

                //    if (onePhaseDif < threePhaseDif)
                //        IsThreePhase = false;
                //    else
                //        IsThreePhase = true;

                //    result = AvaliableChargePower.Value;
                //}

                return Math.Round(result, 1);
            }
        }

        public decimal ChargingPower
        {
            get
            {
                decimal value = MaxChargePower.GetValueOrDefault();
                if (MaxPowerThreePhase < MaxChargePower)
                    value = MaxPowerThreePhase;

                return Math.Round(value, 1);
            }
        }

        public static ElectricCarDataItem GetEmptyElectricCarDataItem()
        {
            ElectricCarDataItem result = "0;0;0;0;0;ST:1;tbA:0;tbD:0;tbK:0;tbS:0;At:0;Dt:01.01.2018 00.00.00;";
            return result;
        }

        public void Merge(ElectricCarDataItem dataItem)
        {
            object locker = new object();

            lock (locker)
            {
                if (dataItem.Id != 0 && Id == 0)
                    Id = dataItem.Id;

                Type t = dataItem.GetType();

                var properties = t.GetProperties();

                foreach (var property in properties)
                {
                    if (!property.CanRead || property.PropertyType.IsEnum)
                        continue;

                    var newValue = property.GetValue(dataItem);
                    var oldValue = property.GetValue(this);

                    var thresholdAttribute = property.GetCustomAttributeOrDefault<NoInstantDropToZeroPossible>();
                    var fullSpecificationNeeded = property.GetCustomAttributeOrDefault<FullSpecificationNeeded>();
                    var canDropToZero = property.GetCustomAttributeOrDefault<CanDropToZero>();
                    var ignoredByMerge = property.GetCustomAttributeOrDefault<IgnoredByMerge>();

                    if (ignoredByMerge != null)
                        continue;

                    if (property.GetValue(this) != newValue && property.CanWrite && newValue != null)
                    {
                        if (fullSpecificationNeeded != null && !dataItem.IsFullySpecified)
                            continue;

                        if ((newValue.Equals(0) || newValue.Equals(0f) || newValue.Equals(0m)) && canDropToZero == null)
                        {
                            if (thresholdAttribute != null && oldValue != null)
                            {
                                int diff = 0;
                                if (newValue.GetType() == typeof(decimal))
                                {
                                    var oldValueDec = ((decimal)oldValue * 10);

                                    if (oldValueDec > 0)
                                    {
                                        var dif = oldValueDec - ((decimal)newValue * 10);
                                        diff = (int)Math.Abs(dif);
                                    }
                                }
                                else
                                {
                                    var oldValueInt = (int)Convert.ChangeType(oldValue, typeof(int));

                                    if (oldValueInt > 0)
                                    {
                                        var dif = oldValueInt - (int)Convert.ChangeType(newValue, typeof(int));
                                        diff = Math.Abs(dif);
                                    }
                                }

                                if (thresholdAttribute.Threshold < diff)
                                    continue;
                            }
                        }

                        property.SetValue(this, newValue);
                        Changed = true;
                    }
                }
            }
        }

        public override string ToString()
        {
            string result = $"{ArduinoTimeSinceIgnition};{Math.Round(AvaliableEnergy * 10, 0)};{SoC};{EstimatedRange};{BatteryTemperature};";

            if (result == "0;0;0;0;0;")
                result = "";

            Type t = this.GetType();

            var properties = t.GetProperties();

            foreach (var property in properties)
            {
                if (!property.CanRead)
                    continue;

                var value = property.GetValue(this);

                if (value != null)
                {
                    if (property.PropertyType.IsEnum)
                        continue;

                    if (value.GetType() == typeof(decimal))
                    {
                        value = Math.Floor((decimal)value * 10);
                    }

                    if (value.GetType() == typeof(DateTime))
                    {
                        if (HasSpecifiedTimestamp)
                            value = ((DateTime)value).ToString("dd.MM.yyyy HH.mm.ss");
                        else
                            value = ((DateTime)value).ToString("dd.MM.yyyy");
                    }

                    foreach (var converter in IndivFields.mapping)
                    {
                        if (converter.PropertyName == property.Name)
                        {
                            result += $"{converter.SerializedPropertyName}:{value};";
                        }
                    }
                }
            }

            return result;
        }

        public ElectricCarDataItem CreateCopy()
        {
            ElectricCarDataItem result = new ElectricCarDataItem();

            result.Merge(this);

            return result;
        }

        public static ElectricCarDataItem Parse(string serialized)
        {
            var logger = IoC.Resolve<ILogger>();

            // do not parse this if # is in front for commenting out lines in import data!
            if (serialized.StartsWith("#"))
                return null;

            var dataItem = new ElectricCarDataItem();
            string[] splitted = serialized.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (splitted.Length <= 1)
                return null;

            int i = 0;

            void parseHeaderV1()
            {
                dataItem.ArduinoTimeSinceIgnition += (short)(Convert.ToInt16(splitted[i++]) * 60);
                dataItem.ArduinoTimeSinceIgnition += Convert.ToByte(splitted[i++]);
                dataItem.AvaliableEnergy = Convert.ToInt16(splitted[i++]) * 0.1m;

                dataItem.SoC = Convert.ToInt16(splitted[i++]);
                dataItem.EstimatedRange = Convert.ToInt16(splitted[i++]);
                dataItem.AmbientTemperature = Convert.ToInt16(splitted[i++]);
            }

            void parseHeaderV2()
            {
                dataItem.ArduinoTimeSinceIgnition = Convert.ToInt16(splitted[i++]);
                dataItem.AvaliableEnergy = Convert.ToInt16(splitted[i++]) * 0.1m;

                dataItem.SoC = Convert.ToInt16(splitted[i++]);
                dataItem.EstimatedRange = Convert.ToInt16(splitted[i++]);
                dataItem.BatteryTemperature = Convert.ToInt16(splitted[i++]);
            }

            void applysplit()
            {
                Parallel.ForEach(splitted.Skip(i), x =>
                //splitted.Skip(i).ToList().ForEach(x =>
                {
                    int index = x.IndexOf(':');

                    if (index == -1)
                        return;

                    string serializedPropertyName = x.Substring(0, index);
                    string propertyValue = x.Substring(++index);

                    Type type = dataItem.GetType();
                    var converter = IndivFields.mapping.Find(y => y.SerializedPropertyName == serializedPropertyName);

                    if (converter != null)
                    {
                        PropertyInfo propInfo = type.GetProperty(converter.PropertyName);
                        propInfo.SetValue(dataItem, converter.GetConvertedValue(propertyValue));
                    }
                    else
                        logger?.LogInformation($"Warning: SerializedPropertyName:{serializedPropertyName} does not exist in mapping; string to parse: {x}");
                });
            }

            try
            {
                SplitHeader(serialized, out string header, out string body);

                var splittedHeader = header.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var headerVersion1 = splittedHeader.Count() == 6;

                if (headerVersion1)
                    parseHeaderV1();
                else if (header != string.Empty)
                    parseHeaderV2();

                applysplit();
            }
            catch (Exception e)
            {
                logger?.LogError($"Ex while parsing serialized string: {serialized}", e);
            }

            if (dataItem.Timestamp.HasValue)
            {
                var dateStr = dataItem.Timestamp.Value.ToString("dd.MM.yyyy HH.mm.ss");

                dataItem.HasSpecifiedTimestamp = serialized.Contains($"Dt:{dateStr}");
            }

            return dataItem;
        }

        public static ElectricCarDataItem TryParse(string serialized, string dataSource = null)
        {
            var logger = IoC.Resolve<ILogger>();
            ElectricCarDataItem result = null;

            if (serialized == string.Empty)
                return null;

            try
            {
                Profiler.MeasureCall(() => result = Parse(serialized), nameof(Parse));

                if (result != null)
                    result.DataSource = dataSource;
            }
            catch (Exception e)
            {
                logger?.LogError("Cannot parse line", e);
            }

            return result;
        }

        public static implicit operator ElectricCarDataItem(string serialized)
        {
            return Parse(serialized);
        }

        private static void SplitHeader(string of, out string header, out string body)
        {
            var pattern = @"[^-;\d]";
            var match = Regex.Match(of, pattern);

            if (match.Success)
            {
                header = of.Substring(0, match.Index);
                body = of.Substring(match.Index);
            }
            else
            {
                header = of;
                body = string.Empty;
            }
        }
    }
}