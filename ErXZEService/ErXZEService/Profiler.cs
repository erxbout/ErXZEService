using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ErXZEService.Utils
{
    public class Profiler
    {
        public static Dictionary<string, List<long>> Measurements = new Dictionary<string, List<long>>();

        public static void PrintCallAverage(string name)
        {
            Debug.WriteLine(GetCallAverageString(name));
        }

        public static string GetCallAverageString(string name)
        {
            var measurement = Measurements.FirstOrDefault(x => x.Key == name);

            if (measurement.Key != null)
                return $"{name} took on avg: {measurement.Value.Average()}ms";
            else
                return "Cannot find call by name: " + name;
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