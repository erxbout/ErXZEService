using ErXZEService.Services.ChargepointPolling.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ErXZEService.Services.ChargepointPolling.Interfaces
{
	public interface IChargepointPoller
	{
		Task<ChargepointPollDto> Poll(string chargepointId);
	}
}
