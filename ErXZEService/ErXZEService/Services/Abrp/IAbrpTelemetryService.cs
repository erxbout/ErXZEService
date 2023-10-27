using System.Threading.Tasks;

namespace ErXZEService.Services.Abrp
{
    public interface IAbrpTelemetryService
    {
        string UserToken { set; }
        Task<bool> SendState(ElectricCarTelemetryItem item);
    }
}