using Newtonsoft.Json;

namespace ErXZEService.Services.Abrp
{
    public class ElectricCarTelemetryItem
    {
        /// <summary>
        /// this is unix epoch in seconds!
        /// </summary>
        [JsonProperty("utc")]
        public long TimeInUnixEpoch { get; set; }

        /// <summary>
        /// default car model for r90 "renault:zoe:q90:40:other"        
        /// </summary>
        [JsonProperty("car_model")]
        public string CarModel { get; set; } = "renault:zoe:r90:40:other";

        [JsonProperty("soc")]
        public int Soc { get; set; }

        /// <summary>
        /// Power in kW.. negative when charging!
        /// </summary>
        [JsonProperty("power")]
        public decimal Power { get; set; }

        [JsonProperty("speed")]
        public int Speed { get; set; }

        [JsonProperty("lat")]
        public int Latitude { get; set; }

        [JsonProperty("lon")]
        public int Longitude { get; set; }

        [JsonProperty("is_charging")]
        public bool IsCharging { get; set; }

        /// <summary>
        /// Here in zoe not possible
        /// </summary>
        [JsonProperty("is_dcfc")]
        public bool IsDcFastCharging { get; set; }

        /// <summary>
        /// Here in zoe not possible
        /// </summary>
        [JsonProperty("is_parked")]
        public bool IsParked { get; set; }

        //optional parameters:
        /// <summary>
        /// Available battery energy in kWh
        /// </summary>
        [JsonProperty("capacity")]
        public decimal AvailableEnergy { get; set; }

        [JsonProperty("soh")]
        public int SoH { get; set; }

        [JsonProperty("batt_temp")]
        public int BatteryTemperature { get; set; }

        [JsonProperty("ext_temp")]
        public int ExternalTemperature { get; set; }

        [JsonProperty("est_battery_range")]
        public int EstimatedRange { get; set; }
    }
}
