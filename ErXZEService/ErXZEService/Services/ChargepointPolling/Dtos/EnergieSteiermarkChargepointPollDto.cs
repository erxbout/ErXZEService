using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ErXZEService.Services.ChargepointPolling.Dtos
{
	public class EnergieSteiermarkChargepointPollDto
	{
		[JsonProperty("success")]
		public bool Success { get; set; }

		[JsonProperty("result")]
		public Result Result { get; set; }
	}

	public class Colors
	{
	}

	public class Connector
	{
		[JsonProperty("plug_type")]
		public string PlugType { get; set; }

		[JsonProperty("max_power_string")]
		public string MaxPowerString { get; set; }

		[JsonProperty("icon")]
		public string Icon { get; set; }
	}

	public class DirectPayment
	{
		[JsonProperty("merchant_id")]
		public string MerchantId { get; set; }

		[JsonProperty("rate_name")]
		public RateName RateName { get; set; }

		[JsonProperty("price")]
		public Price Price { get; set; }

		[JsonProperty("packages")]
		public List<Package> Packages { get; set; }
	}

	public class Gdpr
	{
		[JsonProperty("de")]
		public string De { get; set; }
	}

	public class Invoice
	{
		[JsonProperty("mode")]
		public string Mode { get; set; }

		[JsonProperty("bcc_email")]
		public string BccEmail { get; set; }

		[JsonProperty("bcc_subject")]
		public string BccSubject { get; set; }

		[JsonProperty("prefix")]
		public string Prefix { get; set; }

		[JsonProperty("showExtras")]
		public bool ShowExtras { get; set; }
	}

	public class Legal
	{
		[JsonProperty("de")]
		public string De { get; set; }
	}

	public class Location
	{
		[JsonProperty("latitude")]
		public double Latitude { get; set; }

		[JsonProperty("longitude")]
		public double Longitude { get; set; }
	}

	public class Marketing
	{
		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("contact")]
		public string Contact { get; set; }

		[JsonProperty("payment")]
		public string Payment { get; set; }

		[JsonProperty("hotline")]
		public string Hotline { get; set; }

		[JsonProperty("opening_times")]
		public string OpeningTimes { get; set; }

		[JsonProperty("additional")]
		public string Additional { get; set; }
	}

	public class Package
	{
		[JsonProperty("time_limit")]
		public int TimeLimit { get; set; }

		[JsonProperty("energy_limit")]
		public int EnergyLimit { get; set; }

		[JsonProperty("price_limit")]
		public int PriceLimit { get; set; }
	}

	public class PaymentIntro
	{
		[JsonProperty("de")]
		public string De { get; set; }
	}

	public class PaymentIntroSeperator
	{
		[JsonProperty("de")]
		public string De { get; set; }
	}

	public class Payone
	{
		[JsonProperty("aid")]
		public string Aid { get; set; }

		[JsonProperty("mid")]
		public string Mid { get; set; }

		[JsonProperty("mode")]
		public string Mode { get; set; }

		[JsonProperty("portalid")]
		public string Portalid { get; set; }

		[JsonProperty("hash")]
		public string Hash { get; set; }
	}

	public class Price
	{
		[JsonProperty("starting_fee")]
		public int StartingFee { get; set; }

		[JsonProperty("connected_fee")]
		public double ConnectedFee { get; set; }

		[JsonProperty("energy_fee")]
		public int EnergyFee { get; set; }
	}

	public class Provider
	{
		[JsonProperty("primary_color")]
		public string PrimaryColor { get; set; }

		[JsonProperty("header_color_bg")]
		public string HeaderColorBg { get; set; }

		[JsonProperty("header_color_text")]
		public string HeaderColorText { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("slug")]
		public string Slug { get; set; }

		[JsonProperty("brand")]
		public string Brand { get; set; }

		[JsonProperty("gdpr")]
		public Gdpr Gdpr { get; set; }

		[JsonProperty("legal")]
		public Legal Legal { get; set; }

		[JsonProperty("terms")]
		public Terms Terms { get; set; }

		[JsonProperty("payment_provider")]
		public string PaymentProvider { get; set; }

		[JsonProperty("payment_intro")]
		public PaymentIntro PaymentIntro { get; set; }

		[JsonProperty("payment_intro_seperator")]
		public PaymentIntroSeperator PaymentIntroSeperator { get; set; }

		[JsonProperty("payment_methods")]
		public List<string> PaymentMethods { get; set; }

		[JsonProperty("payment_six_mode")]
		public string PaymentSixMode { get; set; }

		[JsonProperty("search_placeholder")]
		public string SearchPlaceholder { get; set; }

		[JsonProperty("colors")]
		public Colors Colors { get; set; }

		[JsonProperty("invoice")]
		public Invoice Invoice { get; set; }

		[JsonProperty("payone")]
		public Payone Payone { get; set; }
	}

	public class RateName
	{
		[JsonProperty("de")]
		public string De { get; set; }

		[JsonProperty("en")]
		public string En { get; set; }
	}

	public class Result
	{
		[JsonProperty("address_street")]
		public string AddressStreet { get; set; }

		[JsonProperty("address_zip")]
		public string AddressZip { get; set; }

		[JsonProperty("address_city")]
		public string AddressCity { get; set; }

		[JsonProperty("address_country")]
		public string AddressCountry { get; set; }

		[JsonProperty("distance")]
		public object Distance { get; set; }

		[JsonProperty("uuid")]
		public string Uuid { get; set; }

		[JsonProperty("label")]
		public string Label { get; set; }

		[JsonProperty("evseid")]
		public string Evseid { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("last_status_change")]
		public DateTime LastStatusChange { get; set; }

		[JsonProperty("connector")]
		public Connector Connector { get; set; }

		[JsonProperty("accessibility")]
		public object Accessibility { get; set; }

		[JsonProperty("location")]
		public Location Location { get; set; }

		[JsonProperty("offline")]
		public bool Offline { get; set; }

		[JsonProperty("group")]
		public string Group { get; set; }

		[JsonProperty("icd")]
		public bool Icd { get; set; }

		[JsonProperty("marketing")]
		public Marketing Marketing { get; set; }

		[JsonProperty("station")]
		public Station Station { get; set; }

		[JsonProperty("direct_payment")]
		public DirectPayment DirectPayment { get; set; }

		[JsonProperty("provider")]
		public Provider Provider { get; set; }
	}

	public class Station
	{
		[JsonProperty("uuid")]
		public string Uuid { get; set; }

		[JsonProperty("label")]
		public string Label { get; set; }
	}

	public class Terms
	{
		[JsonProperty("de")]
		public string De { get; set; }
	}
}
