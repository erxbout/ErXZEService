using ErXBoutCode.MVVM;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ErXZEServices
{
    public class CanPacket
    {
        public UInt64 PaketData { get; set; }

        public string DataString { get; set; }

        public string DataStringFormat
        {
            get
            {
                StringBuilder result = new StringBuilder();

                for (int i = 0; i < DataString.Length; i++)
                {
                    char c = DataString[i];

                    result.Append(c);
                    if ((i + 1) % 2 == 0)
                        result.Append(' ');
                }

                return result.ToString();
            }
        }

        public string ID { get; set; }

        public CanPacket(string id)
        {
            ID = id;
        }

        public void ParsePaket(string dataString)
        {
            DataString = dataString;
            PaketData = UInt64.Parse(dataString, System.Globalization.NumberStyles.HexNumber);
        }

        public override string ToString()
        {
            return $"ID: {ID} Data: {DataStringFormat}";
        }
    }

    class MainWindowViewModel : ViewModel
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public MainWindowViewModel()
        {
            var path = @"D:\Desktop\Info\LaufendeProjekte\zzpriv\ErXZEServices\ZoeCanReadout\ZoeCanReadout\CanDataLog\DEBUG.txt";
            var fileContent = File.ReadAllLines(path);

            int skipHeader = 1;

            List<CanPacket> packets = new List<CanPacket>();

            #region ReadPackets
            foreach (string line in fileContent)
            {
                try
                {

                    string[] splitLine = line.Split(": ,".ToCharArray());
                    string id = splitLine[2];
                    string data = "";
                    int dataStart = 0;

                    for (int i = 0; i < splitLine.Length; i++)
                    {
                        if (splitLine[i] == "Data")
                        {
                            dataStart = i + 2 + skipHeader;
                            break;
                        }
                    }

                    for (int i = dataStart; i < splitLine.Length - 1; i++)
                    {
                        string dat = splitLine[i];

                        if (dat.Length == 1)
                            dat = "0" + dat;

                        data += dat;
                    }

                    CanPacket toAdd = new CanPacket(id);
                    toAdd.ParsePaket(data);

                    packets.Add(toAdd);
                }
                catch (Exception e)
                { }
            }
            #endregion

            var dataGroups = packets.GroupBy(x => x.ID);
            var chartValues = new ChartValues<double>();
            var otherChartValues = new ChartValues<double>();
            float torqueBefore = 0f;

            foreach (var dataGroup in dataGroups)
            {
                foreach (var dat in dataGroup)
                {
                    try
                    {
                        if (dataGroup.Key == "656")
                        {
                            UInt64 interestingData = dat.PaketData >> 8;
                            interestingData = interestingData & 0xFFu;
                            int ambientTemperature = (int)interestingData - 40;

                            interestingData = dat.PaketData >> 21;
                            interestingData = interestingData & 0x7FFu;

                            //chartValues.Add(interestingData);

                            //otherChartValues.Add(ambientTemperature);
                            //chartValues.Add(interestingData);
                        }

                        if (dataGroup.Key == "634")
                        {
                            //time to full over 17 hours
                            UInt64 interestingData = dat.PaketData >> 49;
                            interestingData = interestingData & 0x7Fu;
                            //chartValues.Add(interestingData);
                        }

                        if (dataGroup.Key == "654")
                        {
                            //time to full
                            UInt64 interestingData = dat.PaketData >> 22;
                            interestingData = interestingData & 0x3FFu;
                            //chartValues.Add(interestingData);
                        }

                        if (dataGroup.Key == "673")
                        {
                            UInt64 interestingData = dat.PaketData >> 16;
                            interestingData = interestingData & 0xFFu;

                            if (interestingData != 0xFFu)
                            {
                                float wheelPressure = interestingData * 13.725f;
                                //chartValues.Add(wheelPressure);
                            }
                        }

                        if (dataGroup.Key == "186")
                        {
                            UInt64 interestingData = dat.PaketData >> 36;
                            interestingData = interestingData & 0xFFFu;

                            float torque = interestingData - 800;
                            torque = torque * 0.5f;

                            if (torque != torqueBefore)
                            {
                                //if (otherChartValues.Count < 500 && torque < 300)
                                //    chartValues.Add(torque);

                                torqueBefore = torque;
                            }
                        }

                        if (dataGroup.Key == "1F8")
                        {
                            UInt64 interestingData = dat.PaketData >> 13;
                            interestingData = interestingData & 0x7FFu;

                            //Engine RPM
                            //chartValues.Add(interestingData * 10);
                        }

                        if (dataGroup.Key == "35C")
                        {
                            UInt64 interestingData = dat.PaketData >> 8;
                            interestingData = interestingData & 0x1u;

                            interestingData = dat.PaketData >> 24;
                            interestingData = interestingData & 0xFFFFFFu;

                            //Locked State
                            //chartValues.Add(interestingData / 100f);
                        }

                        if (dataGroup.Key == "427")
                        {
                            UInt64 interestingData = dat.PaketData >> 16;
                            interestingData = interestingData & 0xFFu;

                            float avaliableChargePower = interestingData * 0.3f;

                            //otherChartValues.Add(avaliableChargePower);
                        }

                        if (dataGroup.Key == "5D7")
                        {
                            UInt64 interestingData = dat.PaketData >> 20;
                            interestingData = interestingData & 0xFFFFFFFu;

                            if (interestingData != 0xFFFFFFFu)
                            {
                                float km = interestingData * 0.01f;
                                //chartValues.Add(km);
                            }

                            //chartValues.Add(interestingData);
                        }

                        if (dataGroup.Key == "5DE")
                        {
                            UInt64 interestingData = dat.PaketData >> 51;
                            interestingData = interestingData & 0x3u;

                            //chartValues.Add(interestingData);
                        }

                        if (dataGroup.Key == "5EE")
                        {
                            UInt64 interestingData = dat.PaketData >> 23;
                            interestingData = interestingData & 0x1u;

                            //Light sensor status
                            //chartValues.Add(interestingData);
                        }

                        if (dataGroup.Key == "646")
                        {
                            //kwh/100
                            UInt64 interestingData = dat.PaketData >> 48;
                            interestingData = interestingData & 0xFFu;

                            //trip b km
                            interestingData = dat.PaketData >> 31;
                            interestingData = interestingData & 0x1FFFFu;
                            //otherChartValues.Add(interestingData * 0.1);

                            //trip b consumption
                            interestingData = dat.PaketData >> 16;
                            interestingData = interestingData & 0x7FFFu;

                            //trip b kwh/100
                            //chartValues.Add(interestingData * 0.1);
                        }

                        if (dataGroup.Key == "658")
                        {
                            //Charging
                            UInt64 interestingData = dat.PaketData >> 21;
                            interestingData = interestingData & 0x1u;

                            //chartValues.Add(interestingData);
                        }


                        if (dataGroup.Key == "65B")
                        {
                            //FluentDriver -> eco points?
                            UInt64 interestingData = dat.PaketData >> 33;
                            interestingData = interestingData & 0x7Fu;
                            //chartValues.Add(interestingData);

                            //Eco mode
                            interestingData = dat.PaketData >> 37;
                            interestingData = interestingData & 0x3u;
                            //chartValues.Add(interestingData);

                            interestingData = dat.PaketData >> 53;
                            interestingData = interestingData & 0x7FFu;

                            //chartValues.Add(interestingData);
                        }

                        if (dataGroup.Key == "699")
                        {
                            //FluentDriver
                            UInt64 interestingData = dat.PaketData >> 49;
                            interestingData = interestingData & 0x1Fu;

                            //chartValues.Add(interestingData * 0.5);
                        }


                        if (dataGroup.Key == "62D")
                        {
                            //ChargingPower
                            UInt64 interestingData = dat.PaketData;
                            interestingData = interestingData & 0x1FFu;

                            //interestingData = dat.PaketData >> 35;
                            //interestingData = interestingData & 0x1FFu;

                            //chartValues.Add(interestingData * 100);
                        }

                        if (dataGroup.Key == "42E")
                        {
                            UInt64 interestingData = dat.PaketData >> 20;
                            interestingData = interestingData & 0x3Fu;
                            //otherChartValues.Add(interestingData); //63

                            //Max Charging Possibile ChargingPower Depending on SoC and Temperature
                            interestingData = dat.PaketData & 0xFFu; //255
                            float chargingPower = interestingData * 0.3f;

                            interestingData = dat.PaketData >> 13;
                            interestingData = interestingData & 0x7Fu;

                            //chartValues.Add(interestingData - 40); //0
                        }

                        if (dataGroup.Key == "1FD")
                        {
                            UInt64 interestingData = dat.PaketData >> 8;
                            interestingData = interestingData & 0xFFu;
                            interestingData = interestingData - 80;

                            //if (interestingData < 200)
                            //chartValues.Add(interestingData);

                            //Max Charging Possibile ChargingPower Depending on SoC and Temperature
                            interestingData = dat.PaketData & 0xFFu;
                            float chargingPower = interestingData * 0.3f;


                            interestingData = dat.PaketData >> 56;
                            interestingData = interestingData & 0xFFu;
                            //otherChartValues.Add(interestingData * 0.390625);


                            interestingData = dat.PaketData >> 24;
                            interestingData = interestingData & 0x7Fu;

                            //chartValues.Add(interestingData);
                        }

                        if (dataGroup.Key == "35C")
                        {
                            UInt64 interestingData = dat.PaketData >> 24;
                            interestingData = interestingData & 0xFFFFFFu;

                            //otherChartValues.Add(interestingData);
                        }

                        if (dataGroup.Key == "638")
                        {
                            //Aux Consumption
                            UInt64 interestingData = dat.PaketData >> 27;
                            interestingData = interestingData & 0x1Fu;
                            //chartValues.Add(interestingData);


                            //Traction Consumption
                            interestingData = dat.PaketData >> 56;
                            interestingData = interestingData & 0xFFu;
                            //chartValues.Add(interestingData - 80);
                        }

                        if (dataGroup.Key == "412")
                        {
                            //Aux Consumption
                            UInt64 interestingData = dat.PaketData >> 27;
                            interestingData = interestingData & 0x1Fu;
                            //chartValues.Add(interestingData);


                            //Traction Consumption
                            interestingData = dat.PaketData >> 56;
                            interestingData = interestingData & 0xFFu;
                            //chartValues.Add(interestingData - 80);
                        }
                    }
                    catch { }
                }
            }

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Nm",
                    Values = chartValues,
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Temperature",
                    Values = otherChartValues,
                    PointGeometry = DefaultGeometries.Circle
                },
                //new LineSeries
                //{
                //    Title = "Series 3",
                //    Values = new ChartValues<double> { 4,2,7,2,7 },
                //    PointGeometry = DefaultGeometries.Square,
                //    PointGeometrySize = 15
                //}
            };

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            YFormatter = value => value.ToString();

            //modifying the series collection will animate and update the chart
            //SeriesCollection.Add(new LineSeries
            //{
            //    Title = "Series 4",
            //    Values = new ChartValues<double> { 5, 3, 2, 4 },
            //    LineSmoothness = 0, //0: straight lines, 1: really smooth lines
            //    PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
            //    PointGeometrySize = 50,
            //    PointForeground = Brushes.Gray
            //});

            //modifying any series values will also animate and update the chart
            //SeriesCollection[2].Values.Add(5d);

            PropChanged(nameof(SeriesCollection));
        }


    }
}
