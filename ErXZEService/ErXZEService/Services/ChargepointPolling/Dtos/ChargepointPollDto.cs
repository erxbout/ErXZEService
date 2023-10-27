namespace ErXZEService.Services.ChargepointPolling.Dtos
{
	public class ChargepointPollDto
	{
		public bool Success { get; set; }

		public string Caption { get; set; }

		public string ChargepointId { get; set; }

		public bool IsAvailable { get; set; }
		public string AvailableStatus { get; set; }
	}
}
