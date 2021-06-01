using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Exiled.DevTools
{
	public class Config : IConfig
	{
		[Description("Indicates whether the plugin is enabled or not")]
		public bool IsEnabled { get; set; } = true;

		[Description("Indicates whether logging events args")]
		public bool LoggingEventArgs { get; set; } = true;

		[Description("Ignored event names for logging")]
		public List<string> DisabledLoggingEvents { get; set; } = new List<string>() { "SyncingData", "Blinking", "ChangingDurability", "UsingRadioBattery" };

		[Description("Ignored network method names(Rpc,Cmd,Target) for logging")]
		public List<string> DisabledLoggingNetworkMethods { get; set; } = new List<string>() { "RpcBlinkTime", "CmdAltIsActive", "CmdSyncItem", "CmdSyncData", "CmdChangeSpeedState", "CmdScp939Noise" };

		[Description("Ignored nest output for logging (Must be Fullname")]
		public List<string> DisabledLoggingClassNameForNest { get; set; } = new List<string>() { "Exiled.API.Features.Player" };
	}
}