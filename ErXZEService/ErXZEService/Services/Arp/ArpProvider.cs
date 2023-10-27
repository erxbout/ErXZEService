using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ErXZEService.Services.Arp
{
    public static class ArpProvider
    {
        private const string DefaultPath = "/proc/net/arp";

        public static List<ArpMapping> GetArpTableContent(string path = null)
        {
            if (path == null)
                path = DefaultPath;

            try
            {
                var fileContent = File.ReadAllLines(path).ToList();
                var arpTable = new List<ArpMapping>();

                fileContent
                    .Skip(1)
                    .ToList()
                    .ForEach(x =>
                    {
                        var splitted = x.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            // [1] = HW Type [2] = flags (possible wired or wireless on my router)
                            arpTable.Add(new ArpMapping { Ip = splitted[0], Mac = splitted[3], Interface = splitted[5] });
                    });

                return arpTable;
            }
            catch
            {

            }

            return new List<ArpMapping>();
        }
    }
}
