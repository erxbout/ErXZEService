using ErXBoutCode.IO;
using ErXBoutCode.Network;
using ErXZEService.ViewModels;
using ErXZEService.Wifi.UnitTest.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Net;
using System.Threading;

namespace ErXZEService.UnitTest
{
    /// <summary>
    /// Used to determine errors in wifi connection and arduino connection
    /// could be savely removed if most issues are solved depending on udp connection
    /// </summary>
    [Ignore]
    [TestClass]
    public class FloodWifiTest
    {
        /// <summary>
        /// needs connected WifiBoard on selected COM Port
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="times"></param>
        [Ignore]
        [TestMethod]
        [DataRow(2000, 20)]
        [DataRow(1000, 40)]
        [DataRow(500, 20)]
        [DataRow(250, 20)]
        public void FloodWifi(int delay, int times)
        {
            var port = new SerialPort("COM8");
            port.BaudRate = 115200;
            port.Open();

            for (int i = 0; i < times; i++)
            {
                port.WriteLine("<del: " + delay + "seq:" + i + ">");

                if (port.BytesToRead > 0)
                {
                    var response = port.ReadLine();
                }

                Thread.Sleep(delay);
            }

            port.Close();
            Thread.Sleep(100);
        }

        [Ignore]
        [TestMethod]
        [DataRow(@"D:\ztestadapter\test.txt")]
        public void ReplicateDataFromFile(string filePath)
        {
            var dataItems = new List<ReplicationDataItem>();
            var txt = new ErXTextFile(filePath);

            foreach (var line in txt.Inhalt)
            {
                try
                {
                    var splitted = line.Split(' ');

                    if (line.Contains("RPM"))
                    {
                        dataItems.Add(new ReplicationDataItem()
                        {
                            ReceiveTime = DateTime.Parse(splitted[0]),
                            DataItemString = splitted[2]
                        });
                    }
                    else
                    {
                        dataItems.Add(new ReplicationDataItem()
                        {
                            ReceiveTime = DateTime.Parse(splitted[0]),
                            DataItemString = line.Contains("2020;") /*|| line.Contains("SOC") */? splitted[2] : splitted[2] + " " + splitted[3]
                        });
                    }
                }
                catch (Exception e)
                {

                }
            }

            var port = new SerialPort("COM5");
            port.BaudRate = 115200;
            port.Open();

            try
            {
                for (int i = 0; i < dataItems.Count; i++)
                {
                    var myDataItem = dataItems[i];
                    var myNextDataItem = (i + 1) < dataItems.Count ? dataItems[i + 1] : null;

                    port.WriteLine("SEND" + myDataItem.DataItemString);
                    Debug.WriteLine("OUTGOING ------------>:" + "SEND" + myDataItem.DataItemString);

                    while (port.BytesToRead > 0)
                    {
                        var response = port.ReadLine();
                        Debug.WriteLine("SERIAL RESPONSE ------" + response.Replace("\r", "<r>").Replace("\n", "<n>").Replace(Environment.NewLine, "<cr>"));
                    }

                    if (myNextDataItem != null)
                        Thread.Sleep(myNextDataItem.ReceiveTime - myDataItem.ReceiveTime);
                }
            }
            finally
            {
                port.Close();
            }
        }

        [Ignore]
        [TestMethod]
        public void CheckArduinoCarriageReturn()
        {
            var port = new SerialPort("COM7");
            port.BaudRate = 115200;
            port.Open();

            try
            {
                port.WriteLine("CLM\r");
                Thread.Sleep(1000);
                for (int i = 0; i < 5; i++)
                {
                    port.WriteLine("CLM\r");
                    Debug.WriteLine("->" + "CLM");

                    while (port.BytesToRead == 0)
                        Thread.Sleep(10);

                    while (port.BytesToRead > 0)
                    {
                        var response = port.ReadLine();
                        Debug.WriteLine("<-" + response.Replace("\r", "<r>").Replace("\n", "<n>").Replace(Environment.NewLine, "<cr>"));
                    }

                    Thread.Sleep(2000);
                }
            }
            finally
            {
                port.Close();
            }
        }
    }
}
