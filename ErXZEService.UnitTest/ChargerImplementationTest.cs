using ErXZEService.Services.Charger.GoeCharger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ErXZEService.UnitTest
{
    [TestClass]
    public class ChargerImplementationTest
    {
        [TestMethod]
        [Ignore]
        public void SetPilotAmpere()
        {
            var charger = new GoeCharger(new GoeChargerSettingItem { Endpoint = "192.168.1.100" });
            charger.RefreshState();

            Thread.Sleep(1000);

            var watch = new Stopwatch();
            watch.Start();
            charger.SetPilotAmpere(8);
            watch.Stop();
            Debug.WriteLine("request took :" + watch.ElapsedMilliseconds.ToString() + "ms");
            Assert.AreEqual(8, charger.DataItem.PilotAmpere);
            
            Thread.Sleep(1000);
            charger.SetPilotAmpere(10);
            Assert.AreEqual(10, charger.DataItem.PilotAmpere);
        }
    }
}
