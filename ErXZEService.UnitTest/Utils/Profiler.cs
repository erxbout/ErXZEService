using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ErXZEService.UnitTest.Utils
{
    public class Profiler
    {
        public static Dictionary<string, List<long>> Measurements = new Dictionary<string, List<long>>();

        public static void PrintCallAverage(string name)
        {
            var measurement = Measurements.FirstOrDefault(x => x.Key == name);

            if (measurement.Key != null)
                Debug.WriteLine($"{name} took on avg: {measurement.Value.Average()}");
            else
                Debug.WriteLine("Cannot find call by name: " + name);
        }

        public static void MeasureCall(Action call, string name = null)
        {
            if (name == null)
                name = "Call";

            var watch = new Stopwatch();
            watch.Start();

            //call action
            call.Invoke();

            watch.Stop();
            Debug.WriteLine($"{name} took: {watch.ElapsedMilliseconds} ms");

            var measurement = Measurements.FirstOrDefault(x => x.Key == name);

            if (measurement.Key == null)
                Measurements.Add(name, new List<long>() { watch.ElapsedMilliseconds });
            else
                measurement.Value.Add(watch.ElapsedMilliseconds);
        }
    }
}
