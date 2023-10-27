using ErXZEService.Services.ChargepointPolling.Dtos;
using ErXZEService.Services.ChargepointPolling.Interfaces;
using System;
using System.Threading.Tasks;

namespace ErXZEService.Services.ChargepointPolling
{
	public class OebbChargepointPoller : IOebbChargepointPoller
	{
		public Task<ChargepointPollDto> Poll(string chargepointId)
		{
			throw new NotImplementedException();
		}
	}
}
