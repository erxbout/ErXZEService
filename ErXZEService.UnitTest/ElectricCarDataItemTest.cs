using ErXZEService.Models;
using ErXZEService.UnitTest.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ErXZEService.UnitTest
{
    [TestClass]
    public class ElectricCarDataItemTest
    {
        [TestMethod]
        public void BasicMerging()
        {
            var resultItem = new ElectricCarDataItem()
            {
                AmbientTemperature = 44,
                AvaliableEnergy = 100.2m,
                MaxChargePower = 120
            };

            var otherItem = new ElectricCarDataItem()
            {
                AmbientTemperature = 42,
                AvaliableEnergy = 100.2m,
                MaxChargePower = 40
            };

            var otherOtherItem = new ElectricCarDataItem()
            {
                AmbientTemperature = 42,
                AvaliableEnergy = 100.2m,
                MaxChargePower = null
            };

            resultItem.Merge(otherItem);
            resultItem.Merge(otherOtherItem);

            Assert.AreEqual(100.2m, resultItem.AvaliableEnergy);
            Assert.AreEqual((short)42, resultItem.AmbientTemperature);
            Assert.AreEqual(40m, resultItem.MaxChargePower);
        }

        [TestMethod]
        [DataRow(100)]
        [DataRow(1000)]
        [DataRow(10000)]
        public void BasicRepeatedMerging(int repeat)
        {
            var resultItem = new ElectricCarDataItem()
            {
                AmbientTemperature = 44,
                AvaliableEnergy = 100.2m,
                MaxChargePower = 120
            };

            var otherItem = new ElectricCarDataItem()
            {
                AmbientTemperature = 42,
                AvaliableEnergy = 100.2m,
                MaxChargePower = 40
            };

            var otherOtherItem = new ElectricCarDataItem()
            {
                AmbientTemperature = 42,
                AvaliableEnergy = 100.2m,
                MaxChargePower = null
            };

            for (int i = 0; i < repeat; i++)
            {
                resultItem.Merge(otherItem);
                resultItem.Merge(otherOtherItem);
            }

            Assert.AreEqual(100.2m, resultItem.AvaliableEnergy);
            Assert.AreEqual((short)42, resultItem.AmbientTemperature);
            Assert.AreEqual(40m, resultItem.MaxChargePower);
        }

        [TestMethod]
        public void EnumMerging()
        {
            var resultItem = new ElectricCarDataItem()
            {
                ClimaState = ClimaMode.Idle,
                State = ElectricCarState.Driving,
                MaxChargePower = 120
            };

            var otherItem = new ElectricCarDataItem()
            {
                ClimaState = ClimaMode.Heat,
                State = ElectricCarState.Parked,
                MaxChargePower = 123
            };

            var destroyItem = new ElectricCarDataItem()
            {
                ClimaState = ClimaMode.none,
                State = ElectricCarState.none,
                MaxChargePower = null
            };

            Profiler.MeasureCall(() => resultItem.Merge(otherItem), nameof(resultItem.Merge));

            Assert.AreEqual(ClimaMode.Heat, resultItem.ClimaState);
            Assert.AreEqual(ElectricCarState.Parked, resultItem.State);
            Assert.AreEqual(123m, resultItem.MaxChargePower);

            Profiler.MeasureCall(() => resultItem.Merge(destroyItem), nameof(resultItem.Merge));

            Assert.AreEqual(ClimaMode.Heat, resultItem.ClimaState);
            Assert.AreEqual(ElectricCarState.Parked, resultItem.State);
            Assert.AreEqual(123m, resultItem.MaxChargePower);
        }

        [TestMethod]
        [DataRow("90;400;100;324;30;ST:1;LdK:99;LdR:80;EvT:180;Dt:14.06.2019 19.14.00;tbA:132;tbD:6053;tbK:80;tbS:437;")]
        public void RandomEnumMerging(string serialized)
        {
            ElectricCarDataItem dataItem = serialized;
            Random rnd = new Random();

            for (int i = 0; i < 20000; i++)
            {
                int nr = rnd.Next(0, 7);
                ClimaMode clm = (ClimaMode)nr;

                ElectricCarDataItem newItem = new ElectricCarDataItem();
                newItem.ClimaState = clm;

                Profiler.MeasureCall(() => dataItem.Merge(newItem), nameof(dataItem.Merge));
            }

            Profiler.PrintCallAverage(nameof(dataItem.Merge));

            Assert.AreNotEqual(ClimaMode.none, dataItem.ClimaState);
        }

        [TestMethod]
        [DataRow("90;400;100;324;30;ST:1;LdK:99;LdR:80;EvT:180;Dt:14.06.2019 19.14.00;tbA:132;tbD:6053;tbK:80;tbS:437;")]
        public void DeserializeString_CorrectFormatOfNumbers(string serialized)
        {
            ElectricCarDataItem dataItem = serialized;

            Assert.AreEqual(9.9m, dataItem.ChargedKWH);
        }

        [TestMethod]
        [DataRow("90;400;100;324;30;ST:1;LdK:99;LdR:80;EvT:180;tbA:132;tbD:6053;tbK:80;tbS:437;Dt:14.06.2019 19.14.00;")]
        public void SerializeAndDeserializeString(string serialized)
        {
            ElectricCarDataItem dataItem = serialized;

            string serial = dataItem.ToString();

            ElectricCarDataItem newItem = serial;

            Assert.AreEqual(dataItem.ToString(), newItem.ToString());
        }

        [TestMethod]
        [DataRow("3;-1;-1;0;0;PA:23;MxC:189;TtF:225;EvT:13;ST:3;DrK:16;DrD:123;MSpd:83;tbA:186;tbD:8948;tbK:166;tbS:506;CM:7;At:1;Dt:29.01.2020 19.33.22;", 100)]
        [DataRow("3;-1;-1;0;0;PA:23;MxC:189;TtF:225;EvT:13;ST:3;DrK:16;DrD:123;MSpd:83;tbA:186;tbD:8948;tbK:166;tbS:506;CM:7;At:1;Dt:29.01.2020 19.33.22;", 1000)]
        public void DeserializeRepeated(string serialized, int repeat)
        {
            for (int i = 0; i < repeat; i++)
            {
                Profiler.MeasureCall(() => ElectricCarDataItem.Parse(serialized), nameof(ElectricCarDataItem.Parse));
            }

            Profiler.PrintCallAverage(nameof(ElectricCarDataItem.Parse));
        }

        [TestMethod]
        [DataRow(@"
#12;119;31;99;27;RPM:250;SPD:2;ConS:1;ACh:510;Dt:05.08.2019 18:34:16
")]
        public void DecodeUdpMessage(string udpMessage)
        {
            ElectricCarDataItem dataItem = new ElectricCarDataItem();
            string[] splitted = udpMessage.Split((Environment.NewLine + "#").ToCharArray());

            foreach (var msg in splitted)
            {
                try
                {
                    ElectricCarDataItem line = msg;

                    if (line.Timestamp != null && Math.Abs(line.Timestamp.Value.Ticks - DateTime.Now.Ticks) > 3600000000) //6 mins
                    {
                        //Log.Instance.Info("Calibration!");
                        //SendCalibration();
                    }
                    line.Timestamp = DateTime.Now;

                    //if (ConnectionType == ConnectionType.WiFiConnectionNotCompletelyEstablished)
                    //  ConnectionType = ConnectionType.WiFiConnection;

                    dataItem.Merge(line);
                    //PropChanged(nameof(CarData));
                }
                catch (Exception e)
                {
                    //Log.Instance.Error("Error while decode Message", e);
                }
            }
        }

        [TestMethod]
        [DataRow("RPM:1000;ConS:10")]
        public void DeserializeProperties(string serialized)
        {
            ElectricCarDataItem dataItem = serialized;

            Assert.AreEqual(dataItem.EngineRPM, 1000);
            Assert.AreEqual(dataItem.Consumption, (byte)10);
        }

        [TestMethod]
        [DataRow("RPM:1000;ConS:10", "31;292;81;171;5;MxC:42;tbA:186;tbD:8800;tbK:164;tbS:505;Dt:27.01.2020 20.25.50;")]
        public void MergeDeserializeProperties(string serialized, string itemToMergeIn)
        {
            ElectricCarDataItem dataItem = serialized;

            ElectricCarDataItem resultItem = itemToMergeIn;

            resultItem.Merge(dataItem);

            Assert.AreEqual(resultItem.AvaliableEnergy, 29.2m);

            dataItem.Merge(resultItem);

            Assert.AreEqual(resultItem.AvaliableEnergy, 29.2m);
            Assert.AreEqual(true, resultItem.IsFullySpecified);
        }

        [TestMethod]
        [DataRow("3;267;75;160;3;tbA:186;tbD:8948;tbK:166;tbS:506;CM:7;Dt:29.01.2020 19.33.39;", "3;-1;-1;0;0;PA:23;MxC:189;TtF:225;EvT:13;ST:3;DrK:16;DrD:123;MSpd:83;tbA:186;tbD:8948;tbK:166;tbS:506;CM:7;At:1;Dt:29.01.2020 19.33.22;")]
        public void MergeDeserializePropertiesSpecialCase(string serialized, string itemToMergeIn)
        {
            ElectricCarDataItem dataItem = serialized;

            ElectricCarDataItem resultItem = itemToMergeIn;

            resultItem.Merge(dataItem);

            Assert.AreEqual(resultItem.AvaliableEnergy, 26.7m);

            dataItem.Merge(resultItem);

            Assert.AreEqual(resultItem.AvaliableEnergy, 26.7m);
            Assert.AreEqual(true, resultItem.IsFullySpecified);
        }

        [TestMethod]
        [DataRow("3;267;75;160;3;tbA:186;tbD:8948;tbK:166;tbS:506;CM:7;Dt:29.01.2020 19.33.39;", "ConS:-2;")]
        public void MergeNegativeConsumption(string serialized, string consumption)
        {
            ElectricCarDataItem dataItem = serialized;

            ElectricCarDataItem resultItem = consumption;

            dataItem.Merge(resultItem);

            Assert.AreEqual(dataItem.Consumption, (short)-2);
        }
    }
}
