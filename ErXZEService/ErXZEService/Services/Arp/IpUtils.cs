using System.Linq;
using System.Text;

namespace ErXZEService.Services.Arp
{
    public static class IpUtils
    {
        public static string ExtractIpSegment(string ip, int segments = 3)
        {
            var ipPart = ip
                .Split('.')
                .Take(segments);

            var ipSegment = new StringBuilder()
                .Append(string.Join(".", ipPart))
                .ToString();

            return ipSegment;
        }
    }
}
