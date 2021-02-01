using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Exiled.DevTools
{
	public class Config : IConfig
	{
		[Description("Indicates whether the plugin is enabled or not")]
		public bool IsEnabled { get; set; } = true;

		[Description("Ignored event names for logging")]
		public List<string> DisabledLoggingEvents { get; set; } = new List<string>() { "SyncingData" };

		[Description("Ignored network method names(Rpc,Cmd,Target) for logging")]
		public List<string> DisabledLoggingNetworkMethods { get; set; } = new List<string>() { "RpcBlinkTime", "CmdAltIsActive", "CmdSyncItem" };
	}
}